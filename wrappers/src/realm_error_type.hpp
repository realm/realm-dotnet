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

namespace realm {

    // Keep this in sync with RealmExceptionCodes.cs
    enum class RealmErrorType : signed char
    {
        /** When no exception was thrown, we will use this code in the NativeException class. */
        NoError = -1,

        RealmError = 0,

        /** Thrown for any I/O related exception scenarios when a realm is opened. */
        RealmFileAccessError = 1,

        RealmDecryptionFailed = 2,

        /** Thrown if no_create was specified and the file did already exist when the realm is opened. */
        RealmFileExists = 3,

        /** Thrown if no_create was specified and the file was not found when the realm is opened. */
        RealmFileNotFound = 4,

        RealmInvalidDatabase = 5,

        RealmOutOfMemory = 6,

        /** Thrown if the user does not have permission to open or create
         the specified file in the specified access mode when the realm is opened. */
        RealmPermissionDenied = 7,

        /** Thrown if the database file is currently open in another
         process which cannot share with the current process due to an
         architecture mismatch. */
        RealmIncompatibleLockFile = 8,

        RealmMismatchedConfig = 9,

        RealmInvalidTransaction = 10,

        RealmIncorrectThread = 11,

        RealmUnitializedRealm = 12,

        RealmFormatUpgradeRequired = 13,

        RealmSchemaMismatch = 14,

        RealmRowDetached = 21,

        RealmTableHasNoPrimaryKey = 22,

        RealmDuplicatePrimaryKeyValue = 23,
        
        RealmClosed = 24,
        
        ObjectManagedByAnotherRealm = 25,

        KeyAlreadyExists = 26,
     
        RealmDotNetExceptionDuringMigration = 30,

        NotNullableProperty = 31,
        
        PropertyMismatch = 32,

        StdArgumentOutOfRange = 100,

        StdIndexOutOfRange = 101,

        StdInvalidOperation = 102,

        AppClientError = 50,

        AppCustomError = 51,

        AppHttpError = 52,

        AppJsonError = 53,

        AppServiceError = 54,

        AppUnknownError = 59,
    };

}   // namespace realm
