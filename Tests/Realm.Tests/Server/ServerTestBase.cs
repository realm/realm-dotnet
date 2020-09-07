////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Threading.Tasks;
using Realms.Server;
using Realms.Sync;
using Realms.Tests.Sync;

namespace Realms.Tests.Server
{
    public abstract class ServerTestBase : SyncTestBase
    {
        private string _baseFolder;

        private string UserRealmFolder => Path.Combine(_baseFolder, "user-realms");

        private string NotifierFolder => Path.Combine(_baseFolder, "notifer");

        protected override bool OverrideDefaultConfig => false;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _baseFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(NotifierFolder);
            Directory.CreateDirectory(UserRealmFolder);
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            try
            {
                Directory.Delete(_baseFolder, recursive: true);
            }
            catch
            {
            }
        }

        protected async Task<NotifierConfiguration> GetConfiguration(params INotificationHandler[] handlers)
        {
            var admin = await SyncTestHelpers.GetAdminUserAsync();
            return new NotifierConfiguration(admin)
            {
                Handlers = handlers.ToList(),
                WorkingDirectory = NotifierFolder
            };
        }

        protected async Task<(Realm Realm, string UserId)> CreateRandomRealmAsync(string path)
        {
            var user = await SyncTestHelpers.GetUserAsync();
            var location = Path.Combine(UserRealmFolder, user.Identity, path);
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            var config = new FullSyncConfiguration(new Uri($"~/{path}", UriKind.Relative), user, location);
            var realm = GetRealm(config);
            await SyncTestHelpers.WaitForUploadAsync(realm);
            return (realm, user.Identity);
        }
    }
}
