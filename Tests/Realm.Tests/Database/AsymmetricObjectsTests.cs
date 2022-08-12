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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Sync.Exceptions;
using Realms.Tests.Sync;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsymmetricObjectsTests : SyncTestBase
    {
        // name format: Action_OptionalCondition_Expectation


        // TODO andrea:
        // 1. add server side check to see if content on server is fine
        // 2. enlarge Asymmetric test classes to have all types
        // 3. dynamic tests for AsymmetricObjects

        [Test]
        public void AddAsymmetricObj_NotInSchema_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConf = await GetFLXIntegrationConfigAsync();

                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConf);

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(new BasicAsymmetricObject());
                    });
                });
            });
        }

        [Test]
        public void AddAllowedInstancesToRealm_Succeds()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionLike = Guid.NewGuid().ToString();
                var flxConf = await GetFLXIntegrationConfigAsync();
                flxConf.Schema = new[]
                {
                    typeof(BasicAsymmetricObject),
                    typeof(AsymmetricContainsEmbeddedObject),
                    typeof(EmbeddedIntPropertyObject)
                };

                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConf);

                Assert.DoesNotThrow(() =>
                {
                    realm.Write(() =>
                    {
                        var basicAsymmetricObj = new BasicAsymmetricObject
                        {
                            PartitionLike = partitionLike
                        };
                        realm.Add(basicAsymmetricObj);

                        var yourInnerObj = new EmbeddedIntPropertyObject();
                        realm.Add(new AsymmetricContainsEmbeddedObject
                        {
                            InnerObj = yourInnerObj,
                            PartitionLike = partitionLike
                        });
                    });
                });
            });
        }

        [Test]
        public void MixingAddingObjectAsymmetricAndNot_Succeeds()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionLike = Guid.NewGuid().ToString();
                var id = new Random().Next();
                var flxConf = await GetFLXIntegrationConfigAsync();
                flxConf.Schema = new[] { typeof(BasicAsymmetricObject), typeof(PrimaryKeyInt32Object) };

                flxConf.PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<PrimaryKeyInt32Object>().Where(n => n.Id == id);
                    realm.Subscriptions.Add(query);
                };

                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConf);

                Assert.DoesNotThrow(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(new BasicAsymmetricObject
                        {
                            PartitionLike = partitionLike
                        });

                        realm.Add(new PrimaryKeyInt32Object
                        {
                            Id = id
                        });
                    });
                });
            });
        }

        [Test, NUnit.Framework.Explicit("Once Daniel Tabacaru's work is done on \"error actions\" this will be an \"application bug\" action: https://github.com/10gen/baas/blob/9f32d54aa79aff6dfb36a6c07742594e38b07441/realm/sync/protocol/protocol_errors.go#L439")]
        public void AsymmetricObjectInPbs_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(BasicAsymmetricObject) };
                var tcs = new TaskCompletionSource<object>();

                config.OnSessionError = (session, error) =>
                {
                    Assert.That(error, Is.InstanceOf<SessionException>());
                    //Assert.That(error.ErrorCode, Is.EqualTo(ErrorCode.ApplicationBug));
                };

                using var realm = await GetRealmAsync(config);

                await tcs.Task;
            });
        }

        [Test]
        public void AsymmetricObjectInLocalRealm_Throws()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;
                config.Schema = new[] { typeof(BasicAsymmetricObject) };

                Exception ex = null;
                try
                {
                    using var realm = await GetRealmAsync(config);
                }
                catch (Exception e)
                {
                    ex = e;
                }

                Assert.That(ex, Is.InstanceOf<RealmSchemaValidationException>());
                Assert.That(ex.Message.Contains($"Asymmetric table \'{nameof(BasicAsymmetricObject)}\'"), Is.True);
            });
        }

        [Explicit]
        private class AsymmetricContainsEmbeddedObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public Guid Id { get; set; } = Guid.NewGuid();

            public string PartitionLike { get; set; }

            public EmbeddedIntPropertyObject InnerObj { get; set; }
        }

        [Explicit]
        private class BasicAsymmetricObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public Guid Id { get; set; } = Guid.NewGuid();

            public string PartitionLike { get; set; }

        }
    }
}
