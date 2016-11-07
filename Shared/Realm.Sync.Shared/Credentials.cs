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

namespace Realms.Sync
{
    public class Credentials
    {
        public static Credentials Custom(string identityProvider, string userIdentifier,
            IDictionary<string, object> userInfo)
        {
            return new Credentials
            {
                IdentityProvider = identityProvider,
                UserIdentifier = userIdentifier,
                UserInfo = userInfo
            };
        }

        public static Credentials Facebook(string facebookToken)
        {
            if (facebookToken == null)
            {
                throw new ArgumentNullException(nameof(facebookToken));
            }

            return new Credentials { IdentityProvider = "facebook", UserIdentifier = facebookToken };
        }

        public static Credentials Google(string googleToken)
        {
            if (googleToken == null)
            {
                throw new ArgumentNullException(nameof(googleToken));
            }

            return new Credentials { IdentityProvider = "google", UserIdentifier = googleToken };
        }

        public static Credentials Twitter(string twitterToken)
        {
            if (twitterToken == null)
            {
                throw new ArgumentNullException(nameof(twitterToken));
            }

            return new Credentials { IdentityProvider = "twitter", UserIdentifier = twitterToken };
        }

        public static Credentials UsernamePassword(string username, string password, bool createUser)
        {
            return new Credentials 
            { 
                IdentityProvider = "password",
                UserIdentifier = username,
                UserInfo = new Dictionary<string, object> { { "register", createUser }, { "password", password } }
            };
        }

        public string IdentityProvider { get; private set; }

        public string UserIdentifier { get; private set; }

        public IDictionary<string, object> UserInfo { get; private set; } = new Dictionary<string, object>();
    }
}
