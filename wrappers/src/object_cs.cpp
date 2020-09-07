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

    REALM_EXPORT void object_get_primitive(const Object& object, ColKey column_key, PrimitiveValue& value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_get(object);

            value.has_value = true;
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
            switch (value.type) {
            case realm::PropertyType::Bool:
                value.value.bool_value = object.obj().get<bool>(column_key);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<bool>>(column_key);
                value.has_value = !!result;
                value.value.bool_value = result.value_or(false);
                break;
            }
            case realm::PropertyType::Int:
                value.value.int_value = object.obj().get<int64_t>(column_key);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<int64_t>>(column_key);
                value.has_value = !!result;
                value.value.int_value = result.value_or(0);
                break;
            }
            case realm::PropertyType::Float:
                value.value.float_value = object.obj().get<float>(column_key);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<float>>(column_key);
                value.has_value = !!result;
                value.value.float_value = result.value_or((float)0);
                break;
            }
            case realm::PropertyType::Double:
                value.value.double_value = object.obj().get<double>(column_key);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<double>>(column_key);
                value.has_value = !!result;
                value.value.double_value = result.value_or((double)0);
                break;
            }
            case realm::PropertyType::Date:
                value.value.int_value = to_ticks(object.obj().get<Timestamp>(column_key));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable: {
                auto result = object.obj().get<Timestamp>(column_key);
                value.has_value = !result.is_null();
                value.value.int_value = result.is_null() ? 0 : to_ticks(result);
                break;
            }
            case realm::PropertyType::Decimal: {
                auto result = object.obj().get<Decimal128>(column_key).raw();
                value.value.low_bytes = result->w[0];
                value.value2.high_bytes = result->w[1];
                break;
            }
            case realm::PropertyType::Decimal | realm::PropertyType::Nullable: {
                auto result = object.obj().get<Decimal128>(column_key);
                value.has_value = !result.is_null();
                value.value.low_bytes = result.is_null() ? 0 : result.raw()->w[0];
                value.value2.high_bytes = result.is_null() ? 0 : result.raw()->w[1];
                break;
            }
            default:
                REALM_UNREACHABLE();
            }
#pragma GCC diagnostic pop
            });
    }

    REALM_EXPORT void object_set_primitive(const Object& object, ColKey column_key, PrimitiveValue& value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_set(object);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
            switch (value.type) {
            case realm::PropertyType::Bool:
                object.obj().set(column_key, value.value.bool_value);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable:
                object.obj().set(column_key, value.has_value ? util::Optional<bool>(value.value.bool_value) : util::Optional<bool>(none));
                break;
            case realm::PropertyType::Int:
                object.obj().set(column_key, value.value.int_value);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable:
                object.obj().set(column_key, value.has_value ? util::Optional<int64_t>(value.value.int_value) : util::Optional<int64_t>(none));
                break;
            case realm::PropertyType::Float:
                object.obj().set(column_key, value.value.float_value);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable:
                object.obj().set(column_key, value.has_value ? util::Optional<float>(value.value.float_value) : util::Optional<float>(none));
                break;
            case realm::PropertyType::Double:
                object.obj().set(column_key, value.value.double_value);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable:
                object.obj().set(column_key, value.has_value ? util::Optional<double>(value.value.double_value) : util::Optional<double>(none));
                break;
            case realm::PropertyType::Date:
                object.obj().set(column_key, from_ticks(value.value.int_value));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable:
                object.obj().set(column_key, value.has_value ? from_ticks(value.value.int_value) : Timestamp());
                break;
            case realm::PropertyType::Decimal: {
                object.obj().set(column_key, to_decimal(value));
                break;
            }
            case realm::PropertyType::Decimal | realm::PropertyType::Nullable: {
                auto decimal = value.has_value ? to_decimal(value) : Decimal128(null());
                object.obj().set(column_key, decimal);
                break;
            }
            default:
                REALM_UNREACHABLE();
            }
#pragma GCC diagnostic pop
            });
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

    REALM_EXPORT void object_set_string(Object& object, ColKey column_key, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
    {
        Utf16StringAccessor str(value, value_len);
        return object_set<StringData>(object, column_key, str, ex);
    }
    
    REALM_EXPORT void object_set_binary(Object& object, ColKey column_key, char* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return object_set<BinaryData>(object, column_key, BinaryData(value, value_len), ex);
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
