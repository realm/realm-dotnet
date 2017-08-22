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
#include "timestamp_helpers.hpp"

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
inline void add(List* list, T value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(value);
    });
}

template<typename T>
inline size_t find(List* list, T value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->find(value);
    });
}

struct PrimitiveValue
{
    realm::PropertyType type;
    bool has_value;
    
    union {
        bool bool_value;
        int64_t int_value;
        float float_value;
        double double_value;
    } value;
};

extern "C" {
  
REALM_EXPORT void list_add_object(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        list->add(object_ptr.row());
    });
}
    
REALM_EXPORT void list_add_primitive(List* list, PrimitiveValue value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        switch (value.type) {
            case realm::PropertyType::Bool:
                list->add(value.value.bool_value);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable:
                list->add(value.has_value ? Optional<bool>(value.value.bool_value) : Optional<bool>(none));
                break;
            case realm::PropertyType::Int:
                list->add(value.value.int_value);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable:
                list->add(value.has_value ? Optional<int64_t>(value.value.int_value) : Optional<int64_t>(none));
                break;
            case realm::PropertyType::Float:
                list->add(value.value.float_value);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable:
                list->add(value.has_value ? Optional<float>(value.value.float_value) : Optional<float>(none));
                break;
            case realm::PropertyType::Double:
                list->add(value.value.double_value);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable:
                list->add(value.has_value ? Optional<double>(value.value.double_value) : Optional<double>(none));
                break;
            case realm::PropertyType::Date:
                list->add(from_ticks(value.value.int_value));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable:
                list->add(value.has_value ? from_ticks(value.value.int_value) : Timestamp());
                break;
            default:
                REALM_UNREACHABLE();
        }
    });
}
    
REALM_EXPORT void list_add_string(List* list, uint16_t* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        Utf16StringAccessor str(value, value_len);
        add(list, (StringData)str, ex);
    }
    else {
        add(list, StringData(), ex);
    }
}
    
REALM_EXPORT void list_add_binary(List* list, char* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        add(list, BinaryData(value, value_len), ex);
    }
    else {
        add(list, BinaryData(), ex);
    }
}

REALM_EXPORT void list_insert_object(List* list, size_t list_ndx, const Object& object_ptr, NativeException::Marshallable& ex)
{
    insert(list, list_ndx, object_ptr.row(), ex);
}
    
REALM_EXPORT void list_insert_primitive(List* list, size_t list_ndx, PrimitiveValue value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (list_ndx > count) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, count);
        }

        switch (value.type) {
            case realm::PropertyType::Bool:
                list->insert(list_ndx, value.value.bool_value);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable:
                list->insert(list_ndx, value.has_value ? Optional<bool>(value.value.bool_value) : Optional<bool>(none));
                break;
            case realm::PropertyType::Int:
                list->insert(list_ndx, value.value.int_value);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable:
                list->insert(list_ndx, value.has_value ? Optional<int64_t>(value.value.int_value) : Optional<int64_t>(none));
                break;
            case realm::PropertyType::Float:
                list->insert(list_ndx, value.value.float_value);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable:
                list->insert(list_ndx, value.has_value ? Optional<float>(value.value.float_value) : Optional<float>(none));
                break;
            case realm::PropertyType::Double:
                list->insert(list_ndx, value.value.double_value);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable:
                list->insert(list_ndx, value.has_value ? Optional<double>(value.value.double_value) : Optional<double>(none));
                break;
            case realm::PropertyType::Date:
                list->insert(list_ndx, from_ticks(value.value.int_value));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable:
                list->insert(list_ndx, value.has_value ? from_ticks(value.value.int_value) : Timestamp());
                break;
            default:
                REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void list_insert_string(List* list, size_t list_ndx, uint16_t* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        Utf16StringAccessor str(value, value_len);
        insert(list, list_ndx, (StringData)str, ex);
    }
    else {
        insert(list, list_ndx, StringData(), ex);
    }
}

REALM_EXPORT void list_insert_binary(List* list, size_t list_ndx, char* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        insert(list, list_ndx, BinaryData(value, value_len), ex);
    }
    else {
        insert(list, list_ndx, BinaryData(), ex);
    }
}

REALM_EXPORT Object* list_get_object(List* list, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        const size_t count = list->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);
        
        return new Object(list->get_realm(), list->get_object_schema(), Row(list->get(ndx)));
    });
}
    
