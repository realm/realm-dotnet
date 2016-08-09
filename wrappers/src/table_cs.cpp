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

#include <memory>
#include "timestamp_helpers.hpp"
#include "object-store/src/results.hpp"
#include "sort_order_wrapper.hpp"

using namespace realm;
using namespace realm::binding;


extern "C" {

REALM_EXPORT void table_unbind(Table* table_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        LangBindHelper::unbind_table_ptr(table_ptr);
    });
}

REALM_EXPORT Row* table_add_empty_row(Table* table_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        size_t row_ndx = table_ptr->add_empty_row(1);
        return new Row((*table_ptr)[row_ndx]);
    });
}

REALM_EXPORT Row* table_get_link(Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
  return handle_errors(ex, [&]() -> Row* {
    const size_t link_row_ndx = table_ptr->get_link(column_ndx, row_ndx);
    if (link_row_ndx == realm::npos)
      return nullptr;
    auto target_table_ptr = table_ptr->get_link_target(column_ndx);
    return new Row((*target_table_ptr)[link_row_ndx]);
  });
}

REALM_EXPORT SharedLinkViewRef* table_get_linklist(Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
  return handle_errors(ex, [&]() -> SharedLinkViewRef* {
    return new SharedLinkViewRef { 
        std::make_shared<LinkViewRef>(table_ptr->get_linklist(column_ndx, row_ndx)) 
    };  // weird double-layering necessary to get a raw pointer to a shared_ptr
  });
}

REALM_EXPORT size_t table_linklist_is_empty(Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]()  {
        return bool_to_size_t(table_ptr->linklist_is_empty(column_ndx, row_ndx));
    });
}


REALM_EXPORT size_t table_get_bool(const Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return bool_to_size_t(table_ptr->get_bool(column_ndx, row_ndx));
    });
}

// Return value is a boolean indicating whether result has a value (i.e. is not null). If true (1), ret_value will contain the actual value.
REALM_EXPORT size_t table_get_nullable_bool(const Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (table_ptr->is_null(column_ndx, row_ndx))
            return 0;

        ret_value = bool_to_size_t(table_ptr->get_bool(column_ndx, row_ndx));
        return 1;
    });
}

REALM_EXPORT int64_t table_get_int64(const Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return table_ptr->get_int(column_ndx, row_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_int64(const Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (table_ptr->is_null(column_ndx, row_ndx))
            return 0;

        ret_value = table_ptr->get_int(column_ndx, row_ndx);
        return 1;
    });
}

REALM_EXPORT float table_get_float(const Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return table_ptr->get_float(column_ndx, row_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_float(const Table* table_ptr, size_t column_ndx, size_t row_ndx, float& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (table_ptr->is_null(column_ndx, row_ndx))
            return 0;

        ret_value = table_ptr->get_float(column_ndx, row_ndx);
        return 1;
    });
}

REALM_EXPORT double table_get_double(const Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return table_ptr->get_double(column_ndx, row_ndx);
    });
}

REALM_EXPORT size_t table_get_nullable_double(const Table* table_ptr, size_t column_ndx, size_t row_ndx, double& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (table_ptr->is_null(column_ndx, row_ndx))
            return 0;

        ret_value = table_ptr->get_double(column_ndx, row_ndx);
        return 1;
    });
}

REALM_EXPORT size_t table_get_string(const Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize, bool* is_null, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> size_t {
        StringData fielddata = table_ptr->get_string(column_ndx, row_ndx);
        if ((*is_null = fielddata.is_null()))
            return 0;
        
        return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
    });
}

REALM_EXPORT size_t table_get_binary(const Table* table_ptr, size_t column_ndx, size_t row_ndx, const char*& return_buffer, size_t& return_size, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto fielddata = table_ptr->get_binary(column_ndx, row_ndx);

        if (fielddata.is_null())
            return 0;

        return_buffer = fielddata.data();
        return_size = fielddata.size();
        return 1;
    });
}

REALM_EXPORT int64_t table_get_timestamp_ticks(const Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return to_ticks(table_ptr->get_timestamp(column_ndx, row_ndx));
    });
}

REALM_EXPORT size_t table_get_nullable_timestamp_ticks(const Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (table_ptr->is_null(column_ndx, row_ndx))
            return 0;

        ret_value = to_ticks(table_ptr->get_timestamp(column_ndx, row_ndx));
        return 1;
    });
}

REALM_EXPORT void table_set_link(Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t target_row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_link(column_ndx, row_ndx, target_row_ndx);
    });
}

REALM_EXPORT void table_clear_link(Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->nullify_link(column_ndx, row_ndx);
    });
}

REALM_EXPORT void table_set_null(Table* table_ptr, size_t column_ndx, size_t row_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (!table_ptr->is_nullable(column_ndx))
            throw new std::invalid_argument("Column is not nullable");

        table_ptr->set_null(column_ndx, row_ndx);
    });
}

REALM_EXPORT void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_bool(column_ndx, row_ndx, size_t_to_bool(value));
    });
}

REALM_EXPORT void table_set_int64(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_int(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_int64_unique(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_int_unique(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_float(Table* table_ptr, size_t column_ndx, size_t row_ndx, float value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_float(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_double(Table* table_ptr, size_t column_ndx, size_t row_ndx, double value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_double(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        table_ptr->set_string(column_ndx, row_ndx, str);
    });
}

REALM_EXPORT void table_set_string_unique(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        table_ptr->set_string_unique(column_ndx, row_ndx, str);
    });
}

REALM_EXPORT void table_set_binary(Table* table_ptr, size_t column_ndx, size_t row_ndx, char* value, size_t value_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_binary(column_ndx, row_ndx, BinaryData(value, value_len));
    });
}

REALM_EXPORT void table_set_timestamp_ticks(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        table_ptr->set_timestamp(column_ndx, row_ndx, from_ticks(value));
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

REALM_EXPORT void table_remove_row(Table* table_ptr, Row* row_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        table_ptr->move_last_over(row_ptr->get_index());
    });
}

REALM_EXPORT Results* table_create_results(Table* table_ptr, SharedRealm* realm, ObjectSchema* object_schema, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(*realm, *object_schema, *table_ptr);
    });
}

REALM_EXPORT Results* table_create_sorted_results(Table* table_ptr, SharedRealm* realm, ObjectSchema* object_schema, SortOrderWrapper* sortorder_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(*realm, *object_schema, table_ptr->where(), sortorder_ptr->sort_order);
    });
}


}   // extern "C"
