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
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/binding_context.hpp>

#include "error_handling.hpp"
#include "timestamp_helpers.hpp"
#include "utf8.hpp"

namespace realm::binding {

/// A struct used when marshaling of `MarshaledVector` cannot be
/// compiled, e.g. for MSVC when returning a `MarshaledVector` from
/// CPP directly, as compared to when nested within another struct.
struct TypeErasedMarshaledVector
{
    const void* items;
    size_t count;

    template <typename T>
    static TypeErasedMarshaledVector for_marshalling(const std::vector<T>& vector) {
        return {vector.data(), vector.size()};
    }
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

    size_t size() const noexcept { return count; }

    struct iterator {
    public:
        using iterator_category = std::random_access_iterator_tag;
        using difference_type = ptrdiff_t;
        using value_type = T;
        using pointer = const T*;
        using reference = const T&;

        reference operator*() const noexcept { return *m_ptr; }
        pointer operator->() const noexcept { return m_ptr; }

        iterator& operator++() noexcept { m_ptr++; return *this; }

        iterator operator++(int)
        {
            iterator tmp = *this;
            ++(*this);
            return tmp;    
        }

        friend bool operator== (const iterator& a, const iterator& b) { return a.m_ptr == b.m_ptr; };
        friend bool operator!= (const iterator& a, const iterator& b) { return a.m_ptr != b.m_ptr; }; 

    private:
        friend struct MarshaledVector<T>;

        iterator(pointer ptr)
        : m_ptr(ptr)
        {
        }

        pointer m_ptr;
    };

    iterator begin() const noexcept { return {items}; }
    iterator end() const noexcept { return {items + count}; }
};

enum class realm_value_type : uint8_t {
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
    RLM_TYPE_LIST,
    RLM_TYPE_SET,
    RLM_TYPE_DICTIONARY,
};

enum class key_path_collection_type : uint8_t {
    DEFAULT,
    SHALLOW,
    FULL
};

enum class query_argument_type : uint8_t {
    PRIMITIVE,
    BOX,
    POLYGON,
    CIRCLE,
};

typedef struct realm_string {
    const char* data;
    size_t size;
} realm_string_t;

typedef struct realm_string_collection {
    const realm_string_t* data;
    size_t size;
} realm_string_collection_t;

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
    TableKey table_key;
} realm_link_t;

typedef struct realm_object_id {
    uint8_t bytes[12];
} realm_object_id_t;

typedef struct realm_uuid {
    uint8_t bytes[16];
} realm_uuid_t;

// This struct is used to marshall C#'s type PrimitiveValue
typedef struct realm_value {
    union {
        int64_t integer;
        realm_string_t string;
        realm_binary_t binary;
        realm_timestamp_t timestamp;
        float fnum;
        double dnum;
        realm_decimal128_t decimal128;
        realm_object_id_t object_id;
        realm_uuid_t uuid;

        realm_link_t link;
        object_store::Collection* collection;

        char data[16];
    };
    realm_value_type type;

    bool is_null() const {
        return type == realm_value_type::RLM_TYPE_NULL;
    }

    bool boolean() const {
        return integer == 1;
    }
} realm_value_t;

struct geo_point {
    double latitude;
    double longitude;
};

struct geo_box {
    double left;
    double top;
    double right;
    double bottom;
};

struct geo_circle {
    geo_point center;
    double radius_radians;
};

struct geo_polygon {
    geo_point* points;
    size_t* rings_lengths;
    size_t num_rings;
};

struct query_argument {
    union {
        realm_value_t primitive;
        geo_box box;
        geo_circle circle;
        geo_polygon polygon;
    };

    query_argument_type type;
};

typedef struct realm_sync_error_compensating_write_info {
    realm_string_t reason;
    realm_string_t object_name;
    realm_value_t primary_key;
} realm_sync_error_compensating_write_info_t;

struct realm_sync_error {
    int32_t error_code;
    realm_string_t message;
    realm_string_t log_url;
    bool is_client_reset;

    MarshaledVector<std::pair<realm_string_t, realm_string_t>> user_info_pairs;
    MarshaledVector<realm_sync_error_compensating_write_info_t> compensating_writes;
};

static inline realm_string_t to_capi(StringData data)
{
    return realm_string_t{ data.data(), data.size() };
}

static inline realm_string_t to_capi(std::string_view data)
{
    return realm_string_t{ data.data(), data.size() };
}

static inline realm_string_t to_capi(const std::string& str)
{
    return realm_string_t{ str.data(), str.length() };
}

static inline realm_value_t to_capi_value(const std::string& str)
{
    realm_value_t val{};
    val.string = to_capi(str);
    val.type = realm_value_type::RLM_TYPE_STRING;
    return val;
}

