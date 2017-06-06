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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Sync;

namespace Tests.Sync
{
#if !ROS_SETUP
    [NUnit.Framework.Explicit]
#endif
    [TestFixture, Preserve(AllMembers = true)]
    public class SSLConfigurationTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TrustedCA_WhenProvided_ValidatesCorrectly(bool openAsync)
        {
            TestSSLCore(config =>
            {
                config.TrustedCAPath = TestHelpers.CopyBundledDatabaseToDocuments("trusted_ca.pem", "trusted_ca.pem");
            }, openAsync);
        }

        [Test]
        public void TrustedCA_WhenFileDoesntExist_Throws()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var config = new SyncConfiguration(user, new Uri(SyncTestHelpers.GetRealmUrl()))
                {
                    TrustedCAPath = "something.pem"
                };
                Assert.That(() => Realm.GetInstance(config), Throws.TypeOf<RealmFileNotFoundException>());
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnableSSLValidation_WhenFalse_ValidatesCorrectly(bool openAsync)
        {
            TestSSLCore(config =>
            {
                config.EnableSSLValidation = false;
            }, openAsync);
        }

        private static void TestSSLCore(Action<SyncConfiguration> setupSecureConfig, bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var realmUrl = SyncTestHelpers.GetRealmUrl();
                var config = new SyncConfiguration(user, new Uri(realmUrl));

                var secureRealmUrl = realmUrl.Replace("realm://", "realms://").Replace("9080", "9443");
                var secureConfig = new SyncConfiguration(user, new Uri(secureRealmUrl), config.DatabasePath + "2");
                setupSecureConfig(secureConfig);

                try
                {
                    using (var realm = Realm.GetInstance(config))
                    {
                        realm.Write(() =>
                        {
                            realm.Add(new IntPrimaryKeyWithValueObject
                            {
                                Id = 1,
                                StringValue = "some value"
                            });
                        });

                        await realm.GetSession().WaitForUploadAsync();
                    }

                    using (var newRealm = await SyncTestHelpers.GetInstanceAsync(secureConfig, openAsync))
                    {
                        var items = newRealm.All<IntPrimaryKeyWithValueObject>();

                        Assert.That(items.Count(), Is.EqualTo(1));
                        Assert.That(items.Single().StringValue, Is.EqualTo("some value"));
                    }
                }
                finally
                {
                    Realm.DeleteRealm(config);
                    Realm.DeleteRealm(secureConfig);
                }
            });
        }
    }
}
