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
 
#ifndef NOTIFICATIONS_CS_HPP
#define NOTIFICATIONS_CS_HPP

#include "marshalling.hpp"
#include <memory>
#include <realm/object-store/collection_notifications.hpp>
#include "error_handling.hpp"

using namespace realm::binding;

namespace realm {
    struct MarshallableCollectionChangeSet {
        struct MarshallableIndexSet {
            size_t* indices;
            size_t count;
        };
        
        MarshallableIndexSet deletions;
        MarshallableIndexSet insertions;
        MarshallableIndexSet modifications;
        MarshallableIndexSet modifications_new;

        struct {
            CollectionChangeSet::Move* moves;
            size_t count;
        } moves;
        
        bool cleared;

        MarshallableIndexSet properties;
    };

    struct MarshallableDictionaryChangeSet {
        struct MarshallableKeySet {
            realm_value_t* keys;
            size_t count;
        };

        MarshallableKeySet deletions;
        MarshallableKeySet insertions;
        MarshallableKeySet modifications;
    };
    
    struct ManagedNotificationTokenContext {
        NotificationToken token;
        void* managed_object;
        ObjectSchema* schema;
    };

    using ObjectNotificationCallbackT = void(void* managed_results, MarshallableCollectionChangeSet*, NativeException::Marshallable*);
    using DictionaryNotificationCallbackT = void(void* managed_results, MarshallableDictionaryChangeSet*, NativeException::Marshallable*);

    extern std::function<ObjectNotificationCallbackT> s_object_notification_callback;
    extern std::function<DictionaryNotificationCallbackT> s_dictionary_notification_callback;

    inline size_t get_property_index(const ObjectSchema* schema, const ColKey column_key) {
        if (!schema)
            return 0;
        
        auto const& props = schema->persisted_properties;
        for (size_t i = 0; i < props.size(); ++i) {
            if (props[i].column_key == column_key) {
                return i;
            }
        }
        
        return -1;
    }
    
    inline std::vector<size_t> get_indexes_vector(const IndexSet& indexSet)
    {
        if (indexSet.count() < (size_t)-1) {
            return std::vector<size_t>(indexSet.as_indexes().begin(), indexSet.as_indexes().end());
        }
        
        return std::vector<size_t>();
    }

    static inline std::vector<realm_value_t> get_keys_vector(const std::vector<Mixed>& keySet)
    {
        std::vector<realm_value_t> result;
        result.reserve(keySet.size());

        for (auto &key : keySet) {
            result.push_back(to_capi(key));
        }

        return result;
    }

    static inline void handle_changes(ManagedNotificationTokenContext* context, CollectionChangeSet changes, std::exception_ptr e) {
        if (e) {
            try {
                std::rethrow_exception(e);
            } catch (...) {
                auto exception = convert_exception();
                auto marshallable_exception = exception.for_marshalling();
                s_object_notification_callback(context->managed_object, nullptr, &marshallable_exception);
            }
        } else if (changes.empty()) {
            
            s_object_notification_callback(context->managed_object, nullptr, nullptr);
        } else {
            auto deletions = get_indexes_vector(changes.deletions);
            auto insertions = get_indexes_vector(changes.insertions);
            auto modifications = get_indexes_vector(changes.modifications);
            auto modifications_new = get_indexes_vector(changes.modifications_new);

            std::vector<size_t> properties;

            for (auto& pair : changes.columns) {
                if (!pair.second.empty()) {
                    properties.emplace_back(get_property_index(context->schema, ColKey(pair.first)));
                }
            }
 
            MarshallableCollectionChangeSet marshallable_changes {
                { deletions.data(), deletions.size() },
                { insertions.data(), insertions.size() },
                { modifications.data(), modifications.size() },
                { modifications_new.data(), modifications_new.size() },
                { changes.moves.data(), changes.moves.size() },
                { changes.collection_was_cleared },
                { properties.data(), properties.size() }
            };
            
            s_object_notification_callback(context->managed_object, &marshallable_changes, nullptr);
        }
    }


    template<typename Subscriber>
    inline ManagedNotificationTokenContext* subscribe_for_notifications(void* managed_object, Subscriber subscriber, ObjectSchema* schema = nullptr)
    {
        auto context = new ManagedNotificationTokenContext();
        context->managed_object = managed_object;
        context->schema = schema;
        context->token = subscriber([context](CollectionChangeSet changes, std::exception_ptr e) {
            handle_changes(context, changes, e);
        });
        
        return context;
    }
}

#endif // NOTIFICATIONS_CS_HPP
