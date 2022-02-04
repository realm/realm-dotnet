////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Runtime.InteropServices;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class GuidRepresentationMigrationTests : RealmTest
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "_realm_flip_guid_for_testing", CallingConvention = CallingConvention.Cdecl)]
        private static extern void flip_guid_for_testing([In, Out] byte[] guid_bytes);

        [Test]
        public void Migration_FlipGuid_ShouldProduceCorrectRepresentation()
        {
            var guid = Guid.NewGuid();
            var expected = GuidConverter.ToBytes(guid, GuidRepresentation.Standard);
            var actual = guid.ToByteArray();

            flip_guid_for_testing(actual);

            Assert.AreEqual(expected, actual);
        }
    }
}
