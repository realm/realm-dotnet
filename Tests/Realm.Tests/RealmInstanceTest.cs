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

namespace Realms.Tests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmInstanceTest : RealmTest
    {
        protected RealmConfiguration _configuration = new RealmConfiguration(Path.GetTempFileName());

        private Lazy<Realm> _lazyRealm;

        protected Realm _realm => _lazyRealm.Value;

        protected override void CustomSetUp()
        {
            _lazyRealm = new Lazy<Realm>(() => Realm.GetInstance(_configuration));
            base.CustomSetUp();
        }

        protected override void CustomTearDown()
        {
            _realm.Dispose();
            Realm.DeleteRealm(_realm.Config);
            base.CustomTearDown();
        }
    }
}
