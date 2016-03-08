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

REALM_EXPORT void results_destroy(Row* results_ptr)
{
    handle_errors([&]() {
        delete results_ptr;
    });
}

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr where(TableHandle handle);



REALM_EXPORT size_t results_get_results_index(const Row* results_ptr)
{
    return handle_errors([&]() {
        return results_ptr->get_index();
    });
}

REALM_EXPORT size_t results_get_is_attached(const Row* results_ptr)
{
    return handle_errors([&]() {
        return bool_to_size_t(results_ptr->is_attached());
    });
}

}   // extern "C"
