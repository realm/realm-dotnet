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

#pragma once

#include <realm.hpp>
#include <realm/util/utf8.hpp>
#include <object_accessor.hpp>
#include "wrapper_exceptions.hpp"
#include "error_handling.hpp"
#include "timestamp_helpers.hpp"

namespace realm {
namespace binding {

struct PrimitiveValue
{
    realm::PropertyType type;
    bool has_value;
    char padding[6];

    union {
        bool bool_value;
        int64_t int_value;
        float float_value;
        double double_value;
        uint64_t low_bytes;
    } value;

    union {
        uint64_t high_bytes;
        uint32_t object_id_remainder;
    } value2;
};

struct StringValue
{
    const char* value;
};

template <typename T>
struct MarshaledVector
{
    const T* items;
    size_t count;

    MarshaledVector(const std::vector<T>& vector)
        : items(vector.data())
        , count(vector.size())
    {
    }

    MarshaledVector(const std::vector<T>&&) = delete;

    MarshaledVector()
        : items(nullptr)
        , count(0)
    {
    }
};

template<typename Collection>
void collection_get_primitive(Collection& collection, size_t ndx, PrimitiveValue& value, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        const size_t count = collection.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from Collection", ndx, count);

        value.has_value = true;
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wswitch"
        switch (value.type) {
            case realm::PropertyType::Bool:
                value.value.bool_value = collection.template get<bool>(ndx);
                break;
            case realm::PropertyType::Bool | realm::PropertyType::Nullable: {
                auto result = collection.template get<util::Optional<bool>>(ndx);
                value.has_value = !!result;
                value.value.bool_value = result.value_or(false);
                break;
            }
            case realm::PropertyType::Int:
                value.value.int_value = collection.template get<int64_t>(ndx);
                break;
            case realm::PropertyType::Int | realm::PropertyType::Nullable: {
                auto result = collection.template get<util::Optional<int64_t>>(ndx);
                value.has_value = !!result;
                value.value.int_value = result.value_or(0);
                break;
            }
            case realm::PropertyType::Float:
                value.value.float_value = collection.template get<float>(ndx);
                break;
            case realm::PropertyType::Float | realm::PropertyType::Nullable: {
                auto result = collection.template get<util::Optional<float>>(ndx);
                value.has_value = !!result;
                value.value.float_value = result.value_or((float)0);
                break;
            }
            case realm::PropertyType::Double:
                value.value.double_value = collection.template get<double>(ndx);
                break;
            case realm::PropertyType::Double | realm::PropertyType::Nullable: {
                auto result = collection.template get<util::Optional<double>>(ndx);
                value.has_value = !!result;
                value.value.double_value = result.value_or((double)0);
                break;
            }
            case realm::PropertyType::Date:
                value.value.int_value = to_ticks(collection.template get<Timestamp>(ndx));
                break;
            case realm::PropertyType::Date | realm::PropertyType::Nullable: {
                auto result = collection.template get<Timestamp>(ndx);
                value.has_value = !result.is_null();
                value.value.int_value = result.is_null() ? 0 : to_ticks(result);
                break;
            }
            default:
                REALM_UNREACHABLE();
        }
#pragma GCC diagnostic pop
    });
}

template<typename T, typename Collection>
inline T get(Collection& collection, size_t ndx, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        const size_t count = collection.size();
        if (ndx >= count)
            throw IndexOutOfRangeException("Get from RealmList", ndx, count);

        return collection.template get<T>(ndx);
    });
}

class Utf16StringAccessor {
public:
    Utf16StringAccessor(const uint16_t* csbuffer, size_t csbufsize);

    operator realm::StringData() const noexcept
    {
        return realm::StringData(m_data.get(), m_size);
    }

    std::string to_string() const
    {
        return std::string(m_data.get(), m_size);
    }

    operator std::string() const noexcept
    {
        return std::string(m_data.get(), m_size);
    }

    const char* data() const { return m_data.get();  }
    size_t size() const { return m_size;  }

    bool error;
private:
    std::unique_ptr<char[]> m_data;
    std::size_t m_size;
};

size_t stringdata_to_csharpstringbuffer(StringData str, uint16_t * csharpbuffer, size_t bufsize); //note bufsize is _in_16bit_words

template<typename Collection>
size_t collection_get_string(Collection& collection, size_t ndx, uint16_t* value, size_t value_len, bool* is_null, NativeException::Marshallable& ex)
{
    auto result = get<StringData>(collection, ndx, ex);

    if ((*is_null = result.is_null()))
        return 0;

    return stringdata_to_csharpstringbuffer(result, value, value_len);
}

template<typename Collection>
size_t collection_get_binary(Collection& collection, size_t ndx, char* return_buffer, size_t buffer_size, bool* is_null, NativeException::Marshallable& ex)
{
    auto result = get<BinaryData>(collection, ndx, ex);

    if ((*is_null = result.is_null()))
        return 0;

    const size_t data_size = result.size();
    if (data_size <= buffer_size)
        std::copy(result.data(), result.data() + data_size, return_buffer);

    return data_size;
}

} // namespace binding
} // namespace realm
