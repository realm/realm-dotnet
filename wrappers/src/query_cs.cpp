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
#include "marshalling.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include <realm/object-store/shared_realm.hpp>
#include <realm/object-store/schema.hpp>
#include "timestamp_helpers.hpp"
#include <realm/object-store/results.hpp>
#include <realm/object-store/object_accessor.hpp>


using namespace realm;
using namespace realm::binding;

inline ColKey get_key_for_prop(Query& query, SharedRealm& realm, size_t property_index) {
    return realm->schema().find(ObjectStore::object_type_for_table_name(query.get_table()->get_name()))->persisted_properties[property_index].column_key;
}

inline util::Optional<Geospatial> to_geospatial(query_argument geo_value) {
    Geospatial geo_store;
    switch (geo_value.type) {
    case query_argument_type::BOX:
        return from_capi(geo_value.box);
    case query_argument_type::CIRCLE:
        return from_capi(geo_value.circle);
    case query_argument_type::POLYGON:
        return from_capi(geo_value.polygon);
    default:
        return util::none;
    }
}

inline TypeOfValue::Attribute attribute_from(realm_value_type type)
{
    switch (type) {
    case realm_value_type::RLM_TYPE_NULL:
        return TypeOfValue::Attribute::Null;
    case realm_value_type::RLM_TYPE_INT:
        return TypeOfValue::Attribute::Int;
    case realm_value_type::RLM_TYPE_BOOL:
        return TypeOfValue::Attribute::Bool;
    case realm_value_type::RLM_TYPE_STRING:
        return TypeOfValue::Attribute::String;
    case realm_value_type::RLM_TYPE_BINARY:
        return TypeOfValue::Attribute::Binary;
    case realm_value_type::RLM_TYPE_TIMESTAMP:
        return TypeOfValue::Attribute::Timestamp;
    case realm_value_type::RLM_TYPE_FLOAT:
        return TypeOfValue::Attribute::Float;
    case realm_value_type::RLM_TYPE_DOUBLE:
        return TypeOfValue::Attribute::Double;
    case realm_value_type::RLM_TYPE_DECIMAL128:
        return TypeOfValue::Attribute::Decimal128;
    case realm_value_type::RLM_TYPE_OBJECT_ID:
        return TypeOfValue::Attribute::ObjectId;
    case realm_value_type::RLM_TYPE_LINK:
        return TypeOfValue::Attribute::ObjectLink;
    case realm_value_type::RLM_TYPE_UUID:
        return TypeOfValue::Attribute::UUID;
    default:
        REALM_UNREACHABLE();
    }
}

extern "C" {

REALM_EXPORT void query_destroy(Query* query)
{
    delete query;
}

REALM_EXPORT size_t query_count(Query& query, DescriptorOrdering& descriptor, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return query.count(descriptor);
    });
}

REALM_EXPORT void query_not(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        query.Not();
    });
}

REALM_EXPORT void query_group_begin(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        query.group();
    });
}

REALM_EXPORT void query_group_end(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        query.end_group();
    });
}

REALM_EXPORT void query_or(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        query.Or();
    });
}

