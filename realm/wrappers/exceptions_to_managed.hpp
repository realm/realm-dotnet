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

#ifndef EXCEPTIONS_TO_MANAGED_H
#define EXCEPTIONS_TO_MANAGED_H


/// @warning Keep these codes aligned with RealmExceptionCodes.cs

#include <exception>
#include <string>

namespace realm {
    /**
    * These numeric codes are based on the Java "test" codes in Util.java
    * To aid anyone comparing code, they have retained the same names.
    */
    enum class RealmExceptionCodes : size_t {
        RealmError = 0,
        RealmFileAccessError = 1,
        RealmDecryptionFailed = 2,
        RealmFileExists = 3,
        RealmFileNotFound = 4,
        RealmInvalidDatabase = 5,
        RealmOutOfMemory = 6,
        RealmPermissionDenied = 7,

        StdArgumentOutOfRange = 100,
        StdIndexOutOfRange = 101,
        StdInvalidOperation = 102
    };

    void ThrowManaged(RealmExceptionCodes exceptionCode, const std::string& message = "");
    void ThrowManaged();
}  // namespace realm

#endif  // EXCEPTIONS_TO_MANAGED_H
