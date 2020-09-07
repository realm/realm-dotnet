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
    RealmErrorType type;
    std::string message;
    std::string detail;
    
    struct Marshallable {
        RealmErrorType type;
        void* messagesBytes;
        size_t messageLength;
        void* detailBytes;
        size_t detailLength;
    };
    
    Marshallable for_marshalling() const {
        auto messageCopy = ::operator new (message.size());
        message.copy(reinterpret_cast<char*>(messageCopy), message.length());

        auto detailCopy = ::operator new (detail.size());
        detail.copy(reinterpret_cast<char*>(detailCopy), detail.length());
        
        return {
            type,
            messageCopy,
            message.size(),
            detailCopy,
            detail.size(),
        };
    }
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
    SetDuplicatePrimaryKeyValueException(std::string object_type, std::string property, std::string value);
};
    
class ObjectManagedByAnotherRealmException : public std::runtime_error {
public:
    ObjectManagedByAnotherRealmException(std::string message) : std::runtime_error(message) {}
};
    
class RealmFeatureUnavailableException : public std::runtime_error {
public:
    RealmFeatureUnavailableException(std::string message) : std::runtime_error(message) {}
};


REALM_EXPORT NativeException convert_exception();

void throw_managed_exception(const NativeException& exception);
    
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