REALM_EXPORT void query_string_contains(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.contains(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_starts_with(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.begins_with(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_ends_with(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.ends_with(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.equal(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_not_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.not_equal(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_like(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.is_null() || primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.like(get_key_for_prop(query, realm, property_index), from_capi(primitive.string), case_sensitive);
    });
}

REALM_EXPORT void query_string_fts(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        REALM_ASSERT(primitive.type == realm_value_type::RLM_TYPE_STRING);
        query.fulltext(get_key_for_prop(query, realm, property_index), from_capi(primitive.string));
    });
}

REALM_EXPORT void query_primitive_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Comparing null values should be done via query_null_equal. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            query.equal(std::move(col_key), primitive.boolean());
            break;
        case realm_value_type::RLM_TYPE_INT:
            query.equal(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.equal(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.equal(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.equal(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.equal(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.equal(std::move(col_key), from_capi(primitive.object_id));
            break;
        case realm_value_type::RLM_TYPE_UUID:
            query.equal(std::move(col_key), from_capi(primitive.uuid));
            break;        
        case realm_value_type::RLM_TYPE_BINARY:
            query.equal(std::move(col_key), from_capi(primitive.binary));
            break;
        case realm_value_type::RLM_TYPE_STRING:
            query.equal(std::move(col_key), from_capi(primitive.string));
            break;
        case realm_value_type::RLM_TYPE_LINK:
            query.equal(std::move(col_key), from_capi(primitive));
            break;
        }
    });
}

REALM_EXPORT void query_primitive_not_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Comparing null values should be done via query_null_equal. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            query.not_equal(std::move(col_key), primitive.boolean());
            break;
        case realm_value_type::RLM_TYPE_INT:
            query.not_equal(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.not_equal(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.not_equal(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.not_equal(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.not_equal(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.not_equal(std::move(col_key), from_capi(primitive.object_id));
            break;
        case realm_value_type::RLM_TYPE_UUID:
            query.not_equal(std::move(col_key), from_capi(primitive.uuid));
            break;        
        case realm_value_type::RLM_TYPE_BINARY:
            query.not_equal(std::move(col_key), from_capi(primitive.binary));
            break;
        case realm_value_type::RLM_TYPE_STRING:
            query.not_equal(std::move(col_key), from_capi(primitive.string));
            break;
        case realm_value_type::RLM_TYPE_LINK:
            query.not_equal(std::move(col_key), from_capi(primitive));
            break;
        }
    });
}

REALM_EXPORT void query_primitive_less(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Using primitive_less with null is not supported. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            throw std::runtime_error("Using primitive_less with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm_value_type::RLM_TYPE_INT:
            query.less(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.less(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.less(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.less(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.less(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.less(std::move(col_key), from_capi(primitive.object_id));
            break;
        default:
            REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void query_primitive_less_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Using primitive_less_equal with null is not supported. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            throw std::runtime_error("Using primitive_less_equal with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm_value_type::RLM_TYPE_INT:
            query.less_equal(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.less_equal(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.less_equal(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.less_equal(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.less_equal(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.less_equal(std::move(col_key), from_capi(primitive.object_id));
            break;
        default:
            REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void query_primitive_greater(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Using primitive_greater with null is not supported. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            throw std::runtime_error("Using primitive_greater with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm_value_type::RLM_TYPE_INT:
            query.greater(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.greater(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.greater(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.greater(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.greater(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.greater(std::move(col_key), from_capi(primitive.object_id));
            break;
        default:
            REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void query_primitive_greater_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_t primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        switch (primitive.type) {
        case realm_value_type::RLM_TYPE_NULL:
            throw std::runtime_error("Using primitive_greater_equal with null is not supported. If you get this error, please report it to help@realm.io.");
        case realm_value_type::RLM_TYPE_BOOL:
            throw std::runtime_error("Using primitive_greater_equal with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm_value_type::RLM_TYPE_INT:
            query.greater_equal(std::move(col_key), primitive.integer);
            break;
        case realm_value_type::RLM_TYPE_FLOAT:
            query.greater_equal(std::move(col_key), primitive.fnum);
            break;
        case realm_value_type::RLM_TYPE_DOUBLE:
            query.greater_equal(std::move(col_key), primitive.dnum);
            break;
        case realm_value_type::RLM_TYPE_TIMESTAMP:
            query.greater_equal(std::move(col_key), from_capi(primitive.timestamp));
            break;
        case realm_value_type::RLM_TYPE_DECIMAL128:
            query.greater_equal(std::move(col_key), from_capi(primitive.decimal128));
            break;
        case realm_value_type::RLM_TYPE_OBJECT_ID:
            query.greater_equal(std::move(col_key), from_capi(primitive.object_id));
            break;
        default:
            REALM_UNREACHABLE();
        }
    });
}

REALM_EXPORT void query_null_equal(Query& query, SharedRealm& realm, size_t property_index, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        if (query.get_table()->get_column_type(col_key) == type_Link) {
            query.and_query(query.get_table()->column<Link>(col_key).is_null());
        }
        else {
            query.equal(col_key, null());
        }
    });
}

REALM_EXPORT void query_null_not_equal(Query& query, SharedRealm& realm, size_t property_index, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        if (query.get_table()->get_column_type(col_key) == type_Link) {
            query.and_query(query.get_table()->column<Link>(col_key).is_not_null());
        }
        else {
            query.not_equal(col_key, null());
        }
    });
}

REALM_EXPORT Results* query_create_results(Query& query, SharedRealm& realm, DescriptorOrdering& descriptor, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(realm, query, descriptor);
    });
}

REALM_EXPORT void query_realm_value_type_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_type realm_value_type, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        query.and_query(query.get_table()->column<Mixed>(col_key).type_of_value() == TypeOfValue(attribute_from(realm_value_type)));
    });
}

REALM_EXPORT void query_realm_value_type_not_equal(Query& query, SharedRealm& realm, size_t property_index, realm_value_type realm_value_type, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto col_key = get_key_for_prop(query, realm, property_index);
        query.and_query(query.get_table()->column<Mixed>(col_key).type_of_value() != TypeOfValue(attribute_from(realm_value_type)));  //Need to check if correct
    });
}

REALM_EXPORT void query_geowithin(Query& query, SharedRealm& realm, size_t property_index, query_argument geo_value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!query.get_table()) {
            return;
        }

        auto geo_store = to_geospatial(geo_value);
        if (!geo_store) {
            REALM_UNREACHABLE();
        }

        query.and_query(query.get_table()->column<Link>(get_key_for_prop(query, realm, property_index)).geo_within(geo_store.value()));
    });
}

REALM_EXPORT void validate_query_argument(query_argument geo_value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        auto geo_store = to_geospatial(geo_value);
        if (geo_store) {
            auto status = geo_store.value().is_valid();
            if (!status.is_ok()) {
                throw GeoSpatialShapeValidationException(status.reason());
            }
        }
    });
}
}   // extern "C"
