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
#include "marshalling.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"
#include "timestamp_helpers.hpp"


using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void query_destroy(Query* query_ptr)
{
    return handle_errors([&]() {
        delete(query_ptr);
    });
}


REALM_EXPORT Row* query_find(Query * query_ptr, size_t begin_at_table_row)
{
    return handle_errors([&]() {
        if (begin_at_table_row >= query_ptr->get_table()->size())
            return (Row*)nullptr;

        size_t row_ndx = query_ptr->find(begin_at_table_row);

        if (row_ndx == not_found)
            return (Row*)nullptr;

        return new Row((*query_ptr->get_table())[row_ndx]);
    });
}

REALM_EXPORT size_t query_count(Query * query_ptr)
{
    return handle_errors([&]() {
        return query_ptr->count();
    });
}


//convert from columnName to columnIndex returns -1 if the string is not a column name
//assuming that the get_table() does not return anything that must be deleted
REALM_EXPORT size_t query_get_column_index(Query* query_ptr, uint16_t *  column_name, size_t column_name_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(column_name, column_name_len);
        return query_ptr->get_table()->get_column_index(str);
    });
}

REALM_EXPORT void query_not(Query * query_ptr)
{
    handle_errors([&]() {
        query_ptr->Not();
    });
}

REALM_EXPORT void query_group_begin(Query * query_ptr)
{
    handle_errors([&]() {
        query_ptr->group();
    });
}

REALM_EXPORT void query_group_end(Query * query_ptr)
{
    handle_errors([&]() {
        query_ptr->end_group();
    });
}

REALM_EXPORT void query_or(Query * query_ptr)
{
    handle_errors([&]() {
        query_ptr->Or();
    });
}

REALM_EXPORT void query_string_contains(Query* query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        query_ptr->contains(columnIndex, str);
    });
}

REALM_EXPORT void query_string_starts_with(Query* query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        query_ptr->begins_with(columnIndex, str);
    });
}

REALM_EXPORT void query_string_ends_with(Query* query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        query_ptr->ends_with(columnIndex, str);
    });
}
    
REALM_EXPORT void query_string_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        query_ptr->equal(columnIndex, str);
    });
}

REALM_EXPORT void query_string_not_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        query_ptr->not_equal(columnIndex, str);
    });
}

REALM_EXPORT void query_bool_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, size_t_to_bool(value));
    });
}

REALM_EXPORT void query_bool_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, size_t_to_bool(value));
    });
}

REALM_EXPORT void query_int_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_less(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->less(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_less_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->less_equal(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_greater(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->greater(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_greater_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    handle_errors([&]() {
        query_ptr->greater_equal(columnIndex, static_cast<int>(value));
    });
}

REALM_EXPORT void query_long_equal(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, value);
    });
}

REALM_EXPORT void query_long_not_equal(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, value);
    });
}

REALM_EXPORT void query_long_less(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->less(columnIndex, value);
    });
}

REALM_EXPORT void query_long_less_equal(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->less_equal(columnIndex, value);
    });
}

REALM_EXPORT void query_long_greater(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->greater(columnIndex, value);
    });
}

REALM_EXPORT void query_long_greater_equal(Query * query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->greater_equal(columnIndex, value);
    });
}
    
    REALM_EXPORT void query_float_equal(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_not_equal(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_less(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->less(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_less_equal(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->less_equal(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_greater(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->greater(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_greater_equal(Query * query_ptr, size_t columnIndex, float value)
{
    handle_errors([&]() {
        query_ptr->greater_equal(columnIndex, static_cast<float>(value));
    });
}

REALM_EXPORT void query_double_equal(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_not_equal(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_less(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->less(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_less_equal(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->less_equal(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_greater(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->greater(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_greater_equal(Query * query_ptr, size_t columnIndex, double value)
{
    handle_errors([&]() {
        query_ptr->greater_equal(columnIndex, static_cast<double>(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_equal(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_not_equal(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_less(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->less(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_less_equal(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->less_equal(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_greater(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->greater(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_timestamp_milliseconds_greater_equal(Query* query_ptr, size_t columnIndex, int64_t value)
{
    handle_errors([&]() {
        query_ptr->greater_equal(columnIndex, from_milliseconds(value));
    });
}

REALM_EXPORT void query_binary_equal(Query* query_ptr, size_t columnIndex, char* buffer, size_t buffer_length)
{
    handle_errors([&]() {
        query_ptr->equal(columnIndex, BinaryData(buffer, buffer_length));
    });
}

REALM_EXPORT void query_binary_not_equal(Query* query_ptr, size_t columnIndex, char* buffer, size_t buffer_length)
{
    handle_errors([&]() {
        query_ptr->not_equal(columnIndex, BinaryData(buffer, buffer_length));
    });
}

}   // extern "C"
