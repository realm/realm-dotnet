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
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_linklist.hpp"
#include "util/format.hpp"

#include <memory>
#include "timestamp_helpers.hpp"
#include "object-store/src/results.hpp"
#include "marshalable_sort_clause.hpp"
#include "object_accessor.hpp"
#include "schema.hpp"

using namespace realm;
using namespace realm::binding;


extern "C" {

REALM_EXPORT void table_unbind(const Table* table_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        LangBindHelper::unbind_table_ptr(table_ptr);
    });
}

REALM_EXPORT Object* table_add_empty_row(Table* table_ptr, SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->get()->verify_in_write();
        
        size_t row_ndx = table_ptr->add_empty_row(1);
        const StringData object_name = ObjectStore::object_type_for_table_name(table_ptr->get_name());
        auto object_schema = realm->get()->schema().find(object_name);
        return new Object(*realm, *object_schema, *new Row((*table_ptr)[row_ndx]));
    });
}

REALM_EXPORT Object* table_get_link(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();

        object_ptr->realm()->verify_thread();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        const size_t link_row_ndx = object_ptr->row().get_link(column_ndx);
        if (link_row_ndx == realm::npos)
            return nullptr;
        
        auto target_table_ptr = object_ptr->row().get_table()->get_link_target(column_ndx);
        const StringData target_name = ObjectStore::object_type_for_table_name(target_table_ptr->get_name());
        const ObjectSchema& target_schema = *object_ptr->realm()->schema().find(target_name);
        return new Object(object_ptr->realm(), target_schema, *new Row((*target_table_ptr)[link_row_ndx]));
    });
}

REALM_EXPORT SharedLinkViewRef* table_get_linklist(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> SharedLinkViewRef* {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();

        object_ptr->realm()->verify_thread();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return new SharedLinkViewRef {
            std::make_shared<LinkViewRef>(object_ptr->row().get_linklist(column_ndx))
        };  // weird double-layering necessary to get a raw pointer to a shared_ptr
    });
}

REALM_EXPORT size_t table_linklist_is_empty(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return bool_to_size_t(object_ptr->row().linklist_is_empty(column_ndx));
    });
}


REALM_EXPORT size_t table_get_bool(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
            
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return bool_to_size_t(object_ptr->row().get_bool(column_ndx));
    });
}

// Return value is a boolean indicating whether result has a value (i.e. is not null). If true (1), ret_value will contain the actual value.
REALM_EXPORT size_t table_get_nullable_bool(const Object* object_ptr, size_t property_ndx, size_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().is_null(column_ndx))
            return 0;

        ret_value = bool_to_size_t(object_ptr->row().get_bool(column_ndx));
        return 1;
    });
}

REALM_EXPORT int64_t table_get_int64(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return object_ptr->row().get_int(column_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_int64(const Object* object_ptr, size_t property_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().is_null(column_ndx))
            return 0;

        ret_value = object_ptr->row().get_int(column_ndx);
        return 1;
    });
}

REALM_EXPORT float table_get_float(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return object_ptr->row().get_float(column_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_float(const Object* object_ptr, size_t property_ndx, float& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().is_null(column_ndx))
            return 0;

        ret_value = object_ptr->row().get_float(column_ndx);
        return 1;
    });
}

REALM_EXPORT double table_get_double(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return object_ptr->row().get_double(column_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_double(const Object* object_ptr, size_t property_ndx, double& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().is_null(column_ndx))
            return 0;

        ret_value = object_ptr->row().get_double(column_ndx);
        return 1;
    });
}

REALM_EXPORT size_t table_get_string(const Object* object_ptr, size_t property_ndx, uint16_t * datatochsarp, size_t bufsize, bool* is_null, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> size_t {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        StringData fielddata = object_ptr->row().get_string(column_ndx);
        if ((*is_null = fielddata.is_null()))
            return 0;
        
        return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
    });
}

REALM_EXPORT size_t table_get_binary(const Object* object_ptr, size_t property_ndx, const char*& return_buffer, size_t& return_size, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        BinaryData fielddata = object_ptr->row().get_binary(column_ndx);

        if (fielddata.is_null())
            return 0;

        return_buffer = fielddata.data();
        return_size = fielddata.size();
        return 1;
    });
}

REALM_EXPORT int64_t table_get_timestamp_ticks(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        return to_ticks(object_ptr->row().get_timestamp(column_ndx));
    });
}

