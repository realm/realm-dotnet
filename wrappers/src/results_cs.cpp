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
#include "filter.hpp"
#include "marshalling.hpp"
#include "notifications_cs.hpp"
#include "schema_cs.hpp"
#include "realm_export_decls.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void results_destroy(Results* results)
{
    delete results;
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
                *value = to_capi(obj, results.get_realm());
            }
            else {
                *value = realm_value_t{};
            }
        }
        else {
            auto val = results.get_any(ndx);
            if (!val.is_null() && val.get_type() == type_TypedLink) {
                *value = to_capi(val.get<ObjLink>(), results.get_realm());
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

REALM_EXPORT ManagedNotificationTokenContext* results_add_notification_callback(Results* results, void* managed_results, bool shallow, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_results, [results, shallow](CollectionChangeCallback callback) {
            return results->add_notification_callback(callback, shallow ? std::make_optional(KeyPathArray()) : std::nullopt);
        }, shallow);
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

REALM_EXPORT Results* results_get_filtered_results(const Results& results, uint16_t* query_buf, size_t query_len, query_argument* arguments, size_t args_count, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return get_filtered_results(results.get_realm(), results.get_table(), results.get_query(), query_buf, query_len, arguments, args_count, results.get_descriptor_ordering());
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

REALM_EXPORT size_t results_find_value(Results& results, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto results_type = results.get_type();
        if (value.is_null() && !is_nullable(results_type)) {
            return (size_t)-1;
        }

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            if (results.get_realm() != value.link.object->realm()) {
                throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
            }
            return results.index_of(value.link.object->get_obj());
        }

        return results.index_of(from_capi(value));
    });
}

REALM_EXPORT Results* results_freeze(Results& results, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(results.freeze(realm));
    });
}

REALM_EXPORT size_t results_get_description(Results& results, uint16_t* buffer, size_t buffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return stringdata_to_csharpstringbuffer(results.get_query().get_description(), buffer, buffer_len);
    });
}

}   // extern "C"
