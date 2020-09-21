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
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly List<Session> _sessions = new List<Session>();
        private readonly List<App> _apps = new List<App>();

        protected App CreateApp(AppConfiguration config)
        {
            var app = App.Create(config);
            _apps.Add(app);
            return app;
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            foreach (var session in _sessions)
            {
                session?.CloseHandle();
            }

            foreach (var app in _apps)
            {
                app.AppHandle.ResetForTesting();
            }
        }

        protected void CleanupOnTearDown(Session session)
        {
            _sessions.Add(session);
        }

        protected Session GetSession(Realm realm)
        {
            var result = realm.GetSession();
            CleanupOnTearDown(result);
            return result;
        }

        protected static async Task WaitForUploadAsync(Realm realm)
        {
            var session = realm.GetSession();
            await session.WaitForUploadAsync();
            session.CloseHandle();
        }

        protected static async Task WaitForDownloadAsync(Realm realm)
        {
            var session = realm.GetSession();
            await session.WaitForDownloadAsync();
            session.CloseHandle();
        }

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config, bool openAsync = true, CancellationToken cancellationToken = default)
        {
            Realm result;
            if (openAsync)
            {
                result = await Realm.GetInstanceAsync(config, cancellationToken);
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
