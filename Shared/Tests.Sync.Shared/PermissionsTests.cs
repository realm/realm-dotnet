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
using System.Threading.Tasks;
using IntegrationTests;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PermissionsTests
    {
        [TestCase("http", "realm")]
        [TestCase("https", "realms")]
        public void User_GetManagementRealm(string authScheme, string syncScheme)
        {
            AsyncContext.Run(async () =>
            {
                const string UriPattern = "{0}://some.fake.server:12345";
                var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isAdmin: true), new Uri(string.Format(UriPattern, authScheme)));

                using (var realm = user.GetManagementRealm())
                {
                    var configuration = (SyncConfiguration)realm.Config;
                    Assert.That(configuration.User, Is.EqualTo(user));
                    Assert.That(configuration.ServerUri.ToString(), Is.EqualTo(string.Format(UriPattern + "/~/__management", syncScheme)));
                }
            });
        }

        [Test]
        public void PermissionChange_ShouldNotBeInDefaultSchema()
        {
            Assert.That(RealmSchema.Default.Find(nameof(PermissionChange)), Is.Null);
        }

        [Test]
        public void PermissionOffer_ShouldNotBeInDefaultSchema()
        {
            Assert.That(RealmSchema.Default.Find(nameof(PermissionOffer)), Is.Null);
        }

        [Test]
        public void PermissionOfferResponse_ShouldNotBeInDefaultSchema()
        {
            Assert.That(RealmSchema.Default.Find(nameof(PermissionOfferResponse)), Is.Null);
        }

        [Test]
        public void Permission_ShouldNotBeInDefaultSchema()
        {
            Assert.That(RealmSchema.Default.Find(nameof(Permission)), Is.Null);
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionChange_IsProcessedByServer()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var permissionChange = await CreateChange(user, "*");
                Assert.That(permissionChange.Status, Is.EqualTo(ManagementObjectStatus.Success));
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionOffer_WhenValid_TokenIsSet()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var permissionOffer = await CreateOffer(user);
                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionOffer.Token, Is.Not.Null);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionOffer_WhenExpired_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var permissionOffer = await CreateOffer(user, expiresAt: DateTimeOffset.UtcNow.AddDays(-1));
                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionOffer.Token, Is.Null);
                Assert.That(permissionOffer.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                Assert.That(permissionOffer.StatusMessage, Is.Not.Null);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionResponse_WhenOfferExpired_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var permissionOffer = await CreateOffer(user, expiresAt: DateTimeOffset.UtcNow.AddSeconds(2));

                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionOffer.Token, Is.Not.Null);

                await Task.Delay(2500);
                var permissionResponse = await CreateResponse(user, permissionOffer.Token);
                Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionResponse.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                Assert.That(permissionResponse.StatusMessage, Is.Not.Null);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionResponse_WhenTokenIsInvalid_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUser();
                var permissionResponse = await CreateResponse(user, "Some string");
                Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionResponse.ErrorCode, Is.EqualTo(ErrorCode.InvalidParameters));
                Assert.That(permissionResponse.StatusMessage, Is.Not.Null);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionResponse_WhenOfferIsValid_ShouldSetRealmUrl()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUser();
                var bob = await SyncTestHelpers.GetUser();

                // Opening a synced realm with just read permission fails.
                // OS issue: https://github.com/realm/realm-object-store/issues/312
                var realmUrl = await GrantPermissions(alice, bob);
                var syncConfig = new SyncConfiguration(bob, new Uri(realmUrl));

                Realm realm = null;
                Assert.That(() => realm = Realm.GetInstance(syncConfig), Throws.Nothing);
                Session.Error += (sender, e) => 
                {
                    Assert.Fail("Opening the realm should not cause an error.", e.Exception);
                };

                await Task.Delay(2000);
                realm.Dispose();
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void Permission_ValidateWrite()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUser();
                var bob = await SyncTestHelpers.GetUser();

                var realmUrl = await GrantPermissions(alice, bob);

                await ValidateWriteAndSync(realmUrl, alice, bob, 1, 2);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void Permission_ValidateManage()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUser();
                var bob = await SyncTestHelpers.GetUser();
                var charlie = await SyncTestHelpers.GetUser();

                var alicesUrl = await GrantPermissions(alice, bob, mayManage: true);

                await GrantPermissions(bob, charlie, realmUrl: alicesUrl);

                await ValidateWriteAndSync(alicesUrl, alice, charlie, 1, 2);

                await ValidateWriteAndSync(alicesUrl, bob, charlie, 3, 4);
            });
        }

        #if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
        #endif
        [Test]
        public void PermissionChange_UpdatesPermissionRealm()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUser();
                var bob = await SyncTestHelpers.GetUser();

                var permissionRealm = alice.GetPermissionRealm();
                var tcs = new TaskCompletionSource<Permission>();
                var aliceId = alice.Identity; // LINQ :/
                var token = permissionRealm.All<Permission>()
                                           .Where(p => p.UserId != aliceId)
                                           .SubscribeForNotifications((sender, changes, error) =>
                                           {
                                               if (sender.Count > 0)
                                               {
                                                   try
                                                   {
                                                       tcs.TrySetResult(sender.Single());
                                                   }
                                                   catch (Exception ex)
                                                   {
                                                       tcs.TrySetException(ex);
                                                   }
                                               }
                                           });

                var realmPath = $"/{alice.Identity}/testPermission";
                var realmUrl = $"realm://{Constants.ServerUrl}{realmPath}";
                EnsureRealmExists(alice, realmUrl);

                await CreateChange(alice, bob.Identity, realmUrl);
                var permission = await tcs.Task;

                Assert.That(permission.UserId, Is.EqualTo(bob.Identity));
                Assert.That(permission.Path, Is.EqualTo(realmPath));
                Assert.That(permission.MayRead, Is.True);
                Assert.That(permission.MayWrite, Is.False);
                Assert.That(permission.MayManage, Is.False);

                token.Dispose();
                permissionRealm.Dispose();
            });
        }

        private static async Task<string> GrantPermissions(User granter, User receiver, bool mayRead = true, bool mayWrite = true, bool mayManage = false, string realmUrl = null)
        {
            var permissionOffer = await CreateOffer(granter, mayRead, mayWrite, mayManage, realmUrl: realmUrl);

            Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
            Assert.That(permissionOffer.Token, Is.Not.Null);

            var permissionResponse = await CreateResponse(receiver, permissionOffer.Token);
            Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Success));
            Assert.That(permissionResponse.RealmUrl, Is.Not.Null);

            return permissionResponse.RealmUrl;
        }

        private static async Task ValidateWriteAndSync(string realmUrl, User first, User second, long firstObjectId, long secondObjectId)
        {
            await Task.Delay(500);

            var firstRealm = Realm.GetInstance(new SyncConfiguration(first, new Uri(realmUrl)));
            var secondRealm = Realm.GetInstance(new SyncConfiguration(second, new Uri(realmUrl)));

            var firstObjects = firstRealm.All<PrimaryKeyInt64Object>();
            var secondObjects = secondRealm.All<PrimaryKeyInt64Object>();

            await Task.Delay(1000);

            Assert.That(firstObjects.Count(), Is.EqualTo(secondObjects.Count()));

            // Assert that second's realm doesn't contain object with Id = firstObjectId
            Assert.That(secondRealm.Find<PrimaryKeyInt64Object>(firstObjectId), Is.Null);

            // first adds an object
            firstRealm.Write(() => firstRealm.Add(new PrimaryKeyInt64Object
            {
                Int64Property = firstObjectId
            }));

            await Task.Delay(1000);

            Assert.That(firstObjects.Count(), Is.EqualTo(secondObjects.Count()));
            Assert.That(secondRealm.Find<PrimaryKeyInt64Object>(firstObjectId), Is.Not.Null);

            // Assert that first's realm doesn't contain object with Id = secondObjectId
            Assert.That(firstRealm.Find<PrimaryKeyInt64Object>(secondObjectId), Is.Null);

            // second adds an object
            secondRealm.Write(() => secondRealm.Add(new PrimaryKeyInt64Object
            {
                Int64Property = secondObjectId
            }));

            await Task.Delay(1000);

            Assert.That(firstObjects.Count(), Is.EqualTo(secondObjects.Count()));
            Assert.That(firstRealm.Find<PrimaryKeyInt64Object>(secondObjectId), Is.Not.Null);

            firstRealm.Dispose();
            secondRealm.Dispose();
        }

        private static Task<PermissionChange> CreateChange(User user, string receiverId, string url = "*", bool mayRead = true, bool mayWrite = false, bool mayManage = false)
        {
            return CreatePermissionObject(user, _ => new PermissionChange(receiverId, url, mayRead, mayWrite, mayManage));
        }

        private static Task<PermissionOffer> CreateOffer(User user, bool mayRead = true, bool mayWrite = false, bool mayManage = false, DateTimeOffset? expiresAt = null, string realmUrl = null)
        {
            return CreatePermissionObject(user, url =>
            {
                return new PermissionOffer(url, mayRead, mayWrite, mayManage, expiresAt);
            }, realmUrl);
        }
    
        private static Task<PermissionOfferResponse> CreateResponse(User user, string token)
        {
            return CreatePermissionObject(user, _ => new PermissionOfferResponse(token));
        }

        private static async Task<T> CreatePermissionObject<T>(User user, Func<string, T> itemFactory, string realmUrl = null) where T : RealmObject, IPermissionObject
        {
            realmUrl = realmUrl ?? $"realm://{Constants.ServerUrl}/{user.Identity}/offer";
            EnsureRealmExists(user, realmUrl);

            var managementRealm = user.GetManagementRealm();
            var item = itemFactory(realmUrl);
            managementRealm.Write(() => managementRealm.Add(item));
            var tcs = new TaskCompletionSource<object>();

            item.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IPermissionObject.Status))
                {
                    tcs.TrySetResult(null);
                }
            };

            var completedProcessingTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            Assert.That(completedProcessingTask, Is.EqualTo(tcs.Task));

            await tcs.Task;

            return item;
        }

        private static void EnsureRealmExists(User user, string realmUrl)
        {
            var syncConfig = new SyncConfiguration(user, new Uri(realmUrl));
            using (var temp = Realm.GetInstance(syncConfig))
            {
                // Make sure the realm exists
            }
        }
    }
}