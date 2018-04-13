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
using System.Collections.Generic;
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
        public void Test_RealmRead()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(RealmPermission.Get(r).Permissions);
                    var role = PermissionRole.Get(r, "reader");
                    role.Users.Add(userA);
                });

                using (var realm = GetRealm(userA, realmUri))
                {
                    var query = realm.All<ObjectWithPermissions>();
                    var subscription = query.Subscribe();
                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    AssertRealmPrivileges(realm, RealmPrivileges.Read);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));

                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var query = realm.All<ObjectWithPermissions>();
                    var subscription = query.Subscribe();
                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    AssertRealmPrivileges(realm, 0);
                    AssertClassPrivileges(realm, 0);

                    Assert.That(subscription.Results.Count(), Is.Zero);
                }
            });
        }

        [Test]
        public void Test_RealmUpdate()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(RealmPermission.Get(r).Permissions);
                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);
                });

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe |
                                          ClassPrivileges.Create | ClassPrivileges.SetPermissions | ClassPrivileges.Update);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read | ObjectPrivileges.Delete |
                                           ObjectPrivileges.SetPermissions | ObjectPrivileges.Update);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));

                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);

                    AssertRealmPrivileges(realm, RealmPrivileges.Read);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                    AddObjectsToRealm(realm, new[] { 7, 8, 9 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(9));

                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                }
            });
        }

        [Test]
        public void Test_ClassRead()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(ClassPermission.Get<ObjectWithPermissions>(r).Permissions);
                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                });

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));

                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, 0);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }
            });
        }

        [Test]
        public void Test_ClassUpdate()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(ClassPermission.Get<ObjectWithPermissions>(r).Permissions);

                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);
                });

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe | ClassPrivileges.Update | ClassPrivileges.Create);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read | ObjectPrivileges.Update | ObjectPrivileges.Delete | ObjectPrivileges.SetPermissions);

                    var obj = realm.Find<ObjectWithPermissions>(1);
                    realm.Write(() =>
                    {
                        obj.StringValue = "New value";
                    });

                    await WaitForSyncAsync(realm);
                    Assert.That(obj.StringValue, Is.EqualTo("New value"));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    var obj = realm.Find<ObjectWithPermissions>(1);
                    realm.Write(() =>
                    {
                        obj.StringValue = "New value 2";
                    });

                    Assert.That(obj.StringValue, Is.EqualTo("New value 2"));
                    await WaitForSyncAsync(realm);

                    // Change is reverted
                    Assert.That(obj.StringValue, Is.EqualTo("New value"));
                }
            });
        }

        [Test]
        public void Test_ClassCreate()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(ClassPermission.Get<ObjectWithPermissions>(r).Permissions);
                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);
                });

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe | ClassPrivileges.Update | ClassPrivileges.Create);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read | ObjectPrivileges.Update | ObjectPrivileges.Delete | ObjectPrivileges.SetPermissions);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));

                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));

                    AddObjectsToRealm(realm, new[] { 7, 8, 9 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(9));
                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));

                }
            });
        }

        [Test]
        public void Test_ClassSetPermissions()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();
                var userC = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    CreatePermissions(ClassPermission.Get<ObjectWithPermissions>(r).Permissions);

                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);
                    reader.Users.Add(userC);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);
                    writer.Users.Add(userB);

                    var admin = PermissionRole.Get(r, "admin");
                    admin.Users.Add(userA);
                });

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);

                    // B is 'writer' - shouldn't be able to update the role access level
                    realm.Write(() =>
                    {
                        var readerPermission = Permission.Get<ObjectWithPermissions>("reader", realm);
                        readerPermission.CanUpdate = true;
                        readerPermission.CanCreate = true;
                    });

                    await WaitForSyncAsync(realm);
                }

                using (var realm = GetRealm(userC, realmUri))
                {
                    // C shouldn't be able to create objects
                    var subscription = await SubscribeToObjectsAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));

                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(6));
                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                }

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);

                    // A should be able to update role access level
                    realm.Write(() =>
                    {
                        var readerPermission = Permission.Get<ObjectWithPermissions>("reader", realm);
                        readerPermission.CanUpdate = true;
                        readerPermission.CanCreate = true;
                    });

                    await WaitForSyncAsync(realm);
                }

                using (var realm = GetRealm(userC, realmUri))
                {
                    // C should now be able to create objects
                    // Why does my subscription timeout?
                    // var subscription = await SubscribeToObjectsAsync(realm);

                    await WaitForSyncAsync(realm);
                    AssertRealmPrivileges(realm, RealmPrivileges.Read | RealmPrivileges.Update | RealmPrivileges.ModifySchema | RealmPrivileges.SetPermissions);
                    AssertClassPrivileges(realm, ClassPrivileges.Read | ClassPrivileges.Subscribe | ClassPrivileges.Update | ClassPrivileges.Create);
                    AssertObjectPrivileges(realm, ObjectPrivileges.Read | ObjectPrivileges.Update | ObjectPrivileges.Delete | ObjectPrivileges.SetPermissions);

                    var objects = realm.All<ObjectWithPermissions>();
                    Assert.That(objects.Count(), Is.EqualTo(3));

                    AddObjectsToRealm(realm, new[] { 4, 5, 6 });

                    Assert.That(objects.Count(), Is.EqualTo(6));
                    await WaitForSyncAsync(realm);
                    Assert.That(objects.Count(), Is.EqualTo(6));
                }
            });
        }

        [Test]
        public void Test_ObjectRead()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    var reader = PermissionRole.Get(r, "reader");

                    reader.Users.Add(userA);

                    var obj1 = r.Add(new ObjectWithPermissions
                    {
                        Id = 1,
                        StringValue = "Value 1"
                    });
                    CreatePermissions(obj1.Permissions);

                    r.Add(new ObjectWithPermissions
                    {
                        Id = 2,
                        StringValue = "Value 2"
                    });
                }, addObjects: false);

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(1));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }
            });
        }

        [Test]
        public void Test_ObjectUpdate()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);

                    var obj1 = r.Add(new ObjectWithPermissions
                    {
                        Id = 1,
                        StringValue = "Value 1"
                    });
                    CreatePermissions(obj1.Permissions);
                }, addObjects: false);

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    var obj1 = subscription.Results.Single();
                    realm.Write(() =>
                    {
                        obj1.StringValue = "New value";
                    });

                    await WaitForSyncAsync(realm);

                    Assert.That(obj1.StringValue, Is.EqualTo("New value"));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    var obj1 = subscription.Results.Single();
                    realm.Write(() =>
                    {
                        obj1.StringValue = "New value #2";
                    });

                    Assert.That(obj1.StringValue, Is.EqualTo("New value #2"));
                    await WaitForSyncAsync(realm);

                    Assert.That(obj1.StringValue, Is.EqualTo("New value"));
                }
            });
        }

        [Test]
        public void Test_ObjectDelete()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(r =>
                {
                    var reader = PermissionRole.Get(r, "reader");
                    reader.Users.Add(userA);
                    reader.Users.Add(userB);

                    var writer = PermissionRole.Get(r, "writer");
                    writer.Users.Add(userA);

                    var obj1 = r.Add(new ObjectWithPermissions
                    {
                        Id = 1,
                        StringValue = "Value 1"
                    });
                    CreatePermissions(obj1.Permissions);
                }, addObjects: false);

                using (var realmA = GetRealm(userA, realmUri))
                using (var realmB = GetRealm(userB, realmUri))
                {
                    var subscriptionB = await SubscribeToObjectsAsync(realmB);
                    var objB = subscriptionB.Results.Single();
                    realmB.Write(() =>
                    {
                        realmB.Remove(objB);
                    });

                    Assert.That(subscriptionB.Results.Count(), Is.Zero);
                    await WaitForSyncAsync(realmB);
                    Assert.That(subscriptionB.Results.Count(), Is.EqualTo(1));
                    objB = subscriptionB.Results.Single();

                    var subscriptionA = await SubscribeToObjectsAsync(realmA);
                    var objA = subscriptionA.Results.Single();
                    realmA.Write(() =>
                    {
                        realmA.Remove(objA);
                    });

                    await WaitForSyncAsync(realmA);
                    await WaitForSyncAsync(realmB);

                    Assert.That(subscriptionA.Results.Count(), Is.Zero);
                    Assert.That(subscriptionB.Results.Count(), Is.Zero);

                    Assert.That(objA.IsValid, Is.False);
                    Assert.That(objB.IsValid, Is.False);
                }
            });
        }

        [Test]
        public void Test_ObjectSetPermissions()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var userA = await SyncTestHelpers.GetUserAsync();
                var userB = await SyncTestHelpers.GetUserAsync();

                var realmUri = await CreateRealm(assignRoles: null, addObjects: false);

                using (var realm = GetRealm(userA, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);

                    realm.Write(() =>
                    {
                        var obj1 = realm.Add(new ObjectWithPermissions
                        {
                            Id = 1,
                            StringValue = "1"
                        });

                        var foo = PermissionRole.Get(realm, "foo");
                        var permission = Permission.Get(foo, obj1);
                        permission.CanRead = true;
                        foo.Users.Add(userB);
                    });

                    Assert.That(subscription.Results.Count(), Is.EqualTo(1));
                    await WaitForSyncAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }

                using (var realm = GetRealm(userB, realmUri))
                {
                    var subscription = await SubscribeToObjectsAsync(realm);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(1));
                }
            });
        }

        private Realm GetRealm(User user, Uri uri)
        {
            var config = new SyncConfiguration(user, uri, Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { typeof(ObjectWithPermissions) },
                IsPartial = true
            };

            return GetRealm(config);
        }

        private async Task<Uri> CreateRealm(Action<Realm> assignRoles = null, bool addObjects = true)
        {
            var uri = SyncTestHelpers.RealmUri(Guid.NewGuid().ToString());
            var admin = await SyncTestHelpers.GetAdminUserAsync();
            var config = new SyncConfiguration(admin, uri)
            {
                ObjectClasses = new[] { typeof(ObjectWithPermissions) },
                IsPartial = true
            };

            using (var realm = GetRealm(config))
            {
                var objects = realm.All<ObjectWithPermissions>();
                var subscription = await SubscribeToObjectsAsync(realm);
                if (addObjects)
                {
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                    AddObjectsToRealm(realm, new[] { 1, 2, 3 });

                    await WaitForSyncAsync(realm);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));
                }

                if (assignRoles != null)
                {
                    realm.Write(() =>
                    {
                        assignRoles(realm);
                    });
                }

                await WaitForSyncAsync(realm);
            }

            return uri;
        }

        private static async Task<Subscription<ObjectWithPermissions>> SubscribeToObjectsAsync(Realm realm)
        {
            var query = realm.All<ObjectWithPermissions>();
            var subscription = query.Subscribe();
            await subscription.WaitForSynchronizationAsync().Timeout(5000);
            return subscription;
        }

        private static void CreatePermissions(IList<Permission> permissions)
        {
            var everyone = Permission.Get("everyone", permissions);
            everyone.CanCreate = false;
            everyone.CanDelete = false;
            everyone.CanModifySchema = false;
            everyone.CanQuery = false;
            everyone.CanRead = false;
            everyone.CanSetPermissions = false;
            everyone.CanUpdate = false;

            var reader = Permission.Get("reader", permissions);
            reader.CanQuery = true;
            reader.CanRead = true;

            var writer = Permission.Get("writer", permissions);
            writer.CanCreate = true;
            writer.CanDelete = true;
            writer.CanUpdate = true;

            var adminPermission = Permission.Get("admin", permissions);
            adminPermission.CanSetPermissions = true;
        }

        private static void AssertRealmPrivileges(Realm realm, RealmPrivileges expected)
        {
            var realmPrivileges = realm.GetPrivileges();
            Assert.That(realmPrivileges, Is.EqualTo(expected));
        }

        private static void AssertClassPrivileges(Realm realm, ClassPrivileges expected)
        {
            var classPrivileges = realm.GetPrivileges<ObjectWithPermissions>();
            Assert.That(classPrivileges, Is.EqualTo(expected));
        }

        private static void AssertObjectPrivileges(Realm realm, ObjectPrivileges expected)
        {
            foreach (var obj in realm.All<ObjectWithPermissions>())
            {
                var objectPrivileges = realm.GetPrivileges(obj);
                Assert.That(objectPrivileges, Is.EqualTo(expected));
            }
        }

        private static void AddObjectsToRealm(Realm realm, int[] ids)
        {
            realm.Write(() =>
            {
                foreach (var id in ids)
                {
                    var obj = realm.Add(new ObjectWithPermissions
                    {
                        Id = id,
                        StringValue = $"Object #{id}"
                    });

                    var permission = Permission.Get("everyone", obj);
                    permission.CanRead = true;
                    permission.CanUpdate = true;
                    permission.CanDelete = true;
                    permission.CanSetPermissions = true;
                }
            });
        }

        [Realms.Explicit]
        private class ObjectWithPermissions : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string StringValue { get; set; }

            public IList<Permission> Permissions { get; }
        }
    }
}
