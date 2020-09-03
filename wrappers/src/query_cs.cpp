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

REALM_EXPORT void query_bool_equal(Query& query, ColKey column_key, bool value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, value);
    });
}

REALM_EXPORT void query_bool_not_equal(Query& query, ColKey column_key, bool value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, value);
    });
}

REALM_EXPORT void query_int_equal(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_not_equal(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_less(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_less_equal(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less_equal(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_greater(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_int_greater_equal(Query& query, ColKey column_key, size_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater_equal(column_key, static_cast<int>(value));
    });
}

REALM_EXPORT void query_long_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, value);
    });
}

REALM_EXPORT void query_long_not_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, value);
    });
}

REALM_EXPORT void query_long_less(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less(column_key, value);
    });
}

REALM_EXPORT void query_long_less_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less_equal(column_key, value);
    });
}

REALM_EXPORT void query_long_greater(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater(column_key, value);
    });
}

REALM_EXPORT void query_long_greater_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater_equal(column_key, value);
    });
}

    REALM_EXPORT void query_float_equal(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_not_equal(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_less(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_less_equal(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less_equal(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_greater(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_float_greater_equal(Query& query, ColKey column_key, float value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater_equal(column_key, static_cast<float>(value));
    });
}

REALM_EXPORT void query_double_equal(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_not_equal(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_less(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_less_equal(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less_equal(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_greater(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_double_greater_equal(Query& query, ColKey column_key, double value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater_equal(column_key, static_cast<double>(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.equal(column_key, from_ticks(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_not_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.not_equal(column_key, from_ticks(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_less(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less(column_key, from_ticks(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_less_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.less_equal(column_key, from_ticks(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_greater(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater(column_key, from_ticks(value));
    });
}

REALM_EXPORT void query_timestamp_ticks_greater_equal(Query& query, ColKey column_key, int64_t value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        query.greater_equal(column_key, from_ticks(value));
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
