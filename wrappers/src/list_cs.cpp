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

template<typename T>
inline void insert(List* list, size_t list_ndx, T value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (list_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, count);
        }
        
        list->insert(list_ndx, value);
    });
}

template<typename T>
inline void insert_nullable(List* list, size_t list_ndx, T value, bool has_value, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, has_value ? Optional<T>(value) : Optional<T>(none), ex);
}

template<typename T>
inline T get(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);

        return list->get<T>(ndx);
    });
}

template<typename T>
inline bool get_nullable(List* list, size_t ndx, T& ret_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);
        
        Optional<T> result = list->get<Optional<T>>(ndx);
        if (!result)
            return false;
        
        ret_value = result.value();
        return true;
    });
}

template<typename T>
inline void add(List* list, T value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(value);
    });
}

template<typename T>
inline void add_nullable(List* list, T value, bool has_value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(has_value ? Optional<T>(value) : Optional<T>(none));
    });
}

template<typename T>
inline size_t find(List* list, T value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->find(value);
    });
}

template<typename T>
inline size_t find_nullable(List* list, T value, bool has_value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto optional = has_value ? Optional<T>(value) : Optional<T>(none);
        return list->find(optional);
    });
}

extern "C" {
  
REALM_EXPORT void list_add(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(object_ptr.row());
    });
}
    
REALM_EXPORT void list_add_nullable_bool(List* list, bool value, bool has_value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(has_value ? Optional<bool>(value) : Optional<bool>(none));
    });
}
    
REALM_EXPORT void list_add_bool(List* list, bool value, NativeException::Marshallable& ex)
{
    add(list, value, ex);
}

REALM_EXPORT void list_add_nullable_int64(List* list, int64_t value, bool has_value, NativeException::Marshallable& ex)
{
    add_nullable(list, value, has_value, ex);
}

REALM_EXPORT void list_add_int64(List* list, int64_t value, NativeException::Marshallable& ex)
{
    add(list, value, ex);
}

REALM_EXPORT void list_add_nullable_float(List* list, float value, bool has_value, NativeException::Marshallable& ex)
{
    add_nullable(list, value, has_value, ex);
}

REALM_EXPORT void list_add_float(List* list, float value, NativeException::Marshallable& ex)
{
    add(list, value, ex);
}

REALM_EXPORT void list_add_nullable_double(List* list, double value, bool has_value, NativeException::Marshallable& ex)
{
    add_nullable(list, value, has_value, ex);
}

REALM_EXPORT void list_add_double(List* list, double value, NativeException::Marshallable& ex)
{
    add(list, value, ex);
}
    
REALM_EXPORT void list_insert(List* list, size_t list_ndx, const Object& object_ptr, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, object_ptr.row(), ex);
}
    
REALM_EXPORT void list_insert_nullable_bool(List* list, size_t list_ndx, bool value, bool has_value, NativeException::Marshallable& ex)
{
    insert_nullable(list, list_ndx, value, has_value, ex);
}

REALM_EXPORT void list_insert_bool(List* list, size_t list_ndx, bool value, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, value, ex);
}
    
REALM_EXPORT void list_insert_nullable_int64(List* list, size_t list_ndx, int64_t value, bool has_value, NativeException::Marshallable& ex)
{
    insert_nullable(list, list_ndx, value, has_value, ex);
}

REALM_EXPORT void list_insert_int64(List* list, size_t list_ndx, int64_t value, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, value, ex);
}

REALM_EXPORT void list_insert_nullable_float(List* list, size_t list_ndx, float value, bool has_value, NativeException::Marshallable& ex)
{
    insert_nullable(list, list_ndx, value, has_value, ex);
}

REALM_EXPORT void list_insert_float(List* list, size_t list_ndx, float value, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, value, ex);
}

REALM_EXPORT void list_insert_nullable_double(List* list, size_t list_ndx, double value, bool has_value, NativeException::Marshallable& ex)
{
    insert_nullable(list, list_ndx, value, has_value, ex);
}

REALM_EXPORT void list_insert_double(List* list, size_t list_ndx, double value, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, value, ex);
}
    
REALM_EXPORT Object* list_get(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        const size_t count = list->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);
        
        return new Object(list->get_realm(), list->get_object_schema(), Row(list->get(ndx)));
    });
}
    
REALM_EXPORT bool list_get_bool(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return get<bool>(list, ndx, ex);
}
    
REALM_EXPORT bool list_get_nullable_bool(List* list, size_t ndx, bool& ret_value, NativeException::Marshallable& ex)
{
    return get_nullable<bool>(list, ndx, ret_value, ex);
}

REALM_EXPORT int64_t list_get_int64(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return get<int64_t>(list, ndx, ex);
}

REALM_EXPORT bool list_get_nullable_int64(List* list, size_t ndx, int64_t& ret_value, NativeException::Marshallable& ex)
{
    return get_nullable<int64_t>(list, ndx, ret_value, ex);
}

REALM_EXPORT float list_get_float(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return get<float>(list, ndx, ex);
}

REALM_EXPORT bool list_get_nullable_float(List* list, size_t ndx, float& ret_value, NativeException::Marshallable& ex)
{
    return get_nullable<float>(list, ndx, ret_value, ex);
}

REALM_EXPORT double list_get_double(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return get<double>(list, ndx, ex);
}

REALM_EXPORT bool list_get_nullable_double(List* list, size_t ndx, double& ret_value, NativeException::Marshallable& ex)
{
    return get_nullable<double>(list, ndx, ret_value, ex);
}
    
REALM_EXPORT size_t list_find(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->find(object_ptr.row());
    });
}

REALM_EXPORT size_t list_find_bool(List* list, bool value, NativeException::Marshallable& ex)
{
    return find(list, value, ex);
}
    
REALM_EXPORT size_t list_find_nullable_bool(List* list, bool value, bool has_value, NativeException::Marshallable& ex)
{
    return find_nullable(list, value, has_value, ex);
}
    
REALM_EXPORT size_t list_find_int64(List* list, int64_t value, NativeException::Marshallable& ex)
{
    return find(list, value, ex);
}

REALM_EXPORT size_t list_find_nullable_int64(List* list, int64_t value, bool has_value, NativeException::Marshallable& ex)
{
    return find_nullable(list, value, has_value, ex);
}

REALM_EXPORT size_t list_find_float(List* list, float value, NativeException::Marshallable& ex)
{
    return find(list, value, ex);
}

REALM_EXPORT size_t list_find_nullable_float(List* list, float value, bool has_value, NativeException::Marshallable& ex)
{
    return find_nullable(list, value, has_value, ex);
}

REALM_EXPORT size_t list_find_double(List* list, double value, NativeException::Marshallable& ex)
{
    return find(list, value, ex);
}

REALM_EXPORT size_t list_find_nullable_double(List* list, double value, bool has_value, NativeException::Marshallable& ex)
{
    return find_nullable(list, value, has_value, ex);
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

REALM_EXPORT ThreadSafeReference<List>* list_get_thread_safe_reference(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new ThreadSafeReference<List>{list.get_realm()->obtain_thread_safe_reference(list)};
    });
}

}   // extern "C"
