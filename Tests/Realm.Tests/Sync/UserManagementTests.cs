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
    [TestFixture]
    public class UserManagementTests : SyncTestBase
    {
        [Test]
        public void AppCurrentUser_WhenThereAreNoUsers_ShouldReturnNull()
        {
            Assert.That(() => _app.CurrentUser, Is.Null);
        }

        [Test]
        public void AppCurrentUser_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);
                var currentUser = _app.CurrentUser;

                Assert.That(currentUser, Is.EqualTo(user));
            });
        }

        [Test]
        public void AppCurrentUser_WhenThereIsMoreThanOneUser_ShouldReturnLastOne()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var first = await SyncTestHelpers.GetFakeUserAsync(_app);
                var second = await SyncTestHelpers.GetFakeUserAsync(_app);

                Assert.That(_app.CurrentUser, Is.EqualTo(second));
                Assert.That(_app.CurrentUser, Is.Not.EqualTo(first));
            });
        }

        [Test]
        public void AppAllUsers_WhenThereAreNoUsers_ShouldReturnEmptyCollection()
        {
            var users = _app.AllUsers;
            Assert.That(users, Is.Empty);
        }

        [Test]
        public void AppAllUsers_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);

                var users = _app.AllUsers;

                Assert.That(users.Length, Is.EqualTo(1));
                Assert.That(users[0], Is.EqualTo(user));
            });
        }

        [Test]
        public void AppAllUsers_WhenThereAreNineUsers_ShouldReturnAllOfThem()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var users = new List<User>();
                for (var i = 0; i < 9; i++)
                {
                    users.Add(await SyncTestHelpers.GetFakeUserAsync(_app));
                }

                var current = _app.AllUsers;

                Assert.That(current, Is.EquivalentTo(users));
            });
        }

        [Test]
        public void AppSwitchUser_SwitchesCurrentUser()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var first = await SyncTestHelpers.GetFakeUserAsync(_app);
                var second = await SyncTestHelpers.GetFakeUserAsync(_app);

                Assert.That(_app.CurrentUser, Is.EqualTo(second));

                _app.SwitchUser(first);

                Assert.That(_app.CurrentUser, Is.EqualTo(first));
            });
        }

        [Test]
        public void AppSwitchUser_WhenUserIsCurrent_DoesNothing()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var first = await SyncTestHelpers.GetFakeUserAsync(_app);
                var second = await SyncTestHelpers.GetFakeUserAsync(_app);

                Assert.That(_app.CurrentUser, Is.EqualTo(second));

                _app.SwitchUser(second);

                Assert.That(_app.CurrentUser, Is.EqualTo(second));
            });
        }

        [Test]
        public void AppSwitchUser_WhenUserIsNull_Throws()
        {
            Assert.That(() => _app.SwitchUser(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void AppRemoveUser_RemovesUser()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var first = await SyncTestHelpers.GetUserAsync(_app);
                var second = await SyncTestHelpers.GetUserAsync(_app);

                Assert.That(_app.CurrentUser, Is.EqualTo(second));

                var rereshToken = second.RefreshToken;
                var secondId = second.Id;

                await _app.RemoveUserAsync(second);

                // TODO: validate that the refresh token is invalidated.
                Assert.That(second.State, Is.EqualTo(UserState.Removed));
                Assert.That(second.AccessToken, Is.Empty);
                Assert.That(second.RefreshToken, Is.Empty);
                Assert.That(second.Id, Is.EqualTo(secondId));

                Assert.That(_app.CurrentUser, Is.EqualTo(first));
            });
        }

        [Test]
        public void EmailPasswordRegisterUser_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var username = Guid.NewGuid().ToString();
                await _app.EmailPasswordAuth.RegisterUserAsync(username, SyncTestHelpers.DefaultPassword);

                var user = await _app.LogInAsync(Credentials.UsernamePassword(username, SyncTestHelpers.DefaultPassword));

                Assert.That(user, Is.Not.Null);
                Assert.That(user.State, Is.EqualTo(UserState.LoggedIn));
                Assert.That(user.AccessToken, Is.Not.Null);
                Assert.That(user.RefreshToken, Is.Not.Null);

                Assert.That(_app.CurrentUser, Is.EqualTo(user));
            });
        }
    }
}
