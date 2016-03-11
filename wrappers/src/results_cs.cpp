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

extern "C" {

REALM_EXPORT void results_destroy(Results* results_ptr)
{
  handle_errors([&]() {
      delete results_ptr;
  });
}


REALM_EXPORT size_t results_is_same_internal_results(Results* lhs, Results* rhs)
{
  return handle_errors([&]() {
      return (lhs == rhs || false /* *lhs == *rhs */);
  });
}

REALM_EXPORT Results* results_create_for_table(SharedRealm* realm, Table* table_ptr, ObjectSchema* object_schema)
{
  return handle_errors([&]() {
      auto ret = new Results(*realm, *object_schema, *table_ptr);
      ret->set_live(true);
      return ret;
  });
}

REALM_EXPORT Results* results_create_for_query(SharedRealm* realm, Query * query_ptr, ObjectSchema* object_schema)
{
  return handle_errors([&]() {
    auto ret = new Results(*realm, *object_schema, *query_ptr/* TODO pass sort order in */);
    ret->set_live(true);
    return ret;
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

}   // extern "C"
