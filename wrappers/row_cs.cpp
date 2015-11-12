/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void row_destroy(Row* row_ptr)
{
    handle_errors([&]() {
        delete row_ptr;
    });
}

REALM_EXPORT size_t row_get_row_index(const Row* row_ptr)
{
    return handle_errors([&]() {
        return row_ptr->get_index();
    });
}

REALM_EXPORT size_t row_get_is_attached(const Row* row_ptr)
{
    return handle_errors([&]() {
        return bool_to_size_t(row_ptr->is_attached());
    });
}

}   // extern "C"
