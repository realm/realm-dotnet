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
#include "shared_realm_cs.hpp"

// core headers for exception types
#include "realm/util/file.hpp" 
#include "realm/alloc_slab.hpp"
#include "object_accessor.hpp"

namespace realm {

    SetDuplicatePrimaryKeyValueException::SetDuplicatePrimaryKeyValueException(std::string object_type, std::string property, std::string value)
        : std::runtime_error(util::format(
            "A %1 object already exists with primary key property %2 == '%3'",
        object_type, property, value))
    {}


    /**
    @note mostly copied from util.cpp in Java but has a much richer range of exceptions
    @warning if you update these codes also update the matching RealmExceptionCodes.cs
    */
    NativeException convert_exception()
    {
        try {
            throw;  // rethrow so we can add typing information by different catches
        }
        catch (const RealmFileException& e) {

            switch (e.kind()) {
            case RealmFileException::Kind::AccessError:
                return { RealmErrorType::RealmFileAccessError, e.what() };
            case RealmFileException::Kind::PermissionDenied:
                return { RealmErrorType::RealmPermissionDenied, e.what() };
            case RealmFileException::Kind::Exists:
                return { RealmErrorType::RealmFileExists, e.what() };
            case RealmFileException::Kind::NotFound:
                return { RealmErrorType::RealmFileNotFound, e.what() };
            case RealmFileException::Kind::IncompatibleLockFile:
                return { RealmErrorType::RealmIncompatibleLockFile, e.what() };
            case RealmFileException::Kind::FormatUpgradeRequired:
                return { RealmErrorType::RealmFormatUpgradeRequired, e.what() };
            default:
                return { RealmErrorType::RealmError, e.what() };
            }
        }
        catch (const SchemaValidationException& e) { // an ObjectStore exception mapped onto same code as older core
            return { RealmErrorType::RealmFormatUpgradeRequired, e.what() };
        }
        catch (const MismatchedConfigException& e) {
            return { RealmErrorType::RealmMismatchedConfig, e.what() };
        }
        catch (const InvalidTransactionException& e) {
            return { RealmErrorType::RealmInvalidTransaction, e.what() };
        }
        catch (const IncorrectThreadException& e) {
            return { RealmErrorType::RealmIncorrectThread, e.what() };
        }
        catch (const UninitializedRealmException& e) {
            return { RealmErrorType::RealmUnitializedRealm, e.what() };
        }
        catch (const SchemaMismatchException& e) {
          // typically shared_realm_open failing because same schemaVersion but changed
          return { RealmErrorType::RealmSchemaMismatch, e.what() };
        }
        //catch (const util::DecryptionFailed& e) {
        //    return { RealmExceptionCodes::RealmDecryptionFailed, e.what(), nullptr);
        //}
        catch (const InvalidDatabase& e) {
            return { RealmErrorType::RealmInvalidDatabase, e.what() };
        }
        catch (const IndexOutOfRangeException& e) {
            return { RealmErrorType::StdIndexOutOfRange, e.what() };
        }
        catch (const RowDetachedException& e) {
            return { RealmErrorType::RealmRowDetached, e.what() };
        }
        catch (const MissingPrimaryKeyException& e) {
            return { RealmErrorType::RealmTableHasNoPrimaryKey, e.what() };
        }
        catch (const ManagedExceptionDuringMigration& e) {
            return { RealmErrorType::RealmDotNetExceptionDuringMigration, e.what() };
        }
        catch (const DuplicatePrimaryKeyValueException& e) {
            return { RealmErrorType::RealmDuplicatePrimaryKeyValue, e.what() };
        }
        catch (const SetDuplicatePrimaryKeyValueException& e) {
            return { RealmErrorType::RealmDuplicatePrimaryKeyValue, e.what() };  // map to same as DuplicatePrimaryKeyValueException
        }
        catch (const RealmClosedException& e) {
            return { RealmErrorType::RealmClosed, e.what() };
        }
        catch (const std::bad_alloc& e) {
            return { RealmErrorType::RealmOutOfMemory, e.what() };
        }
        catch (const std::exception& e) {
            return { RealmErrorType::RealmError, e.what() };
        }
        catch (...) {
            return { RealmErrorType::RealmError, "Unknown exception caught which doesn't descend from std::exception" };
        }
    }

}   // namespace realm

