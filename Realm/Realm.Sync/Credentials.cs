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
using System.Collections.ObjectModel;

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

            internal const string Password = "password";

            internal const string AccessToken = "accessToken";

            internal const string AzureAD = "azuread";
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
        public static Credentials Custom(string identityProvider, string userIdentifier,
            IDictionary<string, object> userInfo)
        {
            return new Credentials
            {
                IdentityProvider = identityProvider,
                Token = userIdentifier,
                UserInfo = new ReadOnlyDictionary<string, object>(userInfo)
            };
        }

        /// <summary>
        /// Creates an instance of <see cref="Credentials"/> to be used during development. Not enabled for Realm Object Server configured for production.
        /// </summary>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Debug()
        {
            return new Credentials
            {
                IdentityProvider = Providers.Debug
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Facebook login.
        /// </summary>
        /// <param name="facebookToken">A Facebook authentication token, obtained by logging into Facebook.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Facebook(string facebookToken)
        {
            if (facebookToken == null)
            {
                throw new ArgumentNullException(nameof(facebookToken));
            }

            return new Credentials { IdentityProvider = Providers.Facebook, Token = facebookToken };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Google login.
        /// </summary>
        /// <param name="googleToken">A Google authentication token, obtained by logging into Google.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Google(string googleToken)
        {
            if (googleToken == null)
            {
                throw new ArgumentNullException(nameof(googleToken));
            }

            return new Credentials { IdentityProvider = Providers.Google, Token = googleToken };
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
            return new Credentials 
            { 
                IdentityProvider = Providers.Password,
                Token = username,
                UserInfo = new Dictionary<string, object> { [Keys.CreateUser] = createUser, [Keys.Password] = password }
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on an Active Directory login.
        /// </summary>
        /// <param name="adToken">An access token, obtained by logging into Azure Active Directory.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials AzureAD(string adToken)
        {
            if (adToken == null)
            {
                throw new ArgumentNullException(nameof(adToken));
            }

            return new Credentials { IdentityProvider = Providers.AzureAD, Token = adToken };
        }

        internal static Credentials AccessToken(string accessToken, string identity, bool isAdmin = false)
        {
            return new Credentials
            {
                IdentityProvider = Providers.AccessToken,
                Token = accessToken,
                UserInfo = new Dictionary<string, object> { [Keys.Identity] = identity, [Keys.IsAdmin] = isAdmin }
            };
        }

        /// <summary>
        /// Gets the identity provider for the credentials.
        /// </summary>
        /// <value>The identity provider, such as Google, Facebook, etc.</value>
        public string IdentityProvider { get; private set; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string Token { get; private set; }

        /// <summary>
        /// Gets additional user information associated with the credentials.
        /// </summary>
        /// <value>A dictionary, containing the additional information.</value>
        public IReadOnlyDictionary<string, object> UserInfo { get; private set; } = new Dictionary<string, object>();

        private Credentials()
        {
        }

        internal IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["data"] = Token,
                ["provider"] = IdentityProvider,
                ["user_info"] = UserInfo
            };
        }
    }
}
