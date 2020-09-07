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
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"
#include "timestamp_helpers.hpp"
#include "object-store/src/results.hpp"
#include "object_accessor.hpp"


using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void query_destroy(Query* query)
{
    delete query;
}

REALM_EXPORT size_t query_count(Query& query, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return query.count();
    });
}

REALM_EXPORT void query_not(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.Not();
    });
}

REALM_EXPORT void query_group_begin(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.group();
    });
}

REALM_EXPORT void query_group_end(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.end_group();
    });
}

REALM_EXPORT void query_or(Query& query, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.Or();
    });
}

REALM_EXPORT void query_string_contains(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.contains(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_string_starts_with(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.begins_with(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_string_ends_with(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.ends_with(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_string_equal(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.equal(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_string_not_equal(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.not_equal(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_string_like(Query& query, ColKey column_key, uint16_t* value, size_t value_len, bool case_sensitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor str(value, value_len);
        query.like(column_key, str, case_sensitive);
    });
}

REALM_EXPORT void query_primitive_equal(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Comparing null values should be done via query_null_equal. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            query.equal(column_key, primitive.value.bool_value);
            break;
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.equal(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.equal(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.equal(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.equal(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.equal(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_primitive_not_equal(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Comparing null values should be done via query_null_not_equal. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            query.not_equal(column_key, primitive.value.bool_value);
            break;
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.not_equal(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.not_equal(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.not_equal(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.not_equal(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.not_equal(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_primitive_less(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Using primitive_less with null is not supported. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            throw std::runtime_error("Using primitive_less with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.less(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.less(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.less(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.less(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.less(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_primitive_less_equal(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Using primitive_less_equal with null is not supported. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            throw std::runtime_error("Using primitive_less_equal with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.less_equal(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.less_equal(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.less_equal(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.less_equal(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.less_equal(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_primitive_greater(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Using primitive_greater with null is not supported. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            throw std::runtime_error("Using primitive_greater with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.greater(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.greater(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.greater(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.greater(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.greater(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_primitive_greater_equal(Query& query, ColKey column_key, PrimitiveValue& primitive, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (!primitive.has_value) {
            throw std::runtime_error("Using primitive_greater_equal with null is not supported. If you get this error, please report it to help@realm.io.");
        }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (primitive.type) {
        case realm::PropertyType::Bool:
        case realm::PropertyType::Bool | realm::PropertyType::Nullable:
            throw std::runtime_error("Using primitive_greater_equal with bool value is not supported. If you get this error, please report it to help@realm.io");
        case realm::PropertyType::Int:
        case realm::PropertyType::Int | realm::PropertyType::Nullable:
            query.greater_equal(column_key, primitive.value.int_value);
            break;
        case realm::PropertyType::Float:
        case realm::PropertyType::Float | realm::PropertyType::Nullable:
            query.greater_equal(column_key, primitive.value.float_value);
            break;
        case realm::PropertyType::Double:
        case realm::PropertyType::Double | realm::PropertyType::Nullable:
            query.greater_equal(column_key, primitive.value.double_value);
            break;
        case realm::PropertyType::Date:
        case realm::PropertyType::Date | realm::PropertyType::Nullable:
            query.greater_equal(column_key, from_ticks(primitive.value.int_value));
            break;
        case realm::PropertyType::Decimal:
        case realm::PropertyType::Decimal | realm::PropertyType::Nullable:
            query.greater_equal(column_key, to_decimal(primitive));
            break;
        default:
            REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

REALM_EXPORT void query_binary_equal(Query& query, ColKey column_key, char* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, BinaryData(buffer, buffer_length));
    });
}

REALM_EXPORT void query_binary_not_equal(Query& query, ColKey column_key, char* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, BinaryData(buffer, buffer_length));
    });
}

REALM_EXPORT void query_object_equal(Query& query, ColKey column_key, Object& object, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.links_to(column_key, object.obj().get_key());
    });
}

REALM_EXPORT void query_null_equal(Query& query, ColKey column_key, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (query.get_table()->get_column_type(column_key) == DataType::type_Link) {
            query.and_query(query.get_table()->column<Link>(column_key).is_null());
        }
        else {
            query.equal(column_key, null());
        }
    });
}

REALM_EXPORT void query_null_not_equal(Query& query, ColKey column_key, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (query.get_table()->get_column_type(column_key) == DataType::type_Link) {
            query.and_query(query.get_table()->column<Link>(column_key).is_not_null());
        }
        else {
            query.not_equal(column_key, null());
        }
    });
}

REALM_EXPORT Results* query_create_results(Query& query, SharedRealm& realm, DescriptorOrdering& descriptor, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(realm, query, descriptor);
    });
}

}   // extern "C"
