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

#include <string>
#include <new>
#include <realm.hpp>
#include "realm_error_type.hpp"
#include "realm_export_decls.hpp"

namespace realm {
struct NativeException {
public:
    union Detail {
        Detail(): managed_error(nullptr) {}
        Detail(std::string str) : string(str) {}
        Detail(void* err) : managed_error(err) {}

        std::string string;
        void* managed_error;

        ~Detail() {};
    };

    enum class DetailType : unsigned char {
        NONE,
        STRING,
        MANAGED_ERROR,
    };

    NativeException(RealmErrorType type, std::string message)
        : m_type(type),
          m_message(message),
          m_detail(),
          m_detail_type(DetailType::NONE) {}

    NativeException(RealmErrorType type, std::string message, std::string detail)
        : m_type(type),
          m_message(message),
          m_detail(detail),
          m_detail_type(DetailType::STRING) {}

    NativeException(std::string message, void* managed_error)
        : m_type(RealmErrorType::RealmDotNetExceptionDuringCallback),
          m_message(message),
          m_detail(managed_error),
          m_detail_type(DetailType::MANAGED_ERROR) {}

    std::string to_string() {
        switch (m_detail_type) {
        case DetailType::STRING:
            return util::format("%1: %2", m_message, m_detail.string);
        default:
            return m_message;
        }
    }

    struct Marshallable {
        RealmErrorType type;
        void* messagesBytes;
        size_t messageLength;
        void* detailBytes;
        size_t detailLength;
    };

    Marshallable for_marshalling() const {
        auto messageCopy = ::operator new (m_message.size());
        m_message.copy(reinterpret_cast<char*>(messageCopy), m_message.length());

        void* marshaled_detail;
        size_t marshaled_detail_length;
        switch (m_detail_type) {
        case DetailType::STRING:
            marshaled_detail = ::operator new (m_detail.string.size());
            m_detail.string.copy(reinterpret_cast<char*>(marshaled_detail), m_detail.string.length());
            marshaled_detail_length = m_detail.string.length();
            break;
        case DetailType::MANAGED_ERROR:
            marshaled_detail = m_detail.managed_error;
            marshaled_detail_length = (size_t)-1;
            break;
        default:
            marshaled_detail = nullptr;
            marshaled_detail_length = 0;
            break;
        }

        return {
            m_type,
            messageCopy,
            m_message.size(),
            marshaled_detail,
            marshaled_detail_length,
        };
    }
private:
    RealmErrorType m_type;
    std::string m_message;
    Detail m_detail;
    DetailType m_detail_type;
};

class RowDetachedException : public std::runtime_error {
public:
    RowDetachedException() : std::runtime_error("Attempted to access detached row") {}
};

class RealmClosedException : public std::runtime_error {
public:
    RealmClosedException() : std::runtime_error("This object belongs to a closed realm.") {}
};

class SetDuplicatePrimaryKeyValueException : public std::runtime_error {
public:
    SetDuplicatePrimaryKeyValueException(std::string object_type, std::string property, std::string value)
        : std::runtime_error(util::format("A %1 object already exists with primary key property %2 == '%3'", object_type, property, value)) {}
};

class InvalidSchemaException : public std::runtime_error {
public:
    InvalidSchemaException(std::string message) : std::runtime_error(message) {}
};

class ObjectManagedByAnotherRealmException : public std::runtime_error {
public:
    ObjectManagedByAnotherRealmException(std::string message) : std::runtime_error(message) {}
};

class NotNullableException : public std::runtime_error {
public:
    NotNullableException(std::string object_type, std::string property)
        : std::runtime_error(util::format("Attempted to set %1.%2 to null, but it is defined as required.", object_type, property)) {}

    NotNullableException() : std::runtime_error("Attempted to add null to a list of required values") {}
};

class PropertyTypeMismatchException : public std::runtime_error {
public:
    PropertyTypeMismatchException(std::string object_type, std::string property, std::string property_type, std::string actual_type)
        : std::runtime_error(util::format("Property type mismatch: %1.%2 is of type %3, but the supplied value is of type %4", object_type, property, property_type, actual_type)) {}

    PropertyTypeMismatchException(std::string property_type, std::string actual_type)
        : std::runtime_error(util::format("List type mismatch: attempted to add a value of type '%1' to a list that expects '%2'", actual_type, property_type)) {}
};

class KeyAlreadyExistsException : public std::runtime_error {
public:
    KeyAlreadyExistsException(std::string key) : std::runtime_error(util::format("An item with the key '%1' has already been added.", key)) {}
};

class DuplicateSubscriptionException : public std::runtime_error {
public:
    DuplicateSubscriptionException(std::string name, std::string old_query, std::string new_query)
        : std::runtime_error(util::format(
            "A subscription with the name '%1' already exists but has a different query. If you meant to update it, set UpdateExisting = true in the subscription options. Existing query: '%2'; new query: '%3'",
            name, old_query, new_query)) {}
};

REALM_EXPORT NativeException convert_exception(std::exception_ptr err = nullptr);

template <class T>
struct Default {
    static T default_value() {
        return T{};
    }
};
template <>
struct Default<void> {
    static void default_value() {}
};

template <class F>
auto handle_errors(NativeException::Marshallable& ex, F&& func) -> decltype(func())
{
    using RetVal = decltype(func());
    ex.type = RealmErrorType::NoError;
    try {
        return func();
    }
    catch (...) {
        ex = convert_exception().for_marshalling();
        return Default<RetVal>::default_value();
    }
}

} // namespace realm
