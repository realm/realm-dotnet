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
using System.Threading;
using System.Threading.Tasks;
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
            InteropConfig.DefaultStorageFolder = Path.Combine(Path.GetTempPath(), $"rt-${System.Diagnostics.Process.GetCurrentProcess().Id}");
            Directory.CreateDirectory(InteropConfig.DefaultStorageFolder);
        }

        [SetUp]
        public void SetUp()
        {
            if (!_isSetup)
            {
                if (OverrideDefaultConfig)
                {
                    RealmConfiguration.DefaultConfiguration = new RealmConfiguration(Guid.NewGuid().ToString());
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

                _isSetup = false;
                try
                {
                    Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
                }
                catch
                {
                }
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
            var result = Realm.GetInstance(config);
            CleanupOnTearDown(result);
            return result;
        }

        protected Realm GetRealm(string path)
        {
            var result = Realm.GetInstance(path);
            CleanupOnTearDown(result);
            return result;
        }

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config, CancellationToken cancellationToken = default)
        {
            var result = await Realm.GetInstanceAsync(config, cancellationToken);
            CleanupOnTearDown(result);
            return result;
        }

        protected Realm Freeze(Realm realm)
        {
            var result = realm.Freeze();
            CleanupOnTearDown(result);
            return result;
        }
    }
}
