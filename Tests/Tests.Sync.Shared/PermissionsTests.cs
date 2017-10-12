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
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;
using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;
using File = System.IO.File;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PermissionsTests : SyncTestBase
    {
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
                var user = await SyncTestHelpers.GetUserAsync();
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
                var user = await SyncTestHelpers.GetUserAsync();
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
                var user = await SyncTestHelpers.GetUserAsync();
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
                var user = await SyncTestHelpers.GetUserAsync();
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
                var user = await SyncTestHelpers.GetUserAsync();
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
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                // Opening a synced realm with just read permission fails.
                // OS issue: https://github.com/realm/realm-object-store/issues/312
                var realmUrl = await GrantPermissions(alice, bob);
                var syncConfig = new SyncConfiguration(bob, new Uri(realmUrl));

                Assert.That(() => GetRealm(syncConfig), Throws.Nothing);
                var handler = new EventHandler<ErrorEventArgs>((sender, e) =>
                {
                    Assert.Fail("Opening the realm should not cause an error.", e.Exception);
                });

                Session.Error += handler;

                await Task.Delay(2000);

                Session.Error -= handler;
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
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

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
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();
                var charlie = await SyncTestHelpers.GetUserAsync();

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
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

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
                var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                await CreateChange(alice, bob.Identity, realmUrl);
                var permission = await tcs.Task.Timeout(2000);

                Assert.That(permission.UserId, Is.EqualTo(bob.Identity));
                Assert.That(permission.Path, Is.EqualTo(realmPath));
                Assert.That(permission.MayRead, Is.True);
                Assert.That(permission.MayWrite, Is.False);
                Assert.That(permission.MayManage, Is.False);

                token.Dispose();
                permissionRealm.Dispose();
            });
        }

        #region User API

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_ApplyPermissions_WithUserId_GrantsAndRevokesPermissions()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                await TestApplyPermissions(alice, bob, PermissionCondition.UserId(bob.Identity)).Timeout(10000);
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_ApplyPermissions_WithEmail_GrantsAndRevokesPermissions()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bobEmail = $"{Guid.NewGuid()}@foo.bar";
                var bobCredentials = Credentials.UsernamePassword(bobEmail, "a", createUser: true);
                var bob = await User.LoginAsync(bobCredentials, SyncTestHelpers.AuthServerUri);

                await TestApplyPermissions(alice, bob, PermissionCondition.Email(bobEmail)).Timeout(10000);
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_OfferPermissions_GrantsPermissions()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmPath = $"/{alice.Identity}/testPermission";
                var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                var token = await alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write).Timeout(2000);
                var alicesUrl = await bob.AcceptPermissionOfferAsync(token).Timeout(2000);

                Assert.That($"realm://{Constants.ServerUrl}:9080/{alicesUrl}", Is.EqualTo(realmUrl));

                await AssertPermissions(alice, bob, realmPath, AccessLevel.Write).Timeout(10000);
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_OfferPermissions_WhenExpired_ShouldThrow()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                await AssertThrows<ArgumentException>(() => alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write, DateTimeOffset.UtcNow.AddDays(-1)));
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_OfferPermissions_WhenNoAccess_ShouldThrow()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                await AssertThrows<ArgumentException>(() => alice.OfferPermissionsAsync(realmUrl, AccessLevel.None));
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_AcceptPermissionOffer_WhenOfferExpired_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                var token = await alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write, expiresAt: DateTimeOffset.UtcNow.AddSeconds(1));

                Assert.That(token, Is.Not.Null);

                await Task.Delay(2000);

                await AssertThrows<PermissionException>(() => bob.AcceptPermissionOfferAsync(token), ex =>
                {
                    Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                });
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void User_AcceptPermissionOffer_WhenTokenIsInvalid_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                await AssertThrows<PermissionException>(() => user.AcceptPermissionOfferAsync("some string"), ex =>
                {
                    Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.InvalidParameters));
                });
            });
        }

        private static async Task AssertThrows<T>(Func<Task> function, Action<T> exceptionAsserts = null)
            where T : Exception
        {
            try
            {
                await function().Timeout(5000);
                Assert.Fail($"Exception of type {typeof(T)} expected.");
            }
            catch (T ex)
            {
                exceptionAsserts?.Invoke(ex);
            }
        }

        private async Task TestApplyPermissions(User alice, User bob, PermissionCondition condition)
        {
            var realmPath = $"/{alice.Identity}/testPermission";
            var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
            EnsureRealmExists(alice, realmUrl);

            // Grant write permissions
            await alice.ApplyPermissionsAsync(condition, realmUrl, AccessLevel.Write);

            await ValidateWriteAndSync(realmUrl, alice, bob, 1, 2);
            await AssertPermissions(alice, bob, realmPath, AccessLevel.Write);

            // Revoke permissions
            await alice.ApplyPermissionsAsync(condition, realmUrl, AccessLevel.None);

            await AssertPermissions(alice, bob, realmPath, AccessLevel.None);
        }

        private static async Task AssertPermissions(User granter, User receiver, string path, AccessLevel level)
        {
            // Seems like there's some time delay before the permission realm is updated
            await Task.Delay(500);
            var granted = (await granter.GetGrantedPermissionsAsync(Recipient.OtherUser)).SingleOrDefault(p => p.UserId == receiver.Identity);
            var received = (await receiver.GetGrantedPermissionsAsync(Recipient.CurrentUser)).SingleOrDefault(p => p.Path == path);

            if (level > AccessLevel.None)
            {
                Assert.That(granted, Is.Not.Null);
                Assert.That(granted.Path, Is.EqualTo(path));

                Assert.That(received, Is.Not.Null);
                Assert.That(received.MayRead, Is.EqualTo(level >= AccessLevel.Read));
                Assert.That(received.MayWrite, Is.EqualTo(level >= AccessLevel.Write));
                Assert.That(received.MayManage, Is.EqualTo(level >= AccessLevel.Admin));
            }
            else
            {
                Assert.That(granted, Is.Null);
                Assert.That(received, Is.Null);
            }
        }

        #endregion

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void WriteToReadOnlyRealm_ThrowsPermissionDenied()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmPath = $"/{alice.Identity}/willBeReadonly";
                var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                // Give Bob just read permissions
                await alice.ApplyPermissionsAsync(PermissionCondition.UserId(bob.Identity), realmUrl, AccessLevel.Read);

                var config = new SyncConfiguration(bob, new Uri(realmUrl));

                var sessionErrorTask = TestHelpers.EventToTask<ErrorEventArgs>(h => Session.Error += h, h => Session.Error -= h);

                using (var realm = GetRealm(config))
                {
                    realm.Write(() => realm.Add(new Person()));

                    try
                    {
                        // Sometimes PermissionDenied will be thrown too fast moving the session to an error state
                        await GetSession(realm).WaitForUploadAsync();
                    }
                    catch
                    {
                    }
                }

                var sessionError = await sessionErrorTask.Timeout(1000);
                Assert.That(sessionError.Exception, Is.TypeOf<PermissionDeniedException>());

                var pde = (PermissionDeniedException)sessionError.Exception;

                Assert.That(pde.ErrorCode, Is.EqualTo(ErrorCode.PermissionDenied));
                Assert.That(File.Exists(config.DatabasePath), Is.True);

                var result = pde.DeleteRealmUserInfo(deleteRealm: true);

                Assert.That(result, Is.True);
                Assert.That(File.Exists(config.DatabasePath), Is.False);
            });
        }

        private async Task<string> GrantPermissions(User granter, User receiver, bool mayRead = true, bool mayWrite = true, bool mayManage = false, string realmUrl = null)
        {
            var permissionOffer = await CreateOffer(granter, mayRead, mayWrite, mayManage, realmUrl: realmUrl);

            Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
            Assert.That(permissionOffer.Token, Is.Not.Null);

            var permissionResponse = await CreateResponse(receiver, permissionOffer.Token);
            Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Success));
            Assert.That(permissionResponse.RealmUrl, Is.Not.Null);

            return $"realm://{Constants.ServerUrl}:9080/{permissionResponse.RealmUrl}";
        }

        private async Task ValidateWriteAndSync(string realmUrl, User first, User second, long firstObjectId, long secondObjectId)
        {
            await Task.Delay(500);

            var firstRealm = GetRealm(new SyncConfiguration(first, new Uri(realmUrl)));
            var secondRealm = GetRealm(new SyncConfiguration(second, new Uri(realmUrl)));

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
        }

        private Task<PermissionChange> CreateChange(User user, string receiverId, string url = "*", bool mayRead = true, bool mayWrite = false, bool mayManage = false)
        {
            return CreatePermissionObject(user, _ => new PermissionChange(receiverId, url, mayRead, mayWrite, mayManage));
        }

        private Task<PermissionOffer> CreateOffer(User user, bool mayRead = true, bool mayWrite = false, bool mayManage = false, DateTimeOffset? expiresAt = null, string realmUrl = null)
        {
            return CreatePermissionObject(user, url =>
            {
                return new PermissionOffer(url, mayRead, mayWrite, mayManage, expiresAt);
            }, realmUrl);
        }

        private Task<PermissionOfferResponse> CreateResponse(User user, string token)
        {
            return CreatePermissionObject(user, _ => new PermissionOfferResponse(token));
        }

        private async Task<T> CreatePermissionObject<T>(User user, Func<string, T> itemFactory, string realmUrl = null) where T : RealmObject, IPermissionObject
        {
            realmUrl = realmUrl ?? SyncTestHelpers.RealmUri($"{user.Identity}/offer").AbsoluteUri;
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

            await tcs.Task.Timeout(5000);

            return item;
        }

        private void EnsureRealmExists(User user, string realmUrl)
        {
            var syncConfig = new SyncConfiguration(user, new Uri(realmUrl));
            using (var realm = GetRealm(syncConfig))
            {
                // Make sure the realm exists
            }
        }
    }
}