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

#include "error_handling.hpp"
#include "marshalling.hpp"
#include "notifications_cs.hpp"
#include "object_cs.hpp"
#include "realm_export_decls.hpp"
#include "timestamp_helpers.hpp"

#include <cstddef>
#include <realm.hpp>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/exceptions.hpp>

namespace realm::binding {
REALM_FORCEINLINE KeyPathArray construct_key_path_array(const ObjectSchema& object)
{
    KeyPathArray keyPathArray;
    for (auto& prop : object.persisted_properties) {
        // We want to filter out all collection properties. By providing keypaths with just the top-level properties
        // means we won't get deep change notifications either.
        bool is_scalar = (unsigned short)(prop.type & ~PropertyType::Collection) == (unsigned short)prop.type;
        if (is_scalar) {
            KeyPath keyPath;
            keyPath.push_back(std::make_pair(object.table_key, prop.column_key));
            keyPathArray.push_back(keyPath);
        }
    }
    return keyPathArray;
}

extern "C" {
    REALM_EXPORT bool object_get_is_valid(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.is_valid();
        });
    }

    REALM_EXPORT void object_destroy(Object* object)
    {
        delete object;
    }

    REALM_EXPORT List* object_get_list(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);

            return new List(object.realm(), object.get_obj(), get_column_key(object, property_ndx));
        });
    }

    REALM_EXPORT object_store::Set* object_get_set(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);

            return new object_store::Set(object.realm(), object.get_obj(), get_column_key(object, property_ndx));
        });
    }

    REALM_EXPORT object_store::Dictionary* object_get_dictionary(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);

            return new object_store::Dictionary(object.realm(), object.get_obj(), get_column_key(object, property_ndx));
        });
    }

    REALM_EXPORT void object_get_value(const Object& object, size_t property_ndx, realm_value_t* value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_get(object);

            auto prop = get_property(object, property_ndx);
            if ((prop.type & ~PropertyType::Flags) == PropertyType::Object) {
                const Obj link_obj = object.get_obj().get_linked_object(prop.column_key);
                if (link_obj) {
                    *value = to_capi(link_obj, object.realm());
                }
                else {
                    value->type = realm_value_type::RLM_TYPE_NULL;
                }

                return;
            }

            auto val = object.get_obj().get_any(prop.column_key);
            if (val.is_null())
            {
                *value = to_capi(val);
                return;
            }
            
            switch (val.get_type()) {
            case type_TypedLink:
                *value = to_capi(val.get<ObjLink>(), object.realm());
                break;
            case type_List:
                *value = to_capi(new List(object.realm(), object.get_obj(), prop.column_key));
                break;
            case type_Dictionary:
                *value = to_capi(new object_store::Dictionary(object.realm(), object.get_obj(), prop.column_key));
                break;
            default:
                *value = to_capi(std::move(val));
                break;
            }
        });
    }

    REALM_EXPORT void object_get_additional_property(const Object& object, realm_string_t property_name, realm_value_t* value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_get(object);

            auto val = object.get_obj().get_additional_prop(capi_to_std(property_name));

            if (val.is_null())
            {
                *value = to_capi(val);
                return;
            }

            Path path = { PathElement(capi_to_std(property_name)) };

            switch (val.get_type()) {
            case type_TypedLink:
                *value = to_capi(val.get<ObjLink>(), object.realm());
                break;
            case type_List:
                *value = to_capi(new List(object.realm(), object.get_obj().get_list_ptr<Mixed>(path)));
                break;
            default:
                *value = to_capi(std::move(val));
                break;
            }
        });
    }

    REALM_EXPORT void object_get_schema(const Object& object, void* managed_callback, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            auto& object_schema = object.get_object_schema();
            Schema schema({object_schema});
            send_schema_to_managed(schema, managed_callback);
        });
    }

    REALM_EXPORT void object_set_value(Object& object, size_t property_ndx, realm_value_t value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_set(object);

            auto prop = get_property(object, property_ndx);

            if (value.is_null() && !is_nullable(prop.type)) {
                auto& schema = object.get_object_schema();
                throw NotNullable(schema.name, prop.name);
            }

            if (!value.is_null() && (prop.type & ~PropertyType::Flags) != PropertyType::Mixed &&
                to_capi(prop.type) != value.type) {
                auto& schema = object.get_object_schema();
                throw PropertyTypeMismatchException(
                    schema.name,
                    schema.persisted_properties[property_ndx].name,
                    to_string(prop.type),
                    to_string(value.type));
            }

            if (value.type == realm_value_type::RLM_TYPE_LINK) {
                // For Mixed, we need ObjLink, otherwise, ObjKey
                if ((prop.type & ~PropertyType::Flags) == PropertyType::Mixed) {
                    object.get_obj().set_any(prop.column_key, ObjLink(value.link.object->get_object_schema().table_key, value.link.object->get_obj().get_key()));
                }
                else {
                    object.get_obj().set(prop.column_key, value.link.object->get_obj().get_key());
                }
            }
            else {
                object.get_obj().set_any(prop.column_key, from_capi(value));
            }
        });
    }

    REALM_EXPORT void object_set_additional_property(Object& object, realm_string_t property_name, realm_value_t value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_set(object);
            object.get_obj().set_additional_prop(capi_to_std(property_name), from_capi(value));
        });
    }

    REALM_EXPORT void object_unset_property(Object& object, realm_string_t property_name, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_set(object);
            object.get_obj().erase_prop(capi_to_std(property_name));
        });
    }

    REALM_EXPORT realm_string_t* object_get_additional_properties(Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto props = object.get_obj().get_additional_properties();

            std::vector<realm_string_t> realm_string_array;

            size_t size = props.size();

            for (size_t i = 0; i < size; ++i) {
                realm_string_array.push_back(to_capi(props[i]));
            }

            return MarshaledVector(realm_string_array);
        });
    }

    REALM_EXPORT void* object_set_collection_value(Object& object, size_t property_ndx, realm_value_type type, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]()-> void* {
            verify_can_set(object);

            auto prop = get_property(object, property_ndx);

            switch (type)
            {
            case realm::binding::realm_value_type::RLM_TYPE_LIST:
            {
                object.get_obj().set_collection(prop.column_key, CollectionType::List);
                auto innerList = new List(object.realm(), object.get_obj(), prop.column_key);
                innerList->remove_all();
                return innerList;
            }
            case realm::binding::realm_value_type::RLM_TYPE_DICTIONARY:
            {
                object.get_obj().set_collection(prop.column_key, CollectionType::Dictionary);
                auto innerDict = new object_store::Dictionary(object.realm(), object.get_obj(), prop.column_key);
                innerDict->remove_all();
                return innerDict;
            }
            default:
                REALM_TERMINATE("Invalid collection type");
            }
        });
    }

    REALM_EXPORT void* object_set_collection_additional_property(Object& object, 
        realm_string_t property_name, realm_value_type type, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]()-> void* {
            verify_can_set(object);

            auto prop = capi_to_std(property_name);

            Path path = { PathElement(prop) };

            switch (type)
            {
                case realm::binding::realm_value_type::RLM_TYPE_LIST:
                {
                
                    object.get_obj().set_collection(prop, CollectionType::List);
                    //TODO We probably need to ask for methods that do not require to build a path
                    auto innerList = new List(object.realm(), object.get_obj().get_list_ptr<Mixed>(path));
                    innerList->remove_all();
                    return innerList;
                }
                case realm::binding::realm_value_type::RLM_TYPE_DICTIONARY:
                {
                    REALM_TERMINATE("Invalid collection type");
                }
                default:
                    REALM_TERMINATE("Invalid collection type");
            }
        });
    }

    REALM_EXPORT Results* object_get_backlinks(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            verify_can_get(object);
            const Property& prop = object.get_object_schema().computed_properties[property_ndx];
            REALM_ASSERT(prop.type == PropertyType::LinkingObjects);

            const ObjectSchema& relationship = *object.realm()->schema().find(prop.object_type);
            const Property& link = *relationship.property_for_name(prop.link_origin_property_name);

            TableRef table = object.realm()->read_group().get_table(relationship.table_key);
            const ColKey column = link.column_key;

            TableView backlink_view = object.get_obj().get_backlink_view(table, column);
            return new Results(object.realm(), std::move(backlink_view));
        });
    }

    REALM_EXPORT Results* object_get_backlinks_for_type(Object& object, TableKey table_key, size_t source_property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            verify_can_get(object);

            const TableRef source_table = get_table(object.realm(), table_key);

            const ObjectSchema& source_object_schema = *object.realm()->schema().find(table_key);
            const Property& source_property = source_object_schema.persisted_properties[source_property_ndx];

            if (source_property.object_type != object.get_object_schema().name) {
                throw InvalidArgument(ErrorCodes::InvalidProperty, util::format("'%1.%2' is not a relationship to '%3'", source_object_schema.name, source_property.name, object.get_object_schema().name));
            }

            TableView backlink_view = object.get_obj().get_backlink_view(source_table, source_property.column_key);
            return new Results(object.realm(), std::move(backlink_view));
        });
    }

    REALM_EXPORT Object* object_create_embedded(Object& parent, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(parent);

            return new Object(parent.realm(), parent.get_obj().create_and_set_linked_object(get_column_key(parent, property_ndx)));
        });
    }

    REALM_EXPORT Object* object_get_parent(Object& child, TableKey& table_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            Obj parent = child.get_obj().get_parent_object();
            table_key = parent.get_table()->get_key();

            return new Object(child.realm(), std::move(parent));
        });
    }

    REALM_EXPORT void object_set_null(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);

            auto column_key = get_column_key(object, property_ndx);
            if (!object.get_obj().get_table()->is_nullable(column_key))
                throw std::invalid_argument("Column is not nullable");

            object.get_obj().set_null(column_key);
        });
    }

    REALM_EXPORT void object_remove(Object& object, SharedRealm& realm, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            if (object.realm() != realm) {
                throw ObjectManagedByAnotherRealmException("Can only delete an object from the Realm it belongs to.");
            }

            verify_can_set(object);

            object.get_obj().get_table()->remove_object(object.get_obj().get_key());
        });
    }

    REALM_EXPORT bool object_equals_object(const Object& object, const Object& other_object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.is_valid() &&
                other_object.is_valid() &&
                (*object.get_obj().get_table()).get_key() == (*other_object.get_obj().get_table()).get_key() &&
                object.get_obj().get_key() == other_object.get_obj().get_key();
        });
    }

    REALM_EXPORT int32_t object_get_hashcode(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            int32_t table_key_value = static_cast<int32_t>(object.get_obj().get_table()->get_key().value);
            int32_t object_key_value = static_cast<int32_t>(object.get_obj().get_key().value);

            int32_t hashCode = -986587137;
            hashCode = (hashCode * -1521134295) + table_key_value;
            hashCode = (hashCode * -1521134295) + object_key_value;
            return hashCode;
        });
    }

    REALM_EXPORT ThreadSafeReference* object_get_thread_safe_reference(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new ThreadSafeReference(object);
        });
    }

    REALM_EXPORT void* object_destroy_notificationtoken(ManagedNotificationTokenContext* token_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            void* managed_object = token_ptr->managed_object;
            delete token_ptr;
            return managed_object;
        });
    }

    REALM_EXPORT ManagedNotificationTokenContext* object_add_notification_callback(Object* object, void* managed_object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return subscribe_for_notifications(managed_object, [&](CollectionChangeCallback callback) {
                auto keyPaths = construct_key_path_array(object->get_object_schema());
                return object->add_notification_callback(callback, keyPaths);
            }, key_path_collection_type::SHALLOW, nullptr, new ObjectSchema(object->get_object_schema()));
        });
    }

    REALM_EXPORT int64_t object_add_int64(Object& object, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);

            auto col_key = get_column_key(object, property_ndx);
            return object.get_obj().add_int(col_key, value).get_any(col_key).get_int();
        });
    }

    REALM_EXPORT size_t object_get_backlink_count(Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.get_obj().get_backlink_count();
        });
    }

    REALM_EXPORT Object* object_freeze(const Object& object, const SharedRealm& realm, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new Object(object.freeze(realm));
        });
    }

} // extern "C"
} // namespace realm::binding