static inline realm_value_t to_capi_value(const StringData& str)
{
    realm_value_t val{};
    if (str.is_null()) {
        val.type = realm_value_type::RLM_TYPE_NULL;
    }
    else {
        val.string = to_capi(str);
        val.type = realm_value_type::RLM_TYPE_STRING;
    }
    return val;
}

static inline std::string capi_to_std(const realm_string_t& str)
{
    if (str.data) {
        return std::string{ str.data, 0, str.size };
    }
    return std::string{};
}

static inline StringData from_capi(const realm_string_t& str)
{
    return StringData{ str.data, str.size };
}

static inline realm_binary_t to_capi(const BinaryData& bin)
{
    return realm_binary_t{ reinterpret_cast<const unsigned char*>(bin.data()), bin.size() };
}

static inline BinaryData from_capi(const realm_binary_t& bin)
{
    return BinaryData{ reinterpret_cast<const char*>(bin.data), bin.size };
}

static inline realm_timestamp_t to_capi(const Timestamp& ts)
{
    return realm_timestamp_t{ ts.get_seconds(), ts.get_nanoseconds() };
}

static inline realm_value_t to_capi_value(const Timestamp& ts)
{
    realm_value_t val{};
    val.timestamp = to_capi(ts);
    val.type = realm_value_type::RLM_TYPE_TIMESTAMP;
    return val;
}

static inline Timestamp from_capi(const realm_timestamp_t& ts)
{
    return Timestamp{ ts.seconds, ts.nanoseconds };
}

static inline realm_decimal128_t to_capi(const Decimal128& dec)
{
    auto raw = dec.raw();
    return realm_decimal128_t{ {raw->w[0], raw->w[1]} };
}

static inline Decimal128 from_capi(const realm_decimal128_t& dec)
{
    return Decimal128{ Decimal128::Bid128{{dec.w[0], dec.w[1]}} };
}

static inline GeoPoint from_capi(const geo_point& point)
{
    return GeoPoint{ point.longitude, point.latitude };
}

static inline GeoBox from_capi(const geo_box& box)
{
    return GeoBox{ GeoPoint{box.left, box.bottom}, GeoPoint{box.right, box.top} };
}

static inline GeoCircle from_capi(const geo_circle& circle)
{
    return GeoCircle{ circle.radius_radians, from_capi(circle.center) };
}

static inline GeoPolygon from_capi(const geo_polygon& polygon)
{
    std::vector<std::vector<GeoPoint>> rings;
    rings.reserve(polygon.num_rings);

    int points_index = 0;
    for (int i = 0; i < polygon.num_rings; i++) {
        std::vector<GeoPoint> points;
        int points_len = (int)polygon.rings_lengths[i];
        points.reserve(points_len);

        for (int j = 0; j < points_len; j++) {
            points.push_back(from_capi(polygon.points[points_index++]));
        }

        rings.push_back(points);
    }

    return GeoPolygon{ rings };
}

static inline realm_object_id_t to_capi(const ObjectId& oid)
{
    const auto& bytes = oid.to_bytes();
    realm_object_id_t result = {};
    for (int i = 0; i < 12; i++)
    {
        result.bytes[i] = bytes[i];
    }

    return result;
}

static inline realm_value_t to_capi_value(const ObjectId& oid)
{
    realm_value_t val{};
    val.object_id = to_capi(oid);
    val.type = realm_value_type::RLM_TYPE_OBJECT_ID;
    return val;
}

static inline ObjectId from_capi(const realm_object_id_t& oid)
{
    std::array<uint8_t, 12> bytes = {};
    std::copy(std::begin(oid.bytes), std::end(oid.bytes), bytes.begin());
    return ObjectId(std::move(bytes));
}

static inline realm_uuid_t to_capi(const UUID& uuid)
{
    const auto& bytes = uuid.to_bytes();
    realm_uuid_t result{};
    for (int i = 0; i < 16; i++)
    {
        result.bytes[i] = bytes[i];
    }

    return result;
}

static inline UUID from_capi(const realm_uuid_t& uuid)
{
    std::array<uint8_t, 16> bytes = {};
    std::copy(std::begin(uuid.bytes), std::end(uuid.bytes), bytes.begin());
    return UUID(std::move(bytes));
}

