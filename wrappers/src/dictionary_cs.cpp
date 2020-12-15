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

#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "wrapper_exceptions.hpp"
#include "notifications_cs.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void realm_dictionary_add(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        // TODO: throw if dictionary_contains returns true.
        dictionary.insert(from_capi(key.string), from_capi(value));
    });
}

REALM_EXPORT void realm_dictionary_set(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        dictionary.insert(from_capi(key.string), from_capi(value));
    });
}

REALM_EXPORT bool realm_dictionary_try_get(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t* value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto val = dictionary.get<Mixed>(from_capi(key.string));
        // TODO: this should use try_get
        throw new std::exception("dictionary doesn't expose get(string) yet.");
        return false;
    });
}

REALM_EXPORT void realm_dictionary_get_at_index(object_store::Dictionary& dictionary, size_t ndx, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = dictionary.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmDictionary", ndx, count);

        throw new std::exception("dictionary doesn't expose get(index) yet.");
        //auto val = dictionary.get(ndx);
        //if (!val.is_null() && val.get_type() == type_TypedLink) {
        //    *value = to_capi(new Object(dictionary.get_realm(), val.get<ObjLink>()));
        //}
        //else {
        //    *value = to_capi(std::move(val));
        //}
    });
}

REALM_EXPORT void realm_dictionary_remove(object_store::Dictionary& dictionary, realm_value_t key, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        dictionary.erase(from_capi(key.string));
    });
}

REALM_EXPORT bool realm_dictionary_contains_key(object_store::Dictionary& dictionary, realm_value_t key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        throw new std::exception("dictionary doesn't expose contains yet.");
        return false;

        //auto set_type = set.get_type();
        //// This doesn't use ensure_types to allow Set<string>.Find(null) to return false
        //if (value.is_null() && !is_nullable(set_type)) {
        //    return false;
        //}
        //
        //if (!value.is_null() && set_type != PropertyType::Mixed && to_capi(set_type) != value.type) {
        //    throw PropertyTypeMismatchException(to_string(set_type), to_string(value.type));
        //}

        //if (value.type == realm_value_type::RLM_TYPE_LINK) {
        //    if (set.get_realm() != value.link.object->realm()) {
        //        throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
        //    }

        //    if ((set_type & PropertyType::Flags) == PropertyType::Mixed) {
        //        return set.find_any(ObjLink(value.link.object->get_object_schema().table_key, value.link.object->obj().get_key())) > -1;
        //    }

        //    return set.find(value.link.object->obj()) != realm::not_found;
        //}

        //return set.find_any(from_capi(value)) != realm::not_found;
    });
}

REALM_EXPORT void realm_dictionary_clear(object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        dictionary.remove_all();
    });
}

REALM_EXPORT size_t realm_dictionary_get_size(object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return dictionary.size();
    });
}

REALM_EXPORT void realm_dictionary_destroy(object_store::Dictionary* dictionary)
{
    delete dictionary;
}

REALM_EXPORT ManagedNotificationTokenContext* realm_dictionary_add_notification_callback(object_store::Set* set, void* managed_set, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        return subscribe_for_notifications(managed_set, callback, [set](CollectionChangeCallback callback) {
            return set->add_notification_callback(callback);
        });
    });
}

REALM_EXPORT bool realm_dictionary_get_is_valid(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return dictionary.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference* realm_dictionary_get_thread_safe_reference(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference(dictionary);
    });
}

REALM_EXPORT Results* realm_dictionary_snapshot(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        // TODO: this should use snapshot
        return new Results(dictionary.as_results());
    });
}

REALM_EXPORT bool realm_dictionary_get_is_frozen(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return dictionary.is_frozen();
    });
}

REALM_EXPORT object_store::Dictionary* realm_dictionary_freeze(const object_store::Dictionary& dictionary, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        // TODO: this should use freeze when exposed
        throw new std::exception("dictionary doesn't expose freeze yet.");
        return nullptr;
        //return new object_store::Dictionary(dictionary.freeze(realm));
    });
}

}   // extern "C"
