////////////////////////////////////////////////////////////////////////////
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

using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmIntegerTests : RealmInstanceTest
    {
        [Test]
        public void ManagedIntegerTests()
        {
            var counter = new CounterObject();
            _realm.Write(() =>
            {
                _realm.Add(counter);
            });

            Assert.That(counter.ByteProperty == 0);
            Assert.That(counter.Int16Property == 0);
            Assert.That(counter.Int32Property == 0);
            Assert.That(counter.Int64Property == 0);
            Assert.That(counter.NullableByteProperty, Is.Null);
            Assert.That(counter.NullableInt16Property, Is.Null);
            Assert.That(counter.NullableInt32Property, Is.Null);
            Assert.That(counter.NullableInt64Property, Is.Null);

            _realm.Write(() =>
            {
                counter.ByteProperty.Increment();
            });

            Assert.That(counter.ByteProperty == 1);

            _realm.Write(() =>
            {
                counter.ByteProperty++;
            });

            Assert.That(counter.ByteProperty == 2);
        }
    }
}
