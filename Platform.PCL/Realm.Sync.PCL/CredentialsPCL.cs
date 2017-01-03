﻿////////////////////////////////////////////////////////////////////////////
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

namespace Realms.Sync
{
    /// <summary>
    /// A class, representing the credentials used for authenticating a <see cref="User"/>.
    /// </summary>
    public class Credentials
    {
        internal static class Providers
        {
            internal const string Debug = "debug";

            internal const string Facebook = "facebook";

            internal const string Google = "google";

            internal const string Twitter = "twitter";

            internal const string Password = "password";

            internal const string AccessToken = "accessToken";
        }

        internal static class Keys
        {
            internal const string CreateUser = "register";

            internal const string Password = "password";

            internal const string Identity = "identity";

            internal const string IsAdmin = "isAdmin";
        }

        /// <summary>
        /// Creates an instance of <see cref="Credentials"/> with a custom provider and user identifier.
        /// </summary>
        /// <param name="identityProvider">Provider used to verify the credentials.</param>
        /// <param name="userIdentifier">String identifying the user. Usually a username of id.</param>
        /// <param name="userInfo">Data describing the user further or null if the user does not have any extra data. The data will be serialized to JSON, so all values must be mappable to a valid JSON data type.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Custom(string identityProvider, string userIdentifier, IDictionary<string, object> userInfo)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Creates an instance of <see cref="Credentials"/> to be used during development. Not enabled for Realm Object Server configured for production.
        /// </summary>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Debug()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Facebook login.
        /// </summary>
        /// <param name="facebookToken">A Facebook authentication token, obtained by logging into Facebook.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Facebook(string facebookToken)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Google login.
        /// </summary>
        /// <param name="googleToken">A Google authentication token, obtained by logging into Google.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Google(string googleToken)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Twitter login.
        /// </summary>
        /// <param name="twitterToken">A Twitter authentication token, obtained by logging into Twitter.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Twitter(string twitterToken)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a login with a username and a password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="createUser"><c>true</c> if the user should be created, <c>false</c> otherwise. It is not possible to create a user twice when logging in, so this flag should only be set to true the first time a user logs in.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials UsernamePassword(string username, string password, bool createUser)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        internal static Credentials AccessToken(string accessToken, string identity, bool isAdmin = false)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Gets the identity provider for the credentials.
        /// </summary>
        public string IdentityProvider
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string Token
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets additional user information associated with the credentials.
        /// </summary>
        public IReadOnlyDictionary<string, object> UserInfo
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        private Credentials()
        {
        }
    }
}
