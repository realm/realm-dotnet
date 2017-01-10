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
using Realms;
using Realms.Exceptions;
using Realms.Sync;

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
        public async void UserCurrent_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var currentUser = User.Current;

            Assert.That(currentUser, Is.EqualTo(user));
        }

        [Test]
        public async void UserCurrent_WhenThereIsMoreThanOneUser_ShouldThrow()
        {
            await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            await User.LoginAsync(Credentials.AccessToken("bar:foo", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));

            Assert.That(() => User.Current, Throws.TypeOf<RealmException>());
        }

        [Test]
        public void UserAllLoggedIn_WhenThereAreNoUsers_ShouldReturnEmptyCollection()
        {
            var users = User.AllLoggedIn;

            Assert.That(users, Is.Empty);
        }

        [Test]
        public async void UserAllLoggedIn_WhenThereIsOneUser_ShouldReturnThatUser()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));

            var users = User.AllLoggedIn;

            Assert.That(users.Length, Is.EqualTo(1));
            Assert.That(users[0], Is.EqualTo(user));
        }

        [Test]
        public async void UserAllLoggedIn_WhenThereAreNineUsers_ShouldReturnAllOfThem()
        {
            var users = new List<User>();
            for (var i = 0; i < 9; i++)
            {
                users.Add(await User.LoginAsync(Credentials.AccessToken("foo:bar" + i, Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080")));
            }

            var current = User.AllLoggedIn;

            Assert.That(current, Is.EquivalentTo(users));
        }
    }
}
