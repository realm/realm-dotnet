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

#include <realm/parser/query_parser.hpp>

#include <realm.hpp>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/object-store/results.hpp>

#include "error_handling.hpp"
#include "marshalling.hpp"
#include "notifications_cs.hpp"
#include "wrapper_exceptions.hpp"
#include "schema_cs.hpp"
#include <realm/object-store/keypath_helpers.hpp>
#include "realm_export_decls.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void results_destroy(Results* results)
{
    delete results;
}

// TODO issue https://github.com/realm/realm-dotnet-private/issues/40 added as needs
// TODO https://github.com/realm/realm-object-store/issues/56 adding Results::operator==
REALM_EXPORT bool results_is_same_internal_results(Results* lhs, Results* rhs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return (lhs == rhs || false /* *lhs == *rhs */);
    });
}

REALM_EXPORT void results_get_value(Results& results, size_t ndx, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        results.get_realm()->verify_thread();

        const size_t count = results.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmResults", ndx, count);

        if ((results.get_type() & ~PropertyType::Flags) == PropertyType::Object) {
            if (auto obj = results.get<Obj>(ndx)) {
                *value = to_capi(new Object(results.get_realm(), results.get_object_schema(), obj));
            }
            else {
                *value = realm_value_t{};
            }
        }
        else {
            auto val = results.get_any(ndx);
            if (!val.is_null() && val.get_type() == type_TypedLink) {
                *value = to_capi(new Object(results.get_realm(), val.get<ObjLink>()));
            }
            else {
                *value = to_capi(std::move(val));
            }
        }
    });
}

REALM_EXPORT void results_clear(Results& results, SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (results.get_realm() != realm) {
            throw ObjectManagedByAnotherRealmException("Can only delete results from the Realm they belong to.");
        }

        results.get_realm()->verify_in_write();

        results.clear();
    });
}

REALM_EXPORT size_t results_count(Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        results.get_realm()->verify_thread();

        return results.size();
    });
}

REALM_EXPORT ManagedNotificationTokenContext* results_add_notification_callback(Results* results, void* managed_results, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_results, callback, [results](CollectionChangeCallback callback) {
            return results->add_notification_callback(callback);
        });
    });
}

REALM_EXPORT Query* results_get_query(Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Query(results.get_query());
    });
}

REALM_EXPORT DescriptorOrdering* results_get_descriptor_ordering(Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new DescriptorOrdering(results.get_descriptor_ordering());
    });
}

REALM_EXPORT Results* results_get_filtered_results(const Results& results, uint16_t* query_buf, size_t query_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor query_string(query_buf, query_len);
        auto const& realm = results.get_realm();

        query_parser::KeyPathMapping mapping;
        realm::populate_keypath_mapping(mapping, *realm);

        query_parser::NoArguments no_args;
        Query parsed_query = results.get_table()->query(query_string, no_args, mapping);
        DescriptorOrdering new_order = results.get_descriptor_ordering();
        if (auto parsed_ordering = parsed_query.get_ordering()) {
            new_order.append(*parsed_ordering);
        }

        return new Results(realm, results.get_query().and_query(std::move(parsed_query)), std::move(new_order));
    });
}

REALM_EXPORT bool results_get_is_valid(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return results.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference* results_get_thread_safe_reference(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference(results);
    });
}

REALM_EXPORT Results* results_snapshot(const Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(results.snapshot());
    });
}

REALM_EXPORT size_t results_find_object(Results& results, const Object& object, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (results.get_realm() != object.realm()) {
            throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
        }
        return results.index_of(object.obj());
    });
}

REALM_EXPORT bool results_get_is_frozen(Results& results, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return results.is_frozen();
    });
}

REALM_EXPORT Results* results_freeze(Results& results, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(results.freeze(realm));
    });
}

}   // extern "C"
