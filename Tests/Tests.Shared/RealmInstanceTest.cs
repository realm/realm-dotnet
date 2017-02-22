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

using System.IO;
using Realms;

namespace IntegrationTests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmInstanceTest : RealmTest
    {
        protected RealmConfiguration _configuration = new RealmConfiguration(Path.GetTempFileName());

        protected Realm _realm;

        public override void SetUp()
        {
            base.SetUp();
            _realm = Realm.GetInstance(_configuration);
        }

        public override void TearDown()
        {
            _realm.Dispose();
            Realm.DeleteRealm(_realm.Config);
            base.TearDown();
        }
    }
}
