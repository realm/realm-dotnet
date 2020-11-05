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
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>

using namespace realm;
using namespace realm::binding;

template <typename T>
inline T object_get(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        verify_can_get(object);

        return object.obj().get<T>(get_column_key(object, property_ndx));
    });
}

template <typename T>
inline void object_set(Object& object, size_t property_ndx, const T& value, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        verify_can_set(object);

        object.obj().set<T>(get_column_key(object, property_ndx), value);
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

    REALM_EXPORT Object* object_get_link(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> Object* {
            verify_can_get(object);

            const Obj link_obj = object.obj().get_linked_object(get_column_key(object, property_ndx));
            if (!link_obj)
                return nullptr;

            const std::string target_name(ObjectStore::object_type_for_table_name(link_obj.get_table()->get_name()));
            auto& target_schema = *object.realm()->schema().find(target_name);
            return new Object(object.realm(), target_schema, link_obj);
        });
    }

    REALM_EXPORT List* object_get_list(const Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> List* {
            verify_can_get(object);

            return new List(object.realm(), object.obj(), get_column_key(object, property_ndx));
        });
    }

    REALM_EXPORT void object_get_primitive(const Object& object, size_t property_ndx, PrimitiveValue& value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_get(object);

            value.has_value = true;
            auto column_key = get_column_key(object, property_ndx);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
            switch (value.type) {
            case realm::PropertyType::Bool:
                value.value.bool_value = object.obj().get<bool>(std::move(column_key));
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<bool>>(std::move(column_key));
                value.has_value = !!result;
                value.value.bool_value = result.value_or(false);
                break;
            }
            case realm::PropertyType::Int:
                value.value.int_value = object.obj().get<int64_t>(std::move(column_key));
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<int64_t>>(std::move(column_key));
                value.has_value = !!result;
                value.value.int_value = result.value_or(0);
                break;
            }
            case realm::PropertyType::Float:
                value.value.float_value = object.obj().get<float>(std::move(column_key));
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<float>>(std::move(column_key));
                value.has_value = !!result;
                value.value.float_value = result.value_or((float)0);
                break;
            }
            case realm::PropertyType::Double:
                value.value.double_value = object.obj().get<double>(std::move(column_key));
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<double>>(std::move(column_key));
                value.has_value = !!result;
                value.value.double_value = result.value_or((double)0);
                break;
            }
            case realm::PropertyType::Date:
                value.value.int_value = to_ticks(object.obj().get<Timestamp>(std::move(column_key)));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable: {
                auto result = object.obj().get<Timestamp>(std::move(column_key));
                value.has_value = !result.is_null();
                value.value.int_value = result.is_null() ? 0 : to_ticks(result);
                break;
            }
            case realm::PropertyType::Decimal: {
                auto result = object.obj().get<Decimal128>(std::move(column_key));
                value.value.decimal_bits = *result.raw();
                break;
            }
            case realm::PropertyType::Decimal | realm::PropertyType::Nullable: {
                auto result = object.obj().get<Decimal128>(std::move(column_key));
                value.has_value = !result.is_null();
                if (value.has_value) {
                    value.value.decimal_bits = *result.raw();
                }
                break;
            }
            case realm::PropertyType::ObjectId: {
                auto result = object.obj().get<ObjectId>(std::move(column_key));
                auto bytes = result.to_bytes();
                for (int i = 0; i < 12; i++)
                {
                    value.value.object_id_bytes[i] = bytes[i];
                }
                break;
            }
            case realm::PropertyType::ObjectId | realm::PropertyType::Nullable: {
                auto result = object.obj().get<util::Optional<ObjectId>>(std::move(column_key));
                value.has_value = !!result;
                if (value.has_value) {
                    auto bytes = result.value().to_bytes();
                    for (int i = 0; i < 12; i++)
                    {
                        value.value.object_id_bytes[i] = bytes[i];
                    }
                }
                break;
            }
            default:
                REALM_UNREACHABLE();
            }
