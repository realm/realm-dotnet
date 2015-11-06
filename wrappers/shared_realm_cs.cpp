////////////////////////////////////////////////////////////////////////////
//
// Copyright 2015 Realm Inc.
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

#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"
#include "object-store/shared_realm.hpp"
#include "object-store/schema.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT SharedRealm* shared_realm_open(Schema* schema, uint16_t* path, size_t path_len, bool read_only, SharedGroup::DurabilityLevel durability,
                        uint16_t* encryption_key, size_t encryption_key_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor pathStr(path, path_len);
        Utf16StringAccessor encryptionStr(encryption_key, encryption_key_len);


        Realm::Config config;
        config.path = pathStr.to_string();
        config.read_only = read_only;
        config.in_memory = durability != SharedGroup::durability_Full;

        config.encryption_key = std::vector<char>(&encryptionStr.data()[0], &encryptionStr.data()[encryptionStr.size()]);

        config.schema.reset(schema);
        return new SharedRealm{Realm::get_shared_realm(config)};
    });
}

REALM_EXPORT void shared_realm_destroy(SharedRealm* realm)
{
    handle_errors([&]() {
        delete realm;
    });
}

REALM_EXPORT size_t shared_realm_has_table(SharedRealm* realm, uint16_t* table_name, size_t table_name_len)
{
    return handle_errors([&]() {
        Group* g = (*realm)->read_group();
        Utf16StringAccessor str(table_name, table_name_len);

        return bool_to_size_t(g->has_table(str));
    });
}

REALM_EXPORT Table* shared_realm_get_table(SharedRealm* realm, uint16_t* table_name, size_t table_name_len)
{
    return handle_errors([&]() {
        Group* g = (*realm)->read_group();
        Utf16StringAccessor str(table_name, table_name_len);

        bool dummy; // get_or_add_table sets this to true if the table was added.
        return LangBindHelper::get_or_add_table(*g, str, &dummy);
    });
}

REALM_EXPORT void shared_realm_begin_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->begin_transaction();
    });
}

REALM_EXPORT void shared_realm_commit_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->commit_transaction();
    });
}

REALM_EXPORT void shared_realm_cancel_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->cancel_transaction();
    });
}

REALM_EXPORT size_t shared_realm_is_in_transaction(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->is_in_transaction());
    });
}

REALM_EXPORT size_t shared_realm_refresh(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->refresh());
    });
}

}
