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
using NUnit.Framework;
using Realms.Sync;

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
            Assert.That(user.CustomData["name"], Is.EqualTo("Timothy"));
            Assert.That(user.CustomData["email"], Is.EqualTo("big_tim@gmail.com"));
            Assert.That(user.CustomData["addresses"].AsBsonArray.Count, Is.EqualTo(2));
            Assert.That(user.CustomData["addresses"][0]["city"], Is.EqualTo("NY"));
            Assert.That(user.CustomData["addresses"][0]["street"], Is.EqualTo("42nd"));
            Assert.That(user.CustomData["addresses"][1]["city"], Is.EqualTo("SF"));
            Assert.That(user.CustomData["addresses"][1]["street"], Is.EqualTo("Main St."));
            Assert.That(user.CustomData["favoriteIds"], Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void UserCustomData_WhenEmpty_ReturnsNull()
        {
            var user = GetFakeUser();

            Assert.That(user.CustomData, Is.Null);
        }
    }
}
