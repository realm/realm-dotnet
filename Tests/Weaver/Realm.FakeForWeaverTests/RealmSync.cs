////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

namespace Realms.Sync
{
    public class Credentials
    {
        public static Credentials Anonymous(bool reuseExisting = true) => null;

        public static Credentials Facebook(string accessToken) => null;

        public static Credentials Google(string credential, GoogleCredentialType type) => null;

        public static Credentials Apple(string accessToken) => null;

        public static Credentials JWT(string customToken) => null;

        public static Credentials EmailPassword(string email, string password) => null;

        public static Credentials Function(object payload) => null;

        public static Credentials ApiKey(string key) => null;

        public static Credentials ServerApiKey(string serverApiKey) => null;
    }

    [Obsolete("Use PartitionSyncConfiguration instead.")]
    public class SyncConfiguration : PartitionSyncConfiguration
    {
        public SyncConfiguration(string partition, User user, string optionalPath = null)
            : base(partition, user, optionalPath)
        {
        }
    }
}
