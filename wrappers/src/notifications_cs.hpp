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
    MarshaledVector<size_t> deletions;
    MarshaledVector<size_t> insertions;
    MarshaledVector<size_t> modifications;
    MarshaledVector<size_t> modifications_new;

    MarshaledVector<CollectionChangeSet::Move> moves;

    bool cleared;

    MarshaledVector<int32_t> properties;
};

struct MarshallableDictionaryChangeSet {
    MarshaledVector<realm_value_t> deletions;
    MarshaledVector<realm_value_t> insertions;
    MarshaledVector<realm_value_t> modifications;
};

struct ManagedNotificationTokenContext {
    NotificationToken token;
    void* managed_object;
    ObjectSchema* schema;
};

using ObjectNotificationCallbackAndKeypathT = void(void* managed_results, MarshallableCollectionChangeSet*, void* callback);
using ObjectNotificationCallbackT = void(void* managed_results, MarshallableCollectionChangeSet*, bool shallow);
using DictionaryNotificationCallbackT = void(void* managed_results, MarshallableDictionaryChangeSet*, bool shallow);

extern std::function<ObjectNotificationCallbackT> s_object_notification_callback;
extern std::function<ObjectNotificationCallbackAndKeypathT> s_object_notification_keypath_callback;
extern std::function<DictionaryNotificationCallbackT> s_dictionary_notification_callback;

inline int32_t get_property_index(const ObjectSchema* schema, const ColKey column_key) {
    if (!schema)
        return 0;

    auto const& props = schema->persisted_properties;
    for (size_t i = 0; i < props.size(); ++i) {
        if (props[i].column_key == column_key) {
            return int32_t(i);
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

    for (auto& key : keySet) {
        result.push_back(to_capi(key));
    }

    return result;
}

// TODO Later I need to investigate why using this method to extrac this functionality does not work
static inline MarshallableCollectionChangeSet build_marshallable_change_set(ObjectSchema* schema, CollectionChangeSet changes) {
    auto deletions = get_indexes_vector(changes.deletions);
    auto insertions = get_indexes_vector(changes.insertions);
    auto modifications = get_indexes_vector(changes.modifications);
    auto modifications_new = get_indexes_vector(changes.modifications_new);

    std::vector<int32_t> properties;

    for (auto& pair : changes.columns) {
        if (!pair.second.empty()) {
            properties.emplace_back(get_property_index(schema, ColKey(pair.first)));
        }
    }

    MarshallableCollectionChangeSet marshallable_changes{
        deletions,
        insertions,
        modifications,
        modifications_new,
        changes.moves,
        changes.collection_was_cleared,
        properties
    };

    return marshallable_changes;
}

static inline void handle_changes_keypath(ManagedNotificationTokenContext* context, CollectionChangeSet changes, void* callback) {
    if (changes.empty()) {
        s_object_notification_keypath_callback(context->managed_object, nullptr, callback);
    }
    else {
        auto deletions = get_indexes_vector(changes.deletions);
        auto insertions = get_indexes_vector(changes.insertions);
        auto modifications = get_indexes_vector(changes.modifications);
        auto modifications_new = get_indexes_vector(changes.modifications_new);

        std::vector<int32_t> properties;

        for (auto& pair : changes.columns) {
            if (!pair.second.empty()) {
                properties.emplace_back(get_property_index(context->schema, ColKey(pair.first)));
            }
        }

        MarshallableCollectionChangeSet marshallable_changes{
            deletions,
            insertions,
            modifications,
            modifications_new,
            changes.moves,
            changes.collection_was_cleared,
            properties
        };

        s_object_notification_keypath_callback(context->managed_object, &marshallable_changes, callback);
    }
}

static inline void handle_changes(ManagedNotificationTokenContext* context, CollectionChangeSet changes, bool shallow) {
    if (changes.empty()) {
        s_object_notification_callback(context->managed_object, nullptr, shallow);
    }
    else {
        auto deletions = get_indexes_vector(changes.deletions);
        auto insertions = get_indexes_vector(changes.insertions);
        auto modifications = get_indexes_vector(changes.modifications);
        auto modifications_new = get_indexes_vector(changes.modifications_new);

        std::vector<int32_t> properties;

        for (auto& pair : changes.columns) {
            if (!pair.second.empty()) {
                properties.emplace_back(get_property_index(context->schema, ColKey(pair.first)));
            }
        }

        // TODO Using this method instead of creating marshallable_changes here makes lots of test fail. Later I want to investigate why
        // The funny thing is that it fails when running, but putting a breakpoint on the line when s_notification_callback gets called
        // makes it succeed in debug. 
        //MarshallableCollectionChangeSet marshallable_changes = build_marshallable_change_set(context->schema, changes);

        MarshallableCollectionChangeSet marshallable_changes{
            deletions,
            insertions,
            modifications,
            modifications_new,
            changes.moves,
            changes.collection_was_cleared,
            properties
        };

        s_object_notification_callback(context->managed_object, &marshallable_changes, shallow);
    }
}

template<typename Subscriber>
inline ManagedNotificationTokenContext* subscribe_for_notifications(void* managed_object, Subscriber subscriber, bool shallow, ObjectSchema* schema = nullptr)
{
    auto context = new ManagedNotificationTokenContext();
    context->managed_object = managed_object;
    context->schema = schema;
    context->token = subscriber([context, shallow](CollectionChangeSet changes) {
        handle_changes(context, changes, shallow);
    });

    return context;
}

template<typename Subscriber>
inline ManagedNotificationTokenContext* subscribe_for_notifications_keypaths(void* managed_object, Subscriber subscriber, void* callback, ObjectSchema* schema = nullptr)
{
    auto context = new ManagedNotificationTokenContext();
    context->managed_object = managed_object;
    context->schema = schema;
    context->token = subscriber([context, callback](CollectionChangeSet changes) {
        handle_changes_keypath(context, changes, callback);
    });

    return context;
}

}

#endif // NOTIFICATIONS_CS_HPP
