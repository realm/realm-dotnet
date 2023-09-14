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

namespace Realms.Sync.Exceptions;

/// <summary>
/// Error code enumeration, indicating the type of the session error.
/// </summary>
/// <seealso cref="SessionException"/>
public enum ErrorCode
{
    /// <summary>
    /// Unrecognized error code. It usually indicates incompatibility between the authentication server and client SDK versions.
    /// </summary>
    Unknown = -1,

    RuntimeError = 1000,

    BadPartitionValue = 1029,

    ProtocolInvariantFailed = 1038,

    /// <summary>
    /// The changeset is invalid.
    /// </summary>
    BadChangeset = 1015,

    /// <summary>
    /// The client attempted to create a subscription for a query is invalid/malformed.
    /// </summary>
    BadQuery = 1031,

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

    AutoClientResetFailed = 1028,

    WrongSyncType = 1043,

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
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. This error code is no longer reported via <see cref="SessionException"/>
    /// and instead is thrown as <see cref="ClientResetException"/>.
    /// </remarks>
    [Obsolete("Use ClientResetException instead")]
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
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. This error code is no longer reported via <see cref="SessionException"/>
    /// and instead is thrown as <see cref="ClientResetException"/>.
    /// </remarks>
    [Obsolete("Use ClientResetException instead")]
    DivergingHistories = ClientReset,

    /// <summary>
    /// The client file is invalid.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. This error code is no longer reported via <see cref="SessionException"/>
    /// and instead is thrown as <see cref="ClientResetException"/>.
    /// </remarks>
    [Obsolete("Use ClientResetException instead")]
    BadClientFile = ClientReset,

    /// <summary>
    /// Client file has expired likely due to history compaction on the server.
    /// </summary>
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. This error code is no longer reported via <see cref="SessionException"/>
    /// and instead is thrown as <see cref="ClientResetException"/>.
    /// </remarks>
    [Obsolete("Use ClientResetException instead")]
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
    /// <remarks>
    /// Sync error reporting has been simplified and some errors have been unified. This error code is no longer reported via <see cref="SessionException"/>
    /// and instead is thrown as <see cref="CompensatingWriteException"/>.
    /// </remarks>
    [Obsolete("Use CompensatingWriteException instead")]
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
