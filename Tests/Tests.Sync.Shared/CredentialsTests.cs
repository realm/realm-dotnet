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
using System.Net.Http;
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
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var currentUser = User.Current;

                Assert.That(currentUser, Is.EqualTo(user));
            });
        }

        [Test]
        public void UserCurrent_WhenThereIsMoreThanOneUser_ShouldThrow()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                try
                {
                    await User.LoginAsync(SyncTestHelpers.CreateCredentials(), new Uri(url));
                    Assert.Fail("Expected exception to be thrown.");
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<ArgumentException>());
                }
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
                var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
                await user.ChangePasswordAsync(NewPassword);
                await user.LogOutAsync();

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
                var userId = Guid.NewGuid().ToString();
                var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
                var identity = user.Identity;
                await user.LogOutAsync();

                var admin = await User.LoginAsync(SyncTestHelpers.AdminCredentials(), SyncTestHelpers.AuthServerUri);
                await admin.ChangePasswordAsync(identity, NewPassword);

                await admin.LogOutAsync();

                Assert.That(async () => await admin.ChangePasswordAsync(identity, "c"), Throws.TypeOf<InvalidOperationException>());

                await TestNewPassword(userId);
            });
        }

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserLogout_RevokesRefreshToken()
        {
            AsyncContext.Run(async () =>
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

                try
                {
                    await AuthenticationHelper.MakeAuthRequestAsync(HttpMethod.Put, new Uri(SyncTestHelpers.AuthServerUri, "auth/password"), json, request =>
                            request.Headers.TryAddWithoutValidation("Authorization", token));

                    Assert.Fail("Expected an error");
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<AuthenticationException>());
                    var aex = (AuthenticationException)ex;

                    Assert.That(aex.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                    Assert.That(aex.ErrorCode, Is.EqualTo(ErrorCode.AccessDenied));
                }
            });
        }


#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserLookup_WhenTargetUserExists_ShouldReturnResponse()
        {
            AsyncContext.Run(async () =>
            {
                var admin = await User.LoginAsync(SyncTestHelpers.AdminCredentials(), SyncTestHelpers.AuthServerUri);

                var aliceUsername = Guid.NewGuid().ToString();
                var alice = await User.LoginAsync(Credentials.UsernamePassword(aliceUsername, "a", createUser: true), SyncTestHelpers.AuthServerUri);

                var lookupResponse = await admin.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, aliceUsername);

                Assert.That(lookupResponse.Identity, Is.EqualTo(alice.Identity));
                Assert.That(lookupResponse.IsAdmin, Is.False);

                Assert.That(lookupResponse.Accounts, Is.Not.Empty);
                var passwordAccount = lookupResponse.Accounts.SingleOrDefault(a => a.Provider == Credentials.Provider.UsernamePassword);

                Assert.That(passwordAccount, Is.Not.Null);
                Assert.That(passwordAccount.ProviderUserIdentity, Is.EqualTo(aliceUsername));
            });
        }

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserLookup_WhenTargetUserDoesNotExist_ShouldReturnNull()
        {
            AsyncContext.Run(async () =>
            {
                var admin = await User.LoginAsync(SyncTestHelpers.AdminCredentials(), SyncTestHelpers.AuthServerUri);

                var lookupResponse = await admin.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, "something");
                Assert.That(lookupResponse, Is.Null);
            });
        }

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserLookup_WhenTargetUserIsSelf_ShouldReturnResponse()
        {
            AsyncContext.Run(async () =>
            {
                var admin = await User.LoginAsync(SyncTestHelpers.AdminCredentials(), SyncTestHelpers.AuthServerUri);

                var lookupResponse = await admin.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, Constants.AdminUsername);

                Assert.That(lookupResponse.Identity, Is.EqualTo(admin.Identity));
                Assert.That(lookupResponse.IsAdmin, Is.True);
                Assert.That(lookupResponse.Accounts, Is.Not.Empty);
                var passwordAccount = lookupResponse.Accounts.SingleOrDefault(a => a.Provider == Credentials.Provider.UsernamePassword);

                Assert.That(passwordAccount, Is.Not.Null);
                Assert.That(passwordAccount.ProviderUserIdentity, Is.EqualTo(Constants.AdminUsername));
            });
        }

#if !ROS_SETUP
        [NUnit.Framework.Explicit]
#endif
        [Test]
        public void UserLookup_WhenUserIsNotAdmin_ShouldThrow()
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                try
                {
                    await alice.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, "some-id");
                    Assert.Fail("Expected an exception to be thrown.");
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<InvalidOperationException>());
                }
            });
        }

        private static async Task TestNewPassword(string userId)
        {
            // Ensure that users are logged out
            await Task.Delay(100);

            Assert.That(User.Current, Is.Null);

            // Try to login with the same credentials
            try
            {
                await User.LoginAsync(Credentials.UsernamePassword(userId, OriginalPassword, createUser: false), SyncTestHelpers.AuthServerUri);
                Assert.Fail("Should be impossible to login with old password");
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.TypeOf<AuthenticationException>());
                var authEx = (AuthenticationException)ex;
                Assert.That(authEx.ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
            }

            var newCredentials = Credentials.UsernamePassword(userId, NewPassword, createUser: false);
            var newUser = await User.LoginAsync(newCredentials, SyncTestHelpers.AuthServerUri);

            Assert.That(newUser.State, Is.EqualTo(UserState.Active));
            Assert.That(newUser, Is.EqualTo(User.Current));
            await newUser.LogOutAsync();
            Assert.That(User.Current, Is.Null);
        }
    }
}
