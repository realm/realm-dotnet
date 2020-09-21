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

using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A class, representing the credentials used for authenticating a <see cref="User"/>.
    /// </summary>
    public class Credentials
    {
        internal enum AuthProvider
        {
            Anonymous,
            Facebook,
            Google,
            Apple,
            JWT,
            UsernamePassword,
            Function,
            ApiKey,
            ServerApiKey
        }

        public static Credentials Anonymous() => new Credentials(AuthProvider.Anonymous);

        public static Credentials Facebook(string facebookToken)
        {
            Argument.NotNull(facebookToken, nameof(facebookToken));

            return new Credentials(AuthProvider.Facebook, facebookToken);
        }

        public static Credentials Google(string googleToken)
        {
            Argument.NotNull(googleToken, nameof(googleToken));

            return new Credentials(AuthProvider.Google, googleToken);
        }

        public static Credentials Apple(string appleToken)
        {
            Argument.NotNull(appleToken, nameof(appleToken));

            return new Credentials(AuthProvider.Apple, appleToken);
        }

        public static Credentials JWT(string customToken)
        {
            Argument.NotNull(customToken, nameof(customToken));

            return new Credentials(AuthProvider.JWT, customToken);
        }

        public static Credentials UsernamePassword(string username, string password)
        {
            return new Credentials(AuthProvider.UsernamePassword, username, password);
        }

        public static Credentials Function(BsonDocument document)
        {
            Argument.NotNull(document, nameof(document));

            return new Credentials(AuthProvider.Function, document.ToString());
        }

        public static Credentials ApiKey(string key)
        {
            Argument.NotNull(key, nameof(key));

            return new Credentials(AuthProvider.ApiKey, key);
        }

        public static Credentials ServerApiKey(string serverApiKey)
        {
            Argument.NotNull(serverApiKey, nameof(serverApiKey));

            return new Credentials(AuthProvider.ServerApiKey, serverApiKey);
        }

        internal AuthProvider Provider { get; }

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
