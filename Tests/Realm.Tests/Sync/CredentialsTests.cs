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

using NUnit.Framework;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    [Ignore("V10TODO: Enable when sync API are wired up.")]
    public class CredentialsTests : SyncTestBase
    {
        //private const string OriginalPassword = "a";
        //private const string NewPassword = "b";

        //[Test]
        //public void UserLogout_RevokesRefreshToken()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        var userId = Guid.NewGuid().ToString();
        //        var credentials = Credentials.UsernamePassword(userId, OriginalPassword, createUser: true);
        //        var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

        //        var token = user.RefreshToken;
        //        await user.LogOutAsync();

        //        // Changing user's password uses the RefreshToken as authorization
        //        var json = new Dictionary<string, object>
        //        {
        //            ["data"] = new Dictionary<string, object>
        //            {
        //                ["new_password"] = "b"
        //            }
        //        };

        //        // V10TODO: find another way to do it
        //        //await TestHelpers.AssertThrows<HttpException>(
        //        //    () => AuthenticationHelper.MakeAuthRequestAsync(HttpMethod.Put, new Uri(SyncTestHelpers.AuthServerUri, "auth/password"), json, token),
        //        //    ex =>
        //        //    {
        //        //        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        //        //        Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.AccessDenied));
        //        //    });
        //    });
        //}

        //[Test]
        //public void UserLogin_WhenAnonymous_LogsUserIn()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        var credentials = Credentials.Anonymous();
        //        var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

        //        Assert.That(user, Is.Not.Null);
        //        Assert.That(user.Identity, Is.Not.Null);
        //    });
        //}

        //[Test]
        //public void UserLogin_WhenAnonymousAndSameCredentials_ShouldLoginDifferentUser()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        var credentials = Credentials.Anonymous();
        //        var first = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
        //        var second = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);

        //        Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
        //        Assert.That(User.AllLoggedIn.Length, Is.EqualTo(2));
        //    });
        //}

        //[Test]
        //public void UserLogin_WhenAnonymousAndDifferentCredentials_ShouldLoginDifferentUser()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);
        //        var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

        //        Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
        //        Assert.That(User.AllLoggedIn.Length, Is.EqualTo(2));
        //    });
        //}

        //[Test]
        //public void UserLogin_WhenAnonymousAndOtherUsersLoggedIn_ShouldLoginDifferentUser()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        // Login a regular user
        //        await SyncTestHelpers.GetUserAsync();

        //        // Login an anonymous user
        //        var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

        //        // Login another regular user
        //        await SyncTestHelpers.GetUserAsync();

        //        // Login a second anonymous user
        //        var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

        //        // Expect that the anonymous users to be different
        //        Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
        //        Assert.That(User.AllLoggedIn.Length, Is.EqualTo(4));
        //    });
        //}

        //[Test]
        //public void UserLogin_WhenAnonymous_AfterLogoutShouldLoginDifferentUser()
        //{
        //    SyncTestHelpers.RunRosTestAsync(async () =>
        //    {
        //        // Login an anonymous user
        //        var first = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);
        //        await first.LogOutAsync();

        //        // Login a second anonymous user
        //        var second = await User.LoginAsync(Credentials.Anonymous(), SyncTestHelpers.AuthServerUri);

        //        // Expect that the anonymous users to be different
        //        Assert.That(first.Identity, Is.Not.EqualTo(second.Identity));
        //        Assert.That(User.AllLoggedIn.Length, Is.EqualTo(1));
        //    });
        //}

        //private static async Task TestNewPassword(string userId)
        //{
        //    // Ensure that users are logged out
        //    await Task.Delay(100);

        //    Assert.That(User.Current, Is.Null);

        //    // Try to login with the same credentials
        //    await TestHelpers.AssertThrows<HttpException>(() => User.LoginAsync(Credentials.UsernamePassword(userId, OriginalPassword, createUser: false), SyncTestHelpers.AuthServerUri), ex =>
        //    {
        //        Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
        //    });

        //    var newCredentials = Credentials.UsernamePassword(userId, NewPassword, createUser: false);
        //    var newUser = await User.LoginAsync(newCredentials, SyncTestHelpers.AuthServerUri);

        //    Assert.That(newUser.State, Is.EqualTo(UserState.Active));
        //    Assert.That(newUser, Is.EqualTo(User.Current));
        //    await newUser.LogOutAsync();
        //    Assert.That(User.Current, Is.Null);
        //}
    }
}
