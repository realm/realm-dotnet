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
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Tests.Database;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MergeByPKTests : SyncTestBase
    {
        [TestCaseSource(nameof(MergeTestCases))]
        public void WhenObjectHasPK_ShouldNotCreateDuplicates(Type objectType, object pkValue, Func<dynamic, bool> pkValueChecker)
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var pkProperty = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                           .Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

                for (var i = 0; i < 5; i++)
                {
                    var instance = (RealmObject)Activator.CreateInstance(objectType);

                    pkProperty.SetValue(instance, (dynamic)pkValue);

                    using (var realm = await GetSyncedRealm(objectType))
                    {
                        try
                        {
                            realm.Write(() => realm.Add(instance));
                        }
                        catch (RealmDuplicatePrimaryKeyValueException)
                        {
                            // Sync went through too quickly (that's why we do 5 attempts)
                        }

                        await GetSession(realm).WaitForUploadAsync();
                    }
                }

                using (var realm = await GetSyncedRealm(objectType))
                {
                    await GetSession(realm).WaitForDownloadAsync();
                    var allObjects = realm.All(objectType.Name).ToArray();

                    Assert.That(allObjects.Count(pkValueChecker), Is.EqualTo(1));
                }
            });
        }

        public static object[] MergeTestCases =
        {
            new object[] { typeof(PrimaryKeyInt64Object), (long)0, new Func<dynamic, bool>(i => Int64ValueChecker(i, 0)) },
            new object[] { typeof(PrimaryKeyInt64Object), (long)1, new Func<dynamic, bool>(i => Int64ValueChecker(i, 1)) },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)null, new Func<dynamic, bool>(i => NullableInt64ValueChecker(i, null)) },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)0, new Func<dynamic, bool>(i => NullableInt64ValueChecker(i, 0)) },
            new object[] { typeof(PrimaryKeyNullableInt64Object), (long?)1, new Func<dynamic, bool>(i => NullableInt64ValueChecker(i, 1)) },
            new object[] { typeof(PrimaryKeyStringObject), string.Empty, new Func<dynamic, bool>(i => StringValueChecker(i, string.Empty)) },
            new object[] { typeof(PrimaryKeyStringObject), "key", new Func<dynamic, bool>(i => StringValueChecker(i, "key")) },
        };

        private static bool Int64ValueChecker(dynamic instance, long pkValue)
        {
            var pkObject = (PrimaryKeyInt64Object)instance;
            return pkObject.Int64Property == pkValue;
        }

        private static bool NullableInt64ValueChecker(dynamic instance, long? pkValue)
        {
            var pkObject = (PrimaryKeyNullableInt64Object)instance;
            return pkObject.Int64Property == pkValue;
        }

        private static bool StringValueChecker(dynamic instance, string pkValue)
        {
            var pkObject = (PrimaryKeyStringObject)instance;
            return pkObject.StringProperty == pkValue;
        }

        private async Task<Realm> GetSyncedRealm(Type objectType)
        {
            var credentials = Credentials.UsernamePassword(Constants.AdminUsername, Constants.AdminPassword, false);
            var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
            var configuration = new FullSyncConfiguration(SyncTestHelpers.RealmUri($"~/merge_by_pk_{objectType.Name}"), user, Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { objectType }
            };

            Realm.DeleteRealm(configuration);

            return GetRealm(configuration);
        }
    }
}