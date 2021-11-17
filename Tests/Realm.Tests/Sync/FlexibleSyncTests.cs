////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using NUnit.Framework;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class FlexibleSyncTests : SyncTestBase
    {
        [Test]
        public void Realm_Subscriptions_WhenLocalRealm_ReturnsNull()
        {
            var realm = GetRealm();

            Assert.That(realm.Subscriptions, Is.Null);
        }

        [Test]
        public void Realm_Subscriptions_WhenPBS_ReturnsNull()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);

            Assert.That(realm.Subscriptions, Is.Null);
        }

        [Test]
        public void Realm_Subscriptions_WhenFLX_ReturnsSubscriptions()
        {
            var config = GetFakeFLXConfig();
            var realm = GetRealm(config);
            Assert.That(realm.Subscriptions, Is.Not.Null);
            Assert.That(realm.Subscriptions.Version, Is.Zero);
        }
    }
}
