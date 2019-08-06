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
        public void UserChangePasswordTest()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
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

        [Test]
        public void AdminChangePasswordTest()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
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
        public void UserLookup_WhenTargetUserExists_ShouldReturnResponse()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
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

        [Test]
        public void UserLookup_WhenTargetUserDoesNotExist_ShouldReturnNull()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var admin = await User.LoginAsync(SyncTestHelpers.AdminCredentials(), SyncTestHelpers.AuthServerUri);

                var lookupResponse = await admin.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, "something");
                Assert.That(lookupResponse, Is.Null);
            });
        }

        [Test]
        public void UserLookup_WhenTargetUserIsSelf_ShouldReturnResponse()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
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

        [Test]
        public void UserLookup_WhenUserIsNotAdmin_ShouldThrow()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();

                await TestHelpers.AssertThrows<InvalidOperationException>(() => alice.RetrieveInfoForUserAsync(Credentials.Provider.UsernamePassword, "some-id"));
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

        #region CustomRefreshTokenTests

        /*
        // Keys

-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCMgFkzVG4bc5DW
VtSVtYgdMRbTIyj6UnhOmmS7LjcT02sPQxs8tB5ggwGJSd4/wtv8J0p1Bsq43wOF
R+LCwYwZqZ/UysLxl2qg3m4LN7NcgIeDfCSNdbc97Vwg1Li0ygBr9vQcUTq9sgKD
7IFu48inn0xF7fxG/FhqGQYo/frLE/RDV3cbchyB5KP9lRoaZ54Wj3yyoiMlnrsX
OES91aZRLld4LYWUMg7aWzrRBm41QgYqAT05SGQ5TaU/Z1x/1i61nJ5jw5w50Mn7
AYvl3EYJ4/E+yh0XUSYjkbr9HGO6Q2+HugGpUl2WjVTmPhQAaAj3k/dDSSJFcYZE
z2yNV/MFAgMBAAECggEATB4ItU9La6HbWNOnzgeP20jJ9c75l0vwk5z/b4zlF9+V
A6q2adenEWBIB8m2F1MI/P2IUAhC8Y8YiC9ewWY78Xc8+Pp0TJBcmxSGB5vAlx+m
yuwJnX2lrW4XWE4GVyOMwPEEZQb4zOZQiIorwRi0j2M03jnFT+vMNoaiGLkoErZJ
xOw93+v83cPivQsR6PeZ8KrPSW0V/lzvH8ZqiQQjpDMm7Y90F4Hr1g6DJ2AogEjv
Tv8yWTlzPYcd5reevRV1eyzSHYwcr6dhdmGl1LQLkL9uPbwiQmXLgjWywm3xmDyX
BWJeIVI88H+E8hPb1A2yZjyr35CXdNPbcRo2B8YGLQKBgQDLQ/W24DvqUu9TZ3/B
EfTTzkXzIU+mUo9qnxtxB+2DnwksTxSK6HG4H1qxo8CCuOR8RBSoqJxqqNtKkq2L
lIYrMGoCpYRPcT0JHP7ZfqVnh15CrAkra5QvFXEzbK4aqrJR//HuzdUvFqvb/aNS
jEyuVaMNUGNiMYDreD0CX+q38wKBgQCw89waZsqdBtea3A2VKo5xMMicdssC2kNt
MJGPCXnXwATqjTHbaFQCbamUJPqlTiMnKVRC4mTr85IXM85gXonFYLYt0CCGX5wd
zyC2LcdCfvQBjgrtr9ytKhvK6gq9kBEPNgWNQO9AzuqN1BXmduLfc/8welErIfgA
HixAcdKfJwKBgQCAi9wK6Ug66nQcBOpQSXDRujOWjMx4XOICBdku5Fqa0KrWcLSH
HHU+geWzTeHjSdaFl/CQsQEqmtsEEDrcePNYwOdqAQ7pxq1Y5BNvrJ4iGQPNmkq6
QPCXzjGm2eZJSwY2wWxZH6bgfq/1EjSFceDUp6fUNbCEWtYzE/lRVSN1bQKBgQCK
P1uc/OYbXHciM/4gpkj3QgfZxi3Bosi/DA0M1XhuCUVOAtYK9y17YDX22hVBBRUN
yYpdXwc+GOPwYLdCL1ov7OkoTcy7bwNHfsWtz4I3/3ufo1wCaz1bxORF2iheBapu
WeRogWzrEz3JZQNfNU73CWc8drPnoPhjDy+/ga3uTQKBgFJHP+wZix0efZymu0VS
SacwuyolDNg1ebQsBA7XZ/ac9HH/cxxGHxFS76cwfxM7KUpgXEhmFUtEJjuy6UME
tw/6uOA95dBQztjvCmAgzdzExq1lSfadgpnj/SYbr70YKBvEnwb1KOPbFlVBnX4f
BuwMRU4Vebrdbe6RGRg8mByy
-----END PRIVATE KEY-----

-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjIBZM1RuG3OQ1lbUlbWI
HTEW0yMo+lJ4Tppkuy43E9NrD0MbPLQeYIMBiUneP8Lb/CdKdQbKuN8DhUfiwsGM
Gamf1MrC8ZdqoN5uCzezXICHg3wkjXW3Pe1cINS4tMoAa/b0HFE6vbICg+yBbuPI
p59MRe38RvxYahkGKP36yxP0Q1d3G3IcgeSj/ZUaGmeeFo98sqIjJZ67FzhEvdWm
US5XeC2FlDIO2ls60QZuNUIGKgE9OUhkOU2lP2dcf9YutZyeY8OcOdDJ+wGL5dxG
CePxPsodF1EmI5G6/RxjukNvh7oBqVJdlo1U5j4UAGgI95P3Q0kiRXGGRM9sjVfz
BQIDAQAB
-----END PUBLIC KEY-----

        ROS Config:

import { BasicServer } from "..";
import * as path from "path";
import { ConsoleLogger } from "../shared";

require("../../feature-token-for-tests.js");

async function main() {
    const server = new BasicServer();
    const publicKey = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjIBZM1RuG3OQ1lbUlbWI
HTEW0yMo+lJ4Tppkuy43E9NrD0MbPLQeYIMBiUneP8Lb/CdKdQbKuN8DhUfiwsGM
Gamf1MrC8ZdqoN5uCzezXICHg3wkjXW3Pe1cINS4tMoAa/b0HFE6vbICg+yBbuPI
p59MRe38RvxYahkGKP36yxP0Q1d3G3IcgeSj/ZUaGmeeFo98sqIjJZ67FzhEvdWm
US5XeC2FlDIO2ls60QZuNUIGKgE9OUhkOU2lP2dcf9YutZyeY8OcOdDJ+wGL5dxG
CePxPsodF1EmI5G6/RxjukNvh7oBqVJdlo1U5j4UAGgI95P3Q0kiRXGGRM9sjVfz
BQIDAQAB
-----END PUBLIC KEY-----`;

    await server.start({
        dataPath: path.resolve("./data"),
        graphQLServiceConfigOverride: (config) => {
            config.disableAuthentication = true;
        },
        logger: new ConsoleLogger("debug"),
        refreshTokenValidators: [
            {
                algorithms: ["RS256"],
                issuer: "myissuer",
                publicKey,
                audience: "myApp",
                isAdminField: "admin"
            }
        ]
    });
}

main().catch((err) => {
    console.log(err);
    process.exit(1);
});

        */

        [Test, NUnit.Framework.Explicit("Requires non-default ROS")]
        public void UserLogin_WhenCustomRefreshToken_LogsUserIn()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJuYW1lIjoiSm9obiBEb2UiLCJhZG1pbiI6dHJ1ZSwiaWF0IjoxNTE2MjM5MDIyLCJhdWQiOiJteUFwcCIsImlzcyI6Im15aXNzdWVyIn0.Xhl39nnVXIgTUqDKEfz2mDiHcfH8vZGDC4gJxAHZmQ_usf-uXTXfDxkjME2W5ynKeWUQrzIhOliHaouJq-XJpzqKPvQ4d70LwtijNC53O4SUaHHaTkhh98OLOZif0md7xHeeEJAI9sixNK4GDzA88a2K5dZ9dmv3XJJ3url481CNK5mSCMgTcN5dzChbewmJ327J7mDsHF74Nvdazevk7UyShLz0YfJaPr2ny9feUXcG7yMRTfg3XoSHGUZ1IDDyvjjslfelTZWIR3ccmiua2wyN1EKAQE0o1Ft89VFHDxIHVvfgdXr9aQvtEaPR7-GChL8rx1WiqujSMJ0DZC80gQ";
                var credentials = Credentials.CustomRefreshToken(token);

                var realmPath = Guid.NewGuid().ToString();
                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
                var config = new FullSyncConfiguration(new Uri($"/~/{realmPath}", UriKind.Relative), user);
                using (var realm = await GetRealmAsync(config))
                {
                    realm.Write(() =>
                    {
                        realm.Add(new PrimaryKeyInt32Object
                        {
                            Int32Property = 123
                        });
                    });

                    await GetSession(realm).WaitForUploadAsync();
                }

                var token2 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0NTYiLCJuYW1lIjoiSm9obiBEb2UiLCJhZG1pbiI6dHJ1ZSwiaWF0IjoxNTE2MjM5MDIyLCJhdWQiOiJteUFwcCIsImlzcyI6Im15aXNzdWVyIn0.Hum9NA5KfBqKNsRN6hckbijSAME4LfH2xwmqwPrfjVEBlHRg6HIOnV4gxjY_KUhaazjsExNjAGEhxAamTiefHgvTryVlXwgLjaVs2DpR7F2t1JkpB9b7bU8fo0XV1ZhQ40s9_s3_t6Gdaf8cewSr2ADe0q71c09kP4VtxHQlzXkKuDjkwVXhaXFKglaJNy2Lhk04ybKJn0g_H-sWv2keTW1-J1RhZCzkB_o1Xv-SqoB_n5lahZ3rSUvbQalcQn20mOetTlfAkYfi3Eee4bYzc0iykDdG124uUnQVXXiQR67qlB4zqJ1LuG84KBYcO7W5g_kIBq7YzNaP68xT_x2YBw";
                var credentials2 = Credentials.CustomRefreshToken(token2);

                var user2 = await User.LoginAsync(credentials2, SyncTestHelpers.AuthServerUri);

                await user.ApplyPermissionsAsync(PermissionCondition.UserId(user2.Identity), $"/~/{realmPath}", AccessLevel.Write);

                var permissions = await user2.GetGrantedPermissionsAsync(Recipient.CurrentUser);

                var userFooPermission = permissions.SingleOrDefault(p => p.Path.EndsWith($"/{realmPath}"));
                Assert.That(userFooPermission, Is.Not.Null);

                var config2 = new FullSyncConfiguration(new Uri(userFooPermission.Path, UriKind.Relative), user2);
                using (var realm = await GetRealmAsync(config2))
                {
                    var objects = realm.All<PrimaryKeyInt32Object>();
                    Assert.That(objects.Count(), Is.EqualTo(1));
                    Assert.That(objects.Single().Int32Property, Is.EqualTo(123));
                }
            });
        }

        [Test, NUnit.Framework.Explicit("Requires non-default ROS")]
        public void User_WhenCustomRefreshToken_CanUpdateToken()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTYiLCJuYW1lIjoiSm9obiBEb2UiLCJhZG1pbiI6dHJ1ZSwiaWF0IjoxNTE2MjM5MDIyLCJhdWQiOiJteUFwcCIsImlzcyI6Im15aXNzdWVyIn0.FmRf5n3o83vduCIShxmXOTWBegJwQqZKWNakIjQN3OAxfjJBK2tkSJGBqBBARN7nkEAWypGQzk1VkjuIKAZfGC1QpSSyv3RBw3D85hNs_aRvHgh2PXIiWbxMvRdZF6N5gN4Zi_47TsL67FqthQV6btOvrwqUuY5EY3vqW8LJT9D-966j6xmLOG7ZeEpWjNVvFx9nR5DmOYIXvamWGLCND_cqYhWcgrSs0I0FMZ6IxfjoiUZST5vc_c18XIbuszongqDUMJEIPbvjmN31tCuLXDuorf3eOpALIIsfR1Dt-RnkoOYAJrPTUjg_NnVqbIj0RzPzdbx7lClP1gZbE3HAjw";
                var credentials = Credentials.CustomRefreshToken(token);

                var user = await User.LoginAsync(credentials, SyncTestHelpers.AuthServerUri);
                Assert.That(token, Is.EqualTo(user.RefreshToken));

                var config = new FullSyncConfiguration(new Uri($"/~/{Guid.NewGuid()}", UriKind.Relative), user);

                // Can't use the async version as out token is expired
                using (var expiredRealm = Realm.GetInstance(config))
                {
                    expiredRealm.Write(() =>
                    {
                        expiredRealm.Add(new PrimaryKeyInt32Object
                        {
                            Int32Property = 456
                        });
                    });
                }

                var newToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTYiLCJuYW1lIjoiSm9obiBEb2UiLCJhZG1pbiI6dHJ1ZSwiaWF0IjoxNTE2MjM5MDIyLCJhdWQiOiJteUFwcCIsImlzcyI6Im15aXNzdWVyIiwic29tZXRoaW5nIjoiZWxzZSJ9.PnfTPoeLmfbjuuKDDzLyZ6BgLVjOmx8qazVdEkVxbjy5jtJnGIyyl77y71E4Auf4sIOqLzqs6Tve4JnrZIltXSzLBnmC76JPW9t3LT0-t09UGG7K0eTYaySlXgzjTZ1bEyc3plnr2Vw4y3g4uonmsU6fliaKoqpWnW-UHDMsPdRJR3BzQYIBkj3SSwCCb-uDRsZWQhyx2CyVvsJgAow_jae5oi38QO5UC6kqCMflUxMHDR5MmSRuhTvtA3Uk0rYMTnh4LzWhmL5yH_uSgBwluTcTJxnxU_jf_S9HqnbuBnyWbwlDVsd-ABffF-LkWhj1uSCW9OpSVBJyF5ekTYDqNQ";
                user.RefreshToken = newToken;
                Assert.That(newToken, Is.EqualTo(user.RefreshToken));

                // Ensure we can still sync
                using (var realm = await GetRealmAsync(config))
                {
                    realm.Write(() =>
                    {
                        realm.Add(new PrimaryKeyInt32Object
                        {
                            Int32Property = 123
                        });
                    });

                    await GetSession(realm).WaitForUploadAsync();

                    // Ensure we have both objects
                    Assert.That(realm.All<PrimaryKeyInt32Object>().Count(), Is.EqualTo(2));
                }
            });
        }


        [Test]
        public void User_WhenCustomRefreshToken_CanLoginAUserDirectly()
        {
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJQZXNobzEyMyIsImlhdCI6MTUxNjIzOTAyMn0.Hu99JXT28Gq2Zf-KjM75I1nBxhU3WxWqWjJSaAc6TP6Bwy4czlR4krw8LDdjrBJ0zyI6CAyVAR_y1lRyFbz3j-jaXmmuwyIgO61PJxOJaWEmBawz1_C3_z8-XjxNeBH8lXgGFcPzDG0HqbmXMgPEAFK4LAY5tFMkzEPP3w5OslZu17ixlzsYtSkia_bQSYId6-KSkj-tZE6KExgumIyF4JnS51s8oDr6U3C4qa1Y6-QkyWqCdFMRMd6558qECbVxV5CPP0x58LFyC6Coz1Xob6zkB2b_ba5FepFO-cJtvXaIBDOYsV3GsD9NfW8cLDCNqeJADbJuCP_iHRIDT4vFdg";
            var credentials = Credentials.CustomRefreshToken(token);
            var user = User.LoginAsync(credentials, new Uri($"http://{SyncTestHelpers.FakeRosUrl}")).Result;

            Assert.That(user.Identity, Is.EqualTo("Pesho123"));
        }

        #endregion

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
