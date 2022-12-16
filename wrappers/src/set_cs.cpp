////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/parser/query_parser.hpp>

#include "error_handling.hpp"
#include "filter.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "wrapper_exceptions.hpp"
#include "notifications_cs.hpp"
#include "schema_cs.hpp"

using namespace realm;
using namespace realm::binding;

namespace {
    inline static void ensure_types(object_store::Set& set, realm_value_t value) {
        if (value.is_null() && !is_nullable(set.get_type())) {
            throw NotNullableException();
        }

        if (!value.is_null() && set.get_type() != PropertyType::Mixed && to_capi(set.get_type()) != value.type) {
            throw PropertyTypeMismatchException(to_string(set.get_type()), to_string(value.type));
        }
    }

    const object_store::Set& hack_remove_me(const object_store::Collection& other_collection) {
        return static_cast<const object_store::Set&>(other_collection);
    }
}

extern "C" {

REALM_EXPORT bool realm_set_add_value(object_store::Set& set, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        ensure_types(set, value);

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            // For Mixed, we need ObjLink, otherwise, ObjKey
            if ((set.get_type() & ~PropertyType::Flags) == PropertyType::Mixed) {
                return set.insert_any(ObjLink(value.link.object->get_object_schema().table_key, value.link.object->obj().get_key())).second;
            }

            return set.insert(value.link.object->obj()).second;
        }

        return set.insert_any(from_capi(value)).second;
    });
}

REALM_EXPORT void realm_set_get_value(object_store::Set& set, size_t ndx, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = set.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmSet", ndx, count);

        if ((set.get_type() & ~PropertyType::Flags) == PropertyType::Object) {
            *value = to_capi(set.get(ndx), set.get_realm());
        }
        else {
            auto val = set.get_any(ndx);
            if (!val.is_null() && val.get_type() == type_TypedLink) {
                *value = to_capi(val.get<ObjLink>(), set.get_realm());
            }
            else {
                *value = to_capi(std::move(val));
            }
        }
    });
}

REALM_EXPORT bool realm_set_remove_value(object_store::Set& set, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        ensure_types(set, value);

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            // For Mixed, we need ObjLink, otherwise, ObjKey
            if ((set.get_type() & ~PropertyType::Flags) == PropertyType::Mixed) {
                return set.remove_any(ObjLink(value.link.object->get_object_schema().table_key, value.link.object->obj().get_key())).second;
            }

            return set.remove(value.link.object->obj()).second;
        }

        return set.remove_any(from_capi(value)).second;
    });
}

REALM_EXPORT bool realm_set_contains_value(object_store::Set& set, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto set_type = set.get_type();
        // This doesn't use ensure_types to allow Set<string>.Find(null) to return false
        if (value.is_null() && !is_nullable(set_type)) {
            return false;
        }
        
        if (!value.is_null() && set_type != PropertyType::Mixed && to_capi(set_type) != value.type) {
            throw PropertyTypeMismatchException(to_string(set_type), to_string(value.type));
        }

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            if (set.get_realm() != value.link.object->realm()) {
                throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
            }

            if ((set_type & ~PropertyType::Flags) == PropertyType::Mixed) {
                return set.find_any(ObjLink(value.link.object->get_object_schema().table_key, value.link.object->obj().get_key())) != not_found;
            }

            return set.find(value.link.object->obj()) != realm::not_found;
        }

        return set.find_any(from_capi(value)) != realm::not_found;
    });
}

REALM_EXPORT void realm_set_clear(object_store::Set& set, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        set.remove_all();
    });
}

REALM_EXPORT size_t realm_set_get_size(object_store::Set& set, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return set.size();
    });
}

REALM_EXPORT void realm_set_destroy(object_store::Set* set)
{
    delete set;
}

REALM_EXPORT ManagedNotificationTokenContext* realm_set_add_notification_callback(object_store::Set* set, void* managed_set, size_t* property_indices, size_t property_count, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_set, [&](CollectionChangeCallback callback) {
            if (!property_indices) {
                return set->add_notification_callback(callback);
            } else {
                auto keyPathArray = construct_key_path_array(set->get_object_schema(), property_indices, property_count);
                return set->add_notification_callback(callback, keyPathArray);
            }
        });
    });
}

REALM_EXPORT bool realm_set_get_is_valid(const object_store::Set& set, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return set.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference* realm_set_get_thread_safe_reference(const object_store::Set& set, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference(set);
    });
}

REALM_EXPORT Results* realm_set_snapshot(const object_store::Set& set, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(set.snapshot());
    });
}

REALM_EXPORT object_store::Set* realm_set_freeze(const object_store::Set& set, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new object_store::Set(set.freeze(realm));
    });
}

REALM_EXPORT void realm_set_except_with(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        set.assign_difference(hack_remove_me(other_collection));
    });
}

REALM_EXPORT void realm_set_symmetric_except_with(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        set.assign_symmetric_difference(hack_remove_me(other_collection));
    });
}

REALM_EXPORT void realm_set_intersect_with(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        set.assign_intersection(hack_remove_me(other_collection));
    });
}

REALM_EXPORT void realm_set_union_with(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        set.assign_union(hack_remove_me(other_collection));
    });
}

REALM_EXPORT bool realm_set_is_subset_of(object_store::Set& set, const object_store::Collection& other_collection, bool proper, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (proper) {
            return set.is_strict_subset_of(hack_remove_me(other_collection));
        }

        return set.is_subset_of(hack_remove_me(other_collection));
    });
}

REALM_EXPORT bool realm_set_is_superset_of(object_store::Set& set, const object_store::Collection& other_collection, bool proper, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (proper) {
            return set.is_strict_superset_of(hack_remove_me(other_collection));
        }

        return set.is_superset_of(hack_remove_me(other_collection));
    });
}

REALM_EXPORT bool realm_set_overlaps(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return set.intersects(hack_remove_me(other_collection));
    });
}

REALM_EXPORT bool realm_set_set_equals(object_store::Set& set, const object_store::Collection& other_collection, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> bool {
        return set.set_equals(hack_remove_me(other_collection));
    });
}

REALM_EXPORT Results* set_to_results(const object_store::Set& set, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(set.as_results());
    });
}

REALM_EXPORT Results* set_get_filtered_results(const object_store::Set& set, uint16_t* query_buf, size_t query_len, realm_value_t* arguments, size_t args_count, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return get_filtered_results(set.get_realm(), set.get_table(), set.get_query(), query_buf, query_len, arguments, args_count, DescriptorOrdering());
    });
}

}   // extern "C"
