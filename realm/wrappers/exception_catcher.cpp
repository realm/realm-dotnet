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

#include "exception_catcher.hpp"
#include "exceptions_to_managed.hpp"

// core headers for exception types
#include "realm/util/file.hpp" 
#include "realm/util/encrypted_file_mapping.hpp"
#include "realm/alloc_slab.hpp"

using namespace realm;

/**
@note mostly copied from util.cpp in Java but has a much richer range of exceptions
*/
void ConvertException(const char *file, int line)
{
    std::ostringstream ss;
    // use a lambda to avoid repeating the following composition in each catch
    auto msg = [&](auto e) -> std::string { 
        ss << e.what() << " in " << file << " line " << line;
        return ss.str();
    };
    try {
        throw;  // rethrow so we can add typing information by different catches
    }
    catch (const util::File::AccessError& e) {
        ThrowManaged(RealmExceptionCodes::RealmFileAccessError, msg(e));
    }
    catch (const util::File::Exists& e) {
        ThrowManaged(RealmExceptionCodes::RealmFileExists, msg(e));
    }
    catch (const util::File::NotFound& e) {
        ThrowManaged(RealmExceptionCodes::RealmFileNotFound, msg(e));
    }
    catch (const util::File::PermissionDenied& e) {
        ThrowManaged(RealmExceptionCodes::RealmPermissionDenied, msg(e));
    }
    catch (const util::DecryptionFailed& e) {
        ThrowManaged(RealmExceptionCodes::RealmDecryptionFailed, msg(e));
    }
    catch (const InvalidDatabase& e) {
        ThrowManaged(RealmExceptionCodes::RealmInvalidDatabase, msg(e));
    }
    catch (const std::bad_alloc& e) {
        ThrowManaged(RealmExceptionCodes::RealmOutOfMemory, msg(e));
    }
    catch (const std::exception& e) {
        ThrowManaged(RealmExceptionCodes::RealmError, msg(e));
    }
    catch (...) {
        ss <<  "Unknown exception caught which doesn't descend from std::exception, in " << file << " line " << line;
        ThrowManaged(RealmExceptionCodes::RealmError, ss.str());
    }
}
