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
#include <realm/parser/query_parser.hpp>
#include <realm/exceptions.hpp>

#include "error_handling.hpp"
#include "filter.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "notifications_cs.hpp"
#include "schema_cs.hpp"

using namespace realm;
using namespace realm::binding;

namespace {
    inline static void ensure_types(List& list, realm_value_t value) {
        if (value.is_null() && !is_nullable(list.get_type())) {
            throw NotNullable("Attempted to add null to a list of required values");
        }

        if (!value.is_null() && list.get_type() != PropertyType::Mixed && to_capi(list.get_type()) != value.type) {
            throw PropertyTypeMismatchException(to_string(list.get_type()), to_string(value.type));
        }
    }
}

extern "C" {

REALM_EXPORT void list_set_value(List& list, size_t list_ndx, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        ensure_types(list, value);
        
        const size_t count = list.size();
        if (list_ndx >= count) {
            throw IndexOutOfRangeException("Set in RealmList", list_ndx, count);
        }

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            // For Mixed, we need ObjLink, otherwise, ObjKey
            if ((list.get_type() & ~PropertyType::Flags) == PropertyType::Mixed) {
                list.set_any(list_ndx, ObjLink(value.link.object->get_object_schema().table_key, value.link.object->get_obj().get_key()));
            }
            else {
                list.set(list_ndx, value.link.object->get_obj());
            }
        }
        else {
            list.set_any(list_ndx, from_capi(value));
        }
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

REALM_EXPORT void* list_set_collection(List& list, size_t list_ndx, realm_value_type type, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]()-> void*{

        if (list_ndx > list.size()) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, list.size());
        }

        switch (type)
        {
        case realm::binding::realm_value_type::RLM_TYPE_LIST:
        {
            list.set_collection(list_ndx, CollectionType::List);
            auto innerList = new List(list.get_list(list_ndx));
            innerList->remove_all();
            return innerList;
        }
        case realm::binding::realm_value_type::RLM_TYPE_DICTIONARY:
        {
            list.set_collection(list_ndx, CollectionType::Dictionary);
            auto innerDict = new object_store::Dictionary(list.get_dictionary(list_ndx));
            innerDict->remove_all();
            return innerDict;
        }
        default:
            REALM_TERMINATE("Invalid collection type");
        }
    });
}

REALM_EXPORT void list_insert_value(List& list, size_t list_ndx, realm_value_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        ensure_types(list, value);

        if (list_ndx > list.size()) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, list.size());
        }

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            // For Mixed, we need ObjLink, otherwise, ObjKey
            if ((list.get_type() & ~PropertyType::Flags) == PropertyType::Mixed) {
                list.insert_any(list_ndx, ObjLink(value.link.object->get_object_schema().table_key, value.link.object->get_obj().get_key()));
            }
            else {
                list.insert(list_ndx, value.link.object->get_obj());
            }
        }
        else {
            list.insert_any(list_ndx, from_capi(value));
        }
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

REALM_EXPORT void* list_insert_collection(List& list, size_t list_ndx, realm_value_type type, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]()-> void*{

        if (list_ndx > list.size()) {
            throw IndexOutOfRangeException("Insert into RealmList", list_ndx, list.size());
        }

        switch (type)
        {
        case realm::binding::realm_value_type::RLM_TYPE_LIST:
            list.insert_collection(list_ndx, CollectionType::List);
            return new List(list.get_list(list_ndx));
        case realm::binding::realm_value_type::RLM_TYPE_DICTIONARY:
            list.insert_collection(list_ndx, CollectionType::Dictionary);
            return new object_store::Dictionary(list.get_dictionary(list_ndx));
        default:
            REALM_TERMINATE("Invalid collection type");
        }
    });
}

REALM_EXPORT void list_add_value(List& list, realm_value_t value, NativeException::Marshallable& ex)
{
    list_insert_value(list, list.size(), value, ex);
}

REALM_EXPORT Object* list_add_embedded(List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Object(list.get_realm(), list.get_object_schema(), list.add_embedded());
    });
}

REALM_EXPORT void* list_add_collection(List& list, realm_value_type type, NativeException::Marshallable& ex)
{
    return list_insert_collection(list, list.size(), type, ex);
}

REALM_EXPORT void list_get_value(List& list, size_t ndx, realm_value_t* value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = list.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);

        if ((list.get_type() & ~PropertyType::Flags) == PropertyType::Object) {
            *value = to_capi(list.get(ndx), list.get_realm());
            return;
        }

        auto val = list.get_any(ndx);

        if (val.is_null()) {
            *value = to_capi(std::move(val));
            return;
        }

        switch (val.get_type()) {
        case type_TypedLink:
            *value = to_capi(val.get<ObjLink>(), list.get_realm());
            break;
        case type_List:
            *value = to_capi(new List(list.get_list(ndx)));
            break;
        case type_Dictionary:
            *value = to_capi(new object_store::Dictionary(list.get_dictionary(ndx)));
            break;
        default:
            *value = to_capi(std::move(val));
            break;
        }
    });
}

REALM_EXPORT size_t list_find_value(List& list, realm_value_t value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto list_type = list.get_type();
        // This doesn't use ensure_types to allow List<string>.Find(null) to return false
        if (value.is_null() && !is_nullable(list_type)) {
            return (size_t)-1;
        }
        
        if (!value.is_null() && list_type != PropertyType::Mixed && to_capi(list_type) != value.type) {
            throw PropertyTypeMismatchException(to_string(list_type), to_string(value.type));
        }

        if (value.type == realm_value_type::RLM_TYPE_LINK) {
            if (list.get_realm() != value.link.object->realm()) {
                throw ObjectManagedByAnotherRealmException("Can't look up index of an object that belongs to a different Realm.");
            }

            if ((list_type & ~PropertyType::Flags) == PropertyType::Mixed) {
                return list.find_any(value.link.object->get_obj());
            }

            return list.find(value.link.object->get_obj());
        }

        return list.find_any(from_capi(value));
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

REALM_EXPORT ManagedNotificationTokenContext* list_add_notification_callback(List* list, void* managed_list,
    key_path_collection_type type, void* managedCallback, realm_string_t* keypaths, size_t keypaths_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        auto keypath_array = build_keypath_array(list, type, keypaths, keypaths_len);
        return subscribe_for_notifications(managed_list, [list, keypath_array](CollectionChangeCallback callback) {
            return list->add_notification_callback(callback, keypath_array);
        }, type, managedCallback);
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

REALM_EXPORT List* list_freeze(const List& list, const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new List(list.freeze(realm));
    });
}

REALM_EXPORT Results* list_to_results(const List& list, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(list.as_results());
    });
}

REALM_EXPORT Results* list_get_filtered_results(const List& list, uint16_t* query_buf, size_t query_len, query_argument* arguments, size_t args_count, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return get_filtered_results(list.get_realm(), list.get_table(), list.get_query(), query_buf, query_len, arguments, args_count, DescriptorOrdering());
    });
}

}   // extern "C"
