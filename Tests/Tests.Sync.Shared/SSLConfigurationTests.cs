////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.IO;
using System.Linq;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    [Ignore("Reenable when ROS fixes SSL support.")]
    [TestFixture, Preserve(AllMembers = true)]
    public class SSLConfigurationTests : SyncTestBase
    {
#if __IOS__
        [Ignore("On iOS TrustedCAPath is ignored.")]
#endif
        [TestCase(true)]
        [TestCase(false)]
        public void TrustedCA_WhenProvided_ValidatesCorrectly(bool openAsync)
        {
            SyncTestHelpers.RequiresRos();

            TestSSLCore(config =>
            {
                config.TrustedCAPath = TestHelpers.CopyBundledDatabaseToDocuments("trusted_ca.pem", "trusted_ca.pem");
            }, openAsync);
        }

        [Test]
        public void TrustedCA_WhenFileDoesntExist_Throws()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();
                var config = new FullSyncConfiguration(SyncTestHelpers.RealmUri("~/TrustedCA_WhenFileDoesntExist_Throws"), user)
                {
                    TrustedCAPath = "something.pem"
                };
                Assert.That(() => GetRealm(config), Throws.TypeOf<FileNotFoundException>());
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnableSSLValidation_WhenFalse_ValidatesCorrectly(bool openAsync)
        {
            SyncTestHelpers.RequiresRos();

            TestSSLCore(config =>
            {
                config.EnableSSLValidation = false;
            }, openAsync);
        }

        private void TestSSLCore(Action<FullSyncConfiguration> setupSecureConfig, bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();
                const string path = "~/TestSSLCore";
                var realmUri = SyncTestHelpers.RealmUri(path);
                var config = new FullSyncConfiguration(realmUri, user);

                var secureRealmUri = SyncTestHelpers.SecureRealmUri(path);
                var secureConfig = new FullSyncConfiguration(secureRealmUri, user, config.DatabasePath + "2");
                setupSecureConfig(secureConfig);

                using (var realm = GetRealm(config))
                {
                    realm.Write(() =>
                    {
                        realm.Add(new IntPrimaryKeyWithValueObject
                        {
                            Id = 1,
                            StringValue = "some value"
                        });
                    });

                    await GetSession(realm).WaitForUploadAsync();
                }

                using (var newRealm = await SyncTestHelpers.GetInstanceAsync(secureConfig, openAsync))
                {
                    CleanupOnTearDown(newRealm);

                    var items = newRealm.All<IntPrimaryKeyWithValueObject>();

                    Assert.That(items.Count(), Is.EqualTo(1));
                    Assert.That(items.Single().StringValue, Is.EqualTo("some value"));
                }
            });
        }
    }
}
