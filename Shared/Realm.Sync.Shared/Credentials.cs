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
using System.Json;

namespace Realms.Sync
{
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

        public static Credentials Debug()
        {
            return new Credentials
            {
                IdentityProvider = Providers.Debug
            };
        }

        public static Credentials Facebook(string facebookToken)
        {
            if (facebookToken == null)
            {
                throw new ArgumentNullException(nameof(facebookToken));
            }

            return new Credentials { IdentityProvider = Providers.Facebook, Token = facebookToken };
        }

        public static Credentials Google(string googleToken)
        {
            if (googleToken == null)
            {
                throw new ArgumentNullException(nameof(googleToken));
            }

            return new Credentials { IdentityProvider = Providers.Google, Token = googleToken };
        }

        public static Credentials Twitter(string twitterToken)
        {
            if (twitterToken == null)
            {
                throw new ArgumentNullException(nameof(twitterToken));
            }

            return new Credentials { IdentityProvider = Providers.Twitter, Token = twitterToken };
        }

        public static Credentials UsernamePassword(string username, string password, bool createUser)
        {
            return new Credentials 
            { 
                IdentityProvider = Providers.Password,
                Token = username,
                UserInfo = new Dictionary<string, object> { [Keys.CreateUser] = createUser, [Keys.Password] = password }
            };
        }

        public static Credentials AccessToken(string accessToken, string identity, bool isAdmin = false)
        {
            return new Credentials
            {
                IdentityProvider = Providers.AccessToken,
                Token = accessToken,
                UserInfo = new Dictionary<string, object> { [Keys.Identity] = identity, [Keys.IsAdmin] = isAdmin }
            };
        }

        public string IdentityProvider { get; private set; }

        public string Token { get; private set; }

        public IReadOnlyDictionary<string, object> UserInfo { get; private set; } = new Dictionary<string, object>();

        private Credentials()
        {
        }

        internal JsonObject ToJson()
        {
            var user_info = new JsonObject();
            foreach (var kvp in UserInfo)
            {
                JsonValue value = null;
                if (kvp.Value is string)
                {
                    value = (string)kvp.Value;
                }
                else if (kvp.Value is bool)
                {
                    value = (bool)kvp.Value;
                }
                else
                {
                    System.Diagnostics.Debug.Fail($"Unsupported type in JSON conversion '{kvp.Value?.GetType()}'");
                }

                user_info.Add(kvp.Key, value);
            }

            return new JsonObject
            {
                ["data"] = Token,
                ["provider"] = IdentityProvider,
                ["user_info"] = user_info
            };
        }
    }
}
