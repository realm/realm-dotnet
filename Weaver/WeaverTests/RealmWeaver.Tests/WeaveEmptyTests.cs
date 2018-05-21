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

using NUnit.Framework;

namespace RealmWeaver
{
    [TestFixture(AssemblyType.NonPCL)]
    [TestFixture(AssemblyType.PCL)]
    public class WeaveEmptyTests : WeaverTestBase 
    {
        private string _sourceAssemblyPath;

        public WeaveEmptyTests(AssemblyType assemblyType) : base(assemblyType)
        {
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {            
            _sourceAssemblyPath = _assemblyType == AssemblyType.NonPCL
                ? typeof(RealmFreeAssemblyToProcess.NonPCLModuleLocator).Assembly.Location
                : typeof(PCLRealmFreeAssemblyToProcess.PCLModuleLocator).Assembly.Location;

            WeaveRealm(_sourceAssemblyPath);
        }

        [Test]
        public void WeavingWithoutRealmsShouldBeRobust()
        {
            // All warnings and errors are gathered once, so in order to ensure only the correct ones
            // were produced, we make one assertion on all of them here.

            var expectedWarnings = new string[0];

            var expectedErrors = new string[0];

            Assert.That(_errors, Is.EquivalentTo(expectedErrors));
            Assert.That(_warnings, Is.EquivalentTo(expectedWarnings));
        }
    }
}
