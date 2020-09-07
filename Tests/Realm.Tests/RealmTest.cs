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

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Realms.Tests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmTest
    {
        private readonly List<Realm> _realms = new List<Realm>();

        private bool _isSetup;

        protected virtual bool OverrideDefaultConfig => true;

        static RealmTest()
        {
            InteropConfig.DefaultStorageFolder = Path.Combine(Path.GetTempPath(), $"realm-tests-${System.Diagnostics.Process.GetCurrentProcess().Id}");
            Directory.CreateDirectory(InteropConfig.DefaultStorageFolder);
        }

        [SetUp]
        public void SetUp()
        {
            if (!_isSetup)
            {
                if (OverrideDefaultConfig)
                {
                    RealmConfiguration.DefaultConfiguration = new RealmConfiguration(Path.GetTempFileName());
                }

                CustomSetUp();
                _isSetup = true;
            }
        }

        protected virtual void CustomSetUp()
        {
        }

        protected void CleanupOnTearDown(Realm realm)
        {
            _realms.Add(realm);
        }

        [TearDown]
        public void TearDown()
        {
            if (_isSetup)
            {
                CustomTearDown();

                Realms.Sync.SharedRealmHandleExtensions.ResetForTesting();
                _isSetup = false;
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            }
        }

        protected virtual void CustomTearDown()
        {
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
        }

        protected Realm GetRealm(RealmConfigurationBase config = null)
        {
            var result = Realm.GetInstance(config ?? RealmConfiguration.DefaultConfiguration);
            CleanupOnTearDown(result);
            return result;
        }
    }
}
