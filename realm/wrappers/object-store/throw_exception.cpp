////////////////////////////////////////////////////////////////////////////
//
// Copyright 2015 Realm Inc.
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

#include <stdexcept>
#include "realm_error_type.hpp"
#include "realm_binding_context.hpp"

namespace realm {

class RealmFileException : public std::runtime_error
{
public:
    enum class Kind
    {
        /** Thrown for any I/O related exception scenarios when a realm is opened. */
        AccessError,
        /** Thrown if the user does not have permission to open or create
         the specified file in the specified access mode when the realm is opened. */
        PermissionDenied,
        /** Thrown if no_create was specified and the file did already exist when the realm is opened. */
        Exists,
        /** Thrown if no_create was specified and the file was not found when the realm is opened. */
        NotFound,
        /** Thrown if the database file is currently open in another
         process which cannot share with the current process due to an
         architecture mismatch. */
        IncompatibleLockFile,
    };
    RealmFileException(Kind kind, std::string message) : std::runtime_error(message), m_kind(kind) {}
    Kind kind() const { return m_kind; }

private:
    Kind m_kind;
};

class MismatchedConfigException : public std::runtime_error
{
public:
    MismatchedConfigException(std::string message) : std::runtime_error(message) {}
};

class InvalidTransactionException : public std::runtime_error
{
public:
    InvalidTransactionException(std::string message) : std::runtime_error(message) {}
};

class IncorrectThreadException : public std::runtime_error
{
public:
    IncorrectThreadException(std::string message) : std::runtime_error(message) {}
};

class UnitializedRealmException : public std::runtime_error
{
public:
    UnitializedRealmException(std::string message) : std::runtime_error(message) {}
};

void throw_exception(RealmErrorType error_type, std::string message, RealmBindingContext* binding_context)
{
    switch (error_type) {
    case RealmErrorType::AccessError:
        throw RealmFileException(RealmFileException::Kind::AccessError, message);

    case RealmErrorType::PermissionDenied:
        throw RealmFileException(RealmFileException::Kind::PermissionDenied, message);

    case RealmErrorType::Exists:
        throw RealmFileException(RealmFileException::Kind::Exists, message);

    case RealmErrorType::NotFound:
        throw RealmFileException(RealmFileException::Kind::NotFound, message);

    case RealmErrorType::IncompatibleLockFile:
        throw RealmFileException(RealmFileException::Kind::IncompatibleLockFile, message);

    case RealmErrorType::MismatchedConfig:
        throw MismatchedConfigException(message);

    case RealmErrorType::InvalidTransaction:
        throw InvalidTransactionException(message);

    case RealmErrorType::IncorrectThread:
        throw IncorrectThreadException(message);

    case RealmErrorType::UnitializedRealm:
        throw UnitializedRealmException(message);
    }
}

}

