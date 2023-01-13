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
#include "filter.hpp"
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
                throw KeyAlreadyExistsException(dict_key);
            }

            dictionary.insert(dict_key, from_capi(value));
        });
    }

    REALM_EXPORT Object* realm_dictionary_add_embedded(object_store::Dictionary& dictionary, realm_value_t key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto dict_key = from_capi(key.string);
            if (dictionary.contains(dict_key))
            {
                throw KeyAlreadyExistsException(dict_key);
            }

            return new Object(dictionary.get_realm(), dictionary.get_object_schema(), dictionary.insert_embedded(dict_key));
        });
    }

    REALM_EXPORT void realm_dictionary_set(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            dictionary.insert(from_capi(key.string), from_capi(value));
        });
    }

    REALM_EXPORT Object* realm_dictionary_set_embedded(object_store::Dictionary& dictionary, realm_value_t key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new Object(dictionary.get_realm(), dictionary.get_object_schema(), dictionary.insert_embedded(from_capi(key.string)));
        });
    }

    REALM_EXPORT bool realm_dictionary_try_get(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t* value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto mixed_value = dictionary.try_get_any(from_capi(key.string));
            if (mixed_value)
            {
                *value = to_capi(dictionary, *mixed_value);
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
            *value = to_capi(dictionary, pair.second);
        });
    }

    REALM_EXPORT bool realm_dictionary_remove(object_store::Dictionary& dictionary, realm_value_t key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto dict_key = from_capi(key.string);
            if (dictionary.contains(dict_key))
            {
                dictionary.erase(dict_key);
                return true;
            }

            return false;
        });
    }

    REALM_EXPORT bool realm_dictionary_remove_value(object_store::Dictionary& dictionary, realm_value_t key, realm_value_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto dict_key = from_capi(key.string);
            auto dict_value = dictionary.try_get_any(dict_key);

            if (dict_value && are_equal(value, *dict_value))
            {
                dictionary.erase(dict_key);
                return true;
            }

            return false;
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

    REALM_EXPORT ManagedNotificationTokenContext* realm_dictionary_add_notification_callback(object_store::Dictionary* dictionary, void* managed_dict, bool shallow, NativeException::Marshallable& ex)
    
    {
        return handle_errors(ex, [=]() {
            return subscribe_for_notifications(managed_dict, [dictionary, shallow](CollectionChangeCallback callback) {
                return dictionary->add_notification_callback(callback, shallow ? std::make_optional(KeyPathArray()) : std::nullopt);
            }, shallow);
        });
    }

    REALM_EXPORT ManagedNotificationTokenContext* realm_dictionary_add_key_notification_callback(object_store::Dictionary* dictionary, void* managed_dict, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [=]() {
            auto context = new ManagedNotificationTokenContext();
            context->managed_object = managed_dict;
            context->token = dictionary->add_key_based_notification_callback([context](DictionaryChangeSet changes) {
                if (changes.deletions.empty() && changes.insertions.empty() && changes.modifications.empty()) {
                    s_dictionary_notification_callback(context->managed_object, nullptr, false);
                }
                else {
                    auto deletions = get_keys_vector(changes.deletions);
                    auto insertions = get_keys_vector(changes.insertions);
                    auto modifications = get_keys_vector(changes.modifications);

                    MarshallableDictionaryChangeSet marshallable_changes{
                        { deletions.data(), deletions.size() },
                        { insertions.data(), insertions.size() },
                        { modifications.data(), modifications.size() },
                    };

                    s_dictionary_notification_callback(context->managed_object, &marshallable_changes, false);
                }
            }, KeyPathArray());

            return context;
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

    REALM_EXPORT object_store::Dictionary* realm_dictionary_freeze(const object_store::Dictionary& dictionary, const SharedRealm& realm, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new object_store::Dictionary(dictionary.freeze(realm));
        });
    }

    REALM_EXPORT Results* realm_dictionary_get_values(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new Results(dictionary.get_values());
        });
    }

    REALM_EXPORT Results* realm_dictionary_get_keys(const object_store::Dictionary& dictionary, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new Results(dictionary.get_keys());
        });
    }

    REALM_EXPORT Results* realm_dictionary_get_filtered_results(const object_store::Dictionary& dictionary, uint16_t* query_buf, size_t query_len, realm_value_t* arguments, size_t args_count, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            realm::Results values = dictionary.get_values();
            return get_filtered_results(values.get_realm(), values.get_table(), values.get_query(), query_buf, query_len, arguments, args_count, values.get_descriptor_ordering());
        });
    }
}   // extern "C"
