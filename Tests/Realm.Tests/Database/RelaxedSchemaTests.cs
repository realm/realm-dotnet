////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture]
    [Preserve(AllMembers = true)]
    public class RelaxedSchemaTests : RealmInstanceTest
    {
        private Person _person = null!;

        protected override RealmConfiguration CreateConfiguration(string path)
        {
            var newConfig = base.CreateConfiguration(path);
            newConfig.RelaxedSchema = true;
            return newConfig;
        }

        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            _person = _realm.Write(() =>
            {
                return _realm.Add(new Person());
            });
        }

        [Test]
        public void GetSet_Basic()
        {
            var testObj = new Person { FirstName = "Luigi" };
            var testList = new List<RealmValue> { 1, "test", true };
            var testDict = new Dictionary<string, RealmValue> { { "t1", true }, { "t2", "string" } };

            // Basic set/get
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("propString", "testval");
                _person.DynamicApi.Set("propInt", 10);
                _person.DynamicApi.Set("propObj", testObj);
                _person.DynamicApi.Set("propList", testList);
                _person.DynamicApi.Set("propDict", testDict);
                _person.DynamicApi.Set("propNull", RealmValue.Null);
            });

            Assert.That(_person.DynamicApi.Get<string>("propString"), Is.EqualTo("testval"));
            Assert.That(_person.DynamicApi.Get<int>("propInt"), Is.EqualTo(10));
            Assert.That(_person.DynamicApi.Get<Person>("propObj"), Is.EqualTo(testObj));
            Assert.That(_person.DynamicApi.Get<IList<RealmValue>>("propList"), Is.EqualTo(testList));
            Assert.That(_person.DynamicApi.Get<IDictionary<string, RealmValue>>("propDict"), Is.EqualTo(testDict));
            Assert.That(_person.DynamicApi.Get<RealmValue>("propNull"), Is.EqualTo(RealmValue.Null));
        }


        [Test]
        public void FlexibleSchema_BaseTest()
        {
            var testObj = new Person { FirstName = "Luigi" };
            var testList = new List<RealmValue> { 1, "test", true };

            //Additional properties should be empty in the beginning
            Assert.That(_person.ExtendedObjectSchema.ExtraProperties, Is.Empty);

            Assert.That(_person.ExtendedObjectSchema.HasProperty("propString"), Is.False);

            // Basic set/get
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("propString", "testval");
                _person.DynamicApi.Set("propInt", 10);
                _person.DynamicApi.Set("propObj", testObj);
                _person.DynamicApi.Set("propList", testList);
                _person.DynamicApi.Set("propNull", RealmValue.Null);
            });

            Assert.That(_person.DynamicApi.Get<string>("propString"), Is.EqualTo("testval"));
            Assert.That(_person.DynamicApi.Get<int>("propInt"), Is.EqualTo(10));
            Assert.That(_person.DynamicApi.Get<Person>("propObj"), Is.EqualTo(testObj));
            Assert.That(_person.DynamicApi.Get<IList<RealmValue>>("propList"), Is.EqualTo(testList));
            Assert.That(_person.DynamicApi.Get<RealmValue>("propNull"), Is.EqualTo(RealmValue.Null));

            Assert.That(_person.ExtendedObjectSchema.HasProperty("propString"), Is.True);

            bool found;

            found = _person.DynamicApi.TryGet<string>("propString", out var stringVal);
            Assert.That(found, Is.True);
            Assert.That(stringVal, Is.EqualTo("testval"));

            found = _person.DynamicApi.TryGet<IList<RealmValue>>("propList", out var listVal);
            Assert.That(found, Is.True);
            Assert.That(listVal, Is.EqualTo(testList));

            // Change type
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("propString", 23);
            });

            Assert.That(_person.DynamicApi.Get<int>("propString"), Is.EqualTo(23));

            // Get unknown property
            Assert.That(() => _person.DynamicApi.Get<int>("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));
            Assert.That(() => _person.DynamicApi.Get("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));

            // TryGet unknown property
            found = _person.DynamicApi.TryGet("unknonProp", out var rvUnKnownValue);
            Assert.That(found, Is.False);
            Assert.That(rvUnKnownValue, Is.EqualTo(RealmValue.Null));

            found = _person.DynamicApi.TryGet<int>("unknonProp", out var intUnknownVal);
            Assert.That(found, Is.False);
            Assert.That(intUnknownVal, Is.EqualTo(default(int)));

            found = _person.DynamicApi.TryGet<IList<RealmValue>>("unknonProp", out var listUnknonwVal);
            Assert.That(found, Is.False);
            Assert.That(listUnknonwVal, Is.EqualTo(default(IList<RealmValue>)));

            // Unset property
            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("propString");
            });
            Assert.That(() => _person.DynamicApi.Get("propString"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: propString"));

            Assert.That(() => _realm.Write(() =>
            {
                _person.DynamicApi.Unset("propString");
            }), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Could not erase property: propString"));

            Assert.That(_person.ExtendedObjectSchema.HasProperty("propString"), Is.False);

            // Unset property in schema
            Assert.That(() => _realm.Write(() =>
            {
                _person.DynamicApi.Unset("FirstName");
            }), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Could not erase property: FirstName"));

            // TryUnset property
            _realm.Write(() =>
            {
                bool unsetVal = _person.DynamicApi.TryUnset("propInt");
                Assert.That(unsetVal, Is.True);
            });
            Assert.That(() => _person.DynamicApi.Get("propInt"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: propInt"));

            _realm.Write(() =>
            {
                bool unsetVal = _person.DynamicApi.TryUnset("propInt");
                Assert.That(unsetVal, Is.False);
            });

            // TryUnset property in schema
            // We need to get a new core method to check if a certain property is in the extra
            // properties
            //realm.Write(() =>
            //{
            //    bool unsetVal = person.DynamicApi.TryUnset("FirstName");
            //    Assert.That(unsetVal, Is.False);
            //});

            // Get all extra properties keys
            Assert.That(_person.ExtendedObjectSchema.ExtraProperties.Select(p => p.Name), Is.EquivalentTo(new[] { "propObj", "propList", "propNull" }));
        }

    }
}
