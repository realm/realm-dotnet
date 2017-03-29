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

#include <sstream>
#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "wrapper_exceptions.hpp"
#include "object_accessor.hpp"
#include "object-store/src/thread_safe_reference.hpp"
#include "notifications_cs.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {
  
REALM_EXPORT void list_add(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(object_ptr.row().get_index());
    });
}

REALM_EXPORT void list_insert(List* list, size_t link_ndx, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (link_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", link_ndx, count);
        }
        list->insert(link_ndx, object_ptr.row().get_index());
    });
}

REALM_EXPORT Object* list_get(List* list, size_t link_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        const size_t count = list->size();
        if (link_ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", link_ndx, count);
        auto rowExpr = list->get(link_ndx);
        return new Object(list->get_realm(), list->get_object_schema(), Row(rowExpr));
    });
}

REALM_EXPORT size_t list_find(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->find(object_ptr.row());
    });
}

REALM_EXPORT void list_erase(List* list, size_t link_ndx, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (link_ndx >= count)
            throw IndexOutOfRangeException("Erase item in RealmList", link_ndx, count);
        
        list->remove(link_ndx);
    });
}

REALM_EXPORT void list_clear(List* list, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->remove_all();
    });
}

REALM_EXPORT size_t list_size(List* list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->size();
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
    
REALM_EXPORT void list_move(List& list, const Object& object_ptr, size_t dest_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (dest_ndx >= count) {
            throw IndexOutOfRangeException("Move within RealmList", dest_ndx, count);
        }

        size_t source_ndx = list.find(object_ptr.row());
        list.move(source_ndx, dest_ndx);
    });
}
    
REALM_EXPORT bool list_get_is_valid(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list.is_valid();
    });
}

REALM_EXPORT ThreadSafeReference<List>* list_get_thread_safe_reference(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference<List>{list.get_realm()->obtain_thread_safe_reference(list)};
    });
}

}   // extern "C"
