﻿////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using Tests.Database;

namespace Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private List<Session> _sessions = new List<Session>();
        private List<Realm> _realms = new List<Realm>();

        private readonly UserPersistenceMode? persistence = TestHelpers.IsMacOS ? UserPersistenceMode.NotEncrypted : (UserPersistenceMode?)null;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            SharedRealmHandleExtensions.ConfigureFileSystem(persistence, null, false);
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
                session.Handle.Close();
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

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config)
        {
            var result = await Realm.GetInstanceAsync(config);
            CleanupOnTearDown(result);
            return result;
        }
    }
}
