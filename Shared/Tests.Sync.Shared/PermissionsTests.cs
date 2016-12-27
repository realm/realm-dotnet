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
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Sync.Shared
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

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionChange_IsProcessedByServer()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionChange = await CreatePermissionObject(user, _ => new PermissionChange("*", "*", mayRead: true));
                Assert.That(permissionChange.Status, Is.EqualTo(ManagementObjectStatus.Success));
            });
        }

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionOffer_WhenValid_TokenIsSet()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionOffer = await CreateOffer(user);
                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionOffer.Token, Is.Not.Null);
            });
        }

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionOffer_WhenExpired_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionOffer = await CreateOffer(user, expiresAt: DateTimeOffset.UtcNow.AddDays(-1));
                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionOffer.Token, Is.Null);
                Assert.That(permissionOffer.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                Assert.That(permissionOffer.StatusMessage, Is.Not.Null);
            });
        }

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionResponse_WhenOfferExpired_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionOffer = await CreateOffer(user, expiresAt: DateTimeOffset.UtcNow.AddSeconds(5));

                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionOffer.Token, Is.Not.Null);

                await Task.Delay(5000);
                var permissionResponse = await CreateResponse(user, permissionOffer.Token);
                Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionResponse.ErrorCode, Is.EqualTo(ErrorCode.ExpiredPermissionOffer));
                Assert.That(permissionResponse.StatusMessage, Is.Not.Null);
            });
        }

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionResponse_WhenTokenIsInvalid_ShouldGetError()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionResponse = await CreateResponse(user, "Some string");
                Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Error));
                Assert.That(permissionResponse.ErrorCode, Is.Not.Null.And.GreaterThan(0));
                Assert.That(permissionResponse.StatusMessage, Is.Not.Null);
            });
        }

        [Test, Explicit("Update Constants.Credentials with values that work on your setup.")]
        public void PermissionResponse_WhenOfferIsValid_ShouldSetRealmUrl()
        {
            AsyncContext.Run(async () =>
            {
                var user = await GetUser();
                var permissionOffer = await CreateOffer(user);

                Assert.That(permissionOffer.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionOffer.Token, Is.Not.Null);

                var receiver = await GetUser(Constants.UserB);
                var permissionResponse = await CreateResponse(receiver, permissionOffer.Token);
                Assert.That(permissionResponse.Status, Is.EqualTo(ManagementObjectStatus.Success));
                Assert.That(permissionResponse.RealmUrl, Is.Not.Null);

                var syncConfig = new SyncConfiguration(receiver, new Uri(permissionResponse.RealmUrl));
                Assert.That(() => Realm.GetInstance(syncConfig), Throws.Nothing);
            });
        }

        private static Task<User> GetUser(string username = Constants.UserA)
        {
            var credentials = Constants.CreateCredentials(username);
            return User.LoginAsync(credentials, new Uri($"http://{Constants.ServerUrl}"));
        }

        private static Task<PermissionOffer> CreateOffer(User user, bool mayRead = true, bool mayWrite = false, bool mayManage = false, DateTimeOffset? expiresAt = null)
        {
            return CreatePermissionObject(user, realmUrl =>
            {
                return new PermissionOffer(realmUrl, mayRead, mayWrite, mayManage, expiresAt);
            });
        }
    
        private static Task<PermissionOfferResponse> CreateResponse(User user, string token)
        {
            return CreatePermissionObject(user, _ => new PermissionOfferResponse(token));
        }

        private static async Task<T> CreatePermissionObject<T>(User user, Func<string, T> itemFactory) where T : RealmObject, IPermissionObject
        {
            var realmUrl = $"realm://{Constants.ServerUrl}/{user.Identity}/offer";
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

            var completedProcessingTask = await Task.WhenAny(tcs.Task, Task.Delay(10000));

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