REALM_EXPORT size_t table_get_nullable_timestamp_ticks(const Object* object_ptr, size_t property_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();

        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().is_null(column_ndx))
            return 0;

        ret_value = to_ticks(object_ptr->row().get_timestamp(column_ndx));
        return 1;
    });
}

REALM_EXPORT void table_set_link(const Object* object_ptr, size_t property_ndx, size_t target_row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_link(column_ndx, target_row_ndx);
    });
}

REALM_EXPORT void table_clear_link(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().nullify_link(column_ndx);
    });
}

REALM_EXPORT void table_set_null(const Object* object_ptr, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (!object_ptr->row().get_table()->is_nullable(column_ndx))
            throw std::invalid_argument("Column is not nullable");

        object_ptr->row().set_null(column_ndx);
    });
}

REALM_EXPORT void table_set_bool(const Object* object_ptr, size_t property_ndx, size_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_bool(column_ndx, size_t_to_bool(value));
    });
}

REALM_EXPORT void table_set_int64(const Object* object_ptr, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_int(column_ndx, value);
    });
}

REALM_EXPORT void table_set_int64_unique(const Object* object_ptr, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        if (object_ptr->row().get_table()->find_first_int(column_ndx, value) != not_found) {
            throw SetDuplicatePrimaryKeyValueException(
                    object_ptr->row().get_table()->get_name(),
                    object_ptr->row().get_table()->get_column_name(column_ndx),
                    util::format("%1", value)
            );
        }
        object_ptr->row().set_int_unique(column_ndx, value);
    });
}

REALM_EXPORT void table_set_float(const Object* object_ptr, size_t property_ndx, float value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_float(column_ndx, value);
    });
}

REALM_EXPORT void table_set_double(const Object* object_ptr, size_t property_ndx, double value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_double(column_ndx, value);
    });
}

REALM_EXPORT void table_set_string(const Object* object_ptr, size_t property_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        Utf16StringAccessor str(value, value_len);
        object_ptr->row().set_string(column_ndx, str);
    });
}

REALM_EXPORT void table_set_string_unique(const Object* object_ptr, size_t property_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        Utf16StringAccessor str(value, value_len);
        if (object_ptr->row().get_table()->find_first_string(column_ndx, str) != not_found) {
            throw SetDuplicatePrimaryKeyValueException(
                    object_ptr->row().get_table()->get_name(),
                    object_ptr->row().get_table()->get_column_name(column_ndx),
                    str.to_string()
            );
        }
        object_ptr->row().set_string_unique(column_ndx, str);
    });
}

REALM_EXPORT void table_set_binary(const Object* object_ptr, size_t property_ndx, char* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_binary(column_ndx, BinaryData(value, value_len));
    });
}

REALM_EXPORT void table_set_timestamp_ticks(const Object* object_ptr, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        const size_t column_ndx = object_ptr->get_object_schema().persisted_properties[property_ndx].table_column;
        object_ptr->row().set_timestamp(column_ndx, from_ticks(value));
    });
}

REALM_EXPORT Query* table_where(Table* table_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Query(table_ptr->where());
    });
}

REALM_EXPORT int64_t table_count_all(Table* table_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return table_ptr->size();
    });
}

REALM_EXPORT size_t table_get_column_index(Table* table_ptr, uint16_t *  column_name, size_t column_name_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
        return table_ptr->get_column_index(str);
    });
}

REALM_EXPORT size_t tableview_get_column_index(TableView* tableView_ptr, uint16_t *  column_name, size_t column_name_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
        return tableView_ptr->get_column_index(str);
    });
}

REALM_EXPORT void table_remove_row(Object* object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
        
        object_ptr->row().get_table()->move_last_over(object_ptr -> row().get_index());
    });
}

REALM_EXPORT Results* table_create_results(Table* table_ptr, SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->get()->verify_thread();
        
        return new Results(*realm, *table_ptr);
    });
}

REALM_EXPORT Results* table_create_sorted_results(Table* table_ptr, SharedRealm* realm, MarshalableSortClause* sort_clauses, size_t clause_count, size_t* flattened_column_indices, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::vector<std::vector<size_t>> column_indices;
        std::vector<bool> ascending;

        auto& properties = realm->get()->schema().find(table_ptr->get_name())->persisted_properties;
        unflatten_sort_clauses(sort_clauses, clause_count, flattened_column_indices, column_indices, ascending, properties);

        auto sort_descriptor = SortDescriptor(*table_ptr, column_indices, ascending);
        return new Results(*realm, table_ptr->where(), sort_descriptor);
    });
}


}   // extern "C"
