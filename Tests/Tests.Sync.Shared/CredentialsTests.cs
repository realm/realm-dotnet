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
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class CredentialsTests
    {
        [SetUp]
        public void SetUp()
        {
            SharedRealmHandleExtensions.ResetForTesting();
        }

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
            AsyncContext.Run(async () =>
            {
                var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
                var currentUser = User.Current;

                Assert.That(currentUser, Is.EqualTo(user));
            });
        }

        [Test]
        public void UserCurrent_WhenThereIsMoreThanOneUser_ShouldThrow()
        {
            AsyncContext.Run(async () =>
            {
                await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
                await User.LoginAsync(Credentials.AccessToken("bar:foo", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));

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
            AsyncContext.Run(async () =>
            {
                var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));

                var users = User.AllLoggedIn;

                Assert.That(users.Length, Is.EqualTo(1));
                Assert.That(users[0], Is.EqualTo(user));
            });
        }

        [Test]
        public void UserAllLoggedIn_WhenThereAreNineUsers_ShouldReturnAllOfThem()
        {
            AsyncContext.Run(async () =>
            {
                var users = new List<User>();
                for (var i = 0; i < 9; i++)
                {
                    users.Add(await User.LoginAsync(Credentials.AccessToken("foo:bar" + i, Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080")));
                }

                var current = User.AllLoggedIn;

                Assert.That(current, Is.EquivalentTo(users));
            });
        }

        private const string OriginalPassword = "a";
        private const string NewPassword = "b";

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserChangePasswordTest()
        {
            AsyncContext.Run(async () =>
            {
                var userId = Guid.NewGuid().ToString();
                var serverUri = new Uri($"http://{Constants.ServerUrl}");
                var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
                var user = await User.LoginAsync(credentials, serverUri);
                await user.ChangePasswordAsync(NewPassword);
                user.LogOut();

                Assert.That(async () => await user.ChangePasswordAsync("c"), Throws.TypeOf<InvalidOperationException>());

                await TestNewPassword(userId);
            });
        }

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void AdminChangePasswordTest()
        {
            AsyncContext.Run(async () =>
            {
                var b = User.AllLoggedIn;
                var a = User.Current;
                var userId = Guid.NewGuid().ToString();
                var serverUri = new Uri($"http://{Constants.ServerUrl}");
                var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
                var user = await User.LoginAsync(credentials, serverUri);
                var identity = user.Identity;
                user.LogOut();

                var admin = await User.LoginAsync(Constants.AdminCredentials(), serverUri);
                await admin.ChangePasswordAsync(identity, NewPassword);

                admin.LogOut();

                Assert.That(async () => await admin.ChangePasswordAsync(identity, "c"), Throws.TypeOf<InvalidOperationException>());

                await TestNewPassword(userId);
            });
        }

        private static async Task TestNewPassword(string userId)
        {
            // Ensure that users are logged out
            await Task.Delay(100);

            Assert.That(User.Current, Is.Null);

            var serverUri = new Uri($"http://{Constants.ServerUrl}");

            // Try to login with the same credentials
            try
            {
                await User.LoginAsync(Credentials.UsernamePassword(userId, OriginalPassword, createUser: false), serverUri);
                Assert.Fail("Should be impossible to login with old password");
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.TypeOf<AuthenticationException>());
                var authEx = (AuthenticationException)ex;
                Assert.That(authEx.ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
            }

            var newCredentials = Credentials.UsernamePassword(userId, NewPassword, createUser: false);
            var newUser = await User.LoginAsync(newCredentials, serverUri);

            Assert.That(newUser.State, Is.EqualTo(UserState.Active));
            Assert.That(newUser, Is.EqualTo(User.Current));
            newUser.LogOut();
            Assert.That(User.Current, Is.Null);
        }
    }
}