REALM_EXPORT void list_get_primitive(List* list, size_t ndx, PrimitiveValue& value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list->size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);

        value.has_value = true;
        switch (value.type) {
            case realm::PropertyType::Bool:
                value.value.bool_value = list->get<bool>(ndx);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable: {
                auto result = list->get<Optional<bool>>(ndx);
                value.has_value = !!result;
                value.value.bool_value = result.value_or(false);
                break;
            }
            case realm::PropertyType::Int:
                value.value.int_value = list->get<int64_t>(ndx);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable: {
                auto result = list->get<Optional<int64_t>>(ndx);
                value.has_value = !!result;
                value.value.int_value = result.value_or(0);
                break;
            }
            case realm::PropertyType::Float:
                value.value.float_value = list->get<float>(ndx);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable: {
                auto result = list->get<Optional<float>>(ndx);
                value.has_value = !!result;
                value.value.float_value = result.value_or((float)0);
                break;
            }
            case realm::PropertyType::Double:
                value.value.double_value = list->get<double>(ndx);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable: {
                auto result = list->get<Optional<double>>(ndx);
                value.has_value = !!result;
                value.value.double_value = result.value_or((double)0);
                break;
            }
            case realm::PropertyType::Date:
                value.value.int_value = to_ticks(list->get<Timestamp>(ndx));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable: {
                auto result = list->get<Timestamp>(ndx);
                value.has_value = !result.is_null();
                value.value.int_value = result.is_null() ? 0 : to_ticks(result);
                break;
            }
            default:
                REALM_UNREACHABLE();
        }
    });
}
    
REALM_EXPORT size_t list_get_string(List* list, size_t ndx, uint16_t* value, size_t value_len, bool* is_null, NativeException::Marshallable& ex)
{
    auto result = get<StringData>(list, ndx, ex);
    
    if ((*is_null = result.is_null()))
        return 0;
    
    return stringdata_to_csharpstringbuffer(result, value, value_len);
}
    
REALM_EXPORT size_t list_get_binary(List* list, size_t ndx, char* return_buffer, size_t buffer_size, bool* is_null, NativeException::Marshallable& ex)
{
    auto result = get<BinaryData>(list, ndx, ex);
    
    if ((*is_null = result.is_null()))
        return 0;
    
    const size_t data_size = result.size();
    if (data_size <= buffer_size)
        std::copy(result.data(), result.data() + data_size, return_buffer);
    
    return data_size;
}
    
REALM_EXPORT size_t list_find_object(List* list, const Object& object_ptr, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return list->find(object_ptr.row());
    });
}
    
REALM_EXPORT size_t list_find_primitive(List* list, PrimitiveValue value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        switch (value.type) {
            case realm::PropertyType::Bool:
                return list->find(value.value.bool_value);
            case realm::PropertyType::Bool | realm::PropertyType::Nullable:
                return list->find(value.has_value ? Optional<bool>(value.value.bool_value) : Optional<bool>(none));
            case realm::PropertyType::Int:
                return list->find(value.value.int_value);
            case realm::PropertyType::Int | realm::PropertyType::Nullable:
                return list->find(value.has_value ? Optional<int64_t>(value.value.int_value) : Optional<int64_t>(none));
            case realm::PropertyType::Float:
                return list->find(value.value.float_value);
            case realm::PropertyType::Float | realm::PropertyType::Nullable:
                return list->find(value.has_value ? Optional<float>(value.value.float_value) : Optional<float>(none));
            case realm::PropertyType::Double:
                return list->find(value.value.double_value);
            case realm::PropertyType::Double | realm::PropertyType::Nullable:
                return list->find(value.has_value ? Optional<double>(value.value.double_value) : Optional<double>(none));
            case realm::PropertyType::Date:
                return list->find(from_ticks(value.value.int_value));
            case realm::PropertyType::Date | realm::PropertyType::Nullable:
                return list->find(value.has_value ? from_ticks(value.value.int_value) : Timestamp());
            default:
                REALM_UNREACHABLE();
        }
    });
}
    
REALM_EXPORT size_t list_find_string(List* list, uint16_t* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        Utf16StringAccessor str(value, value_len);
        return find(list, (StringData)str, ex);
    }
    
    return find(list, StringData(), ex);
}
    
REALM_EXPORT size_t list_find_binary(List* list, char* value, size_t value_len, bool has_value, NativeException::Marshallable& ex)
{
    if (has_value) {
        return find(list, BinaryData(value, value_len), ex);
    }
    
    return find(list, BinaryData(), ex);
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
