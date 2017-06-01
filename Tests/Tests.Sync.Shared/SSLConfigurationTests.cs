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
using System.Linq;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
#if !ROS_SETUP
    [NUnit.Framework.Explicit]
#endif
    [TestFixture, Preserve(AllMembers = true)]
    public class SSLConfigurationTests
    {
        [Test]
        public void TrustedRootCA_WhenProvided_ValidatesCorrectly()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var realmUrl = SyncTestHelpers.GetRealmUrl("bb");
                var oldConfig = new SyncConfiguration(user, new Uri(realmUrl));

                using (var realm = Realm.GetInstance(oldConfig))
                {
                    Session.Error += (sender, e) => 
                    {
                        var a = e;
                    };
                    realm.Write(() =>
                    {
                        realm.Add(new IntPrimaryKeyWithValueObject
                        {
                            Id = 1,
                            StringValue = "some value"
                        });
                    });

                    await realm.GetSession().WaitForUploadAsync();
                };

                Realm.DeleteRealm(oldConfig);

                var secureRealmUrl = realmUrl.Replace("realm://", "realms://").Replace("9080", "9443");
                var newConfig = new SyncConfiguration(user, new Uri(secureRealmUrl))
                {
                    TrustedCAPath = TestHelpers.CopyBundledDatabaseToDocuments("trusted_ca.pem", "trusted_ca.pem")
                };

                using (var newRealm = await Realm.GetInstanceAsync(newConfig))
                {
                    var items = newRealm.All<IntPrimaryKeyWithValueObject>();

                    Assert.That(items.Count(), Is.EqualTo(1));
                    Assert.That(items.Single().StringValue, Is.EqualTo("some value"));
                }

                Realm.DeleteRealm(newConfig);
            });
        }
    }
}
