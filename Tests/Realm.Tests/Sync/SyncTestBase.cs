////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly List<Session> _sessions = new List<Session>();
        private readonly List<Realm> _realms = new List<Realm>();

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            var defaultFolder = InteropConfig.DefaultStorageFolder;
            if (TestHelpers.IsWindows)
            {
                // We do this to reduce the length of the folders in Windows
                var testsIndex = defaultFolder.IndexOf("\\Tests\\");
                var docsIndex = defaultFolder.IndexOf("\\Documents") + 1;

                if (testsIndex > -1 && docsIndex > testsIndex)
                {
                    defaultFolder = Path.Combine(defaultFolder.Substring(0, testsIndex), defaultFolder.Substring(docsIndex))
                                        .Replace("\\Documents", "\\D");

                    Directory.CreateDirectory(defaultFolder);
                }
            }

            if (TestHelpers.IsMacOS)
            {
                // VS for Mac hangs when Realm files are written in a location it doesn't ignore.
                defaultFolder = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Documents");
                Directory.CreateDirectory(defaultFolder);
            }

            SyncConfigurationBase.UserAgent = GetType().Name;
            SyncConfigurationBase.Initialize(UserPersistenceMode.NotEncrypted, null, false, defaultFolder);
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            foreach (var realm in _realms)
            {
                try
                {
                    realm.Dispose();
                    Realm.DeleteRealm(realm.Config);
                }
                catch
                {
                }
            }

            foreach (var session in _sessions)
            {
                session?.CloseHandle();
            }
        }

        protected void CleanupOnTearDown(Session session)
        {
            _sessions.Add(session);
        }

        protected void CleanupOnTearDown(Realm realm)
        {
            _realms.Add(realm);
        }

        protected Session GetSession(Realm realm)
        {
            var result = realm.GetSession();
            CleanupOnTearDown(result);
            return result;
        }

        protected Realm GetRealm(RealmConfigurationBase config)
        {
            var result = Realm.GetInstance(config);
            CleanupOnTearDown(result);
            return result;
        }

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config, bool openAsync = true)
        {
            Realm result;
            if (openAsync)
            {
                result = await Realm.GetInstanceAsync(config);
            }
            else
            {
                result = Realm.GetInstance(config);
                await SyncTestHelpers.WaitForDownloadAsync(result);
            }

            CleanupOnTearDown(result);
            return result;
        }
    }
}
