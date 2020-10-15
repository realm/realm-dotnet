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
using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
            Assert.That(DefaultApp.CurrentUser, Is.EqualTo(first));

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

            var customData = user.GetCustomData();
            Assert.That(customData, Is.Not.Null);
            Assert.That(customData["name"].AsString, Is.EqualTo("Timothy"));
            Assert.That(customData["email"].AsString, Is.EqualTo("big_tim@gmail.com"));
            Assert.That(customData["addresses"].AsBsonArray.Count, Is.EqualTo(2));
            Assert.That(customData["addresses"][0]["city"].AsString, Is.EqualTo("NY"));
            Assert.That(customData["addresses"][0]["street"].AsString, Is.EqualTo("42nd"));
            Assert.That(customData["addresses"][1]["city"].AsString, Is.EqualTo("SF"));
            Assert.That(customData["addresses"][1]["street"].AsString, Is.EqualTo("Main St."));
            Assert.That(customData["favoriteIds"].AsBsonArray.Select(i => i.AsInt64), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Preserve(AllMembers = true)]
        private class AccessTokenCustomData
        {
            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("email")]
            public string Email { get; set; }

            [BsonElement("addresses")]
            public Address[] Addresses { get; set; }

            [BsonElement("favoriteIds")]
            public long[] FavoriteIds { get; set; }

            [Preserve(AllMembers = true)]
            public class Address
            {
                [BsonElement("city")]
                public string City { get; set; }

                [BsonElement("street")]
                public string Street { get; set; }
            }
        }

        [Test]
        public void UserCustomData_Generic_ReadsFromAccessToken()
        {
            const string tokenWithCustomData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjIsInVzZXJfZGF0YSI6eyJuYW1lIjoiVGltb3RoeSIsImVtYWlsIjoiYmlnX3RpbUBnbWFpbC5jb20iLCJhZGRyZXNzZXMiOlt7ImNpdHkiOiJOWSIsInN0cmVldCI6IjQybmQifSx7ImNpdHkiOiJTRiIsInN0cmVldCI6Ik1haW4gU3QuIn1dLCJmYXZvcml0ZUlkcyI6WzEsMiwzXX19.wYYtavafunx-iEKFNwXC6DR0C3vBDunwhvIox6XgqDE";
            var user = GetFakeUser(accessToken: tokenWithCustomData);

            var customData = user.GetCustomData<AccessTokenCustomData>();
            Assert.That(customData, Is.Not.Null);
            Assert.That(customData.Name, Is.EqualTo("Timothy"));
            Assert.That(customData.Email, Is.EqualTo("big_tim@gmail.com"));
            Assert.That(customData.Addresses.Length, Is.EqualTo(2));
            Assert.That(customData.Addresses[0].City, Is.EqualTo("NY"));
            Assert.That(customData.Addresses[0].Street, Is.EqualTo("42nd"));
            Assert.That(customData.Addresses[1].City, Is.EqualTo("SF"));
            Assert.That(customData.Addresses[1].Street, Is.EqualTo("Main St."));
            Assert.That(customData.FavoriteIds, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void UserCustomData_WhenEmpty_ReturnsNull()
        {
            var user = GetFakeUser();

            Assert.That(user.GetCustomData(), Is.Null);
        }

        [Test]
        public void User_LinkCredentials_AllowsLoginWithNewCredentials()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous());

                Assert.That(user.Identities, Has.Length.EqualTo(1));
                Assert.That(user.Identities[0].Provider, Is.EqualTo(Credentials.AuthProvider.Anonymous));
                Assert.That(user.Identities[0].Id, Is.Not.Null);

                var email = SyncTestHelpers.GetVerifiedUsername();
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(email, SyncTestHelpers.DefaultPassword);
                var linkedUser = await user.LinkCredentialsAsync(Credentials.EmailPassword(email, SyncTestHelpers.DefaultPassword));

                Assert.That(user.Identities, Has.Length.EqualTo(2));
                Assert.That(user.Identities[1].Provider, Is.EqualTo(Credentials.AuthProvider.EmailPassword));
                Assert.That(user.Identities[1].Id, Is.Not.Null);

                Assert.That(linkedUser.Identities, Has.Length.EqualTo(2));
                Assert.That(linkedUser.Id, Is.EqualTo(user.Id));
                Assert.That(linkedUser.Identities, Is.EquivalentTo(user.Identities));

                var emailPasswordUser = await DefaultApp.LogInAsync(Credentials.EmailPassword(email, SyncTestHelpers.DefaultPassword));

                Assert.That(emailPasswordUser.Id, Is.EqualTo(user.Id));
                Assert.That(emailPasswordUser.Identities, Is.EquivalentTo(user.Identities));
            });
        }

        [Test]
        public void User_LinkCredentials_MultipleTimes_AllowsLoginWithAllCredentials()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous());

                var email = SyncTestHelpers.GetVerifiedUsername();
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(email, SyncTestHelpers.DefaultPassword);
                var linkedUser1 = await user.LinkCredentialsAsync(Credentials.EmailPassword(email, SyncTestHelpers.DefaultPassword));
                Assert.That(linkedUser1.Id, Is.EqualTo(user.Id));

                var functionId = Guid.NewGuid().ToString();
                var linkedUser2 = await user.LinkCredentialsAsync(Credentials.Function(new { realmCustomAuthFuncUserId = functionId }));
                Assert.That(linkedUser2.Id, Is.EqualTo(user.Id));

                var emailPasswordUser = await DefaultApp.LogInAsync(Credentials.EmailPassword(email, SyncTestHelpers.DefaultPassword));
                Assert.That(emailPasswordUser.Id, Is.EqualTo(user.Id));

                var functionUser = await DefaultApp.LogInAsync(Credentials.Function(new { realmCustomAuthFuncUserId = functionId }));
                Assert.That(functionUser.Id, Is.EqualTo(user.Id));

                Assert.That(user.Identities, Has.Length.EqualTo(3));
                Assert.That(user.Identities[0].Provider, Is.EqualTo(Credentials.AuthProvider.Anonymous));
                Assert.That(user.Identities[0].Id, Is.Not.Null);

                Assert.That(user.Identities[1].Provider, Is.EqualTo(Credentials.AuthProvider.EmailPassword));
                Assert.That(user.Identities[1].Id, Is.Not.Null);

                Assert.That(user.Identities[2].Provider, Is.EqualTo(Credentials.AuthProvider.Function));
                Assert.That(user.Identities[2].Id, Is.EqualTo(functionId));
            });
        }

        [Test]
        public void User_LinkCredentials_MultipleTimesSameCredentials_IsNoOp()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous());

                var functionId = Guid.NewGuid().ToString();
                var linkedUser = await user.LinkCredentialsAsync(Credentials.Function(new { realmCustomAuthFuncUserId = functionId }));
                Assert.That(linkedUser.Id, Is.EqualTo(user.Id));

                var sameLinkedUser = await user.LinkCredentialsAsync(Credentials.Function(new { realmCustomAuthFuncUserId = functionId }));
                Assert.That(sameLinkedUser.Id, Is.EqualTo(user.Id));

                var functionUser = await DefaultApp.LogInAsync(Credentials.Function(new { realmCustomAuthFuncUserId = functionId }));
                Assert.That(functionUser.Id, Is.EqualTo(user.Id));

                Assert.That(user.Identities, Has.Length.EqualTo(2));
                Assert.That(user.Identities[1].Id, Is.EqualTo(functionId));
                Assert.That(user.Identities[1].Provider, Is.EqualTo(Credentials.AuthProvider.Function));
            });
        }

        [Test]
        public void User_LinkCredentials_WhenMultipleEmailPassword_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var email2 = SyncTestHelpers.GetVerifiedUsername();
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(email2, SyncTestHelpers.DefaultPassword);

                var ex = await TestHelpers.AssertThrows<AppException>(() => user.LinkCredentialsAsync(Credentials.EmailPassword(email2, SyncTestHelpers.DefaultPassword)));

                // TODO: this should be bad request when https://jira.mongodb.org/browse/REALMC-7028 is fixed
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
                Assert.That(ex.Message, Does.Contain("linking a local-userpass identity is not allowed when one is already linked"));
            });
        }

        [Test]
        public void User_LinkCredentials_WhenAnonymous_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var ex = await TestHelpers.AssertThrows<AppException>(() => user.LinkCredentialsAsync(Credentials.Anonymous()));

                // TODO: this should be bad request when https://jira.mongodb.org/browse/REALMC-7028 is fixed
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
                Assert.That(ex.Message, Does.Contain("linking an anonymous identity is not allowed"));
            });
        }

        [Test]
        public void User_LinkCredentials_WhenInUse_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var existingEmail = SyncTestHelpers.GetVerifiedUsername();
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(existingEmail, SyncTestHelpers.DefaultPassword);
                var emailUser = await DefaultApp.LogInAsync(Credentials.EmailPassword(existingEmail, SyncTestHelpers.DefaultPassword));

                var anonUser = await DefaultApp.LogInAsync(Credentials.Anonymous());

                var ex = await TestHelpers.AssertThrows<AppException>(() => anonUser.LinkCredentialsAsync(Credentials.EmailPassword(existingEmail, SyncTestHelpers.DefaultPassword)));

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(ex.Message, Does.Contain("a user already exists with the specified provider"));
            });
        }

        [Test]
        public void User_Push_RegisterDevice()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                await user.GetPushClient("gcm").RegisterDeviceAsync("hello");
            });
        }

        [Test]
        public void User_Push_RegisterDevice_WrongService()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var ex = await TestHelpers.AssertThrows<AppException>(() => user.GetPushClient("non-existent").RegisterDeviceAsync("hello"));
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(ex.Message, Does.Contain("service not found: 'non-existent'"));
            });
        }

        [Test]
        public void User_Push_DeregisterDevice()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                await user.GetPushClient("gcm").DeregisterDeviceAsync();
            });
        }

        [Test]
        public void User_JWT_LogsInAndReadsDataFromToken()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                const string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxMjM0NTY3ODkwIiwic3ViIjoiMTIzNDU2Nzg5MCIsIm5hbWUiOnsiZmlyc3QiOiJKb2huIiwibGFzdCI6IkRvZSJ9LCJqb2JUaXRsZSI6IkJyZWFrZXIgb2YgdGhpbmdzIiwiZW1haWwiOiJqb2huQGRvZS5jb20iLCJwaWN0dXJlVXJsIjoiaHR0cHM6Ly9kb2UuY29tL215cGljdHVyZSIsImdlbmRlciI6Im90aGVyIiwiYmlydGhkYXkiOiIxOTM0LTA1LTE1IiwibWluQWdlIjoiODAiLCJtYXhBZ2UiOiI5MCIsImlhdCI6MTUxNjIzOTAyMiwiZXhwIjoyMDE2MjM5MDIyLCJhdWQiOiJteS1hdWRpZW5jZSJ9.B6u3SkU-pzCH_LA_HsevAJF1EI1LbAOfL6GP3bhjVpP4FBtrmZYQD_b7Z_wJLE0vaffX1eN6U_vE9t26bmXz2ig4jJRmbg7Kx9ka1BkcE7MF9nmdC90ffHgNBvU40yKpMBtVL9VNQCe-F6mSvUqpox2tQQpNKaXf8yQslAf_tfvqTvF0mPXnqU1v_5KtieMybOb7O8nV6LITrjsAA5ff4spWSgcskjXcyjq6DIdWbLlVJycodr-MjKu94fNXXsBLf0iK5XHYpL1Bs-ILs494_aK_Pf2GD3pYa56XjqN-nO_cYbIxzmsBkNtAp0hvg_Gp0O6QFi66Qkr7ORbkRasGAg";
                var credentials = Credentials.JWT(token);
                var user = await DefaultApp.LogInAsync(credentials);

                Assert.That(user.Profile.FirstName, Is.EqualTo("John"));
                Assert.That(user.Profile.LastName, Is.EqualTo("Doe"));
                Assert.That(user.Profile.Email, Is.EqualTo("john@doe.com"));
                Assert.That(user.Profile.Birthday, Is.EqualTo("1934-05-15"));
                Assert.That(user.Profile.Gender, Is.EqualTo("other"));
                Assert.That(user.Profile.MinAge, Is.EqualTo("80"));
                Assert.That(user.Profile.MaxAge, Is.EqualTo("90"));
                Assert.That(user.Profile.PictureUrl.AbsoluteUri, Is.EqualTo("https://doe.com/mypicture"));

                // TODO: add other checks once https://github.com/realm/realm-object-store/issues/1123 is implemented.
            });
        }

        [Test, NUnit.Framework.Explicit("Requires manually getting a fb token")]
        public void User_Facebook_LogsInAndReadsDataFromFacebook()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                const string fbToken = "EAAFYw2aZAL1EBAHBBH22XBDZAutJFQ65KxH0bZAexYul5KtsHcjhI722XYEr4jKlaNvlosFsdZCT8dGUQNy2euZB684mpvtIIJEWWYMoH66bbEbKIrHRWqZBC8KMpSscoyzhFTJMpDYsrIilZBRN1A6bicXGaUNXVz5A0ucyZB7WkmQ8uUmdRWel9q6S8BJH3ZBCZAzWtcZCYmgEwZDZD";
                var credentials = Credentials.Facebook(fbToken);
                var user = await DefaultApp.LogInAsync(credentials);

                Assert.That(user.Id, Is.Not.Null);

                Assert.That(user.Profile.FirstName, Is.Not.Null);
                Assert.That(user.Profile.LastName, Is.Not.Null);
            });
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

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
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

        [Test]
        public void UserCustomData()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                Assert.That(user.GetCustomData(), Is.Null);

                var updatedData = await user.RefreshCustomDataAsync();
                Assert.That(updatedData, Is.Null);

                var collection = user.GetMongoClient("BackingDB").GetDatabase("my-db").GetCollection("users");

                var customDataDoc = BsonDocument.Parse(@"{
                    _id: ObjectId(""" + ObjectId.GenerateNewId() + @"""),
                    user_id: """ + user.Id + @""",
                    age: 153,
                    interests: [ ""painting"", ""sci-fi"" ]
                }");

                await collection.InsertOneAsync(customDataDoc);

                updatedData = await user.RefreshCustomDataAsync();

                Assert.That(updatedData, Is.Not.Null);
                Assert.That(updatedData["age"].AsInt32, Is.EqualTo(153));
                Assert.That(updatedData["interests"].AsBsonArray.Select(i => i.AsString), Is.EquivalentTo(new[] { "painting", "sci-fi" }));

                Assert.That(user.GetCustomData(), Is.Not.Null);
                Assert.That(user.GetCustomData()["age"].AsInt32, Is.EqualTo(153));
            });
        }

        [Test]
        public void UserCustomData_Generic()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                Assert.That(user.GetCustomData<CustomDataDocument>(), Is.Null);

                var updatedData = await user.RefreshCustomDataAsync<CustomDataDocument>();
                Assert.That(updatedData, Is.Null);

                var collection = user.GetMongoClient("BackingDB").GetDatabase("my-db").GetCollection<CustomDataDocument>("users");

                var customDataDoc = new CustomDataDocument
                {
                    UserId = user.Id,
                    Age = 45,
                    Interests = new[] { "swimming", "biking" }
                };

                await collection.InsertOneAsync(customDataDoc);

                updatedData = await user.RefreshCustomDataAsync<CustomDataDocument>();

                Assert.That(updatedData, Is.Not.Null);
                Assert.That(updatedData.Age, Is.EqualTo(45));
                Assert.That(updatedData.Interests, Is.EquivalentTo(new[] { "swimming", "biking" }));

                var customData = user.GetCustomData<CustomDataDocument>();

                Assert.That(customData, Is.Not.Null);
                Assert.That(customData.Age, Is.EqualTo(45));
                Assert.That(customData.Interests, Is.EquivalentTo(new[] { "swimming", "biking" }));
            });
        }

        private class CustomDataDocument
        {
            [BsonElement("_id")]
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

            [BsonElement("user_id")]
            public string UserId { get; set; }

            [BsonElement("age")]
            public int Age { get; set; }

            [BsonElement("interests")]
            public string[] Interests { get; set; }
        }
    }
}
