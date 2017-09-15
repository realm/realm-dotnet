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
using System.IO;
using System.Linq;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Sync;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests : SyncTestBase
    {
        [Test]
        public void SyncConfiguration_WithoutPath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new SyncConfiguration(user, serverUri);

                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
            });
        }

        [Test]
        public void SyncConfiguration_WithRelativePath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new SyncConfiguration(user, serverUri, "myrealm.realm");

                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
                Assert.That(config.DatabasePath.EndsWith("myrealm.realm"));
            });
        }

        [Test]
        public void SyncConfiguration_WithAbsolutePath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");

                var path = Path.GetTempFileName();
                var config = new SyncConfiguration(user, serverUri, path);

                Realm.DeleteRealm(config);
                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
                Assert.That(config.DatabasePath, Is.EqualTo(path));
            });
        }

        [Test]
        public void SyncConfiguration_WithEncryptionKey_DoesntThrow()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

                var config = new SyncConfiguration(user, new Uri("realm://foobar"))
                {
                    EncryptionKey = TestHelpers.GetEncryptionKey(key)
                };

                Assert.That(() => GetRealm(config), Throws.Nothing);
            });
        }

        [TestCase("http://localhost/~/foo")]
        [TestCase("https://localhost/~/foo")]
        [TestCase("foo://bar/~/foo")]
        public void SyncConfiguration_WrongProtocolTests(string url)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => new SyncConfiguration(user, new Uri(url)), Throws.TypeOf<ArgumentException>());
            });
        }

        [TestCaseSource(nameof(TokenTestCases))]
        public void FeatureTokens_WhenPaid_AllowSync(string token)
        {
            if (!TestHelpers.IsLinux)
            {
                Assert.Ignore("Feature tokens are not required on non-linux platforms");
            }

            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var config = new SyncConfiguration(user, new Uri("realm://foobar"));

                SyncConfiguration.SetFeatureToken(token);
                Assert.That(() => GetRealm(config), Throws.Nothing);
            });
        }

        [Test]
        public void FeatureToken_WhenDeveloper_PreventsSync()
        {
            if (!TestHelpers.IsLinux)
            {
                Assert.Ignore("Feature tokens are not required on non-linux platforms");
            }

            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var config = new SyncConfiguration(user, new Uri("realm://foobar"));

                SyncConfiguration.SetFeatureToken(SyncTestHelpers.DeveloperFeatureToken);
                Assert.That(() => GetRealm(config), Throws.TypeOf<RealmFeatureUnavailableException>());
            });
        }

        private static IEnumerable<object> TokenTestCases()
        {
            yield return new object[] { SyncTestHelpers.ProfessionalFeatureToken };
            yield return new object[] { SyncTestHelpers.EnterpriseFeatureToken };
        }
    }
}