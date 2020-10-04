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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class UserManagementTests : SyncTestBase
    {
        [Test]
        public void AppCurrentUser_WhenThereAreNoUsers_ShouldReturnNull()
        {
            Assert.That(() => DefaultApp.CurrentUser, Is.Null);
        }

        [Test]
        public void AppCurrentUser_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            var user = GetFakeUser();
            var currentUser = DefaultApp.CurrentUser;

            Assert.That(currentUser, Is.EqualTo(user));
        }

        [Test]
        public void AppCurrentUser_WhenThereIsMoreThanOneUser_ShouldReturnLastOne()
        {
            var first = GetFakeUser();
            var second = GetFakeUser();

            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));
            Assert.That(DefaultApp.CurrentUser, Is.Not.EqualTo(first));
        }

        [Test]
        public void AppAllUsers_WhenThereAreNoUsers_ShouldReturnEmptyCollection()
        {
            var users = DefaultApp.AllUsers;
            Assert.That(users, Is.Empty);
        }

        [Test]
        public void AppAllUsers_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            var user = GetFakeUser();

            var users = DefaultApp.AllUsers;

            Assert.That(users.Length, Is.EqualTo(1));
            Assert.That(users[0], Is.EqualTo(user));
        }

        [Test]
        public void AppAllUsers_WhenThereAreNineUsers_ShouldReturnAllOfThem()
        {
            var users = new List<User>();
            for (var i = 0; i < 9; i++)
            {
                users.Add(GetFakeUser());
            }

            var current = DefaultApp.AllUsers;

            Assert.That(current, Is.EquivalentTo(users));
        }

        [Test]
        public void AppSwitchUser_SwitchesCurrentUser()
        {
            var first = GetFakeUser();
            var second = GetFakeUser();

            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));

            DefaultApp.SwitchUser(first);

            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(first));
        }

        [Test]
        public void AppSwitchUser_WhenUserIsCurrent_DoesNothing()
        {
            var first = GetFakeUser();
            var second = GetFakeUser();

            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));

            DefaultApp.SwitchUser(second);

            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));
        }

        [Test]
        public void AppSwitchUser_WhenUserIsNull_Throws()
        {
            Assert.That(() => DefaultApp.SwitchUser(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void AppRemoveUser_RemovesUser()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var first = await GetUserAsync();
                var second = await GetUserAsync();

                Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));

                var rereshToken = second.RefreshToken;
                var secondId = second.Id;

                await DefaultApp.RemoveUserAsync(second);

                // TODO: validate that the refresh token is invalidated.
                Assert.That(second.State, Is.EqualTo(UserState.Removed));
                Assert.That(second.AccessToken, Is.Empty);
                Assert.That(second.RefreshToken, Is.Empty);
                Assert.That(second.Id, Is.EqualTo(secondId));

                Assert.That(DefaultApp.CurrentUser, Is.EqualTo(first));
            });
        }

        [Test]
        public void EmailPasswordRegisterUser_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var username = SyncTestHelpers.GetVerifiedUsername();
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(username, SyncTestHelpers.DefaultPassword);

                var user = await DefaultApp.LogInAsync(Credentials.EmailPassword(username, SyncTestHelpers.DefaultPassword));

                Assert.That(user, Is.Not.Null);
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));
                Assert.That(user.Provider, Is.EqualTo(Credentials.AuthProvider.EmailPassword));
                Assert.That(user.AccessToken, Is.Not.Empty);
                Assert.That(user.RefreshToken, Is.Not.Empty);

                Assert.That(DefaultApp.CurrentUser, Is.EqualTo(user));
            });
        }

        [Test]
        public void UserCustomData_ReadsFromAccessToken()
        {
            const string tokenWithCustomData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjIsInVzZXJfZGF0YSI6eyJuYW1lIjoiVGltb3RoeSIsImVtYWlsIjoiYmlnX3RpbUBnbWFpbC5jb20iLCJhZGRyZXNzZXMiOlt7ImNpdHkiOiJOWSIsInN0cmVldCI6IjQybmQifSx7ImNpdHkiOiJTRiIsInN0cmVldCI6Ik1haW4gU3QuIn1dLCJmYXZvcml0ZUlkcyI6WzEsMiwzXX19.wYYtavafunx-iEKFNwXC6DR0C3vBDunwhvIox6XgqDE";
            var user = GetFakeUser(accessToken: tokenWithCustomData);

            Assert.That(user.CustomData, Is.Not.Null);
            Assert.That(user.CustomData["name"].AsString, Is.EqualTo("Timothy"));
            Assert.That(user.CustomData["email"].AsString, Is.EqualTo("big_tim@gmail.com"));
            Assert.That(user.CustomData["addresses"].AsBsonArray.Count, Is.EqualTo(2));
            Assert.That(user.CustomData["addresses"][0]["city"].AsString, Is.EqualTo("NY"));
            Assert.That(user.CustomData["addresses"][0]["street"].AsString, Is.EqualTo("42nd"));
            Assert.That(user.CustomData["addresses"][1]["city"].AsString, Is.EqualTo("SF"));
            Assert.That(user.CustomData["addresses"][1]["street"].AsString, Is.EqualTo("Main St."));
            Assert.That(user.CustomData["favoriteIds"].AsBsonArray.Select(i => i.AsInt64), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void UserCustomData_WhenEmpty_ReturnsNull()
        {
            var user = GetFakeUser();

            Assert.That(user.CustomData, Is.Null);
        }

        #region API Keys

        [Test]
        public void UserApiKeys_Create_CreatesApiKeyAndRevealsValue()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var apiKey = await user.ApiKeys.CreateAsync("my-api-key");

                Assert.That(apiKey.IsEnabled);
                Assert.That(apiKey.Name, Is.EqualTo("my-api-key"));
                Assert.That(apiKey.Value, Is.Not.Null);
                Assert.That(apiKey.Id, Is.Not.EqualTo(default(ObjectId)));
            });
        }

        [Test]
        public void UserApiKeys_Create_WithInvalidName_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var ex = await TestHelpers.AssertThrows<AppException>(() => user.ApiKeys.CreateAsync("My very cool key"));

                Assert.That(ex.Message, Does.Contain("InvalidParameter"));
                Assert.That(ex.Message, Does.Contain("can only contain ASCII letters, numbers, underscores, and hyphens"));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        [Test]
        public void UserApiKeys_Fetch_WhenNoneExist_ReturnsNull()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var apiKey = await user.ApiKeys.FetchAsync(ObjectId.GenerateNewId());

                Assert.That(apiKey, Is.Null);
            });
        }

        [Test]
        public void UserApiKeys_Fetch_WhenIdDoesntMatch_ReturnsNull()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                await user.ApiKeys.CreateAsync("foo");

                var apiKey = await user.ApiKeys.FetchAsync(ObjectId.GenerateNewId());

                Assert.That(apiKey, Is.Null);
            });
        }

        [Test]
        public void UserApiKeys_Fetch_WhenIdMatches_ReturnsKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var key = await user.ApiKeys.CreateAsync("foo");

                var fetched = await user.ApiKeys.FetchAsync(key.Id);

                Assert.That(fetched, Is.Not.Null);
                AssertKeysAreSame(key, fetched);
            });
        }

        [Test]
        public void UserApiKeys_FetchAll_WithNoKeys()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var keys = await user.ApiKeys.FetchAllAsync();
                Assert.That(keys, Is.Empty);
            });
        }

        [Test]
        public void UserApiKeys_FetchAll_WithOneKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var key1 = await user.ApiKeys.CreateAsync("foo");

                var keys = await user.ApiKeys.FetchAllAsync();
                Assert.That(keys.Count(), Is.EqualTo(1));

                AssertKeysAreSame(key1, keys.Single());
            });
        }

        [Test]
        public void UserApiKeys_FetchAll_WithMultipleKeys()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var originals = new List<ApiKey>();
                for (var i = 0; i < 5; i++)
                {
                    originals.Add(await user.ApiKeys.CreateAsync($"key-{i}"));
                }

                var keys = await user.ApiKeys.FetchAllAsync();
                Assert.That(keys.Count(), Is.EqualTo(originals.Count));

                for (var i = 0; i < originals.Count; i++)
                {
                    AssertKeysAreSame(originals[i], keys.ElementAt(i));
                }
            });
        }

        [Test]
        public void UserApiKeys_DeleteKey_WithExistingId()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var toDelete = await user.ApiKeys.CreateAsync("to-delete");
                var toRemain = await user.ApiKeys.CreateAsync("to-remain");

                await user.ApiKeys.DeleteAsync(toDelete.Id);

                var fetchedDeleted = await user.ApiKeys.FetchAsync(toDelete.Id);
                Assert.That(fetchedDeleted, Is.Null);

                var fetchedRemained = await user.ApiKeys.FetchAsync(toRemain.Id);
                AssertKeysAreSame(toRemain, fetchedRemained);

                var allKeys = await user.ApiKeys.FetchAllAsync();

                Assert.That(allKeys.Count(), Is.EqualTo(1));
                AssertKeysAreSame(toRemain, allKeys.Single());
            });
        }

        [Test]
        public void UserApiKeys_DeleteKey_WithNonExistingId()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var first = await user.ApiKeys.CreateAsync("first");
                var second = await user.ApiKeys.CreateAsync("second");

                await user.ApiKeys.DeleteAsync(ObjectId.GenerateNewId());

                var allKeys = await user.ApiKeys.FetchAllAsync();

                Assert.That(allKeys.Count(), Is.EqualTo(2));
                AssertKeysAreSame(first, allKeys.ElementAt(0));
                AssertKeysAreSame(second, allKeys.ElementAt(1));
            });
        }

        [Test]
        public void UserApiKeys_DisableApiKey_WhenNonExistent_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var id = ObjectId.GenerateNewId();
                var ex = await TestHelpers.AssertThrows<AppException>(() => user.ApiKeys.DisableAsync(id));

                Assert.That(ex.Message, Does.Contain("doesn't exist"));
                Assert.That(ex.Message, Does.Contain(id.ToString()));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        [Test]
        public void UserApiKeys_EnableApiKey_WhenNonExistent_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var id = ObjectId.GenerateNewId();
                var ex = await TestHelpers.AssertThrows<AppException>(() => user.ApiKeys.EnableAsync(id));

                Assert.That(ex.Message, Does.Contain("doesn't exist"));
                Assert.That(ex.Message, Does.Contain(id.ToString()));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        [Test]
        public void UserApiKeys_EnableApiKey_WhenEnabled_IsNoOp()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var key = await user.ApiKeys.CreateAsync("foo");
                Assert.That(key.IsEnabled);

                await user.ApiKeys.EnableAsync(key.Id);

                var fetched = await user.ApiKeys.FetchAsync(key.Id);
                Assert.That(fetched.IsEnabled);
            });
        }

        [Test]
        public void UserApiKeys_DisableApiKey_WhenDisabled_IsNoOp()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var key = await user.ApiKeys.CreateAsync("foo");
                Assert.That(key.IsEnabled);

                await user.ApiKeys.DisableAsync(key.Id);

                var fetched = await user.ApiKeys.FetchAsync(key.Id);
                Assert.IsFalse(fetched.IsEnabled);

                await user.ApiKeys.DisableAsync(key.Id);

                var refetched = await user.ApiKeys.FetchAsync(key.Id);
                Assert.IsFalse(refetched.IsEnabled);
            });
        }

        [Test]
        public void UserApiKeys_Disable_DisablesKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var first = await user.ApiKeys.CreateAsync("first");
                var second = await user.ApiKeys.CreateAsync("second");

                Assert.That(first.IsEnabled);
                Assert.That(second.IsEnabled);

                await user.ApiKeys.DisableAsync(first.Id);

                var keys = await user.ApiKeys.FetchAllAsync();

                Assert.That(keys.ElementAt(0).Id, Is.EqualTo(first.Id));
                Assert.IsFalse(keys.ElementAt(0).IsEnabled);

                Assert.That(keys.ElementAt(1).Id, Is.EqualTo(second.Id));
                Assert.IsTrue(keys.ElementAt(1).IsEnabled);
            });
        }

        [Test]
        public void UserApiKeys_Enable_ReenablesKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var first = await user.ApiKeys.CreateAsync("first");
                var second = await user.ApiKeys.CreateAsync("second");

                Assert.That(first.IsEnabled);
                Assert.That(second.IsEnabled);

                await user.ApiKeys.DisableAsync(first.Id);

                var keys = await user.ApiKeys.FetchAllAsync();

                Assert.That(keys.ElementAt(0).Id, Is.EqualTo(first.Id));
                Assert.IsFalse(keys.ElementAt(0).IsEnabled);

                Assert.That(keys.ElementAt(1).Id, Is.EqualTo(second.Id));
                Assert.IsTrue(keys.ElementAt(1).IsEnabled);

                await user.ApiKeys.EnableAsync(first.Id);

                keys = await user.ApiKeys.FetchAllAsync();

                Assert.That(keys.ElementAt(0).Id, Is.EqualTo(first.Id));
                Assert.IsTrue(keys.ElementAt(0).IsEnabled);

                Assert.That(keys.ElementAt(1).Id, Is.EqualTo(second.Id));
                Assert.IsTrue(keys.ElementAt(1).IsEnabled);
            });
        }

        [Test]
        public void UserApiKeys_CanLoginWithGeneratedKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var apiKey = await user.ApiKeys.CreateAsync("my-api-key");

                var credentials = Credentials.ApiKey(apiKey.Value);
                var apiKeyUser = await DefaultApp.LogInAsync(credentials);

                Assert.That(apiKeyUser.Id, Is.EqualTo(user.Id));

                Assert.That(apiKeyUser.Provider, Is.EqualTo(Credentials.AuthProvider.ApiKey));
                Assert.That(apiKeyUser.RefreshToken, Is.Not.EqualTo(user.RefreshToken));
            });
        }

        [Test]
        public void UserApiKeys_CanLoginWithReenabledKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var apiKey = await user.ApiKeys.CreateAsync("my-api-key");

                await user.ApiKeys.DisableAsync(apiKey.Id);

                var credentials = Credentials.ApiKey(apiKey.Value);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
                Assert.That(ex.Message, Is.EqualTo("AuthError: invalid API key"));

                await user.ApiKeys.EnableAsync(apiKey.Id);

                var apiKeyUser = await DefaultApp.LogInAsync(credentials);

                Assert.That(apiKeyUser.Id, Is.EqualTo(user.Id));

                Assert.That(apiKeyUser.Provider, Is.EqualTo(Credentials.AuthProvider.ApiKey));
                Assert.That(apiKeyUser.RefreshToken, Is.Not.EqualTo(user.RefreshToken));
            });
        }

        [Test]
        public void UserApiKeys_CantLoginWithDisabledKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var apiKey = await user.ApiKeys.CreateAsync("my-api-key");

                await user.ApiKeys.DisableAsync(apiKey.Id);

                var credentials = Credentials.ApiKey(apiKey.Value);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));

                Assert.That(ex.Message, Is.EqualTo("AuthError: invalid API key"));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        [Test]
        public void UserApiKeys_CantLoginWithDeletedKey()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var apiKey = await user.ApiKeys.CreateAsync("my-api-key");

                await user.ApiKeys.DeleteAsync(apiKey.Id);

                var credentials = Credentials.ApiKey(apiKey.Value);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));

                Assert.That(ex.Message, Is.EqualTo("AuthError: invalid API key"));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        private static void AssertKeysAreSame(ApiKey original, ApiKey fetched)
        {
            Assert.That(fetched.Id, Is.EqualTo(original.Id));
            Assert.That(fetched.IsEnabled, Is.EqualTo(original.IsEnabled));
            Assert.That(fetched.Name, Is.EqualTo(original.Name));
            Assert.That(fetched.Value, Is.Null);
        }

        #endregion
    }
}
