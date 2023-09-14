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
using Realms.Sync.ErrorHandling;
using static System.Net.WebRequestMethods;

namespace Realms.Sync.Exceptions;

/// <summary>
/// Error code enumeration, indicating the type of the session error.
/// </summary>
/// <seealso cref="SessionException"/>
public enum ErrorCode
{
    /// <summary>
    /// Unrecognized error code. It usually indicates incompatibility between the App Services server and client SDK versions.
    /// </summary>
    RuntimeError = 1000,

    /// <summary>
    /// The partition value specified by the user is not valid - i.e. its the wrong type or is encoded incorrectly.
    /// </summary>
    BadPartitionValue = 1029,

    /// <summary>
    /// A fundamental invariant in the communication between the client and the server was not upheld. This typically indicates
    /// a bug in the synchronization layer and should be reported at https://github.com/realm/realm-core/issues.
    /// </summary>
    ProtocolInvariantFailed = 1038,

    /// <summary>
    /// The changeset is invalid.
    /// </summary>
    BadChangeset = 1015,

    /// <summary>
    /// The client attempted to create a subscription for a query is invalid/malformed.
    /// </summary>
    BadQuery = 1031,

    /// <summary>
    /// A client reset has occurred. This error code will only be reported via a <see cref="ClientResetException"/> and only
    /// in the case manual client reset handling is required - either via <see cref="ManualRecoveryHandler"/> or when
    /// <c>ManualResetFallback</c> is invoked on one of the automatic client reset handlers.
    /// </summary>
    /// <seealso cref="SyncConfigurationBase.ClientResetHandler"/>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset"/>
    ClientReset = 1032,

    /// <summary>
    /// The client attempted to upload an invalid schema change - either an additive schema change
    /// when developer mode is <c>off</c> or a destructive schema change.
    /// </summary>
    InvalidSchemaChange = 1036,

    /// <summary>
    /// Permission to Realm has been denied.
    /// </summary>
    PermissionDenied = 1037,

    /// <summary>
    /// The server permissions for this file have changed since the last time it was used.
    /// </summary>
    ServerPermissionsChanged = 1040,

    /// <summary>
    /// The user for this session doesn't match the user who originally created the file. This can happen
    /// if you explicitly specify the Realm file path in the configuration and you open the Realm first with
    /// user A, then with user B without changing the on-disk path.
    /// </summary>
    UserMismatch = 1041,

    /// <summary>
    /// Client attempted a write that is disallowed by permissions, or modifies an object
    /// outside the current query - requires client reset.
    /// </summary>
    WriteNotAllowed = 1044,

    /// <summary>
    /// Automatic client reset has failed. This will only be reported via <see cref="ClientResetException"/>
    /// when an automatic client reset handler was used but it failed to perform the client reset operation -
    /// typically due to a breaking schema change in the server schema or due to an exception occurring in the
    /// before or after client reset callbacks.
    /// </summary>
    AutoClientResetFailed = 1028,

    /// <summary>
    /// The wrong sync type was used to connect to the server. This means that you're using <see cref="PartitionSyncConfiguration"/>
    /// to connect to an app configured for flexible sync or that you're using <see cref="FlexibleSyncConfiguration"/> to connect
    /// to an app configured to use partition sync.
    /// </summary>
    WrongSyncType = 1043,

    /// <summary>
    /// Unrecognized error code. It usually indicates incompatibility between the App Services server and client SDK versions.
    /// </summary>
    [Obsolete("Use RuntimeError instead.")]
    Unknown = RuntimeError,

    /// <summary>
    /// Other session level error has occurred.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. See the obsoletion message for details on the new error code.
    /// </remarks>
    [Obsolete("Use RuntimeError instead.")]
    OtherSessionError = RuntimeError,

    /// <summary>
    /// Path to Realm is invalid.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. See the obsoletion message for details on the new error code.
    /// </remarks>
    [Obsolete("Use BadPartitionValue instead")]
    IllegalRealmPath = BadPartitionValue,

    /// <summary>
    /// The client file identifier is invalid.
    /// </summary>
    /// <seealso cref="ClientResetException"/>
    [Obsolete("Use ClientReset instead")]
    BadClientFileIdentifier = ClientReset,

    /// <summary>
    /// The server version is invalid.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. See the obsoletion message for details on the new error code.
    /// </remarks>
    [Obsolete("Use ProtocolInvariantFailed instead")]
    BadServerVersion = ProtocolInvariantFailed,

    /// <summary>
    /// The client version is invalid.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. See the obsoletion message for details on the new error code.
    /// </remarks>
    [Obsolete("Use ProtocolInvariantFailed instead")]
    BadClientVersion = ProtocolInvariantFailed,

    /// <summary>
    /// Histories have diverged and cannot be merged.
    /// </summary>
    /// <seealso cref="ClientResetException"/>
    [Obsolete("Use ClientReset instead")]
    DivergingHistories = ClientReset,

    /// <summary>
    /// The client file is invalid.
    /// </summary>
    /// <seealso cref="ClientResetException"/>
    [Obsolete("Use ClientReset instead")]
    BadClientFile = ClientReset,

    /// <summary>
    /// Client file has expired likely due to history compaction on the server.
    /// </summary>
    /// <seealso cref="ClientResetException"/>
    [Obsolete("Use ClientReset instead")]
    ClientFileExpired = ClientReset,

    /// <summary>
    /// The server has received too many sessions from this client. This is typically a transient error
    /// but can also indicate that the client has too many Realms open at the same time.
    /// </summary>
    [Obsolete("This error code is no longer reported")]
    TooManySessions = -2,

    /// <summary>
    /// The client attempted to create an object that already exists outside their view.
    /// </summary>
    [Obsolete("This error code is no longer reported")]
    ObjectAlreadyExists = -3,

    /// <summary>
    /// The client tried to synchronize before initial sync has completed. Please wait for
    /// the server process to complete and try again.
    /// </summary>
    [Obsolete("This error code is no longer reported")]
    InitialSyncNotCompleted = -4,

    /// <summary>
    /// Client attempted a write that is disallowed by permissions, or modifies an
    /// object outside the current query, and the server undid the modification.
    /// </summary>
    /// <seealso cref="CompensatingWriteException"/>
    CompensatingWrite = 1033,

    /// <summary>
    /// An error sent by the server when its data structures used to track client progress
    /// become corrupted.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. See the obsoletion message for details on the new error code.
    /// </remarks>
    [Obsolete("Use ProtocolInvariantFailed instead")]
    BadProgress = ProtocolInvariantFailed,
}
