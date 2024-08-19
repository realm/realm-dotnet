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

namespace Realms.Exceptions
{
    /// <summary>
    /// Base for Realm specific exceptions.
    /// </summary>
    public class RealmException : Exception
    {
        internal RealmException(string detailMessage) : base(detailMessage)
        {
        }

        internal RealmException(string detailMessage, Exception innerException) : base(detailMessage, innerException)
        {
        }

        internal static Exception Create(RealmExceptionCodes exceptionCode, string message, RealmExceptionCategories categories)
        {
            // these are increasing enum value order
            switch (exceptionCode)
            {
                case RealmExceptionCodes.RLM_ERR_DECRYPTION_FAILED:
                    return new RealmDecryptionFailedException(message);

                case RealmExceptionCodes.RLM_ERR_FILE_ALREADY_EXISTS:
                    return new RealmFileExistsException(message);

                case RealmExceptionCodes.RLM_ERR_FILE_NOT_FOUND:
                    return new RealmFileNotFoundException(message);

                case RealmExceptionCodes.RLM_ERR_INVALID_DATABASE:
                    return new RealmInvalidDatabaseException(message);

                case RealmExceptionCodes.RLM_ERR_OUT_OF_MEMORY:
                    return new RealmOutOfMemoryException(message);

                case RealmExceptionCodes.RLM_ERR_FILE_PERMISSION_DENIED:
                    return new RealmPermissionDeniedException(message);

                case RealmExceptionCodes.RLM_ERR_MISMATCHED_CONFIG:
                    return new RealmMismatchedConfigException(message);

                case RealmExceptionCodes.RLM_ERR_WRONG_TRANSACTION_STATE:
                    return new RealmInvalidTransactionException(message);

                case RealmExceptionCodes.RLM_ERR_FILE_FORMAT_UPGRADE_REQUIRED:
                case RealmExceptionCodes.RLM_ERR_SCHEMA_MISMATCH:
                    return new RealmMigrationNeededException(message);

                case RealmExceptionCodes.RowDetached:
                    return new RealmInvalidObjectException(message);

                case RealmExceptionCodes.RLM_ERR_MISSING_PRIMARY_KEY:
                    return new RealmClassLacksPrimaryKeyException(message);

                case RealmExceptionCodes.DuplicatePrimaryKey:
                    return new RealmDuplicatePrimaryKeyValueException(message);

                case RealmExceptionCodes.RealmClosed:
                    return new RealmClosedException(message);

                case RealmExceptionCodes.RLM_ERR_SCHEMA_VALIDATION_FAILED:
                    return new RealmSchemaValidationException(message);

                case RealmExceptionCodes.PropertyTypeMismatch:
                    return new RealmException(message);

                case RealmExceptionCodes.IndexOutOfRange:
                    return new ArgumentOutOfRangeException(message);

                case RealmExceptionCodes.ObjectManagedByAnotherRealm:
                    return new RealmObjectManagedByAnotherRealmException(message);

                case RealmExceptionCodes.KeyAlreadyExists:
                case RealmExceptionCodes.DuplicateSubscription:
                    return new ArgumentException(message);

                case RealmExceptionCodes.InvalidGeospatialShape:
                    return new ArgumentException(message);

                case RealmExceptionCodes.RLM_ERR_DELETE_OPENED_REALM:
                    return new RealmInUseException(message);

                case RealmExceptionCodes.RLM_ERR_MIGRATION_FAILED:
                    return new RealmMigrationException(message);
            }

            if (categories.HasFlag(RealmExceptionCategories.RLM_ERR_CAT_FILE_ACCESS))
            {
                return new RealmFileAccessErrorException(message);
            }

            if (categories.HasFlag(RealmExceptionCategories.RLM_ERR_CAT_INVALID_ARG))
            {
                return new ArgumentException(message);
            }

            return new RealmException(message);
        }
    }
}
