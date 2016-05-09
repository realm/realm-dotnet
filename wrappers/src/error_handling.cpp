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
#include <stdexcept>
#include <sstream>
#include <cassert>
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/object_store.hpp"
#include "wrapper_exceptions.hpp"
#include "realm_export_decls.hpp"
#include "error_handling.hpp"
#include "realm_error_type.hpp"

// core headers for exception types
#include "realm/util/file.hpp" 
#include "realm/alloc_slab.hpp"

using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);

// CALLBACK TO THROW IN MANAGED SPACE
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;

namespace realm {

    void throw_exception(RealmErrorType error_type, const std::string message)
    {
        assert(ManagedExceptionThrower != nullptr);
        ManagedExceptionThrower((size_t)error_type, (void*)message.data(), message.size());
    }

    /**
    @note mostly copied from util.cpp in Java but has a much richer range of exceptions
    @warning if you update these codes also update the matching RealmExceptionCodes.cs
    */
    void convert_exception()
    {
        try {
            throw;  // rethrow so we can add typing information by different catches
        }
        catch (const RealmFileException& e) {

            switch (e.kind()) {
            case RealmFileException::Kind::AccessError:
                throw_exception(RealmErrorType::RealmFileAccessError, e.what());
                break;
            case RealmFileException::Kind::PermissionDenied:
                throw_exception(RealmErrorType::RealmPermissionDenied, e.what());
                break;
            case RealmFileException::Kind::Exists:
                throw_exception(RealmErrorType::RealmFileExists, e.what());
                break;
            case RealmFileException::Kind::NotFound:
                throw_exception(RealmErrorType::RealmFileNotFound, e.what());
                break;
            case RealmFileException::Kind::IncompatibleLockFile:
                throw_exception(RealmErrorType::RealmIncompatibleLockFile, e.what());
                break;
            case RealmFileException::Kind::FormatUpgradeRequired:
                throw_exception(RealmErrorType::RealmFormatUpgradeRequired, e.what());
                break;
            default:
                throw_exception(RealmErrorType::RealmError, e.what());
            }
        }
        catch (const SchemaValidationException& e) { // an ObjectStore exception mapped onto same code as older core
            throw_exception(RealmErrorType::RealmFormatUpgradeRequired, e.what());
        }
        catch (const MismatchedConfigException& e) {
            throw_exception(RealmErrorType::RealmMismatchedConfig, e.what());
        }
        catch (const InvalidTransactionException& e) {
            throw_exception(RealmErrorType::RealmInvalidTransaction, e.what());
        }
        catch (const IncorrectThreadException& e) {
            throw_exception(RealmErrorType::RealmIncorrectThread, e.what());
        }
        catch (const UnitializedRealmException& e) {
            throw_exception(RealmErrorType::RealmUnitializedRealm, e.what());
        }
        //catch (const util::DecryptionFailed& e) {
        //    throw_exception(RealmExceptionCodes::RealmDecryptionFailed, e.what(), nullptr);
        //}
        catch (const InvalidDatabase& e) {
            throw_exception(RealmErrorType::RealmInvalidDatabase, e.what());
        }
        catch (const IndexOutOfRangeException& e) {
            throw_exception(RealmErrorType::StdIndexOutOfRange, e.what());
        }
        catch (const std::bad_alloc& e) {
            throw_exception(RealmErrorType::RealmOutOfMemory, e.what());
        }
        catch (const std::exception& e) {
            throw_exception(RealmErrorType::RealmError, e.what());
        }
        catch (...) {
            throw_exception(RealmErrorType::RealmError, "Unknown exception caught which doesn't descend from std::exception");
        }
    }

}   // namespace realm

extern "C" {
    
REALM_EXPORT void set_exception_thrower(ManagedExceptionThrowerT userThrower)
{
    ManagedExceptionThrower = userThrower;
}

// allow C# test code to generate an exception being thrown back
REALM_EXPORT void fake_a_native_exception(int errorType)
{
    realm::throw_exception(realm::RealmErrorType(errorType), "this is fake_exception");
}

}   // extern "C"
