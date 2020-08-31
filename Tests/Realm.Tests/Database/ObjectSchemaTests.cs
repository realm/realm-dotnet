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

using System.Reflection;
using NUnit.Framework;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectSchemaTests
    {
        private class RequiredPropertyClass : RealmObject
        {
            [Required]
            public string FooRequired { get; set; }
        }

        [Test]
        public void Property_WhenRequired_ShouldBeNonNullable()
        {
            var schema = ObjectSchema.FromType(typeof(RequiredPropertyClass).GetTypeInfo());

            if (!schema.TryFindProperty(nameof(RequiredPropertyClass.FooRequired), out var prop))
            {
                Assert.Fail("Could not find property");
            }

            Assert.That(prop.Type.HasFlag(PropertyType.Nullable), Is.False);
        }

        [Realms.Explicit]
        private class ExplicitClass : RealmObject
        {
            public int Foo { get; set; }
        }

        [Test]
        public void Class_WhenExplicit_ShouldNotBeInDefaultSchema()
        {
            Assert.That(RealmSchema.Default.Find(nameof(ExplicitClass)), Is.Null);
        }
    }
}