static inline realm_value_type to_capi(PropertyType type)
{
    switch (type & ~PropertyType::Flags)
    {
    case PropertyType::Int:
        return realm_value_type::RLM_TYPE_INT;
    case PropertyType::Bool:
        return realm_value_type::RLM_TYPE_BOOL;
    case PropertyType::String:
        return realm_value_type::RLM_TYPE_STRING;
    case PropertyType::Data:
        return realm_value_type::RLM_TYPE_BINARY;
    case PropertyType::Date:
        return realm_value_type::RLM_TYPE_TIMESTAMP;
    case PropertyType::Float:
        return realm_value_type::RLM_TYPE_FLOAT;
    case PropertyType::Double:
        return realm_value_type::RLM_TYPE_DOUBLE;
    case PropertyType::Decimal:
        return realm_value_type::RLM_TYPE_DECIMAL128;
    case PropertyType::ObjectId:
        return realm_value_type::RLM_TYPE_OBJECT_ID;
    case PropertyType::Object:
        return realm_value_type::RLM_TYPE_LINK;
    case PropertyType::UUID:
        return realm_value_type::RLM_TYPE_UUID;
    default:
        REALM_UNREACHABLE();
    }
}

static inline std::string to_string(realm_value_type type)
{
    switch (type)
    {
    case realm_value_type::RLM_TYPE_INT:
        return "int64";
    case realm_value_type::RLM_TYPE_BOOL:
        return "bool";
    case realm_value_type::RLM_TYPE_STRING:
        return "string";
    case realm_value_type::RLM_TYPE_BINARY:
        return "binary";
    case realm_value_type::RLM_TYPE_TIMESTAMP:
        return "date";
    case realm_value_type::RLM_TYPE_FLOAT:
        return "float";
    case realm_value_type::RLM_TYPE_DOUBLE:
        return "double";
    case realm_value_type::RLM_TYPE_DECIMAL128:
        return "decimal";
    case realm_value_type::RLM_TYPE_OBJECT_ID:
        return "objectId";
    case realm_value_type::RLM_TYPE_LINK:
        return "link";
    case realm_value_type::RLM_TYPE_UUID:
        return "uuid";
    default:
        REALM_UNREACHABLE();
    }
}

static inline std::string to_string(PropertyType type)
{
    return to_string(to_capi(type));
}

static inline Mixed from_capi(Object* obj, bool isMixedColumn)
{
    if (!isMixedColumn)
    {
        return Mixed{ obj->get_obj().get_key() };
    }

    return Mixed{ ObjLink{obj->get_obj().get_table()->get_key(), obj->get_obj().get_key()} };
}

static inline Mixed from_capi(const realm_value_t& val)
{
    switch (val.type) {
    case realm_value_type::RLM_TYPE_NULL:
        return Mixed{};
    case realm_value_type::RLM_TYPE_INT:
        return Mixed{ val.integer };
    case realm_value_type::RLM_TYPE_BOOL:
        return Mixed{ val.boolean() };
    case realm_value_type::RLM_TYPE_STRING:
        return Mixed{ from_capi(val.string) };
    case realm_value_type::RLM_TYPE_BINARY:
        return Mixed{ from_capi(val.binary) };
    case realm_value_type::RLM_TYPE_TIMESTAMP:
        return Mixed{ from_capi(val.timestamp) };
    case realm_value_type::RLM_TYPE_FLOAT:
        return Mixed{ val.fnum };
    case realm_value_type::RLM_TYPE_DOUBLE:
        return Mixed{ val.dnum };
    case realm_value_type::RLM_TYPE_DECIMAL128:
        return Mixed{ from_capi(val.decimal128) };
    case realm_value_type::RLM_TYPE_OBJECT_ID:
        return Mixed{ from_capi(val.object_id) };
    case realm_value_type::RLM_TYPE_UUID:
        return Mixed{ from_capi(val.uuid) };
    case realm_value_type::RLM_TYPE_LINK:
        return from_capi(val.link.object, true);
    default:
        REALM_TERMINATE("Invalid realm_value_t");
    }
}

realm_value_t to_capi(Obj obj, SharedRealm realm);

static inline realm_value_t to_capi(ObjLink obj_link, SharedRealm realm)
{
    return to_capi(realm->read_group().get_object(obj_link), realm);
}

// Collections need to have their own overload of to_capi, as at the moment there's no API to retrieve a collection value
// from Mixed (like val.get<int64>), so we need first to retrieve the collection itself with the specific methods like
// list.get_list, list.get_dictionary and so on
static inline realm_value_t to_capi(List* list)
{
    realm_value_t val{};
    val.type = realm_value_type::RLM_TYPE_LIST;
    val.collection = list;
    return val;
}

static inline realm_value_t to_capi(object_store::Dictionary* dictionary)
{
    realm_value_t val{};
    val.type = realm_value_type::RLM_TYPE_DICTIONARY;
    val.collection = dictionary;
    return val;
}

