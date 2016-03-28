/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "results.hpp"

using namespace realm;
using namespace realm::binding;

/// Simple wrapper to keep the Table* we need to lookup column indices when we add clauses to a SortOrder.
struct SortOrderWrapper {
    SortOrder sort_order;
    Table* table;

    SortOrderWrapper(Table* in_table) : table(in_table) {}

    void add_sort(size_t col, bool ascendingCol)
    {
      sort_order.columnIndices.push_back(col);
      sort_order.ascending.push_back(ascendingCol);
    }
};


extern "C" {

REALM_EXPORT void results_destroy(Results* results_ptr)
{
  handle_errors([&]() {
      delete results_ptr;
  });
}

// TODO issue https://github.com/realm/realm-dotnet-private/issues/40 added as needs
// TODO https://github.com/realm/realm-object-store/issues/56 adding Results::operator==
REALM_EXPORT size_t results_is_same_internal_results(Results* lhs, Results* rhs)
{
  return handle_errors([&]() {
      return (lhs == rhs || false /* *lhs == *rhs */);
  });
}

  REALM_EXPORT Results* results_create_for_table(SharedRealm* realm, Table* table_ptr, ObjectSchema* object_schema)
  {
    return handle_errors([&]() {
      return new Results(*realm, *object_schema, *table_ptr);
    });
  }
  
  REALM_EXPORT Results* results_create_for_table_sorted(SharedRealm* realm, Table* table_ptr, ObjectSchema* object_schema, SortOrderWrapper* sortorder_ptr)
  {
    return handle_errors([&]() {
      return new Results(*realm, *object_schema, Query(table_ptr->where()), sortorder_ptr->sort_order);
    });
  }
  
REALM_EXPORT Results* results_create_for_query(SharedRealm* realm, Query * query_ptr, ObjectSchema* object_schema)
{
  return handle_errors([&]() {
      return new Results(*realm, *object_schema, *query_ptr);
  });
}

REALM_EXPORT Results* results_create_for_query_sorted(SharedRealm* realm, Query * query_ptr, ObjectSchema* object_schema, SortOrderWrapper* sortorder_ptr)
{
  return handle_errors([&]() {
      return new Results(*realm, *object_schema, *query_ptr, sortorder_ptr->sort_order);
  });
}

REALM_EXPORT Row* results_get_row(Results* results_ptr, size_t ndx)
{
  return handle_errors([&]() {
    try {
      return new Row(results_ptr->get(ndx));
    }
    catch (std::out_of_range &exp) {
      return (Row*)nullptr;
    }
  });
}

REALM_EXPORT void results_clear(Results* results_ptr)
{
  handle_errors([&]() {
      results_ptr->clear();
  });
}

REALM_EXPORT SortOrderWrapper* sortorder_create_for_table(Table* table_ptr)
{
  return handle_errors([&]() {
      return new SortOrderWrapper(table_ptr);
  });
}

REALM_EXPORT void sortorder_destroy(SortOrderWrapper* sortorder_ptr)
{
  handle_errors([&]() {
      delete sortorder_ptr;
  });
}


REALM_EXPORT void sortorder_add_clause(SortOrderWrapper* sortorder_ptr, uint16_t *  column_name, size_t column_name_len, size_t ascending)
{
  return handle_errors([&]() {
      Utf16StringAccessor str(column_name, column_name_len);
      auto colIndex = sortorder_ptr->table->get_column_index(str);
      sortorder_ptr->add_sort(colIndex, ascending==1);
  });
}


}   // extern "C"
