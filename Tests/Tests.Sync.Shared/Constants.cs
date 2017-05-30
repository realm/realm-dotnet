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
using Realms.Sync;

namespace Tests.Sync
{
    public static class Constants
    {
        // The server url as visible from the testing device
        public const string ServerUrl = "localhost:9080";

        public const string AdminUsername = "a@a";
        public const string AdminPassword = "a";

        public static Credentials CreateCredentials()
        {
            return Credentials.UsernamePassword(Guid.NewGuid().ToString(), "a", createUser: true);
        }

        public static Credentials AdminCredentials()
        {
            return Credentials.UsernamePassword(AdminUsername, AdminPassword, createUser: false);
        }
    }
}