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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// Error code enumeration, indicating the type of the error.
    /// </summary>
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
        DisabledSession = 213,

        /// <summary>
        /// Your request parameters did not validate.
        /// </summary>
        InvalidParameters = 601,

        /// <summary>
        /// Your request did not validate because of missing parameters.
        /// </summary>
        MissingParameters = 602,

        /// <summary>
        /// The provided credentials are invalid.
        /// </summary>
        InvalidCredentials = 611,

        /// <summary>
        /// The account does not exist.
        /// </summary>
        UnknownAccount = 612,

        /// <summary>
        /// The account cannot be registered as it exists already.
        /// </summary>
        ExistingAccount = 613,

        /// <summary>
        /// The path is invalid or current user has no access.
        /// </summary>
        AccessDenied = 614,

        /// <summary>
        /// The refresh token is expired.
        /// </summary>
        ExpiredRefreshToken = 615,

        /// <summary>
        /// The server is not authoritative for this URL.
        /// </summary>
        InvalidHost = 616,

        /// <summary>
        /// The permission offer is expired.
        /// </summary>
        ExpiredPermissionOffer = 701,

        /// <summary>
        /// The token used on the permission request does match more than a single permission offer.
        /// </summary>
        AmbiguousPermissionOfferToken = 702,

        /// <summary>
        /// The Realm file at the specified path is not available for shared access.
        /// </summary>
        FileMayNotBeShared = 703,
    }

    /// <summary>
    /// A set of extensions that simplify checking for common error scenarios.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ErrorCodeExtensions
    {
        private static readonly IEnumerable<ErrorCode> clientResetCodes = new[]
        {
            ErrorCode.BadServerFileIdentifier,
            ErrorCode.BadClientFileIdentifier,
            ErrorCode.BadServerVersion,
            ErrorCode.DivergingHistories,
        };

        /// <summary>
        /// Checks if an error code indicates that a client reset is needed.
        /// </summary>
        /// <returns><c>true</c>, if the code indicates a client reset error, <c>false</c> otherwise.</returns>
        /// <param name="code">The error code.</param>
        public static bool IsClientResetError(this ErrorCode code)
        {
            return clientResetCodes.Contains(code);
        }
    }
}