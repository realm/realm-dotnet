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
        auto dict_key = from_capi(key.string);
        if (dictionary.contains(dict_key))
        {
            throw std::logic_error("Duplicate key exception; throw something more concrete!");
        }

        dictionary.insert(dict_key, from_capi(value));
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
        auto mixed_value = dictionary.try_get_any(from_capi(key.string));
        if (mixed_value)
        {
            *value = to_capi(mixed_value.value());
            return true;
        }

        return false;
    });
}

REALM_EXPORT void realm_dictionary_get_at_index(object_store::Dictionary& dictionary, size_t ndx, realm_value_t* key, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = dictionary.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmDictionary", ndx, count);

        auto pair = dictionary.get_pair(ndx);
        *key = to_capi(Mixed(pair.first));
        *value = to_capi(pair.second);
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
        return dictionary.contains(from_capi(key.string));
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

// TODO: this should not return nullptr, but blocked on https://github.com/realm/realm-core/issues/4258
REALM_EXPORT ThreadSafeReference* realm_dictionary_get_thread_safe_reference(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        // return new ThreadSafeReference(dictionary);
        return nullptr;
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
        return new object_store::Dictionary(dictionary.freeze(realm));
    });
}

REALM_EXPORT Results* realm_dictionary_get_values(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(dictionary.as_results());
    });
}

// TODO: this should use dictionary.get_keys()
REALM_EXPORT Results* realm_dictionary_get_keys(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        throw std::logic_error("Dictionary::get_keys is not implemented yet");
        return new Results(dictionary.as_results());
    });
}

}   // extern "C"
