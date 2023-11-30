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
using System.Threading.Tasks;
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
            Assert.That(() => DefaultApp.SwitchUser(null!), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void AppRemoveUser_RemovesUser()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var first = await GetUserAsync();
                var second = await GetUserAsync();

                Assert.That(DefaultApp.CurrentUser, Is.EqualTo(second));

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
        public void AppDeleteUserFromServer_RemovesUser()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var username = SyncTestHelpers.GetVerifiedUsername();
                var password = SyncTestHelpers.DefaultPassword;
                var user = await GetUserAsync(app: null, username, password);

                Assert.That(DefaultApp.CurrentUser, Is.EqualTo(user));

                await DefaultApp.DeleteUserFromServerAsync(user);
                Assert.That(DefaultApp.CurrentUser, Is.Null);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(Credentials.EmailPassword(username, password)));
                Assert.That(ex.Message, Is.EqualTo("InvalidPassword: invalid username/password"));
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

            var customData = user.GetCustomData()!;
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
            [Preserve]
            public string Name { get; set; } = string.Empty;

            [BsonElement("email")]
            [Preserve]
            public string Email { get; set; } = string.Empty;

            [BsonElement("addresses")]
            [Preserve]
            public Address[] Addresses { get; set; } = Array.Empty<Address>();

            [BsonElement("favoriteIds")]
            [Preserve]
            public long[] FavoriteIds { get; set; } = Array.Empty<long>();

            [Preserve(AllMembers = true)]
            public class Address
            {
                [BsonElement("city")]
                [Preserve]
                public string City { get; set; } = string.Empty;

                [BsonElement("street")]
                [Preserve]
                public string Street { get; set; } = string.Empty;
            }
        }

        [Test]
        public void UserCustomData_Generic_ReadsFromAccessToken()
        {
            const string tokenWithCustomData = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjIsInVzZXJfZGF0YSI6eyJuYW1lIjoiVGltb3RoeSIsImVtYWlsIjoiYmlnX3RpbUBnbWFpbC5jb20iLCJhZGRyZXNzZXMiOlt7ImNpdHkiOiJOWSIsInN0cmVldCI6IjQybmQifSx7ImNpdHkiOiJTRiIsInN0cmVldCI6Ik1haW4gU3QuIn1dLCJmYXZvcml0ZUlkcyI6WzEsMiwzXX19.wYYtavafunx-iEKFNwXC6DR0C3vBDunwhvIox6XgqDE";
            var user = GetFakeUser(accessToken: tokenWithCustomData);

            var customData = user.GetCustomData<AccessTokenCustomData>()!;
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
        public void User_RetryCustomConfirmationAsync_RerunsConfirmation()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // Standard case
                var unconfirmedMail = SyncTestHelpers.GetUnconfirmedUsername();
                var credentials = Credentials.EmailPassword(unconfirmedMail, SyncTestHelpers.DefaultPassword);

                // The first time the confirmation function is called we return "pending", so the user needs to be confirmed.
                // At the same time we save the user email in a collection.
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(unconfirmedMail, SyncTestHelpers.DefaultPassword).Timeout(10_000, detail: "Failed to register user");

                var ex3 = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));
                Assert.That(ex3.Message, Does.Contain("confirmation required"));

                // The second time we call the confirmation function we find the email we saved in the collection and return "success", so the user
                // gets confirmed and can log in.
                await DefaultApp.EmailPasswordAuth.RetryCustomConfirmationAsync(unconfirmedMail);
                var user = await DefaultApp.LogInAsync(credentials);
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));

                // Logged in user case
                var loggedInUser = await GetUserAsync();
                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.EmailPasswordAuth.RetryCustomConfirmationAsync(loggedInUser.Profile.Email!));
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(ex.Message, Does.Contain("already confirmed"));

                // Unknown user case
                var invalidEmail = "test@gmail.com";
                var ex2 = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.EmailPasswordAuth.RetryCustomConfirmationAsync(invalidEmail));
                Assert.That(ex2.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(ex2.Message, Does.Contain("user not found"));
            });
        }

        [Test]
        public void User_ConfirmUserAsync_ConfirmsUser()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var unconfirmedMail = SyncTestHelpers.GetUnconfirmedUsername();
                var credentials = Credentials.EmailPassword(unconfirmedMail, SyncTestHelpers.DefaultPassword);

                // The first time the confirmation function is called we return "pending", so the user needs to be confirmed.
                // At the same time we save the user email, token and tokenId in a collection.
                await DefaultApp.EmailPasswordAuth.RegisterUserAsync(unconfirmedMail, SyncTestHelpers.DefaultPassword).Timeout(10_000, detail: "Failed to register user");

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));
                Assert.That(ex.Message, Does.Contain("confirmation required"));

                // This retrieves the token and tokenId we saved in the confirmation function
                var functionUser = await GetUserAsync();
                var result = await functionUser.Functions.CallAsync("confirmationInfo", unconfirmedMail);
                var token = result["token"].AsString;
                var tokenId = result["tokenId"].AsString;

                await DefaultApp.EmailPasswordAuth.ConfirmUserAsync(token, tokenId);
                var user = await DefaultApp.LogInAsync(credentials);
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));
            });
        }

        [Test]
        public void User_CallResetPasswordFunctionAsync_ResetsUserPassword()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var email = user.Profile.Email!;

                await user.LogOutAsync();
                Assert.That(user.State, Is.EqualTo(UserState.Removed));

                var newPassword = "realm_tests_do_reset-testPassword";
                await DefaultApp.EmailPasswordAuth.CallResetPasswordFunctionAsync(email, newPassword);

                user = await DefaultApp.LogInAsync(Credentials.EmailPassword(email, newPassword));
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));
            });
        }

        [Test]
        public void User_ResetPasswordAsync_ConfirmsResetPassword()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var email = user.Profile.Email!;

                await user.LogOutAsync();
                Assert.That(user.State, Is.EqualTo(UserState.Removed));

                // This returns "pending" the first time, so the password change is not valid yet. We save the token and tokenId
                // passed to the reset function, to confirm the password change later.
                var newPassword = "realm_tests_do_not_reset-testPassword";
                await DefaultApp.EmailPasswordAuth.CallResetPasswordFunctionAsync(email, newPassword);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(Credentials.EmailPassword(email, newPassword)));
                Assert.That(ex.Message, Does.Contain("invalid username/password"));

                // This retrieves the token and tokenId we saved in the password reset function.
                var functionUser = await GetUserAsync();
                var result = await functionUser.Functions.CallAsync("resetInfo", email);
                var token = result["token"].AsString;
                var tokenId = result["tokenId"].AsString;

                await DefaultApp.EmailPasswordAuth.ResetPasswordAsync(newPassword, token, tokenId);

                user = await DefaultApp.LogInAsync(Credentials.EmailPassword(email, newPassword));
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));
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
                Assert.That(user.Profile.PictureUrl!.AbsoluteUri, Is.EqualTo("https://doe.com/mypicture"));

                // TODO: add other checks once https://github.com/realm/realm-core/issues/4131 is implemented.
            });
        }

        [Test, Ignore("Requires manually getting a fb token")]
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
                Assert.That(fetched!.IsEnabled);
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
                Assert.IsFalse(fetched!.IsEnabled);

                await user.ApiKeys.DisableAsync(key.Id);

                var refetched = await user.ApiKeys.FetchAsync(key.Id);
                Assert.IsFalse(refetched!.IsEnabled);
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

                var credentials = Credentials.ApiKey(apiKey.Value!);
                var apiKeyUser = await DefaultApp.LogInAsync(credentials);

                Assert.That(apiKeyUser.Id, Is.EqualTo(user.Id));

                Assert.That(apiKeyUser.Identities.Select(i => i.Provider), Does.Contain(Credentials.AuthProvider.ApiKey));
                Assert.That(apiKeyUser.RefreshToken, Is.EqualTo(user.RefreshToken));
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

                var credentials = Credentials.ApiKey(apiKey.Value!);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
                Assert.That(ex.Message, Is.EqualTo("AuthError: invalid API key"));

                await user.ApiKeys.EnableAsync(apiKey.Id);

                var apiKeyUser = await DefaultApp.LogInAsync(credentials);

                Assert.That(apiKeyUser.Id, Is.EqualTo(user.Id));

                Assert.That(apiKeyUser.RefreshToken, Is.EqualTo(user.RefreshToken));
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

                var credentials = Credentials.ApiKey(apiKey.Value!);

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

                var credentials = Credentials.ApiKey(apiKey.Value!);

                var ex = await TestHelpers.AssertThrows<AppException>(() => DefaultApp.LogInAsync(credentials));

                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(ex.Message, Is.EqualTo("AuthError: invalid API key"));
                Assert.That(ex.HelpLink, Does.Contain("logs?co_id="));
            });
        }

        private static void AssertKeysAreSame(ApiKey original, ApiKey? fetched)
        {
            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Id, Is.EqualTo(original.Id));
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

                var collection = user.GetMongoClient("BackingDB").GetDatabase(SyncTestHelpers.RemoteMongoDBName()).GetCollection("users");

                var customDataDoc = BsonDocument.Parse(@"{
                    _id: ObjectId(""" + ObjectId.GenerateNewId() + @"""),
                    user_id: """ + user.Id + @""",
                    age: 153,
                    interests: [ ""painting"", ""sci-fi"" ]
                }");

                await collection.InsertOneAsync(customDataDoc);

                updatedData = await user.RefreshCustomDataAsync();

                Assert.That(updatedData, Is.Not.Null);
                Assert.That(updatedData!["age"].AsInt32, Is.EqualTo(153));
                Assert.That(updatedData["interests"].AsBsonArray.Select(i => i.AsString), Is.EquivalentTo(new[] { "painting", "sci-fi" }));

                Assert.That(user.GetCustomData(), Is.Not.Null);
                Assert.That(user.GetCustomData()!["age"].AsInt32, Is.EqualTo(153));
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

                var collection = user.GetMongoClient("BackingDB").GetDatabase(SyncTestHelpers.RemoteMongoDBName()).GetCollection<CustomDataDocument>("users");

                var customDataDoc = new CustomDataDocument
                {
                    UserId = user.Id,
                    Age = 45,
                    Interests = new[] { "swimming", "biking" }
                };

                await collection.InsertOneAsync(customDataDoc);

                updatedData = await user.RefreshCustomDataAsync<CustomDataDocument>();

                Assert.That(updatedData, Is.Not.Null);
                Assert.That(updatedData!.Age, Is.EqualTo(45));
                Assert.That(updatedData.Interests, Is.EquivalentTo(new[] { "swimming", "biking" }));

                var customData = user.GetCustomData<CustomDataDocument>();

                Assert.That(customData, Is.Not.Null);
                Assert.That(customData!.Age, Is.EqualTo(45));
                Assert.That(customData.Interests, Is.EquivalentTo(new[] { "swimming", "biking" }));
            });
        }

        [Test]
        public void UserAnonymous([Values(true, false)] bool firstReuse, [Values(true, false)] bool secondReuse)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: firstReuse));
                Assert.That(user, Is.Not.Null);
                Assert.That(user.Id, Is.Not.Null);

                var anotherUser = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: secondReuse));
                Assert.That(anotherUser, Is.Not.Null);
                Assert.That(anotherUser.Id, Is.Not.Null);

                // We only expect both users to be the same if they both reused their credentials
                Assert.That(user.Id == anotherUser.Id, Is.EqualTo(secondReuse), $"Expected Ids to {(secondReuse ? string.Empty : "not ")}match");
                Assert.That(user == anotherUser, Is.EqualTo(secondReuse), $"Expected Users to {(secondReuse ? string.Empty : "not ")}match");
            });
        }

        [Test]
        public void UserAnonymous_CombiningReuseAndNotReuse()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var anonA = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: false));
                var reusedA1 = await DefaultApp.LogInAsync(Credentials.Anonymous());
                var reusedA2 = await DefaultApp.LogInAsync(Credentials.Anonymous());
                var anonB = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: false));
                var reusedB = await DefaultApp.LogInAsync(Credentials.Anonymous());

                Assert.That(anonA, Is.EqualTo(reusedA1));
                Assert.That(anonA, Is.EqualTo(reusedA2));

                Assert.That(anonB, Is.Not.EqualTo(anonA));
                Assert.That(anonB, Is.EqualTo(reusedB));

                await anonB.LogOutAsync();

                Assert.That(anonB.State, Is.EqualTo(UserState.Removed));
                Assert.That(reusedB.State, Is.EqualTo(UserState.Removed));

                var reusedA3 = await DefaultApp.LogInAsync(Credentials.Anonymous());

                Assert.That(reusedA3, Is.EqualTo(anonA));
            });
        }

        [Test]
        public void UserEqualsOverrides()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: false));
                var currentUser = DefaultApp.CurrentUser!;

                Assert.That(user.Id, Is.EqualTo(currentUser.Id));
                Assert.That(user.Equals(currentUser));
                Assert.That(user == currentUser);

                var anotherUser = await DefaultApp.LogInAsync(Credentials.Anonymous(reuseExisting: false));
                Assert.That(user.Id, Is.Not.EqualTo(anotherUser.Id));
                Assert.That(user.Equals(anotherUser), Is.False);
                Assert.That(user != anotherUser);
            });
        }

        [Test]
        public void UserToStringOverride()
        {
            var user = GetFakeUser();
            Assert.That(user.ToString(), Does.Contain(user.Id));
        }

        [Test]
        public void UserLogOut_RaisesChanged()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var tcs = new TaskCompletionSource();
                user.Changed += (s, _) =>
                {
                    try
                    {
                        Assert.That(s, Is.EqualTo(user));
                        Assert.That(user.State, Is.EqualTo(UserState.LoggedOut));
                        tcs.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };

                await user.LogOutAsync();

                await tcs.Task;

                Assert.That(user.State, Is.EqualTo(UserState.LoggedOut));
            });
        }

        [Test]
        public void UserChanged_DoesntKeepObjectAlive()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var references = await new Func<Task<WeakReference>>(async () =>
                {
                    var user = await GetUserAsync();
                    user.Changed += (_, _) => { };

                    return new WeakReference(user);
                })();

                await TestHelpers.WaitUntilReferencesAreCollected(10000, references);
            });
        }

        [Test]
        public void UserCustomDataChange_RaisesChanged()
        {
            var tcs = new TaskCompletionSource();
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                user.Changed += OnUserChanged;

                var collection = user.GetMongoClient("BackingDB").GetDatabase(SyncTestHelpers.RemoteMongoDBName()).GetCollection("users");

                var customDataDoc = new BsonDocument
                {
                    ["_id"] = ObjectId.GenerateNewId(),
                    ["user_id"] = user.Id,
                    ["age"] = 5
                };

                await collection.InsertOneAsync(customDataDoc);

                var customUserData = await user.RefreshCustomDataAsync();
                Assert.That(customUserData!["age"].AsInt32, Is.EqualTo(5));

                await tcs.Task;

                tcs = new();

                // Unsubscribe and verify that it no longer raises user changed
                user.Changed -= OnUserChanged;

                var filter = BsonDocument.Parse(@"{
                    user_id: { $eq: """ + user.Id + @""" }
                }");
                var update = BsonDocument.Parse(@"{
                    $set: {
                        age: 199
                    }
                }");

                await collection.UpdateOneAsync(filter, update);

                customUserData = await user.RefreshCustomDataAsync();
                Assert.That(customUserData!["age"].AsInt32, Is.EqualTo(199));

                await TestHelpers.AssertThrows<TimeoutException>(() => tcs.Task.Timeout(2000));
            });

            void OnUserChanged(object? sender, EventArgs e)
            {
                tcs.TrySetResult();
            }
        }

        private class CustomDataDocument
        {
            [Preserve]
            [BsonElement("_id")]
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

            [Preserve]
            [BsonElement("user_id")]
            public string UserId { get; set; } = string.Empty;

            [Preserve]
            [BsonElement("age")]
            public int Age { get; set; }

            [Preserve]
            [BsonElement("interests")]
            public string[] Interests { get; set; } = Array.Empty<string>();
        }
    }
}
