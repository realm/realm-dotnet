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
      sort_order.column_indices.push_back(col);
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

REALM_EXPORT size_t results_count(Results* results_ptr)
{
  return handle_errors([&]() {
      return results_ptr->size();
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

struct ManagedAsyncQueryCancellationContext {
  NotificationToken token;
  void* managed_results;
  void(*callback)(void*);
};

REALM_EXPORT ManagedAsyncQueryCancellationContext* results_async(Results* results_ptr, void(*callback)(void*), void* managed_results)
{
  return handle_errors([=]() {
    auto context = new ManagedAsyncQueryCancellationContext();
    context->managed_results = managed_results;
    context->callback = callback;
    context->token = std::move(results_ptr->async([context](std::exception_ptr e) {
      if (e) {
        try {
          std::rethrow_exception(e);
        } catch (const std::exception& ex) {
          std::cerr << "received exception in results change callback: " << ex.what();
          return;
        }
      }
      
      context->callback(context->managed_results);
    }));

    return context;
  });
}

REALM_EXPORT void* asyncquerycancellationtoken_destroy(ManagedAsyncQueryCancellationContext* token_ptr)
{
  return handle_errors([&]() {
    void* managed_results = token_ptr->managed_results;
    delete token_ptr;
    return managed_results;
  });
}

}   // extern "C"
