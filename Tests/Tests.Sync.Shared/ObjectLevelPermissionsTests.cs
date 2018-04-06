////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectLevelPermissionsTests : SyncTestBase
    {
        [Test]
        public void ReadAccessTests()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    var role = PermissionRole.Get(r, "reader");
                    role.Users.Add(userA);
                });

                var config = new SyncConfiguration(userA, realmUri, Guid.NewGuid().ToString())
                {
                    IsPartial = true
                };
                using (var userARealm = GetRealm(config))
                {
                    var query = userARealm.All<IntPrimaryKeyWithValueObject>().Where(o => o.Id > 0);
                    var userASubscription = query.Subscribe();
                    await userASubscription.WaitForSynchronizationAsync().Timeout(2000);

                    AssertRealmPrivileges(userARealm, RealmPrivileges.Read);
                    AssertClassPrivileges(userARealm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(userARealm, ObjectPrivileges.Read);

                    Assert.That(userASubscription.Results.Count(), Is.EqualTo(3));
                    AddObjectsToRealm(userARealm, 4, 5, 6);

                    Assert.That(userASubscription.Results.Count(), Is.EqualTo(6));

                    // We don't have a deterministic predictor on when the server is going to remove the changes.
                    await TestHelpers.WaitForConditionAsync(() => userASubscription.Results.Count() == 3);
                    Assert.That(userASubscription.Results.Count(), Is.EqualTo(3));
                }
            });
        }

        private async Task<Uri> CreateRealm(Action<Realm> assignRoles)
        {
            var uri = SyncTestHelpers.RealmUri(Guid.NewGuid().ToString());
            var admin = await SyncTestHelpers.GetAdminUserAsync();
            var config = new SyncConfiguration(admin, uri)
            {
                IsPartial = true
            };

            using (var realm = GetRealm(config))
            {
                var objects = realm.All<IntPrimaryKeyWithValueObject>().Where(o => o.Id > 0);

                AddObjectsToRealm(realm, 1, 2, 3);

                Assert.That(objects.Count(), Is.EqualTo(3));

                await WaitForSyncAsync(realm);

                Assert.That(objects.Count(), Is.Zero);

                var subscription = objects.Subscribe();
                await subscription.WaitForSynchronizationAsync().Timeout(2000);

                Assert.That(objects.Count(), Is.EqualTo(3));

                // Assign roles
                realm.Write(() =>
                {
                    var permissions = RealmPermission.Get(realm).Permissions;
                    var everyone = permissions.GetOrCreatePermission("everyone");
                    everyone.CanCreate = false;
                    everyone.CanDelete = false;
                    everyone.CanModifySchema = false;
                    everyone.CanQuery = false;
                    everyone.CanRead = false;
                    everyone.CanSetPermissions = false;
                    everyone.CanUpdate = false;

                    var reader = permissions.GetOrCreatePermission("reader");
                    reader.CanQuery = true;
                    reader.CanRead = true;

                    var writer = permissions.GetOrCreatePermission("writer");
                    writer.CanCreate = true;
                    writer.CanDelete = true;
                    writer.CanUpdate = true;

                    var adminPermission = permissions.GetOrCreatePermission("admin");
                    adminPermission.CanSetPermissions = true;

                    assignRoles(realm);
                });

                await WaitForSyncAsync(realm);
            }

            return uri;
        }

        private static void AssertRealmPrivileges(Realm realm, RealmPrivileges expected)
        {
            var realmPrivileges = realm.GetPrivileges();
            Assert.That(realmPrivileges, Is.EqualTo(expected));
        }

        private static void AssertClassPrivileges(Realm realm, ClassPrivileges expected)
        {
            var classPrivileges = realm.GetPrivileges<IntPrimaryKeyWithValueObject>();
            Assert.That(classPrivileges, Is.EqualTo(expected));
        }

        private static void AssertObjectPrivileges(Realm realm, ObjectPrivileges expected)
        {
            foreach (var obj in realm.All<IntPrimaryKeyWithValueObject>())
            {
                var objectPrivileges = realm.GetPrivileges(obj);
                Assert.That(objectPrivileges, Is.EqualTo(expected));
            }
        }

        private static void AddObjectsToRealm(Realm realm, params int[] ids)
        {
            realm.Write(() =>
            {
                foreach (var id in ids)
                {
                    realm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = id,
                        StringValue = $"Object #{id}"
                    });
                }
            });
        }
    }
}
