﻿////////////////////////////////////////////////////////////////////////////
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

namespace IntegrationTests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmTest
    {
        [SetUp]
        public virtual void SetUp()
        {
            RealmConfiguration.DefaultConfiguration = new RealmConfiguration(Path.GetTempFileName());
        }

        [TearDown]
        public virtual void TearDown()
        {
            NativeCommon.reset_for_testing();
        }
    }
}
