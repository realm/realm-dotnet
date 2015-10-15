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
#include "marshalling.hpp"
#include "realm_export_decls.h"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT Table* new_table()
{
    return LangBindHelper::new_table();
}

REALM_EXPORT void table_unbind(Table* table_ptr)
{
    LangBindHelper::unbind_table_ptr(table_ptr);
}

REALM_EXPORT size_t table_add_column(Table* table_ptr, size_t type, uint16_t * name, size_t name_len, size_t nullable)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(name, name_len);
        return table_ptr->add_column(size_t_to_datatype(type), str, size_t_to_bool(nullable));
    });
}

REALM_EXPORT Row* table_add_empty_row(Table* table_ptr)
{
    return handle_errors([&]() {
        size_t row_ndx = table_ptr->add_empty_row(1);
        return new Row((*table_ptr)[row_ndx]);
    });
}

REALM_EXPORT size_t table_get_bool(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return bool_to_size_t(table_ptr->get_bool(column_ndx, row_ndx));
}

REALM_EXPORT int64_t table_get_int64(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return table_ptr->get_int(column_ndx, row_ndx);
}

REALM_EXPORT size_t table_get_string(const Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize)
{
    StringData fielddata = table_ptr->get_string(column_ndx, row_ndx);
    return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
}

REALM_EXPORT void table_set_bool(Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t value)
{
    return handle_errors([&]() {
        table_ptr->set_bool(column_ndx, row_ndx, size_t_to_bool(value));
    });
}

REALM_EXPORT void table_set_int64(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
    return handle_errors([&]() {
        table_ptr->set_int(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value, size_t value_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        table_ptr->set_string(column_ndx, row_ndx, str);
    });
}

REALM_EXPORT Query* table_where(Table* table_ptr)
{
    return new Query(table_ptr->where());
}

REALM_EXPORT size_t table_get_column_index(Table* table_ptr, uint16_t *  column_name, size_t column_name_len)
{
    Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
    return table_ptr->get_column_index(str);
}

REALM_EXPORT size_t tableview_get_column_index(TableView* tableView_ptr, uint16_t *  column_name, size_t column_name_len)
{
    Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
    return tableView_ptr->get_column_index(str);
}

REALM_EXPORT void table_remove_row(Table* table_ptr, Row* row_ptr)
{
    table_ptr->move_last_over(row_ptr->get_index());
}

}   // extern "C"
