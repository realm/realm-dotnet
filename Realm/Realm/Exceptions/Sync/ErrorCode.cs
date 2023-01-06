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

namespace Realms.Sync.Exceptions
{
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

        /// <summary>
        /// Session has been closed (no error).
        /// </summary>
        SessionClosed = 200,

        /// <summary>
        /// Other session level error has occurred.
        /// </summary>
        OtherSessionError = 201,

        /// <summary>
        /// Access token has already expired.
        /// </summary>
        AccessTokenExpired = 202,

        /// <summary>
        /// Failed to authenticate user.
        /// </summary>
        BadUserAuthentication = 203,

        /// <summary>
        /// Path to Realm is invalid.
        /// </summary>
        IllegalRealmPath = 204,

        /// <summary>
        /// Path points to non-existing Realm.
        /// </summary>
        NoSuchRealm = 205,

        /// <summary>
        /// Permission to Realm has been denied.
        /// </summary>
        PermissionDenied = 206,

        /// <summary>
        /// The server file identifier is invalid.
        /// </summary>
        BadServerFileIdentifier = 207,

        /// <summary>
        /// The client file identifier is invalid.
        /// </summary>
        BadClientFileIdentifier = 208,

        /// <summary>
        /// The server version is invalid.
        /// </summary>
        BadServerVersion = 209,

        /// <summary>
        /// The client version is invalid.
        /// </summary>
        BadClientVersion = 210,

        /// <summary>
        /// Histories have diverged and cannot be merged.
        /// </summary>
        DivergingHistories = 211,

        /// <summary>
        /// The changeset is invalid.
        /// </summary>
        BadChangeset = 212,

        /// <summary>
        /// The session has been disabled.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        DisabledSession = 213,

        /// <summary>
        /// The client file is invalid.
        /// </summary>
        BadClientFile = 217,

        /// <summary>
        /// Client file has expired likely due to history compaction on the server.
        /// </summary>
        ClientFileExpired = 222,

        /// <summary>
        /// The user for this session doesn't match the user who originally created the file. This can happen
        /// if you explicitly specify the Realm file path in the configuration and you open the Realm first with
        /// user A, then with user B without changing the on-disk path.
        /// </summary>
        UserMismatch = 223,

        /// <summary>
        /// The server has received too many sessions from this client. This is typically a transient error
        /// but can also indicate that the client has too many Realms open at the same time.
        /// </summary>
        TooManySessions = 224,

        /// <summary>
        /// The client attempted to upload an invalid schema change - either an additive schema change
        /// when developer mode is <c>off</c> or a destructive schema change.
        /// </summary>
        InvalidSchemaChange = 225,

        /// <summary>
        /// The client attempted to create a subscription for a query is invalid/malformed.
        /// </summary>
        BadQuery = 226,

        /// <summary>
        /// The client attempted to create an object that already exists outside their view.
        /// </summary>
        ObjectAlreadyExists = 227,

        /// <summary>
        /// The server permissions for this file have changed since the last time it was used.
        /// </summary>
        ServerPermissionsChanged = 228,

        /// <summary>
        /// The client tried to synchronize before initial sync has completed. Please wait for
        /// the server process to complete and try again.
        /// </summary>
        InitialSyncNotCompleted = 229,

        /// <summary>
        /// Client attempted a write that is disallowed by permissions, or modifies an object
        /// outside the current query - requires client reset.
        /// </summary>
        WriteNotAllowed = 230,

        /// <summary>
        /// Client attempted a write that is disallowed by permissions, or modifies an
        /// object outside the current query, and the server undid the modification.
        /// </summary>
        CompensatingWrite = 231,

        /// <summary>
        /// Your request parameters did not validate.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        InvalidParameters = 601,

        /// <summary>
        /// Your request did not validate because of missing parameters.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        MissingParameters = 602,

        /// <summary>
        /// The provided credentials are invalid.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        InvalidCredentials = 611,

        /// <summary>
        /// The account does not exist.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        UnknownAccount = 612,

        /// <summary>
        /// The account cannot be registered as it exists already.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        ExistingAccount = 613,

        /// <summary>
        /// The path is invalid or current user has no access.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        AccessDenied = 614,

        /// <summary>
        /// The refresh token is expired.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        ExpiredRefreshToken = 615,

        /// <summary>
        /// The server is not authoritative for this URL.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        InvalidHost = 616,

        /// <summary>
        /// The permission offer is expired.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        ExpiredPermissionOffer = 701,

        /// <summary>
        /// The token used on the permission request does match more than a single permission offer.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        AmbiguousPermissionOfferToken = 702,

        /// <summary>
        /// The Realm file at the specified path is not available for shared access.
        /// </summary>
        [Obsolete("This error can no longer happen")]
        FileMayNotBeShared = 703,
    }
}
