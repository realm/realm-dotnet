/*
* Copyright 2015 Realm Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#include <stdexcept>
#include <sstream>
#include <cassert>
#include "object-store/shared_realm.hpp"
#include "realm_export_decls.hpp"
#include "error_handling.hpp"
#include "realm_error_type.hpp"

// core headers for exception types
#include "realm/util/file.hpp" 
//#include "realm/util/encrypted_file_mapping.hpp"
#include "realm/alloc_slab.hpp"

using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);

// CALLBACK TO THROW IN MANAGED SPACE
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;

namespace realm {

    /**
    @note mostly copied from util.cpp in Java but has a much richer range of exceptions
    */
    void realm::convert_exception()
    {
        std::ostringstream ss;
        // use a lambda to avoid repeating the following composition in each catch
        auto msg = [&](auto e) -> std::string {
            ss << e.what(); // << " in " << file << " line " << line;
            return ss.str();
        };
        try {
            throw;  // rethrow so we can add typing information by different catches
        }
        catch (const RealmFileException& e) {

            switch (e.kind) {
            case RealmFileException::Kind::AccessError:
                throw_exception(RealmErrorType::RealmFileAccessError, msg(e));
                break;
            case RealmFileException::Kind::Exists:
                throw_exception(RealmErrorType::RealmFileExists, msg(e));
                break;
            case RealmFileException::Kind::IncompatibleLockFile:
                throw_exception(RealmErrorType::RealmIncompatibleLockFile, msg(e));
                break;
            case RealmFileException::Kind::NotFound:
                throw_exception(RealmErrorType::RealmFileNotFound, msg(e));
                break;
            case RealmFileException::Kind::PermissionDenied:
                throw_exception(RealmErrorType::RealmPermissionDenied, msg(e));
                break;
            default:
                throw_exception(RealmErrorType::RealmError, msg(e));
            }
        }
        catch (const MismatchedConfigException& e) {
            throw_exception(RealmErrorType::RealmMismatchedConfig, msg(e));
        }
        catch (const InvalidTransactionException& e) {
            throw_exception(RealmErrorType::RealmInvalidTransaction, msg(e));
        }
        catch (const IncorrectThreadException& e) {
            throw_exception(RealmErrorType::RealmIncorrectThread, msg(e));
        }
        catch (const UnitializedRealmException& e) {
            throw_exception(RealmErrorType::RealmUnitializedRealm, msg(e));
        }
        //catch (const util::DecryptionFailed& e) {
        //    throw_exception(RealmExceptionCodes::RealmDecryptionFailed, msg(e), nullptr);
        //}
        catch (const InvalidDatabase& e) {
            throw_exception(RealmErrorType::RealmInvalidDatabase, msg(e));
        }
        catch (const std::bad_alloc& e) {
            throw_exception(RealmErrorType::RealmOutOfMemory, msg(e));
        }
        catch (const std::exception& e) {
            throw_exception(RealmErrorType::RealmError, msg(e));
        }
        catch (...) {
            ss << "Unknown exception caught which doesn't descend from std::exception"; //, in " << file << " line " << line;
            throw_exception(RealmErrorType::RealmError, ss.str());
        }
    }

    void throw_exception(RealmErrorType error_type, const std::string message)
    {
        assert(ManagedExceptionThrower != nullptr);
        ManagedExceptionThrower((size_t)error_type, (void*)message.data(), message.size());
    }

}   // namespace realm

extern "C" {
    
REALM_EXPORT void set_exception_thrower(ManagedExceptionThrowerT userThrower)
{
    ManagedExceptionThrower = userThrower;
}

}   // extern "C"
