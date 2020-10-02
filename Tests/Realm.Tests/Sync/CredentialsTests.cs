﻿////////////////////////////////////////////////////////////////////////////
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
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Database;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class CredentialsTests : SyncTestBase
    {
        [Test]
        public void BasicTests()
        {
            // Arrange and Act
            var fb = Credentials.Facebook("token");

            // Assert
            Assert.That(fb.IdentityProvider, Is.EqualTo("facebook"));
            Assert.That(fb.UserInfo, Is.Empty);
        }

        [Test]
        public void UserCurrent_WhenThereAreNoUsers_ShouldReturnNull()
        {
            Assert.That(() => User.Current, Is.Null);
        }

        [Test]
        public void UserCurrent_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var currentUser = User.Current;

                Assert.That(currentUser, Is.EqualTo(user));
            });
        }

        [Test]
        public void UserCurrent_WhenThereIsMoreThanOneUser_ShouldThrow()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await SyncTestHelpers.GetFakeUserAsync();
                await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => User.Current, Throws.TypeOf<RealmException>());
            });
        }

        [Test]
        public void UserAllLoggedIn_WhenThereAreNoUsers_ShouldReturnEmptyCollection()
        {
            var users = User.AllLoggedIn;

            Assert.That(users, Is.Empty);
        }

        [Test]
        public void UserAllLoggedIn_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();

                var users = User.AllLoggedIn;

                Assert.That(users.Length, Is.EqualTo(1));
                Assert.That(users[0], Is.EqualTo(user));
            });
        }

        [Test]
        public void UserAllLoggedIn_WhenThereAreNineUsers_ShouldReturnAllOfThem()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var users = new List<User>();
                for (var i = 0; i < 9; i++)
                {
                    users.Add(await SyncTestHelpers.GetFakeUserAsync());
                }

                var current = User.AllLoggedIn;

                Assert.That(current, Is.EquivalentTo(users));
            });
        }

        [TestCase("realm://localhost")]
        [TestCase("realms://localhost")]
        [TestCase("foo://bar")]
        public void UserLogin_WrongProtocolTestCases(string url)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.AssertThrows<ArgumentException>(() => User.LoginAsync(SyncTestHelpers.CreateCredentials(), new Uri(url)));
            });
        }

        private const string OriginalPassword = "a";
        private const string NewPassword = "b";

        [Test]
        public void UserLogout_RevokesRefreshToken()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var userId = Guid.NewGuid().ToString();
                var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

                var token = user.RefreshToken;
                await user.LogOutAsync();

                // Changing user's password uses the RefreshToken as authorization
                var json = new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["new_password"] = "b"
                    }
                };

                await TestHelpers.AssertThrows<HttpException>(
                    () => AuthenticationHelper.MakeAuthRequestAsync(HttpMethod.Put, new Uri(SyncTestHelpers.AuthServerUri, "auth/password"), json, token),
                    ex =>
                    {
                        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                        Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.AccessDenied));
                    });
            });
        }

        [Test]
        public void UserLogin_WhenAnonymous_LogsUserIn()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var credentials = Credentials.Anonymous();
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

                Assert.That(user, Is.Not.Null);
                Assert.That(user.Identity, Is.Not.Null);
            });
        }

        [Test]
        public void UserLogin_WhenAnonymousAndSameCredentials_ShouldLoginDifferentUser()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var credentials = Credentials.Anonymous();
                var first = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
                var second = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

                Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
                Assert.That(User.AllLoggedIn.Length, Is.EqualTo(2));
            });
        }

        [Test]
        public void UserLogin_WhenAnonymousAndDifferentCredentials_ShouldLoginDifferentUser()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);
                var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

                Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
                Assert.That(User.AllLoggedIn.Length, Is.EqualTo(2));
            });
        }

        [Test]
        public void UserLogin_WhenAnonymousAndOtherUsersLoggedIn_ShouldLoginDifferentUser()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                // Login a regular user
                await SyncTestHelpers.GetUserAsync();

                // Login an anonymous user
                var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

                // Login another regular user
                await SyncTestHelpers.GetUserAsync();

                // Login a second anonymous user
                var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

                // Expect that the anonymous users to be different
                Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
                Assert.That(User.AllLoggedIn.Length, Is.EqualTo(4));
            });
        }

        [Test]
        public void UserLogin_WhenAnonymous_AfterLogoutShouldLoginDifferentUser()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                // Login an anonymous user
                var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);
                await first.LogOutAsync();

                // Login a second anonymous user
                var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

                // Expect that the anonymous users to be different
                Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
                Assert.That(User.AllLoggedIn.Length, Is.EqualTo(1));
            });
        }

#pragma warning disable CS0618 // Type or member is obsolete

        [Test]
        public void UserLogin_WhenNickname_LogsUserIn()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var credentials = Credentials.Nickname(Guid.NewGuid().ToString());
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

                Assert.That(user, Is.Not.Null);
                Assert.That(user.Identity, Is.Not.Null);
            });
        }

        [Test]
        public void UserLogin_WhenNickname_LogsSameUserIn()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var nickname = Guid.NewGuid().ToString();
                var first = await User.LoginAsync(Credentials.Nickname(nickname), SyncTestHelpers.AuthServerUri);

                Assert.That(first, Is.Not.Null);
                Assert.That(first.Identity, Is.Not.Null);

                var second = await User.LoginAsync(Credentials.Nickname(nickname), SyncTestHelpers.AuthServerUri);

                Assert.That(first.Identity, Is.EqualTo(second.Identity));
            });
        }

        [Test]
        public void UserLogin_WhenNicknameAfterLogout_LogsSameUserIn()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var nickname = Guid.NewGuid().ToString();
                var first = await User.LoginAsync(Credentials.Nickname(nickname), SyncTestHelpers.AuthServerUri);
                await first.LogOutAsync();

                var second = await User.LoginAsync(Credentials.Nickname(nickname), SyncTestHelpers.AuthServerUri);

                Assert.That(first.Identity, Is.EqualTo(second.Identity));
            });
        }

        private static async Task TestNewPassword(string userId)
        {
            // Ensure that users are logged out
            await Task.Delay(100);

            Assert.That(User.Current, Is.Null);

            // Try to login with the same credentials
            await TestHelpers.AssertThrows<HttpException>(() => User.LoginAsync(Credentials.UsernamePassword(userId, OriginalPassword, createUser: false), SyncTestHelpers.AuthServerUri), ex =>
            {
                Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
            });

            var newCredentials = Credentials.UsernamePassword(userId, NewPassword, createUser: false);
            var newUser = await User.LoginAsync(newCredentials, SyncTestHelpers.AuthServerUri);

            Assert.That(newUser.State, Is.EqualTo(UserState.Active));
            Assert.That(newUser, Is.EqualTo(User.Current));
            await newUser.LogOutAsync();
            Assert.That(User.Current, Is.Null);
        }
    }
}
