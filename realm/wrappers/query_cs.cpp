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
#include "marshalling.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.h"
#include "object-store/shared_realm.hpp"
#include "object-store/schema.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

#pragma region query general // {{{

REALM_EXPORT void query_destroy(Query* query_ptr)
{
    delete(query_ptr);
}

// TODO: Replace this with TableView.
REALM_EXPORT Row* query_find(Query * query_ptr, size_t begin_at_table_row)
{
    if (begin_at_table_row >= query_ptr->get_table()->size())
        return nullptr;

    size_t row_ndx = query_ptr->find(begin_at_table_row);

    if (row_ndx == not_found)
        return nullptr;

    return new Row((*query_ptr->get_table())[row_ndx]);
}

//convert from columnName to columnIndex returns -1 if the string is not a column name
//assuming that the get_table() does not return anything that must be deleted
REALM_EXPORT size_t query_get_column_index(Query* query_ptr, uint16_t *  column_name, size_t column_name_len)
{
    Utf16StringAccessor str(column_name, column_name_len);
    return query_ptr->get_table()->get_column_index(str);
}

#pragma endregion // }}}

#pragma region query group // {{{

REALM_EXPORT void query_group_begin(Query * query_ptr)
{
    query_ptr->group();
}

REALM_EXPORT void query_group_end(Query * query_ptr)
{
    query_ptr->end_group();
}

REALM_EXPORT void query_or(Query * query_ptr)
{
    query_ptr->Or();
}

#pragma endregion // }}}

#pragma region query string // {{{

REALM_EXPORT void query_string_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    Utf16StringAccessor str(value, value_len);
    query_ptr->equal(columnIndex, str);
}

REALM_EXPORT void query_string_not_equal(Query * query_ptr, size_t columnIndex, uint16_t* value, size_t value_len)
{
    Utf16StringAccessor str(value, value_len);
    query_ptr->not_equal(columnIndex, str);
}

#pragma endregion // }}}

#pragma region query bool // {{{
REALM_EXPORT void query_bool_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->equal(columnIndex, size_t_to_bool(value));
}

REALM_EXPORT void query_bool_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->not_equal(columnIndex, size_t_to_bool(value));
}

#pragma endregion // }}}


#pragma region query int // {{{
REALM_EXPORT void query_int_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->equal(columnIndex, static_cast<int>(value));
}

REALM_EXPORT void query_int_not_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->not_equal(columnIndex, static_cast<int>(value));
}

REALM_EXPORT void query_int_less(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->less(columnIndex, static_cast<int>(value));
}

REALM_EXPORT void query_int_less_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->less_equal(columnIndex, static_cast<int>(value));
}

REALM_EXPORT void query_int_greater(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->greater(columnIndex, static_cast<int>(value));
}

REALM_EXPORT void query_int_greater_equal(Query * query_ptr, size_t columnIndex, size_t value)
{
    query_ptr->greater_equal(columnIndex, static_cast<int>(value));
}

#pragma endregion // }}}

}   // extern "C"
