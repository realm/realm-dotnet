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
 
namespace Realms {

/// <summary>Codes used in forwarding exceptions from the native C++ core, to be regenerated in C#.</summary>
/// <remarks> <b>Warning:</b> Keep these codes aligned with realm_error_type.hpp in wrappers.</remarks>
    public enum RealmExceptionCodes {
        RealmError = 0,
        RealmFileAccessError = 1,
        RealmDecryptionFailed = 2,
        RealmFileExists = 3,
        RealmFileNotFound = 4,
        RealmInvalidDatabase = 5,
        RealmOutOfMemory = 6,
        RealmPermissionDenied = 7,
        RealmMismatchedConfig = 9,
        RealmInvalidTransaction = 10,
        RealmFormatUpgradeRequired = 13,

        StdArgumentOutOfRange = 100,
        StdIndexOutOfRange = 101,
        StdInvalidOperation = 102
    }
} // namespace Realms
