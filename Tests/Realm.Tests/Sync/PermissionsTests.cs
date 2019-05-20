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
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Database;

using File = System.IO.File;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PermissionsTests : SyncTestBase
    {
        [Test]
        public void User_ApplyPermissions_WithUserId_GrantsAndRevokesPermissions()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                await TestApplyPermissions(alice, bob, PermissionCondition.UserId(bob.Identity)).Timeout(20000);
            });
        }

        [Test]
        public void User_ApplyPermissions_WithEmail_GrantsAndRevokesPermissions()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bobEmail = $"{Guid.NewGuid()}@foo.bar";
                var bobCredentials = Credentials.UsernamePassword(bobEmail, "a", createUser: true);
                var bob = await User.LoginAsync(bobCredentials, SyncTestHelpers.AuthServerUri);

                await TestApplyPermissions(alice, bob, PermissionCondition.Email(bobEmail)).Timeout(20000);
            });
        }

        [Test]
        public void User_OfferPermissions_GrantsPermissions()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmPath = $"/{alice.Identity}/testPermission";
                var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                var token = await alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write).Timeout(2000);
                var alicesUrl = await bob.AcceptPermissionOfferAsync(token).Timeout(2000);

                Assert.That(alicesUrl, Is.EqualTo(realmPath));

                await AssertPermissions(alice, bob, realmPath, AccessLevel.Write).Timeout(10000);
            });
        }

        [Test]
        public void User_OfferPermissions_WhenExpired_ShouldThrow()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                await TestHelpers.AssertThrows<ArgumentException>(() => alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write, DateTimeOffset.UtcNow.AddDays(-1)));
            });
        }

        [Test]
        public void User_OfferPermissions_WhenNoAccess_ShouldThrow()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                await TestHelpers.AssertThrows<ArgumentException>(() => alice.OfferPermissionsAsync(realmUrl, AccessLevel.None));
            });
        }

        [Test]
        public void User_AcceptPermissionOffer_WhenOfferExpired_ShouldGetError()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmUrl = SyncTestHelpers.RealmUri($"{alice.Identity}/testPermission").AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                var token = await alice.OfferPermissionsAsync(realmUrl, AccessLevel.Write, expiresAt: DateTimeOffset.UtcNow.AddSeconds(1));

                Assert.That(token, Is.Not.Null);

                await Task.Delay(2000);

                await TestHelpers.AssertThrows<HttpException>(() => bob.AcceptPermissionOfferAsync(token), ex =>
                {
                    Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                });
            });
        }

        [Test]
        public void User_AcceptPermissionOffer_WhenTokenIsInvalid_ShouldGetError()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                await TestHelpers.AssertThrows<HttpException>(() => user.AcceptPermissionOfferAsync("some string"), ex =>
                {
                    Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.InvalidParameters));
                });
            });
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
            var granted = (await granter.GetGrantedPermissionsAsync(Recipient.OtherUser)).SingleOrDefault(p => p.UserId == receiver.Identity);
            var received = (await receiver.GetGrantedPermissionsAsync(Recipient.CurrentUser)).SingleOrDefault(p => p.Path == path);

            if (level > AccessLevel.None)
            {
                Assert.That(granted, Is.Not.Null);
                Assert.That(granted.Path, Is.EqualTo(path));

                Assert.That(received, Is.Not.Null);
                Assert.That(received.AccessLevel, Is.EqualTo(level));
            }
            else
            {
                Assert.That(granted, Is.Null);
                Assert.That(received, Is.Null);
            }
        }

        [Test]
        public void WriteToReadOnlyRealm_ThrowsPermissionDenied()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmPath = $"/{alice.Identity}/willBeReadonly";
                var realmUrl = SyncTestHelpers.RealmUri(realmPath).AbsoluteUri;
                EnsureRealmExists(alice, realmUrl);

                // Give Bob just read permissions
                await alice.ApplyPermissionsAsync(PermissionCondition.UserId(bob.Identity), realmUrl, AccessLevel.Read);

                var config = new FullSyncConfiguration(new Uri(realmUrl), bob, Guid.NewGuid().ToString());

                var sessionErrorTask = TestHelpers.EventToTask<ErrorEventArgs>(h => Session.Error += h, h => Session.Error -= h);

                using (var realm = GetRealm(config))
                {
                    realm.Write(() => realm.Add(new Person()));

                    try
                    {
                        // Sometimes PermissionDenied will be thrown too fast moving the session to an error state
                        await SyncTestHelpers.WaitForUploadAsync(realm).Timeout(1000);
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

        private async Task ValidateWriteAndSync(string realmUrl, User first, User second, long firstObjectId, long secondObjectId)
        {
            await Task.Delay(500);

            var firstRealm = GetRealm(new FullSyncConfiguration(new Uri(realmUrl), first, Guid.NewGuid().ToString()));
            var secondRealm = GetRealm(new FullSyncConfiguration(new Uri(realmUrl), second, Guid.NewGuid().ToString()));

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

        private void EnsureRealmExists(User user, string realmUrl)
        {
            var syncConfig = new FullSyncConfiguration(new Uri(realmUrl), user, Guid.NewGuid().ToString());
            using (var realm = GetRealm(syncConfig))
            {
                // Make sure the realm exists
            }
        }
    }
}