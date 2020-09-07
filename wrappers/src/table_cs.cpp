////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"

#include <memory>
#include "timestamp_helpers.hpp"
#include "object-store/src/results.hpp"
#include "object_accessor.hpp"
#include "schema.hpp"

using namespace realm;
using namespace realm::binding;

template <typename T>
Object* get_object_for_primarykey(TableRef& table, SharedRealm& realm, const T& value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        realm->verify_thread();

        const std::string object_name(ObjectStore::object_type_for_table_name(table->get_name()));
        auto& object_schema = *realm->schema().find(object_name);
        if (object_schema.primary_key.empty()) {
            const std::string name(table->get_name());
            throw MissingPrimaryKeyException(name);
        }

        const ColKey column_key = object_schema.primary_key_property()->column_key;
        const ObjKey obj_key = table->find_first(column_key, value);
        if (!obj_key)
            return nullptr;

        return new Object(realm, object_schema, table->get_object(obj_key));
        });
}

extern "C" {

REALM_EXPORT void table_destroy(TableRef* table, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        delete table;
    });
}

REALM_EXPORT Object* table_add_empty_object(TableRef& table, SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_in_write();

        Obj obj = table->create_object();
        const std::string object_name(ObjectStore::object_type_for_table_name(table->get_name()));
        auto& object_schema = *realm->schema().find(object_name);
        return new Object(realm, object_schema, obj);
    });
}

REALM_EXPORT Results* table_create_results(TableRef& table, SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_thread();

        return new Results(realm, table);
    });
}

REALM_EXPORT size_t table_get_name(TableRef& table, uint16_t* string_buffer, size_t buffer_size, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return stringdata_to_csharpstringbuffer(table->get_name(), string_buffer, buffer_size);
    });
}

REALM_EXPORT size_t table_get_column_name(TableRef& table, const ColKey column_key, uint16_t* string_buffer, size_t buffer_size, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return stringdata_to_csharpstringbuffer(table->get_column_name(column_key), string_buffer, buffer_size);
    });
}

REALM_EXPORT Object* table_get_object(TableRef& table, SharedRealm& realm, ObjKey object_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        realm->verify_thread();

        Obj obj = table->get_object(object_key);
        if (!obj) {
            return nullptr;
        }

        return new Object(realm, obj);
    });
}

REALM_EXPORT Object* table_get_object_for_int_primarykey(TableRef& table, SharedRealm& realm, int64_t value, NativeException::Marshallable& ex)
{
    return get_object_for_primarykey(table, realm, value, ex);
}

REALM_EXPORT Object* table_get_object_for_null_primarykey(TableRef& table, SharedRealm& realm, NativeException::Marshallable& ex)
{
    return get_object_for_primarykey(table, realm, null{}, ex);
}

REALM_EXPORT Object* table_get_object_for_string_primarykey(TableRef& table, SharedRealm& realm, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    Utf16StringAccessor str(value, value_len);
    return get_object_for_primarykey(table, realm, StringData(str), ex);
}

}   // extern "C"