static inline realm_value_t to_capi(const Mixed& value)
{
    realm_value_t val{};
    if (value.is_null()) {
        val.type = realm_value_type::RLM_TYPE_NULL;
    }
    else {
        switch (value.get_type()) {
        case type_Int: {
            val.type = realm_value_type::RLM_TYPE_INT;
            val.integer = value.get<int64_t>();
            break;
        }
        case type_Bool: {
            val.type = realm_value_type::RLM_TYPE_BOOL;
            val.integer = value.get<bool>() ? 1 : 0;
            break;
        }
        case type_String: {
            val.type = realm_value_type::RLM_TYPE_STRING;
            val.string = to_capi(value.get<StringData>());
            break;
        }
        case type_Binary: {
            val.type = realm_value_type::RLM_TYPE_BINARY;
            val.binary = to_capi(value.get<BinaryData>());
            break;
        }
        case type_Timestamp: {
            val.type = realm_value_type::RLM_TYPE_TIMESTAMP;
            val.timestamp = to_capi(value.get<Timestamp>());
            break;
        }
        case type_Float: {
            val.type = realm_value_type::RLM_TYPE_FLOAT;
            val.fnum = value.get<float>();
            break;
        }
        case type_Double: {
            val.type = realm_value_type::RLM_TYPE_DOUBLE;
            val.dnum = value.get<double>();
            break;
        }
        case type_Decimal: {
            val.type = realm_value_type::RLM_TYPE_DECIMAL128;
            val.decimal128 = to_capi(value.get<Decimal128>());
            break;
        }
        case type_TypedLink:
            [[fallthrough]];
        case type_Link:
            REALM_TERMINATE("Can't use this overload of to_capi on values containing links, use to_capi(Obj, SharedRealm) instead.");
        case type_ObjectId: {
            val.type = realm_value_type::RLM_TYPE_OBJECT_ID;
            val.object_id = to_capi(value.get<ObjectId>());
            break;
        }
        case type_UUID: {
            val.type = realm_value_type::RLM_TYPE_UUID;
            val.uuid = to_capi(value.get<UUID>());
            break;
        }
        case type_List:
        case type_Dictionary:
            REALM_TERMINATE("Can't use this overload of to_capi on values containing collections, use to_capi(Collection*) instead.");
        default:
            REALM_TERMINATE("Invalid Mixed value type");
        }
    }

    return val;
}

inline realm_value_t to_capi(const object_store::Dictionary& dictionary, const Mixed& val, const StringData key)
{
    if (val.is_null()) {
        return to_capi(std::move(val));
    }

    switch (val.get_type()) {
    case type_Link:
        if ((dictionary.get_type() & ~PropertyType::Flags) == PropertyType::Object) {
            return to_capi(ObjLink(dictionary.get_object_schema().table_key, val.get<ObjKey>()), dictionary.get_realm());
        }
        REALM_UNREACHABLE();
    case type_TypedLink:
        return to_capi(val.get_link(), dictionary.get_realm());
    case type_List:
        return to_capi(new List(dictionary.get_list(key)));
        break;
    case type_Dictionary:
        return to_capi(new object_store::Dictionary(dictionary.get_dictionary(key)));
        break;
    default:
        return to_capi(std::move(val));
    }
}

static inline bool are_equal(const realm_value_t& realm_value, const Mixed& mixed_value)
{
    // from_capi returns TypedLink for objects, but the mixed_value may contain just Link - let's ensure that we're comparing apples to apples
    return (mixed_value.is_type(realm::DataType::Type::Link) && realm_value.type == realm_value_type::RLM_TYPE_LINK && mixed_value == from_capi(realm_value.link.object, false)) ||
        mixed_value == from_capi(realm_value);
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

    operator std::string_view() const noexcept
    {
        return std::string_view(m_data.get(), m_size);
    }

    operator std::string() const noexcept
    {
        return to_string();
    }

    const char* data() const { return m_data.get(); }
    size_t size() const { return m_size; }

    bool error;
private:
    std::unique_ptr<char[]> m_data;
    std::size_t m_size;
};

size_t stringdata_to_csharpstringbuffer(StringData str, uint16_t* csharpbuffer, size_t bufsize); //note bufsize is _in_16bit_words

extern std::atomic<bool> s_can_call_managed;

template <typename TReturn, typename ...TArgs>
inline auto wrap_managed_callback(TReturn(*func)(TArgs... args))
{
    return [func](TArgs... args) -> TReturn {
        if constexpr (std::is_same_v<TReturn, void>) {
            if (realm::binding::s_can_call_managed) {
                func(std::forward<TArgs>(args)...);
            }
        }
        else {
            if (realm::binding::s_can_call_managed) {
                return func(std::forward<TArgs>(args)...);
            }

            return {};
        }
    };
}
} // namespace realm::binding
