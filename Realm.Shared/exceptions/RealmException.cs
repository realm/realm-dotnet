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
