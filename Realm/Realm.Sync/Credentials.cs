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
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A class, representing the credentials used for authenticating a <see cref="User"/>.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// A class, containing the names for the built in providers.
        /// </summary>
        public static class Provider
        {
            internal const string Debug = "debug";

            internal const string AdminToken = "adminToken";

            /// <summary>
            /// The Facebook provider, associated with <see cref="Credentials.Facebook"/>.
            /// </summary>
            public const string Facebook = "facebook";

            /// <summary>
            /// The Google provider, associated with <see cref="Credentials.Google"/>.
            /// </summary>
            public const string Google = "google";

            /// <summary>
            /// The Username/Password provider, associated with <see cref="Credentials.UsernamePassword"/>.
            /// </summary>
            public const string UsernamePassword = "password";

            /// <summary>
            /// The Azure Active Directory provider, associated with <see cref="Credentials.AzureAD"/>.
            /// </summary>
            public const string AzureAD = "azuread";

            /// <summary>
            /// The Json Web Token provider, associated with <see cref="Credentials.JWT"/>.
            /// </summary>
            public const string JWT = "jwt";
        }

        internal static class Keys
        {
            internal const string CreateUser = "register";

            internal const string Password = "password";

            internal const string Identity = "identity";

            internal const string IsAdmin = "isAdmin";

            internal const string IsAnonymous = "is_anonymous";
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
                IdentityProvider = Provider.Debug
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Facebook login.
        /// </summary>
        /// <param name="facebookToken">A Facebook authentication token, obtained by logging into Facebook.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Facebook(string facebookToken)
        {
            Argument.NotNull(facebookToken, nameof(facebookToken));

            return new Credentials
            {
                IdentityProvider = Provider.Facebook,
                Token = facebookToken
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Google login.
        /// </summary>
        /// <param name="googleToken">A Google authentication token, obtained by logging into Google.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials Google(string googleToken)
        {
            Argument.NotNull(googleToken, nameof(googleToken));

            return new Credentials
            {
                IdentityProvider = Provider.Google,
                Token = googleToken
            };
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
                IdentityProvider = Provider.UsernamePassword,
                Token = username,
                UserInfo = new Dictionary<string, object> { [Keys.CreateUser] = createUser, [Keys.Password] = password }
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> for an anonymous user.
        /// </summary>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        /// <remarks>
        /// This is using the <see cref="UsernamePassword"/> credentials by providing random username and password.
        /// </remarks>
        public static Credentials Anonymous()
        {
            return new Credentials
            {
                IdentityProvider = Provider.UsernamePassword,
                Token = $"Anonymous_{Guid.NewGuid()}",
                UserInfo = new Dictionary<string, object>
                {
                    [Keys.CreateUser] = true,
                    [Keys.Password] = Guid.NewGuid().ToString(),
                    [Keys.IsAnonymous] = true
                }
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a login with a nickname. If multiple users try to login
        /// with the same nickname, they'll get the same underlying sync user.
        /// </summary>
        /// <param name="value">The nickname of the user.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        /// <remarks>
        /// This is using the <see cref="UsernamePassword"/> credentials by providing value as username and empty string as password.
        /// </remarks>
        public static Credentials Nickname(string value)
        {
            return new Credentials
            {
                IdentityProvider = Provider.UsernamePassword,
                Token = $"Nickname_{value}",
                UserInfo = new Dictionary<string, object>
                {
                    [Keys.Password] = string.Empty
                }
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on an Active Directory login.
        /// </summary>
        /// <param name="adToken">An access token, obtained by logging into Azure Active Directory.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials AzureAD(string adToken)
        {
            Argument.NotNull(adToken, nameof(adToken));

            return new Credentials
            {
                IdentityProvider = Provider.AzureAD,
                Token = adToken
            };
        }

        /// <summary>
        /// Creates <see cref="Credentials"/> based on a Facebook login.
        /// </summary>
        /// <param name="token">A Json Web Token, obtained by logging into Facebook.</param>
        /// <returns>An instance of <see cref="Credentials"/> that can be used in <see cref="User.LoginAsync"/></returns>
        public static Credentials JWT(string token)
        {
            Argument.NotNull(token, nameof(token));

            return new Credentials
            {
                IdentityProvider = Provider.JWT,
                Token = token
            };
        }

        internal static Credentials AdminToken(string token)
        {
            return new Credentials
            {
                IdentityProvider = Provider.AdminToken,
                Token = token
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
