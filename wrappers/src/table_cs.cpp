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

REALM_EXPORT Object* table_add_empty_object(Table* table_ptr, SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->get()->verify_in_write();
        
        size_t row_ndx = table_ptr->add_empty_row(1);
        const std::string object_name(ObjectStore::object_type_for_table_name(table_ptr->get_name()));
        auto& object_schema = *realm->get()->schema().find(object_name);
        return new Object(*realm, object_schema, Row((*table_ptr)[row_ndx]));
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

        DescriptorOrdering ordering;
        ordering.append_sort({*table_ptr, column_indices, ascending});
        return new Results(*realm, table_ptr->where(), std::move(ordering));
    });
}
    
Object* object_for_primarykey(Table* table_ptr, SharedRealm* realm, std::function<size_t(size_t, Table*)> finder, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object*{
        realm->get()->verify_thread();
        
        const std::string object_name(ObjectStore::object_type_for_table_name(table_ptr->get_name()));
        auto& object_schema = *realm->get()->schema().find(object_name);
        if (object_schema.primary_key.empty()) {
            const std::string name(table_ptr->get_name());
            throw MissingPrimaryKeyException(name);
        }
        
        const size_t column_index = object_schema.primary_key_property()->table_column;
        const size_t row_ndx = finder(column_index, table_ptr);
        if (row_ndx == not_found)
            return nullptr;
        
        return new Object(*realm, object_schema, Row(table_ptr->get(row_ndx)));
    });
}

REALM_EXPORT Object* object_for_int_primarykey(Table* table_ptr, SharedRealm* realm, int64_t value, NativeException::Marshallable& ex)
{
    return object_for_primarykey(table_ptr, realm, [=](size_t column_index, Table* table) {
        return table->find_first_int(column_index, value);
    }, ex);
}
    
REALM_EXPORT Object* object_for_null_primarykey(Table* table_ptr, SharedRealm* realm, NativeException::Marshallable& ex)
{
    return object_for_primarykey(table_ptr, realm, [=](size_t column_index, Table* table) {
        return table->find_first_null(column_index);
    }, ex);
}

REALM_EXPORT Object* object_for_string_primarykey(Table* table_ptr, SharedRealm* realm, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    Utf16StringAccessor str(value, value_len);
    return object_for_primarykey(table_ptr, realm, [&](size_t column_index, Table* table) {
        return table->find_first_string(column_index, str);
    }, ex);
}

}   // extern "C"