#pragma GCC diagnostic pop
        });
    }

    REALM_EXPORT void object_set_primitive(const Object& object, size_t property_ndx, PrimitiveValue& value, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            verify_can_set(object);

            auto column_key = get_column_key(object, property_ndx);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
            switch (value.type) {
            case realm::PropertyType::Bool:
                object.obj().set(std::move(column_key), value.value.bool_value);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable:
                object.obj().set(std::move(column_key), value.has_value ? util::Optional<bool>(value.value.bool_value) : util::Optional<bool>(none));
                break;
            case realm::PropertyType::Int:
                object.obj().set(std::move(column_key), value.value.int_value);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable:
                object.obj().set(std::move(column_key), value.has_value ? util::Optional<int64_t>(value.value.int_value) : util::Optional<int64_t>(none));
                break;
            case realm::PropertyType::Float:
                object.obj().set(std::move(column_key), value.value.float_value);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable:
                object.obj().set(std::move(column_key), value.has_value ? util::Optional<float>(value.value.float_value) : util::Optional<float>(none));
                break;
            case realm::PropertyType::Double:
                object.obj().set(std::move(column_key), value.value.double_value);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable:
                object.obj().set(std::move(column_key), value.has_value ? util::Optional<double>(value.value.double_value) : util::Optional<double>(none));
                break;
            case realm::PropertyType::Date:
                object.obj().set(std::move(column_key), from_ticks(value.value.int_value));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable:
                object.obj().set(std::move(column_key), value.has_value ? from_ticks(value.value.int_value) : Timestamp());
                break;
            case realm::PropertyType::Decimal: {
                object.obj().set(std::move(column_key), realm::Decimal128(value.value.decimal_bits));
                break;
            }
            case realm::PropertyType::Decimal | realm::PropertyType::Nullable: {
                auto decimal = value.has_value ? realm::Decimal128(value.value.decimal_bits) : Decimal128(null());
                object.obj().set(std::move(column_key), decimal);
                break;
            }
            case realm::PropertyType::ObjectId: {
                object.obj().set(std::move(column_key), to_object_id(value));
                break;
            }
            case realm::PropertyType::ObjectId | realm::PropertyType::Nullable: {
                object.obj().set(std::move(column_key), value.has_value ? util::Optional<ObjectId>(to_object_id(value)) : util::Optional<ObjectId>());
                break;
            }
            default:
                REALM_UNREACHABLE();
            }
#pragma GCC diagnostic pop
            });
    }

    REALM_EXPORT size_t object_get_string(const Object& object, size_t property_ndx, uint16_t* string_buffer, size_t buffer_size, bool& is_null, NativeException::Marshallable& ex)
    {
        StringData field_data = object_get<StringData>(object, property_ndx, ex);
        if (ex.type != RealmErrorType::NoError) {
            return -1;
        }

        if ((is_null = field_data.is_null())) {
            return 0;
        }

        return stringdata_to_csharpstringbuffer(field_data, string_buffer, buffer_size);
    }

    REALM_EXPORT size_t object_get_binary(const Object& object, size_t property_ndx, char* return_buffer, size_t buffer_size, bool& is_null, NativeException::Marshallable& ex)
    {
        BinaryData field_data = object_get<BinaryData>(object, property_ndx, ex);
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

    REALM_EXPORT Results* object_get_backlinks_for_type(Object& object, TableRef& source_table, size_t source_property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            verify_can_get(object);

            const ObjectSchema& source_object_schema = *object.realm()->schema().find(ObjectStore::object_type_for_table_name(source_table->get_name()));
            const Property& source_property = source_object_schema.persisted_properties[source_property_ndx];

            if (source_property.object_type != object.get_object_schema().name) {
                throw std::logic_error(util::format("'%1.%2' is not a relationship to '%3'", source_object_schema.name, source_property.name, object.get_object_schema().name));
            }

            TableView backlink_view = object.obj().get_backlink_view(source_table, source_property.column_key);
            return new Results(object.realm(), std::move(backlink_view));
        });
    }

    REALM_EXPORT void object_set_link(Object& object, size_t property_ndx, const Object& target_object, NativeException::Marshallable& ex)
    {
        return object_set<ObjKey>(object, property_ndx, target_object.obj().get_key(), ex);
    }

    REALM_EXPORT Object* object_create_embedded(Object& parent, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(parent);

            return new Object(parent.realm(), parent.obj().create_and_set_linked_object(get_column_key(parent, property_ndx)));
        });
    }

    REALM_EXPORT void object_clear_link(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return object_set<ObjKey>(object, property_ndx, null_key, ex);
    }

    REALM_EXPORT void object_set_null(Object& object, size_t property_ndx, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);

            auto column_key = get_column_key(object, property_ndx);
            if (!object.obj().get_table()->is_nullable(column_key))
                throw std::invalid_argument("Column is not nullable");

            object.obj().set_null(column_key);
        });
    }

    REALM_EXPORT void object_set_string(Object& object, size_t property_ndx, uint16_t* value, size_t value_len, NativeException::Marshallable& ex)
    {
        Utf16StringAccessor str(value, value_len);
        return object_set<StringData>(object, property_ndx, str, ex);
    }

    REALM_EXPORT void object_set_binary(Object& object, size_t property_ndx, char* value, size_t value_len, NativeException::Marshallable& ex)
    {
        return object_set<BinaryData>(object, property_ndx, BinaryData(value, value_len), ex);
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

    REALM_EXPORT void object_add_int64(Object& object, size_t property_ndx, int64_t value, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            verify_can_set(object);

            object.obj().add_int(get_column_key(object, property_ndx), value);
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
