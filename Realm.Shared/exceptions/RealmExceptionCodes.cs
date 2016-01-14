/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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
        RealmFormatUpgradeRequired = 13,

        StdArgumentOutOfRange = 100,
        StdIndexOutOfRange = 101,
        StdInvalidOperation = 102
    }
} // namespace Realms
