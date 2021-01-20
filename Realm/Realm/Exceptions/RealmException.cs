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
using System.Collections.Generic;
using Realms.Sync.Exceptions;

namespace Realms.Exceptions
{
    /// <summary>
    /// Base for Realm specific exceptions.
    /// </summary>
    public class RealmException : Exception
    {
        private static readonly IDictionary<RealmExceptionCodes, Func<string, string, Exception>> _overriders = new Dictionary<RealmExceptionCodes, Func<string, string, Exception>>();

        internal static void AddOverrider(RealmExceptionCodes code, Func<string, string, Exception> handler)
        {
            if (!_overriders.ContainsKey(code))
            {
                _overriders.Add(code, handler);
            }
        }

        internal RealmException(string detailMessage) : base(detailMessage)
        {
        }

        internal RealmException(string detailMessage, Exception innerException) : base(detailMessage, innerException)
        {
        }

        internal static Exception Create(RealmExceptionCodes exceptionCode, string message, string detail)
        {
            if (_overriders.TryGetValue(exceptionCode, out var handler))
            {
                return handler(message, detail);
            }

            // these are increasing enum value order
            switch (exceptionCode)
            {
                case RealmExceptionCodes.RealmError:
                    return new RealmException(message);

                case RealmExceptionCodes.RealmFileAccessError:
                    return new RealmFileAccessErrorException(message);

                case RealmExceptionCodes.RealmDecryptionFailed:
                    return new RealmDecryptionFailedException(message);

                case RealmExceptionCodes.RealmFileExists:
                    return new RealmFileExistsException(message);

                case RealmExceptionCodes.RealmFileNotFound:
                    return new RealmFileNotFoundException(message);

                case RealmExceptionCodes.RealmInvalidDatabase:
                    return new RealmInvalidDatabaseException(message);

                case RealmExceptionCodes.RealmOutOfMemory:
                    return new RealmOutOfMemoryException(message);

                case RealmExceptionCodes.RealmPermissionDenied:
                    return new RealmPermissionDeniedException(message);

                case RealmExceptionCodes.RealmMismatchedConfig:
                    return new RealmMismatchedConfigException(message);

                case RealmExceptionCodes.RealmInvalidTransaction:
                    return new RealmInvalidTransactionException(message);

                case RealmExceptionCodes.RealmFormatUpgradeRequired:
                    return new RealmException(message);  // rare unrecoverable case for now

                case RealmExceptionCodes.RealmSchemaMismatch:
                    return new RealmMigrationNeededException(message);

                case RealmExceptionCodes.RealmRowDetached:
                    return new RealmInvalidObjectException(message);

                case RealmExceptionCodes.RealmTableHasNoPrimaryKey:
                    return new RealmClassLacksPrimaryKeyException(message);

                case RealmExceptionCodes.RealmDuplicatePrimaryKeyValue:
                    return new RealmDuplicatePrimaryKeyValueException(message);

                case RealmExceptionCodes.RealmDotNetExceptionDuringMigration:
                    return new ManagedExceptionDuringMigrationException(message);

                case RealmExceptionCodes.RealmClosed:
                    return new RealmClosedException(message);

                case RealmExceptionCodes.NotNullableProperty:
                case RealmExceptionCodes.PropertyMismatch:
                    return new RealmException(message);

                case RealmExceptionCodes.AppClientError:
                case RealmExceptionCodes.AppCustomError:
                case RealmExceptionCodes.AppHttpError:
                case RealmExceptionCodes.AppJsonError:
                case RealmExceptionCodes.AppServiceError:
                case RealmExceptionCodes.AppUnknownError:
                    return new AppException(message, helpLink: null, httpStatusCode: 0);

                case RealmExceptionCodes.StdArgumentOutOfRange:
                case RealmExceptionCodes.StdIndexOutOfRange:
                    return new ArgumentOutOfRangeException(message);

                case RealmExceptionCodes.StdInvalidOperation:
                    return new InvalidOperationException(message);

                case RealmExceptionCodes.ObjectManagedByAnotherRealm:
                    return new RealmObjectManagedByAnotherRealmException(message);

                case RealmExceptionCodes.KeyAlreadyExists:
                    return new ArgumentException(message);

                default:
                    return new Exception(message);
            }
        }
    }
}