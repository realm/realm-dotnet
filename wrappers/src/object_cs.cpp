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

#include "object_cs.hpp"
#include "timestamp_helpers.hpp"
#include "notifications_cs.hpp"
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"

#include <realm.hpp>
#include <object_accessor.hpp>
#include <thread_safe_reference.hpp>


using namespace realm;
using namespace realm::binding;

template <typename T>
inline T object_get(const Object& object, ColKey column_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        verify_can_get(object);

        return object.obj().get<T>(column_key);
    });
}

template <typename T>
inline bool object_get(const Object& object, ColKey column_key, bool is_nullable, T& ret_value, NativeException::Marshallable& ex)
{
    if (!is_nullable) {
        ret_value = object_get<T>(object, column_key, ex);
        return true;
    }

    return handle_errors(ex, [&]() {
        verify_can_get(object);

        auto result = object.obj().get<util::Optional<T>>(column_key);
        if (!result) {
            return false;
        }

        ret_value = *result;
        return true;
    });
}

template <typename T>
inline void object_set(Object& object, ColKey column_key, const T& value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        verify_can_set(object);

        object.obj().set<T>(column_key, value);
    });
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

    REALM_EXPORT void object_get_key(const Object& object, ObjKey& key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            key = object.obj().get_key();
        });
    }

    REALM_EXPORT Object* object_get_link(const Object& object, ColKey column_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> Object* {
            verify_can_get(object);

            const Obj link_obj = object.obj().get_linked_object(column_key);
            if (!link_obj)
                return nullptr;

            const std::string target_name(ObjectStore::object_type_for_table_name(link_obj.get_table()->get_name()));
            auto& target_schema = *object.realm()->schema().find(target_name);
            return new Object(object.realm(), target_schema, link_obj);
        });
    }

    REALM_EXPORT List* object_get_list(const Object& object, ColKey column_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> List* {
            verify_can_get(object);

            return new List(object.realm(), object.obj(), column_key);
        });
    }

    REALM_EXPORT bool object_get_bool(const Object& object, ColKey column_key, bool is_nullable, bool& ret_value, NativeException::Marshallable& ex)
    {
        return object_get<bool>(object, column_key, is_nullable, ret_value, ex);
    }

    REALM_EXPORT bool object_get_int64(const Object& object, ColKey column_key, bool is_nullable, int64_t& ret_value, NativeException::Marshallable& ex)
    {
        return object_get<int64_t>(object, column_key, is_nullable, ret_value, ex);
    }

    REALM_EXPORT bool object_get_float(const Object& object, ColKey column_key, bool is_nullable, float& ret_value, NativeException::Marshallable& ex)
    {
        return object_get<float>(object, column_key, is_nullable, ret_value, ex);
    }

    REALM_EXPORT bool object_get_double(const Object& object, ColKey column_key, bool is_nullable, double& ret_value, NativeException::Marshallable& ex)
    {
        return object_get<double>(object, column_key, is_nullable, ret_value, ex);
    }

    REALM_EXPORT size_t object_get_string(const Object& object, ColKey column_key, uint16_t* string_buffer, size_t buffer_size, bool& is_null, NativeException::Marshallable& ex)
    {
        StringData field_data = object_get<StringData>(object, column_key, ex);
        if (ex.type != RealmErrorType::NoError) {
            return -1;
        }

        if ((is_null = field_data.is_null())) {
            return 0;
        }

        return stringdata_to_csharpstringbuffer(field_data, string_buffer, buffer_size);
    }

    REALM_EXPORT size_t object_get_binary(const Object& object, ColKey column_key, char* return_buffer, size_t buffer_size, bool& is_null, NativeException::Marshallable& ex)
    {
        BinaryData field_data = object_get<BinaryData>(object, column_key, ex);
        if (ex.type != RealmErrorType::NoError) {
            return -1;
        }

        if ((is_null = field_data.is_null())) {
            return 0;
        }

        const size_t data_size = field_data.size();
        if (data_size <= buffer_size) {
            std::copy(field_data.data(), field_data.data() + data_size, return_buffer);
        }

        return data_size;
    }

    REALM_EXPORT bool object_get_timestamp_ticks(const Object& object, ColKey column_key, bool is_nullable, int64_t& ret_value, NativeException::Marshallable& ex)
    {
        Timestamp field_data = object_get<Timestamp>(object, column_key, ex);
        if (ex.type != RealmErrorType::NoError || field_data.is_null()) {
            return false;
        }
 
        ret_value = to_ticks(field_data);
        return true;
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

            TableView backlink_view = object.obj().get_backlink_view(table, column);
            return new Results(object.realm(), std::move(backlink_view));
        });
    }

    REALM_EXPORT Results* object_get_backlinks_for_type(Object& object, TableRef& source_table, ColKey source_column_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            verify_can_get(object);
            
            const ObjectSchema& source_object_schema = *object.realm()->schema().find(ObjectStore::object_type_for_table_name(source_table->get_name()));
            const Property& source_property = *std::find_if(source_object_schema.persisted_properties.begin(), source_object_schema.persisted_properties.end(), [&](Property p) {
                return p.column_key == source_column_key;
            });
        
            if (source_property.object_type != object.get_object_schema().name) {
                throw std::logic_error(util::format("'%1.%2' is not a relationship to '%3'", source_object_schema.name, source_property.name, object.get_object_schema().name));
            }
        
            TableView backlink_view = object.obj().get_backlink_view(source_table, source_column_key);
            return new Results(object.realm(), std::move(backlink_view));
        });
    }
    
    REALM_EXPORT void object_set_link(Object& object, ColKey column_key, const Object& target_object, NativeException::Marshallable& ex)
    {
        return object_set<ObjKey>(object, column_key, target_object.obj().get_key(), ex);
    }

    REALM_EXPORT void object_clear_link(Object& object, ColKey column_key, NativeException::Marshallable& ex)
    {
        return object_set<ObjKey>(object, column_key, null_key, ex);
    }

    REALM_EXPORT void object_set_null(Object& object, ColKey column_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);

            if (!object.obj().get_table()->is_nullable(column_key))
                throw std::invalid_argument("Column is not nullable");

            object.obj().set_null(column_key);
        });
    }
    
    REALM_EXPORT void object_set_bool(Object& object, ColKey column_key, bool value, NativeException::Marshallable& ex)
    {
        return object_set<bool>(object, column_key, value, ex);
    }

    REALM_EXPORT void object_set_int64(Object& object, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
    {
        return object_set<int64_t>(object, column_key, value, ex);
    }
    
    REALM_EXPORT void object_set_float(Object& object, ColKey column_key, float value, NativeException::Marshallable& ex)
    {
        return object_set<float>(object, column_key, value, ex);
    }

    REALM_EXPORT void object_set_double(Object& object, ColKey column_key, double value, NativeException::Marshallable& ex)
    {
        return object_set<double>(object, column_key, value, ex);
    }

    REALM_EXPORT void object_set_string(Object& object, ColKey column_key, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
    {
        Utf16StringAccessor str(value, value_len);
        return object_set<StringData>(object, column_key, str, ex);
    }
    
    REALM_EXPORT void object_set_binary(Object& object, ColKey column_key, char* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return object_set<BinaryData>(object, column_key, BinaryData(value, value_len), ex);
    }

    REALM_EXPORT void object_set_timestamp_ticks(Object& object, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
    {
        return object_set<Timestamp>(object, column_key, from_ticks(value), ex);
    }

    REALM_EXPORT void object_remove(Object& object, SharedRealm& realm, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            if (object.realm() != realm) {
                throw ObjectManagedByAnotherRealmException("Can only delete an object from the Realm it belongs to.");
            }

            verify_can_set(object);

            object.obj().get_table()->remove_object(object.obj().get_key());
        });
    }

    REALM_EXPORT bool object_equals_object(const Object& object, const Object& other_object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.is_valid() &&
                other_object.is_valid() &&
                (*object.obj().get_table()).get_key() == (*other_object.obj().get_table()).get_key() &&
                object.obj().get_key() == other_object.obj().get_key();
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

    REALM_EXPORT ManagedNotificationTokenContext* object_add_notification_callback(Object* object, void* managed_object, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return subscribe_for_notifications(managed_object, callback, [object](CollectionChangeCallback callback) {
                return object->add_notification_callback(callback);
            }, new ObjectSchema(object->get_object_schema()));
        });
    }
    
    REALM_EXPORT void object_add_int64(Object& object, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            object.obj().add_int(column_key, value);
        });
    }

    REALM_EXPORT size_t object_get_backlink_count(Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.obj().get_backlink_count();
        });
    }

    REALM_EXPORT bool object_get_is_frozen(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.is_frozen();
        });
    }

    REALM_EXPORT Object* object_freeze(const Object& object, const SharedRealm& realm, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new Object(object.freeze(realm));
        });
    }

}   // extern "C"
