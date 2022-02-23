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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Helpers;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MergeByPKTests : SyncTestBase
    {
        [TestCaseSource(nameof(MergeTestCases))]
        public void WhenObjectHasPK_ShouldNotCreateDuplicates(Type objectType, object pkValue)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var pkProperty = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                           .Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

                var partition = Guid.NewGuid().ToString();
                for (var i = 0; i < 5; i++)
                {
                    var instance = (RealmObject)Activator.CreateInstance(objectType);

                    pkProperty.SetValue(instance, pkValue);

                    using var realm = await GetSyncedRealm(partition);
                    try
                    {
                        realm.Write(() => realm.Add(instance));
                    }
                    catch (RealmDuplicatePrimaryKeyValueException)
                    {
                        // Sync went through too quickly (that's why we do 5 attempts)
                    }

                    await WaitForUploadAsync(realm);
                }

                using (var realm = await GetSyncedRealm(partition))
                {
                    var expectedPK = Operator.Convert<RealmValue>(pkValue);
                    await TestHelpers.WaitForConditionAsync(() => realm.DynamicApi.FindCore(objectType.Name, expectedPK) != null);

                    var objectCount = ((IQueryable<RealmObject>)realm.DynamicApi.All(objectType.Name))
                        .ToArray()
                        .Count(o => o.DynamicApi.Get<RealmValue>("_id") == expectedPK);

                    Assert.That(objectCount, Is.EqualTo(1));
                }
            });
        }

        public static object[] MergeTestCases =
        {
            new object[] { typeof(PrimaryKeyInt64Object), 0L },
            new object[] { typeof(PrimaryKeyInt64Object), 1L },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)null },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)0, },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)1, },
            new object[] { typeof(PrimaryKeyStringObject), string.Empty },
            new object[] { typeof(PrimaryKeyStringObject), "key" },
        };

        private async Task<Realm> GetSyncedRealm(string partition)
        {
            var config = await GetIntegrationConfigAsync(partition);
            config.Schema = new[] { typeof(PrimaryKeyNullableInt64Object), typeof(PrimaryKeyStringObject), typeof(PrimaryKeyInt64Object) };

            return GetRealm(config);
        }
    }
}
