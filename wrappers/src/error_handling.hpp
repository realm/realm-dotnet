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
#include "realm_export_decls.hpp"

namespace realm {
enum CustomError : std::int32_t {
    None = 0,
    RowDetached = 1000004000,
    RealmClosed = 1000004001,
    DuplicatePrimaryKey = 1000004002,
    InvalidSchema = 1000004003,
    ObjectManagedByAnotherRealm = 1000004004,
    PropertyTypeMismatch = 1000004006,
    KeyAlreadyExists = 1000004007,
    DuplicateSubscription = 1000004008,
    IndexOutOfRange = 1000004009,
    InvalidGeospatialShape = 1000004010,
};

class CustomException : public RuntimeError {
public:
    CustomError custom_error;

    CustomException(CustomError error, std::string_view message) : RuntimeError(ErrorCodes::Error::CustomError, message),
        custom_error(error) {}
};

struct NativeException {
public:
    NativeException(const Exception& ex, CustomError custom_error = CustomError::None, void* managed_error = nullptr)
        : m_code(ex.code()),
          m_custom_error(custom_error),
          m_message(ex.reason()),
          m_managed_error(managed_error) {}

    NativeException(ErrorCodes::Error code, std::string message)
        : m_code(code),
          m_custom_error(CustomError::None),
          m_message(message),
          m_managed_error(nullptr) {}

    std::string to_string() {
        return m_message;
    }

    struct Marshallable {
        int code;
        int m_categories;
        char* messagesBytes;
        size_t messageLength;
        void* managed_error = nullptr;
    };

    Marshallable for_marshalling() const {
        int code = m_code;
        if (m_custom_error != CustomError::None) {
            REALM_ASSERT_DEBUG(m_code == ErrorCodes::Error::CustomError);
            code = m_custom_error;
        }

        char* messageBytes = reinterpret_cast<char*>(malloc(m_message.size()));
        memcpy(messageBytes, m_message.c_str(), m_message.size());

        return {
            (ErrorCodes::Error)code,
            ErrorCodes::error_categories(m_code).value(),
            messageBytes, // to be freed with realm_free() on the managed side
            m_message.size(),
            m_managed_error,
        };
    }
private:
    ErrorCodes::Error m_code;
    CustomError m_custom_error;
    std::string m_message;
    void* m_managed_error;
};

class RowDetachedException : public CustomException{
public:
    RowDetachedException() : CustomException(CustomError::RowDetached, "Attempted to access detached row") {}
};

class RealmClosedException : public CustomException {
public:
    RealmClosedException() : CustomException(CustomError::RealmClosed, "This object belongs to a closed realm.") {}
};

class SetDuplicatePrimaryKeyValueException : public CustomException {
public:
    SetDuplicatePrimaryKeyValueException(std::string object_type, std::string property, std::string value)
        : CustomException(CustomError::DuplicatePrimaryKey, util::format("A %1 object already exists with primary key property %2 == '%3'", object_type, property, value)) {}
};

class InvalidSchemaException : public CustomException {
public:
    InvalidSchemaException(std::string message) : CustomException(CustomError::InvalidSchema, message) {}
};

class ObjectManagedByAnotherRealmException : public CustomException {
public:
    ObjectManagedByAnotherRealmException(std::string message) : CustomException(CustomError::ObjectManagedByAnotherRealm, message) {}
};

class PropertyTypeMismatchException : public CustomException {
public:
    PropertyTypeMismatchException(std::string object_type, std::string property, std::string property_type, std::string actual_type)
        : CustomException(CustomError::PropertyTypeMismatch, util::format("Property type mismatch: %1.%2 is of type %3, but the supplied value is of type %4", object_type, property, property_type, actual_type)) {}

    PropertyTypeMismatchException(std::string property_type, std::string actual_type)
        : CustomException(CustomError::PropertyTypeMismatch, util::format("List type mismatch: attempted to add a value of type '%1' to a list that expects '%2'", actual_type, property_type)) {}
};

class KeyAlreadyExistsException : public CustomException {
public:
    KeyAlreadyExistsException(std::string key) : CustomException(CustomError::KeyAlreadyExists, util::format("An item with the key '%1' has already been added.", key)) {}
};

class DuplicateSubscriptionException : public CustomException {
public:
    DuplicateSubscriptionException(std::string name, std::string old_query, std::string new_query)
        : CustomException(CustomError::DuplicateSubscription, util::format(
            "A subscription with the name '%1' already exists but has a different query. If you meant to update it, set UpdateExisting = true in the subscription options. Existing query: '%2'; new query: '%3'",
            name, old_query, new_query)) {}
};

class IndexOutOfRangeException : public CustomException {
public:
    IndexOutOfRangeException(std::string message) : CustomException(CustomError::IndexOutOfRange, message) {}
    IndexOutOfRangeException(std::string context, size_t bad_index, size_t count) :
        CustomException(CustomError::IndexOutOfRange, util::format("%1 index: %2 beyond range of: %3", context, bad_index, count)) {}
};

class ManagedExceptionDuringCallback : public RuntimeError {
public:
    ManagedExceptionDuringCallback(std::string message, void* managed_error)
        : RuntimeError(ErrorCodes::Error::CallbackFailed, message),
          m_managed_error(managed_error) {}

    void* m_managed_error;
};

class GeoSpatialShapeValidationException : public CustomException {
public:
    GeoSpatialShapeValidationException(std::string reason)
        : CustomException(CustomError::InvalidGeospatialShape, util::format("Validation failed for a geospatial shape: %1", reason)) {}
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
    ex.code = ErrorCodes::Error::OK;
    try {
        return func();
    }
    catch (...) {
        ex = convert_exception().for_marshalling();
        return Default<RetVal>::default_value();
    }
}

} // namespace realm
