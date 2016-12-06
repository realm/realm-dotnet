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
using System.IO;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests
    {
        [Test]
        public async void SyncConfiguration_WithoutPath()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            var config = new SyncConfiguration(user, serverUri);

            Realm.DeleteRealm(config);

            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = Realm.GetInstance(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
        }

        [Test]
        public async void SyncConfiguration_WithRelativePath()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            var config = new SyncConfiguration(user, serverUri, "myrealm.realm");

            Realm.DeleteRealm(config);

            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = Realm.GetInstance(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
            Assert.That(config.DatabasePath.EndsWith("myrealm.realm"));
        }

        [Test]
        public async void SyncConfiguration_WithAbsolutePath()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "bla.realm");
            var config = new SyncConfiguration(user, serverUri, path);

            Realm.DeleteRealm(config);

            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);
            
            using (var realm = Realm.GetInstance(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
            Assert.That(config.DatabasePath, Is.EqualTo(path));
        }
    }
}