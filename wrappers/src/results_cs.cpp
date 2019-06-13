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
#include "object_accessor.hpp"
#include "object-store/src/thread_safe_reference.hpp"
#include "notifications_cs.hpp"
#include "wrapper_exceptions.hpp"
#include "timestamp_helpers.hpp"
#include "schema_cs.hpp"
#include <realm/parser/parser.hpp>
#include <realm/parser/query_builder.hpp>

using namespace realm;
using namespace realm::binding;

template<typename T>
inline T get(Results* results, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = results->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmResults", ndx, count);

        return results->get<T>(ndx);
    });
}

extern "C" {

REALM_EXPORT void results_destroy(Results* results_ptr)
{
    delete results_ptr;
}

// TODO issue https://github.com/realm/realm-dotnet-private/issues/40 added as needs
// TODO https://github.com/realm/realm-object-store/issues/56 adding Results::operator==
REALM_EXPORT size_t results_is_same_internal_results(Results* lhs, Results* rhs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return (lhs == rhs || false /* *lhs == *rhs */);
    });
}

REALM_EXPORT Object* results_get_object(Results* results_ptr, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        try {
            results_ptr->get_realm()->verify_thread();
            
            return new Object(results_ptr->get_realm(), results_ptr->get_object_schema(), results_ptr->get(ndx));
        }
        catch (std::out_of_range) {
            return static_cast<Object*>(nullptr);
        }
    });
}
    
REALM_EXPORT void results_get_primitive(Results* results, size_t ndx, PrimitiveValue& value, NativeException::Marshallable& ex)
{
    collection_get_primitive(results, ndx, value, ex);
}

REALM_EXPORT size_t results_get_string(Results* results, size_t ndx, uint16_t* value, size_t value_len, bool* is_null, NativeException::Marshallable& ex)
{
    return collection_get_string(results, ndx, value, value_len, is_null, ex);
}

REALM_EXPORT size_t results_get_binary(Results* results, size_t ndx, char* return_buffer, size_t buffer_size, bool* is_null, NativeException::Marshallable& ex)
{
    return collection_get_binary(results, ndx, return_buffer, buffer_size, is_null, ex);
}

REALM_EXPORT void results_clear(Results* results_ptr, SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (results_ptr->get_realm() != realm) {
            throw ObjectManagedByAnotherRealmException("Can only delete results from the Realm they belong to.");
        }
        
        results_ptr->get_realm()->verify_in_write();
      
        results_ptr->clear();
    });
}

REALM_EXPORT size_t results_count(Results* results_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        results_ptr->get_realm()->verify_thread();
        
        return results_ptr->size();
    });
}

REALM_EXPORT ManagedNotificationTokenContext* results_add_notification_callback(Results* results_ptr, void* managed_results, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_results, callback, [results_ptr](CollectionChangeCallback callback) {
            return results_ptr->add_notification_callback(callback);
        });
    });
}
    
REALM_EXPORT Query* results_get_query(Results* results_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Query(results_ptr->get_query());
    });
}
    
REALM_EXPORT Results* results_get_filtered_results(const Results& results, uint16_t* query_buf, size_t query_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor query_string(query_buf, query_len);
        auto query = results.get_query();
        auto const &realm = results.get_realm();
        
        parser::ParserResult result = parser::parse(query_string.to_string());
        
        parser::KeyPathMapping mapping;
        alias_backlinks(mapping, realm);
        
        query_builder::NoArguments no_args;
        query_builder::apply_predicate(query, result.predicate, no_args, mapping);
        
        DescriptorOrdering ordering;
        query_builder::apply_ordering(ordering, query.get_table(), result.ordering);
        return new Results(realm, std::move(query), std::move(ordering));
    });
}
    
REALM_EXPORT bool results_get_is_valid(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return results.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference<Results>* results_get_thread_safe_reference(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference<Results>{results.get_realm()->obtain_thread_safe_reference(results)};
    });
}
    
REALM_EXPORT Results* results_snapshot(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(results.snapshot());
    });
}

}   // extern "C"
