////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

#include <realm/object-store/shared_realm.hpp>
#include <realm/obj.hpp>
#include <realm/mixed.hpp>
#include <realm/list.hpp>
#include <realm/set.hpp>
#include <realm/dictionary.hpp>

#include "realm_export_decls.hpp"

using namespace realm;

// Microsoft's GUID layout, matching how System.Guid is represented
struct RealmGUID {
public:
    inline void swap_endianness() {
        Data1 = swap(Data1);
        Data2 = swap(Data2);
        Data3 = swap(Data3);
    }

private:
    uint32_t Data1;
    uint16_t Data2;
    uint16_t Data3;
    uint8_t Data4[8];
    
    inline unsigned short swap(uint16_t value) {
        return ((value & 0xff) << 8) | ((value & 0xff00) >> 8);
    }
    
    inline unsigned long swap(uint32_t value) {
        unsigned long tmp = ((value << 8) & 0xFF00FF00) | ((value >> 8) & 0xFF00FF);
        return (tmp << 16) | (tmp >> 16);
    }
};

static bool flip_guid(Mixed& mixed, bool& found_non_v4_uuid) {
    if (!mixed.is_null() && mixed.get_type() == type_UUID) {
        UUID::UUIDBytes bytes = mixed.get_uuid().to_bytes();

        // try to detect if this is a Microsoft GUID or a UUID
        // We do this by checking the version (see https://en.wikipedia.org/wiki/Universally_unique_identifier#Format).
        // If the first four bits of the 7th byte in the array are equal to 4, then this is a valid UUID of version 4 (random-generated UUID).
        // A Microsoft GUID on the other hand will have the version in the 8th byte, because the Data3 field is encoded in a big-endian format.
        // We can assume that if the 4th byte doesn't carry the version 4 bits, but the 8th one does then this is a Microsoft GUID
        // created with Guid.NewGuid(). Manual testing shows that Guid.NewGuid() has only about ~6% probability of generating a GUID where
        // both the 7th and 8th bits match the bit pattern, so a realm file created with the .NET SDK should hold a majority
        // of GUID values where only the 8th byte matches the version 4 bit pattern.
        found_non_v4_uuid |= (bytes[6] >> 4) != 4 && (bytes[7] >> 4) == 4;

        RealmGUID& guid = *reinterpret_cast<RealmGUID*>(bytes.data());
        guid.swap_endianness();
        mixed = Mixed(UUID(std::move(bytes)));
        return true;
    }
    return false;
}

static bool byteswap_guids(TableRef table, bool& found_non_v4_uuid)
{
    bool has_uuid_column = false;
    std::vector<ColKey> primitive_columns;
    std::vector<ColKey> list_columns;
    std::vector<ColKey> set_columns;
    std::vector<ColKey> dictionary_columns;
    table->for_each_public_column([&](ColKey col) {
        if (col.get_type() == col_type_UUID || col.get_type() == col_type_Mixed) {
            has_uuid_column = true;

            if (col.is_list()) {
                list_columns.push_back(col);
            } else if (col.is_set()) {
                set_columns.push_back(col);
            } else if (col.is_dictionary()) {
                dictionary_columns.push_back(col);
            } else {
                REALM_ASSERT(!col.is_collection());
                primitive_columns.push_back(col);
            }
        }
        return IteratorControl::AdvanceToNext; // keep iterating
    });

    if (!has_uuid_column)
    {
        return false;
    }

    for (Obj& obj : *table) {
        for (auto col : primitive_columns) {
            Mixed m = obj.get_any(col);
            if (flip_guid(m, found_non_v4_uuid)) {
                obj.set_any(col, m);
            }
        }
        for (auto col : list_columns) {
            auto list = obj.get_listbase_ptr(col);
            for (size_t i = 0; i < list->size(); i++) {
                Mixed value = list->get_any(i);
                if (flip_guid(value, found_non_v4_uuid)) {
                    list->set_any(i, value);
                }
            }
        }
        for (auto col : set_columns) {
            auto set = obj.get_setbase_ptr(col);
            std::vector<Mixed> values(set->size());
            for (size_t i = 0; i < set->size(); i++) {
                Mixed value = set->get_any(i);
                flip_guid(value, found_non_v4_uuid);
                values[i] = std::move(value);
            }
            set->clear();
            for (auto& value : values) {
                set->insert_any(std::move(value));
            }
        }
        for (auto col : dictionary_columns) {
            auto dict = obj.get_dictionary_ptr(col);
            std::map<Mixed, Mixed> values;
            for (auto pair : *dict) {
                flip_guid(pair.second, found_non_v4_uuid);
                values.emplace(pair.first, pair.second);
            }
            dict->clear();
            for (auto pair : values) {
                dict->insert(pair.first, pair.second);
            }
        }
    }

    return table->size() > 0;
}

// marker table to say that this file has been processed already and can be skipped.
static constexpr char c_guid_fix_table[] = "dotnet_guid_representation_fixed";

namespace realm {
bool requires_guid_representation_fix(SharedRealm& realm)
{
    return !realm->read_group().has_table(c_guid_fix_table);
}

void apply_guid_representation_fix(SharedRealm& realm, bool& found_non_v4_uuid, bool& found_guid_columns)
{
    realm->begin_transaction();
    auto* group = &realm->read_group();

    // Check if someone has added the marker table before us - if that's the case, we should return
    // rather than do the work to swap guids.
    if (group->has_table(c_guid_fix_table)) {
        realm->cancel_transaction();
        return;
    }

    for (TableKey key : group->get_table_keys()) {
        if (group->table_is_public(key)) {
            found_guid_columns |= byteswap_guids(group->get_table(key), found_non_v4_uuid);
        }
    }

    if (!found_non_v4_uuid) {
        // we didn't find any Microsoft GUID (see comment in flip_guid())
        // so this is likely a realm file that wasn't created with the .NET SDK
        // or doesn't have big-endian GUID values anyway
        // let's cancel the current transaction and start a new one just to record the marker table
        realm->cancel_transaction();
        realm->begin_transaction();
        group = &realm->read_group();

        // Re-check for the marker table as we exited a write transaction, so another thread
        // might have added the table before re-acquiring the write lock.
        if (group->has_table(c_guid_fix_table)) {
            realm->cancel_transaction();
            return;
        }
    }


    group->add_table(c_guid_fix_table);
    realm->commit_transaction();
}
} // namespace realm

extern "C" REALM_EXPORT void _realm_flip_guid_for_testing(uint8_t* buffer)
{
    reinterpret_cast<RealmGUID*>(buffer)->swap_endianness();
}
