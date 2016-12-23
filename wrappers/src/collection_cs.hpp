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
 
#ifndef COLLECTION_CS_HPP
#define COLLECTION_CS_HPP

#include <memory>
#include "collection_notifications.hpp"

namespace realm {
    struct MarshallableCollectionChangeSet {
        struct MarshallableIndexSet {
            size_t* indices;
            size_t count;
        };
        
        MarshallableIndexSet deletions;
        MarshallableIndexSet insertions;
        MarshallableIndexSet modifications;
        
        struct {
            CollectionChangeSet::Move* moves;
            size_t count;
        } moves;
    };
    
    typedef void (*ManagedNotificationCallback)(void* managed_results, MarshallableCollectionChangeSet*, NativeException::Marshallable*);
    
    struct ManagedNotificationTokenContext {
        NotificationToken token;
        void* managed_collection;
        ManagedNotificationCallback callback;
    };
    
    inline ManagedNotificationTokenContext* subscribe_for_notifications(void* managed_collection, ManagedNotificationCallback callback, std::function<NotificationToken(CollectionChangeCallback)> subscriber)
    {
        auto context = new ManagedNotificationTokenContext();
        context->managed_collection = managed_collection;
        context->callback = callback;
        context->token = subscriber([context](CollectionChangeSet changes, std::exception_ptr e) {
            if (e) {
                try {
                    std::rethrow_exception(e);
                } catch (...) {
                    auto exception = convert_exception();
                    auto marshallable_exception = exception.for_marshalling();
                    context->callback(context->managed_collection, nullptr, &marshallable_exception);
                }
            } else if (changes.empty()) {
                context->callback(context->managed_collection, nullptr, nullptr);
            } else {
                std::vector<size_t> deletions(changes.deletions.as_indexes().begin(), changes.deletions.as_indexes().end());
                std::vector<size_t> insertions(changes.insertions.as_indexes().begin(), changes.insertions.as_indexes().end());
                std::vector<size_t> modifications(changes.modifications.as_indexes().begin(), changes.modifications.as_indexes().end());
                
                MarshallableCollectionChangeSet marshallable_changes {
                    { deletions.data(), deletions.size() },
                    { insertions.data(), insertions.size() },
                    { modifications.data(), modifications.size() },
                    { changes.moves.data(), changes.moves.size() }
                };
                context->callback(context->managed_collection, &marshallable_changes, nullptr);
            }
        });
        
        return context;
    }
}

#endif // COLLECTION_CS_HPP
