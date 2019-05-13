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

using NUnit.Framework;

namespace RealmWeaver
{
    public class WeaveEmptyTests : WeaverTestBase
    {
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            WeaveRealm(typeof(RealmFreeAssemblyToProcess.SimpleClassWithoutRealm).Assembly.Location);
        }

        [Test]
        public void WeavingWithoutRealmsShouldBeRobust()
        {
            // All warnings and errors are gathered once, so in order to ensure only the correct ones
            // were produced, we make one assertion on all of them here.

            Assert.That(_errors, Is.Empty);
            Assert.That(_warnings, Is.Empty);
            Assert.That(_messages, Is.EquivalentTo(new[] { "Not weaving assembly 'RealmFreeAssemblyToProcess, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' because it doesn't reference Realm." }));
        }
    }
}
