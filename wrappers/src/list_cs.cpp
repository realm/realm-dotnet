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
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>

#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "wrapper_exceptions.hpp"
#include "notifications_cs.hpp"

using namespace realm;
using namespace realm::binding;

namespace {
    inline static void ensure_types(List& list, realm_value_t value) {
        if (value.is_null() && !is_nullable(list.get_type())) {
            throw NotNullableException();
        }

        // TODO: add list.get_type() != PropertyType::Mixed
        if (!value.is_null() && to_capi(list.get_type()) != value.type) {
            throw PropertyTypeMismatchException(to_string(list.get_type()), to_string(value.type));
        }
    }
}

extern "C" {

REALM_EXPORT void list_add_object(List& list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list.add(object_ptr.obj());
    });
}

REALM_EXPORT void list_add_primitive(List& list, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        ensure_types(list, value);

        list.add(from_capi(value));
    });
}

REALM_EXPORT Object* list_add_embedded(List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Object(list.get_realm(), list.get_object_schema(), list.add_embedded());
    });
}

REALM_EXPORT void list_set_object(List& list, size_t list_ndx, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (list_ndx >= count) {
            throw IndexOutOfRangeException("Set in RealmList", list_ndx, count);
        }

        list.set(list_ndx, object_ptr.obj());
    });

}

REALM_EXPORT void list_set_primitive(List& list, size_t list_ndx, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        ensure_types(list, value);
        
        const size_t count = list.size();
        if (list_ndx >= count) {
            throw IndexOutOfRangeException("Set into RealmList", list_ndx, count);
        }

        list.set(list_ndx, from_capi(value));
    });
}

REALM_EXPORT Object* list_set_embedded(List& list, size_t list_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (list_ndx >= count) {
            throw IndexOutOfRangeException("Set in RealmList", list_ndx, count);
        }

        return new Object(list.get_realm(), list.get_object_schema(), list.set_embedded(list_ndx));
    });
}

REALM_EXPORT void list_insert_object(List& list, size_t list_ndx, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (list_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, count);
        }

        list.insert(list_ndx, object_ptr.obj());
    });
}

REALM_EXPORT void list_insert_primitive(List& list, size_t list_ndx, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        ensure_types(list, value);

        const size_t count = list.size();
        if (list_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, count);
        }

        list.insert(list_ndx, from_capi(value));
    });
}

REALM_EXPORT Object* list_insert_embedded(List& list, size_t list_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (list_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, count);
        }

        return new Object(list.get_realm(), list.get_object_schema(), list.insert_embedded(list_ndx));
    });
}

REALM_EXPORT Object* list_get_object(List& list, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        const size_t count = list.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);

        return new Object(list.get_realm(), list.get_object_schema(), list.get(ndx));
    });
}

REALM_EXPORT void list_get_primitive(List& list, size_t ndx, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from Collection", ndx, count);

        auto val = list.get<Mixed>(ndx);
        *value = to_capi(val);
    });
}

REALM_EXPORT size_t list_find_object(List& list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (list.get_realm() != object_ptr.realm()) {
            throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
        }

        return list.find(object_ptr.obj());
    });
}

REALM_EXPORT size_t list_find_primitive(List& list, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        // This doesn't use ensure_types to allow List<string>.Find(null) to return false
        if (value.is_null() && !is_nullable(list.get_type())) {
            return (size_t)-1;
        }
        
        // TODO: add list.get_type() != PropertyType::Mixed
        if (!value.is_null() && to_capi(list.get_type()) != value.type) {
            throw PropertyTypeMismatchException(to_string(list.get_type()), to_string(value.type));
        }

        // TODO: this should eventually use list.find(from_capi(value));
        switch (list.get_type() & ~PropertyType::Collection) {
        case PropertyType::Bool:
            return list.find(value.boolean);
        case PropertyType::Bool | PropertyType::Nullable:
            return list.find(value.is_null() ? util::Optional<bool>(none) : util::Optional<bool>(value.boolean));

        case PropertyType::Int:
            return list.find(value.integer);
        case PropertyType::Int | PropertyType::Nullable:
            return list.find(value.is_null() ? util::Optional<int64_t>(none) : util::Optional<int64_t>(value.integer));

        case PropertyType::Float:
            return list.find(value.fnum);
        case PropertyType::Float | PropertyType::Nullable:
            return list.find(value.is_null() ? util::Optional<float>(none) : util::Optional<float>(value.fnum));
        
        case PropertyType::Double:
            return list.find(value.dnum);
        case PropertyType::Double | PropertyType::Nullable:
            return list.find(value.is_null() ? util::Optional<double>(none) : util::Optional<double>(value.dnum));

        case PropertyType::Date:
            return list.find(from_capi(value.timestamp));
        case PropertyType::Date | PropertyType::Nullable:
            return list.find(value.is_null() ? Timestamp() : from_capi(value.timestamp));

        case PropertyType::Decimal:
            return list.find(from_capi(value.decimal128));
        case PropertyType::Decimal | PropertyType::Nullable:
            return list.find(value.is_null() ? Decimal128(null()): from_capi(value.decimal128));
        
        case PropertyType::ObjectId:
            return list.find(from_capi(value.object_id));
        case PropertyType::ObjectId | PropertyType::Nullable:
            return list.find(value.is_null() ? util::Optional<ObjectId>(none) : util::Optional<ObjectId>(from_capi(value.object_id)));

        case PropertyType::Data:
            return list.find(from_capi(value.binary));
        case PropertyType::Data | PropertyType::Nullable:
            return list.find(value.is_null() ? BinaryData() : from_capi(value.binary));

        case PropertyType::String:
            return list.find(from_capi(value.string));
        case PropertyType::String | PropertyType::Nullable:
            return list.find(value.is_null() ? StringData() : from_capi(value.string));

        default:
            REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void list_erase(List& list, size_t link_ndx, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (link_ndx >= count)
            throw IndexOutOfRangeException("Erase item in RealmList", link_ndx, count);

        list.remove(link_ndx);
    });
}

REALM_EXPORT void list_clear(List& list, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list.remove_all();
    });
}

REALM_EXPORT size_t list_size(List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list.size();
    });
}

REALM_EXPORT void list_destroy(List* list)
{
    delete list;
}

REALM_EXPORT ManagedNotificationTokenContext* list_add_notification_callback(List* list, void* managed_list, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_list, callback, [list](CollectionChangeCallback callback) {
            return list->add_notification_callback(callback);
        });
    });
}

REALM_EXPORT void list_move(List& list, size_t source_ndx, size_t dest_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list.size();

        // Indices are >= 0 validated by .NET
        if (dest_ndx >= count) {
            throw IndexOutOfRangeException("Move within RealmList", dest_ndx, count);
        }

        if (source_ndx >= count) {
            throw IndexOutOfRangeException("Move within RealmList", source_ndx, count);
        }

        list.move(source_ndx, dest_ndx);
    });
}

REALM_EXPORT bool list_get_is_valid(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference* list_get_thread_safe_reference(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference(list);
    });
}

REALM_EXPORT Results* list_snapshot(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(list.snapshot());
    });
}

REALM_EXPORT bool list_get_is_frozen(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list.is_frozen();
    });
}

REALM_EXPORT List* list_freeze(const List& list, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new List(list.freeze(realm));
    });
}

}   // extern "C"
