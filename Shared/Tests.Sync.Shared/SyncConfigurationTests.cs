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
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests
    {
        private Realm _realm;

        [SetUp]
        public void SetUp()
        {
            var credentials = Credentials.AccessToken("token", Guid.NewGuid().ToString());
            var user = Task.Run(() => User.LoginAsync(credentials, null)).Result;
            _realm = Realm.GetInstance(new SyncConfiguration(user, new Uri("realm://localhost:9080")));
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
        }

        [Test]
        public void Foo()
        {
        }
    }
}
