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
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [Preserve(AllMembers = true)]
    public abstract class RealmTest
    {
        private bool _isSetup;

        [SetUp]
        public void SetUp()
        {
            if (!_isSetup)
            {
                RealmConfiguration.DefaultConfiguration = new RealmConfiguration(Path.GetTempFileName());
                CustomSetUp();
                _isSetup = true;
            }
        }

        protected virtual void CustomSetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            if (_isSetup)
            {
                CustomTearDown();
                NativeCommon.reset_for_testing();
                _isSetup = false;
            }
        }

        protected virtual void CustomTearDown()
        {
        }
    }
}
