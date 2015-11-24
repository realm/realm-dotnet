/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_linklist.hpp"

#include <memory>

using namespace realm;
using namespace realm::binding;


extern "C" {

REALM_EXPORT void table_unbind(Table* table_ptr)
{
    return handle_errors([&]() {
        LangBindHelper::unbind_table_ptr(table_ptr);
    });
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

REALM_EXPORT Row* table_get_link(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
  return handle_errors([&]() -> Row* {
    const size_t link_row_ndx = table_ptr->get_link(column_ndx, row_ndx);
    if (link_row_ndx == realm::npos)
      return nullptr;
    auto target_table_ptr = table_ptr->get_link_target(column_ndx);
    return new Row((*target_table_ptr)[link_row_ndx]);
  });
}

REALM_EXPORT SharedLinkViewRef* table_get_linklist(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
  return handle_errors([&]() -> SharedLinkViewRef* {
    SharedLinkViewRef sr = std::make_shared<LinkViewRef>(table_ptr->get_linklist(column_ndx, row_ndx));
    return new SharedLinkViewRef{ sr };  // weird double-layering necessary to get a raw pointer to a shared_ptr
  });
}


REALM_EXPORT size_t table_get_bool(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return handle_errors([&]() {
        return bool_to_size_t(table_ptr->get_bool(column_ndx, row_ndx));
    });
}

REALM_EXPORT int64_t table_get_int64(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return handle_errors([&]() {
        return table_ptr->get_int(column_ndx, row_ndx);
    });
}

REALM_EXPORT float table_get_float(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return handle_errors([&]() {
        return table_ptr->get_float(column_ndx, row_ndx);
    });
}

REALM_EXPORT double table_get_double(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return handle_errors([&]() {
        return table_ptr->get_double(column_ndx, row_ndx);
    });
}

REALM_EXPORT size_t table_get_string(const Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t * datatochsarp, size_t bufsize)
{
    return handle_errors([&]() {
        StringData fielddata = table_ptr->get_string(column_ndx, row_ndx);
        return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
    });
}

REALM_EXPORT int64_t table_get_datetime_seconds(const Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
	return handle_errors([&]() {
		return table_ptr->get_datetime(column_ndx, row_ndx).get_datetime();
	});
}

REALM_EXPORT void table_set_link(Table* table_ptr, size_t column_ndx, size_t row_ndx, size_t target_row_ndx)
{
    return handle_errors([&]() {
        table_ptr->set_link(column_ndx, row_ndx, target_row_ndx);
    });
}

REALM_EXPORT void table_clear_link(Table* table_ptr, size_t column_ndx, size_t row_ndx)
{
    return handle_errors([&]() {
        table_ptr->nullify_link(column_ndx, row_ndx);
    });
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

REALM_EXPORT void table_set_float(Table* table_ptr, size_t column_ndx, size_t row_ndx, float value)
{
    return handle_errors([&]() {
        table_ptr->set_float(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_double(Table* table_ptr, size_t column_ndx, size_t row_ndx, double value)
{
    return handle_errors([&]() {
        table_ptr->set_double(column_ndx, row_ndx, value);
    });
}

REALM_EXPORT void table_set_string(Table* table_ptr, size_t column_ndx, size_t row_ndx, uint16_t* value, size_t value_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str(value, value_len);
        table_ptr->set_string(column_ndx, row_ndx, str);
    });
}

REALM_EXPORT void table_set_datetime_seconds(Table* table_ptr, size_t column_ndx, size_t row_ndx, int64_t value)
{
	return handle_errors([&]() {
		DateTime dt(value);
		table_ptr->set_datetime(column_ndx, row_ndx, dt);
	});
}

REALM_EXPORT Query* table_where(Table* table_ptr)
{
    return handle_errors([&]() {
        return new Query(table_ptr->where());
    });
}

REALM_EXPORT size_t table_get_column_index(Table* table_ptr, uint16_t *  column_name, size_t column_name_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
        return table_ptr->get_column_index(str);
    });
}

REALM_EXPORT size_t tableview_get_column_index(TableView* tableView_ptr, uint16_t *  column_name, size_t column_name_len)
{
    return handle_errors([&]() {
        Utf16StringAccessor str = Utf16StringAccessor(column_name, column_name_len);
        return tableView_ptr->get_column_index(str);
    });
}

REALM_EXPORT void table_remove_row(Table* table_ptr, Row* row_ptr)
{
    handle_errors([&]() {
        table_ptr->move_last_over(row_ptr->get_index());
    });
}

}   // extern "C"
