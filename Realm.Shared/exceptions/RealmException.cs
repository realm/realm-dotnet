/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace Realms {

    /// <summary>
    /// Base for Realm specific exceptions. Use only for catching.
    /// </summary>
    public class RealmException :  Exception {

        internal RealmException(String detailMessage) : base(detailMessage)
        {
        }

        public static Exception Create(RealmExceptionCodes exceptionCode, string message)
        {
            // these are increasing enum value order
            switch (exceptionCode) {
                case RealmExceptionCodes.RealmError:
                    return new RealmException(message);

                case RealmExceptionCodes.RealmFileAccessError:
                    return new RealmFileAccessErrorException(message);

                case RealmExceptionCodes.RealmDecryptionFailed:
                    return new RealmDecryptionFailedException(message);

                case RealmExceptionCodes.RealmFileExists:
                    return new RealmFileExistsException(message);

                case RealmExceptionCodes.RealmFileNotFound :
                    return new RealmFileNotFoundException(message);

                case RealmExceptionCodes.RealmInvalidDatabase :
                    return new RealmInvalidDatabaseException(message);

                case RealmExceptionCodes.RealmOutOfMemory :
                    return new RealmOutOfMemoryException(message);

                case RealmExceptionCodes.RealmPermissionDenied :
                    return new RealmPermissionDeniedException(message);

                case RealmExceptionCodes.RealmMismatchedConfig:
                    return new RealmMismatchedConfigException(message);

                case RealmExceptionCodes.RealmInvalidTransaction:
                    return new RealmInvalidTransactionException(message);

                case RealmExceptionCodes.RealmFormatUpgradeRequired :
                    return new RealmMigrationNeededException(message);

                case RealmExceptionCodes.StdArgumentOutOfRange :
                    return new ArgumentOutOfRangeException(message);

                case RealmExceptionCodes.StdIndexOutOfRange :
                    return new IndexOutOfRangeException(message);

                case RealmExceptionCodes.StdInvalidOperation :
                    return new InvalidOperationException(message);

                default:
                    return new Exception(message);
            }

        }
}

}  // namespace Realms
