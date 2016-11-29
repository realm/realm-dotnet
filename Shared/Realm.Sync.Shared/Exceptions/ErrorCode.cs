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

namespace Realms.Sync
{
    public enum ErrorCode
    {
        /// <summary>
        /// Unrecognized error code. It usually indicates incompatibility between the authentication server and client SDK versions.
        /// </summary>
        Unknown = -1,

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
}