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
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "object_accessor.hpp"
#include "timestamp_helpers.hpp"
#include "object_cs.hpp"
#include "object-store/src/thread_safe_reference.hpp"
#include "notifications_cs.hpp"

using namespace realm;
using namespace realm::binding;

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
    
    REALM_EXPORT size_t object_get_row_index(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            if (!object.is_valid())
                throw RowDetachedException();
            return object.row().get_index();
        });
    }
    
    
    REALM_EXPORT Object* object_get_link(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> Object* {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            const size_t link_row_ndx = object.row().get_link(column_ndx);
            if (link_row_ndx == realm::npos)
                return nullptr;
            
            auto target_table_ptr = object.row().get_table()->get_link_target(column_ndx);
            const std::string target_name(ObjectStore::object_type_for_table_name(target_table_ptr->get_name()));
            auto& target_schema = *object.realm()->schema().find(target_name);
            return new Object(object.realm(), target_schema, Row((*target_table_ptr)[link_row_ndx]));
        });
    }
    
    REALM_EXPORT List* object_get_list(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> List* {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return new List(object.realm(), object.row().get_linklist(column_ndx));
        });
    }
    
    REALM_EXPORT size_t object_list_is_empty(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return bool_to_size_t(object.row().linklist_is_empty(column_ndx));
        });
    }
    
    
    REALM_EXPORT size_t object_get_bool(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return bool_to_size_t(object.row().get_bool(column_ndx));
        });
    }
    
    // Return value is a boolean indicating whether result has a value (i.e. is not null). If true (1), ret_value will contain the actual value.
    REALM_EXPORT size_t object_get_nullable_bool(const Object& object, size_t property_ndx, size_t& ret_value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (object.row().is_null(column_ndx))
                return 0;
            
            ret_value = bool_to_size_t(object.row().get_bool(column_ndx));
            return 1;
        });
    }
    
    REALM_EXPORT int64_t object_get_int64(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return object.row().get_int(column_ndx);
        });
    }
    
    REALM_EXPORT size_t object_get_nullable_int64(const Object& object, size_t property_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (object.row().is_null(column_ndx))
                return 0;
            
            ret_value = object.row().get_int(column_ndx);
            return 1;
        });
    }
    
    REALM_EXPORT float object_get_float(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return object.row().get_float(column_ndx);
        });
    }
    
    REALM_EXPORT size_t object_get_nullable_float(const Object& object, size_t property_ndx, float& ret_value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (object.row().is_null(column_ndx))
                return 0;
            
            ret_value = object.row().get_float(column_ndx);
            return 1;
        });
    }
    
    REALM_EXPORT double object_get_double(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return object.row().get_double(column_ndx);
        });
    }
    
    REALM_EXPORT size_t object_get_nullable_double(const Object& object, size_t property_ndx, double& ret_value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (object.row().is_null(column_ndx))
                return 0;
            
            ret_value = object.row().get_double(column_ndx);
            return 1;
        });
    }
    
    REALM_EXPORT size_t object_get_string(const Object& object, size_t property_ndx, uint16_t * datatochsarp, size_t bufsize, bool* is_null, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> size_t {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            const StringData fielddata(object.row().get_string(column_ndx));
            if ((*is_null = fielddata.is_null()))
                return 0;
            
            return stringdata_to_csharpstringbuffer(fielddata, datatochsarp, bufsize);
        });
    }
    
    REALM_EXPORT size_t object_get_binary(const Object& object, size_t property_ndx, const char*& return_buffer, size_t& return_size, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            const BinaryData fielddata = object.row().get_binary(column_ndx);
            
            if (fielddata.is_null())
                return 0;
            
            return_buffer = fielddata.data();
            return_size = fielddata.size();
            return 1;
        });
    }
    
    REALM_EXPORT int64_t object_get_timestamp_ticks(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            return to_ticks(object.row().get_timestamp(column_ndx));
        });
    }
    
    REALM_EXPORT size_t object_get_nullable_timestamp_ticks(const Object& object, size_t property_ndx, int64_t& ret_value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_get(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (object.row().is_null(column_ndx))
                return 0;
            
            ret_value = to_ticks(object.row().get_timestamp(column_ndx));
            return 1;
        });
    }
    
    REALM_EXPORT Results* object_get_backlinks(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            verify_can_get(object);
            const Property& prop = object.get_object_schema().computed_properties[property_ndx];
            REALM_ASSERT_DEBUG(prop.type == PropertyType::LinkingObjects);
            
            const ObjectSchema& relationship = *object.realm()->schema().find(prop.object_type);
            const TableRef table = ObjectStore::table_for_object_type(object.realm()->read_group(), relationship.name);
            const Property& link = *relationship.property_for_name(prop.link_origin_property_name);
            
            TableView backlink_view = object.row().get_table()->get_backlink_view(object.row().get_index(), table.get(), link.table_column);
            return new Results(object.realm(), backlink_view);
        });
    }
    
    REALM_EXPORT void object_set_link(Object& object, size_t property_ndx, const Object& target_object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_link(column_ndx, target_object.row().get_index());
        });
    }
    
    REALM_EXPORT void object_clear_link(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().nullify_link(column_ndx);
        });
    }
    
    REALM_EXPORT void object_set_null(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (!object.row().get_table()->is_nullable(column_ndx))
                throw std::invalid_argument("Column is not nullable");
            
            object.row().set_null(column_ndx);
        });
    }
    
    REALM_EXPORT void object_set_null_unique(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            if (!object.row().get_table()->is_nullable(column_ndx))
                throw std::invalid_argument("Column is not nullable");
            
            auto existing = object.row().get_table()->find_first_null(column_ndx);
            if (existing != object.row().get_index()) {
                if (existing != not_found) {
                    throw SetDuplicatePrimaryKeyValueException(
                                                               object.row().get_table()->get_name(),
                                                               object.row().get_table()->get_column_name(column_ndx),
                                                               "null");
                }
            }
            
            object.row().set_null_unique(column_ndx);
        });
    }
    
    REALM_EXPORT void object_set_bool(Object& object, size_t property_ndx, size_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_bool(column_ndx, size_t_to_bool(value));
        });
    }
    
    REALM_EXPORT void object_set_int64(Object& object, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_int(column_ndx, value);
        });
    }
    
    REALM_EXPORT void object_set_int64_unique(Object& object, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            auto existing = object.row().get_table()->find_first_int(column_ndx, value);
            if (existing != object.row().get_index()) {
                if (existing != not_found) {
                    throw SetDuplicatePrimaryKeyValueException(
                                                               object.row().get_table()->get_name(),
                                                               object.row().get_table()->get_column_name(column_ndx),
                                                               util::format("%1", value)
                                                               );
                }
            }
            
            object.row().set_int_unique(column_ndx, value);
        });
    }
    
    REALM_EXPORT void object_set_float(Object& object, size_t property_ndx, float value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_float(column_ndx, value);
        });
    }
    
    REALM_EXPORT void object_set_double(Object& object, size_t property_ndx, double value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_double(column_ndx, value);
        });
    }
    
    REALM_EXPORT void object_set_string(Object& object, size_t property_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            Utf16StringAccessor str(value, value_len);
            object.row().set_string(column_ndx, str);
        });
    }
    
    REALM_EXPORT void object_set_string_unique(Object& object, size_t property_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            Utf16StringAccessor str(value, value_len);
            auto existing = object.row().get_table()->find_first_string(column_ndx, str);
            if (existing != object.row().get_index()) {
                if (object.row().get_table()->find_first_string(column_ndx, str) != not_found) {
                    throw SetDuplicatePrimaryKeyValueException(
                                                               object.row().get_table()->get_name(),
                                                               object.row().get_table()->get_column_name(column_ndx),
                                                               str.to_string()
                                                               );
                }
            }
            
            object.row().set_string_unique(column_ndx, str);
        });
    }
    
    REALM_EXPORT void object_set_binary(Object& object, size_t property_ndx, char* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_binary(column_ndx, BinaryData(value, value_len));
        });
    }
    
    REALM_EXPORT void object_set_timestamp_ticks(Object& object, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);
            
            const size_t column_ndx = get_column_index(object, property_ndx);
            object.row().set_timestamp(column_ndx, from_ticks(value));
        });
    }
    
    REALM_EXPORT void object_remove_row(Object& object, SharedRealm& realm, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            if (object.realm() != realm) {
                throw ObjectManagedByAnotherRealmException("Can only delete an object from the Realm it belongs to.");
            }
            
            verify_can_set(object);
            
            auto const row_index = object.row().get_index();
            object.row().get_table()->move_last_over(row_index);
        });
    }
    
    REALM_EXPORT bool object_equals_object(const Object& object, const Object& other_object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return object.row().get_index() == other_object.row().get_index();
        });
    }
    
    REALM_EXPORT ThreadSafeReference<Object>* object_get_thread_safe_reference(const Object& object, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return new ThreadSafeReference<Object>{object.realm()->obtain_thread_safe_reference(object)};
        });
    }
    
    REALM_EXPORT void* object_destroy_notificationtoken(ManagedNotificationTokenContext* token_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            void* managed_collection = token_ptr->managed_object;
            delete token_ptr;
            return managed_collection;
        });
    }
    
    REALM_EXPORT ManagedNotificationTokenContext* object_add_notification_callback(Object* object, void* managed_object, ManagedNotificationCallback callback, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [=]() {
            return subscribe_for_notifications(managed_object, callback, [object](CollectionChangeCallback callback) {
                return object->add_notification_callback(callback);
            }, new ObjectSchema(object->get_object_schema()));
        });
    }
    
}   // extern "C"
