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

using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A class, representing the credentials used for authenticating a <see cref="User"/>.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// An enum containing the possible authentication providers. These have to manually be enabled for
        /// your app before they can be used.
        /// </summary>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/providers/"/>
        public enum AuthProvider
        {
            /// <summary>
            /// Mechanism for authenticating without credentials.
            /// </summary>
            Anonymous = 0,

            /// <summary>
            /// OAuth2-based mechanism for logging in with an existing Facebook account.
            /// </summary>
            Facebook = 1,

            /// <summary>
            /// OAuth2-based mechanism for logging in with an existing Google account.
            /// </summary>
            Google = 2,

            /// <summary>
            /// OAuth2-based mechanism for logging in with an Apple ID.
            /// </summary>
            Apple = 3,

            /// <summary>
            /// Allow users to log in with JWT-based credentials generated by a service external to Realm.
            /// </summary>
            JWT = 4,

            /// <summary>
            /// Mechanism for authenticating with an email and a password.
            /// </summary>
            EmailPassword = 5,

            /// <summary>
            /// Allow users to log in with arbitrary credentials according to custom authentication logic that you define
            /// on the server.
            /// </summary>
            Function = 6,

            /// <summary>
            /// Mechanism for logging in with API keys generated by the client SDK.
            /// </summary>
            ApiKey = 7,

            /// <summary>
            /// Mechanism for logging in with API keys generated in the server UI.
            /// </summary>
            ServerApiKey = 8,

            /// <summary>
            /// A provider that is not among the well known provider types. This is most likely the result of the server
            /// introducing a new provider type that this version of the SDK doesn't know about.
            /// </summary>
            Unknown = 999,
        }

        /// <summary>
        /// Creates credentials representing an anonymous user.
        /// </summary>
        /// <returns>A Credentials that can be used to authenticate an anonymous user.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/anonymous/"/>
        public static Credentials Anonymous() => new Credentials(AuthProvider.Anonymous);

        /// <summary>
        /// Creates credentials representing a login using a Facebook access token.
        /// </summary>
        /// <param name="facebookToken">The OAuth 2.0 access token representing the Facebook user.</param>
        /// <returns>A Credentials that can be used to authenticate a user with Facebook.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/facebook/"/>
        public static Credentials Facebook(string facebookToken)
        {
            Argument.NotNull(facebookToken, nameof(facebookToken));

            return new Credentials(AuthProvider.Facebook, facebookToken);
        }

        /// <summary>
        /// Creates credentials representing a login using a Google access token.
        /// </summary>
        /// <param name="googleToken">The OAuth 2.0 access token representing the Google user.</param>
        /// <returns>A Credentials that can be used to authenticate a user with Google.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/google/"/>
        public static Credentials Google(string googleToken)
        {
            Argument.NotNull(googleToken, nameof(googleToken));

            return new Credentials(AuthProvider.Google, googleToken);
        }

        /// <summary>
        /// Creates credentials representing a login using an Apple ID access token.
        /// </summary>
        /// <param name="appleToken">The OAuth 2.0 access token representing the user's Apple ID.</param>
        /// <returns>A Credentials that can be used to authenticate a user via an Apple ID.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/google/"/>
        public static Credentials Apple(string appleToken)
        {
            Argument.NotNull(appleToken, nameof(appleToken));

            return new Credentials(AuthProvider.Apple, appleToken);
        }

        /// <summary>
        /// Creates credentials representing a login using a JWT Token.
        /// </summary>
        /// <param name="customToken">The custom JWT token representing the user.</param>
        /// <returns>A Credentials that can be used to authenticate a user with a custom JWT Token.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/custom-jwt/"/>
        public static Credentials JWT(string customToken)
        {
            Argument.NotNull(customToken, nameof(customToken));

            return new Credentials(AuthProvider.JWT, customToken);
        }

        /// <summary>
        /// Creates credentials representing a login using an email and password.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A Credentials that can be used to authenticate a user with their email and password.</returns>
        /// <remarks>
        /// A user can login with email and password only after they've registered their account and verified their
        /// email. To register an email/password user via the SDK, use <see cref="App.EmailPasswordApi.RegisterUserAsync"/>.
        /// To verify an email from the SDK, use <see cref="App.EmailPasswordApi.ConfirmUserAsync"/>. The email/password
        /// provider can also be configured to automatically confirm users or to run a custom confirmation function upon
        /// user registration.
        /// </remarks>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/email-password/"/>
        public static Credentials EmailPassword(string email, string password)
        {
            return new Credentials(AuthProvider.EmailPassword, email, password);
        }

        /// <summary>
        /// Creates credentials represetning a login with Realm function.
        /// </summary>
        /// <param name="payload">The payload that will be passed as an argument to the server function.</param>
        /// <returns>A Credentials that can be used to authenticate a user by invoking a server function.</returns>
        /// <remarks>
        /// The payload object will be serialized and parsed when invoking the Realm function. This means that
        /// unserializable values, such as references to functions or cyclic object graphs will not work.
        /// Additionally, the names of the fields/properties must match exactly the names that your function
        /// expects.
        /// </remarks>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/anonymous/"/>
        public static Credentials Function(object payload)
        {
            var jsonPayload = payload == null ? string.Empty : BsonDocument.Create(payload).ToString();

            return new Credentials(AuthProvider.Function, jsonPayload);
        }

        /// <summary>
        /// Creates credentials representing a login using an API key generated by a client SDK.
        /// </summary>
        /// <param name="key">The API key to use for login.</param>
        /// <returns>A Credentials that can be used to authenticate user with an API key.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/api-key/"/>
        public static Credentials ApiKey(string key)
        {
            Argument.NotNull(key, nameof(key));

            return new Credentials(AuthProvider.ApiKey, key);
        }

        /// <summary>
        /// Creates credentials representing a login using an API key generated in the server UI.
        /// </summary>
        /// <param name="serverApiKey">The server API key to use for login.</param>
        /// <returns>A Credentials that can be used to authenticate user with an API key.</returns>
        /// <seealso href="https://docs.mongodb.com/realm/authentication/api-key/"/>
        public static Credentials ServerApiKey(string serverApiKey)
        {
            Argument.NotNull(serverApiKey, nameof(serverApiKey));

            return new Credentials(AuthProvider.ServerApiKey, serverApiKey);
        }

        /// <summary>
        /// Gets a value indicating which <see cref="AuthProvider"/> these Credentials are using.
        /// </summary>
        public AuthProvider Provider { get; }

        internal string Token { get; }

        internal string Password { get; }

        private Credentials(AuthProvider provider, string token = null, string password = null)
        {
            Provider = provider;
            Token = token;
            Password = password;
        }

        internal Native.Credentials ToNative()
        {
            return new Native.Credentials
            {
                provider = Provider,
                Token = Token,
                Password = Password,
            };
        }
    }
}
