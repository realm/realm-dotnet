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
#include <realm/object-store/object_accessor.hpp>
#include "wrapper_exceptions.hpp"
#include "error_handling.hpp"
#include "timestamp_helpers.hpp"

namespace realm {
namespace binding {

typedef enum realm_value_type {
    RLM_TYPE_NULL,
    RLM_TYPE_INT,
    RLM_TYPE_BOOL,
    RLM_TYPE_STRING,
    RLM_TYPE_BINARY,
    RLM_TYPE_TIMESTAMP,
    RLM_TYPE_FLOAT,
    RLM_TYPE_DOUBLE,
    RLM_TYPE_DECIMAL128,
    RLM_TYPE_OBJECT_ID,
    RLM_TYPE_LINK,
    RLM_TYPE_UUID,
} realm_value_type_e;

typedef struct realm_string {
    const char* data;
    size_t size;
} realm_string_t;

typedef struct realm_binary {
    const uint8_t* data;
    size_t size;
} realm_binary_t;

typedef struct realm_timestamp {
    int64_t seconds;
    int32_t nanoseconds;
} realm_timestamp_t;

typedef struct realm_decimal128 {
    uint64_t w[2];
} realm_decimal128_t;

typedef struct realm_link {
    Object* object;
} realm_link_t;

typedef struct realm_object_id {
    uint8_t bytes[12];
} realm_object_id_t;

typedef struct realm_uuid {
    uint8_t bytes[16];
} realm_uuid_t;

typedef struct realm_value {
    union {
        int64_t integer;
        bool boolean;
        realm_string_t string;
        realm_binary_t binary;
        realm_timestamp_t timestamp;
        float fnum;
        double dnum;
        realm_decimal128_t decimal128;
        realm_object_id_t object_id;
        realm_uuid_t uuid;

        realm_link_t link;

        char data[16];
    };
    realm_value_type_e type;
} realm_value_t;

static inline realm_string_t to_capi(StringData data)
{
    return realm_string_t{ data.data(), data.size() };
}

static inline realm_string_t to_capi(const std::string& str)
{
    return to_capi(StringData{ str });
}

static inline std::string capi_to_std(realm_string_t str)
{
    if (str.data) {
        return std::string{ str.data, 0, str.size };
    }
    return std::string{};
}

static inline StringData from_capi(realm_string_t str)
{
    return StringData{ str.data, str.size };
}

static inline realm_binary_t to_capi(BinaryData bin)
{
    return realm_binary_t{ reinterpret_cast<const unsigned char*>(bin.data()), bin.size() };
}

static inline BinaryData from_capi(realm_binary_t bin)
{
    return BinaryData{ reinterpret_cast<const char*>(bin.data), bin.size };
}

static inline realm_timestamp_t to_capi(Timestamp ts)
{
    return realm_timestamp_t{ ts.get_seconds(), ts.get_nanoseconds() };
}

static inline Timestamp from_capi(realm_timestamp_t ts)
{
    return Timestamp{ ts.seconds, ts.nanoseconds };
}

static inline realm_decimal128_t to_capi(const Decimal128& dec)
{
    auto raw = dec.raw();
    return realm_decimal128_t{ {raw->w[0], raw->w[1]} };
}

static inline Decimal128 from_capi(realm_decimal128_t dec)
{
    return Decimal128{ Decimal128::Bid128{{dec.w[0], dec.w[1]}} };
}

static inline realm_object_id_t to_capi(ObjectId oid)
{
    auto bytes = oid.to_bytes();
    realm_object_id_t result;
    for (int i = 0; i < 12; i++)
    {
        result.bytes[i] = bytes[i];
    }

    return result;
}

static inline ObjectId from_capi(realm_object_id_t oid)
{
    std::array<uint8_t, 12> bytes;
    std::copy(std::begin(oid.bytes), std::end(oid.bytes), bytes.begin());
    return ObjectId(std::move(bytes));
}

static inline Mixed from_capi(realm_value_t val)
{
    switch (val.type) {
    case RLM_TYPE_NULL:
        return Mixed{};
    case RLM_TYPE_INT:
        return Mixed{ val.integer };
    case RLM_TYPE_BOOL:
        return Mixed{ val.boolean };
    case RLM_TYPE_STRING:
        return Mixed{ from_capi(val.string) };
    case RLM_TYPE_BINARY:
        return Mixed{ from_capi(val.binary) };
    case RLM_TYPE_TIMESTAMP:
        return Mixed{ from_capi(val.timestamp) };
    case RLM_TYPE_FLOAT:
        return Mixed{ val.fnum };
    case RLM_TYPE_DOUBLE:
        return Mixed{ val.dnum };
    case RLM_TYPE_DECIMAL128:
        return Mixed{ from_capi(val.decimal128) };
    case RLM_TYPE_OBJECT_ID:
        return Mixed{ from_capi(val.object_id) };
    case RLM_TYPE_LINK:
        return Mixed{ ObjLink{val.link.object->obj().get_table()->get_key(), val.link.object->obj().get_key()} };
    }
    REALM_TERMINATE("Invalid realm_value_t");
}

static inline realm_value_t to_capi(Mixed value)
{
    realm_value_t val;
    if (value.is_null()) {
        val.type = RLM_TYPE_NULL;
    }
    else {
        switch (value.get_type()) {
        case type_Int: {
            val.type = RLM_TYPE_INT;
            val.integer = value.get<int64_t>();
            break;
        }
        case type_Bool: {
            val.type = RLM_TYPE_BOOL;
            val.boolean = value.get<bool>();
            break;
        }
        case type_String: {
            val.type = RLM_TYPE_STRING;
            val.string = to_capi(value.get<StringData>());
            break;
        }
        case type_Binary: {
            val.type = RLM_TYPE_BINARY;
            val.binary = to_capi(value.get<BinaryData>());
            break;
        }
        case type_Timestamp: {
            val.type = RLM_TYPE_TIMESTAMP;
            val.timestamp = to_capi(value.get<Timestamp>());
            break;
        }
        case type_Float: {
            val.type = RLM_TYPE_FLOAT;
            val.fnum = value.get<float>();
            break;
        }
        case type_Double: {
            val.type = RLM_TYPE_DOUBLE;
            val.dnum = value.get<double>();
            break;
        }
        case type_Decimal: {
            val.type = RLM_TYPE_DECIMAL128;
            val.decimal128 = to_capi(value.get<Decimal128>());
            break;
        }
        case type_Link: {
            REALM_TERMINATE("Not implemented yet");
        }
        case type_ObjectId: {
            val.type = RLM_TYPE_OBJECT_ID;
            val.object_id = to_capi(value.get<ObjectId>());
            break;
        }
        case type_TypedLink: {
            val.type = RLM_TYPE_LINK;
            auto link = value.get<ObjLink>();

            REALM_TERMINATE("Implement me!!");
            // BLAH!
            // val.link.target_table = to_capi(link.get_table_key());
            // val.link.target = to_capi(link.get_obj_key());
            break;
        }

        case type_LinkList:
            [[fallthrough]];
        case type_Mixed:
            [[fallthrough]];
        case type_OldTable:
            [[fallthrough]];
        case type_OldDateTime:
            [[fallthrough]];
        default:
            REALM_TERMINATE("Invalid Mixed value type");
        }
    }

    return val;
}

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
