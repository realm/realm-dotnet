/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "marshalling.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"

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

}   // extern "C"
