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

            Assert.That(_person.DynamicApi.Get("propString").As<string>(), Is.EqualTo("testval"));
            Assert.That(_person.DynamicApi.Get("propInt").As<int>(), Is.EqualTo(10));
            Assert.That(_person.DynamicApi.Get("propObj").As<Person>, Is.EqualTo(testObj));
            Assert.That(_person.DynamicApi.Get("propList").As<IList<RealmValue>>, Is.EqualTo(testList));
            Assert.That(_person.DynamicApi.Get("propDict").As<IDictionary<string, RealmValue>>(), Is.EqualTo(testDict));
            Assert.That(_person.DynamicApi.Get("propNull"), Is.EqualTo(RealmValue.Null));
        }

        [Test]
        public void GetSet_OnEmbeddedObject()
        {
            var obj = new ObjectWithEmbeddedProperties { AllTypesObject = new EmbeddedAllTypesObject() };
            var embeddedObj = obj.AllTypesObject;

            var testObj = new Person { FirstName = "Luigi" };
            var testList = new List<RealmValue> { 1, "test", true };
            var testDict = new Dictionary<string, RealmValue> { { "t1", true }, { "t2", "string" } };

            _realm.Write(() =>
            {
                _realm.Add(obj);

                embeddedObj.DynamicApi.Set("propString", "testval");
                embeddedObj.DynamicApi.Set("propInt", 10);
                embeddedObj.DynamicApi.Set("propObj", testObj);
                embeddedObj.DynamicApi.Set("propList", testList);
                embeddedObj.DynamicApi.Set("propDict", testDict);
                embeddedObj.DynamicApi.Set("propNull", RealmValue.Null);
            });

            Assert.That(embeddedObj.DynamicApi.Get<string>("propString"), Is.EqualTo("testval"));
            Assert.That(embeddedObj.DynamicApi.Get<int>("propInt"), Is.EqualTo(10));
            Assert.That(embeddedObj.DynamicApi.Get<Person>("propObj"), Is.EqualTo(testObj));
            Assert.That(embeddedObj.DynamicApi.Get<IList<RealmValue>>("propList"), Is.EqualTo(testList));
            Assert.That(embeddedObj.DynamicApi.Get<IDictionary<string, RealmValue>>("propDict"), Is.EqualTo(testDict));
            Assert.That(embeddedObj.DynamicApi.Get<RealmValue>("propNull"), Is.EqualTo(RealmValue.Null));

            Assert.That(embeddedObj.DynamicApi.Get("propString").As<string>(), Is.EqualTo("testval"));
            Assert.That(embeddedObj.DynamicApi.Get("propInt").As<int>(), Is.EqualTo(10));
            Assert.That(embeddedObj.DynamicApi.Get("propObj").As<Person>, Is.EqualTo(testObj));
            Assert.That(embeddedObj.DynamicApi.Get("propList").As<IList<RealmValue>>, Is.EqualTo(testList));
            Assert.That(embeddedObj.DynamicApi.Get("propDict").As<IDictionary<string, RealmValue>>(), Is.EqualTo(testDict));
            Assert.That(embeddedObj.DynamicApi.Get("propNull"), Is.EqualTo(RealmValue.Null));
        }

        [Test]
        public void Get_OnMissingProperty_Throws()
        {
            Assert.That(() => _person.DynamicApi.Get<int>("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));
            Assert.That(() => _person.DynamicApi.Get("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));
        }

        [Test]
        public void TryGet_OnMissingProperty_ReturnsFalse()
        {
            bool found;

            found = _person.DynamicApi.TryGet("unknonProp", out var rvUnKnownValue);
            Assert.That(found, Is.False);
            Assert.That(rvUnKnownValue, Is.EqualTo(RealmValue.Null));

            found = _person.DynamicApi.TryGet<int>("unknonProp", out var intUnknownVal);
            Assert.That(found, Is.False);
            Assert.That(intUnknownVal, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryGet_OnExistingProperty_ReturnsTrue()
        {
            var testList = new List<RealmValue> { 1, "test", true };

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("propString", "testval");
                _person.DynamicApi.Set("propList", testList);
            });

            bool found;

            found = _person.DynamicApi.TryGet<string>("propString", out var stringVal);
            Assert.That(found, Is.True);
            Assert.That(stringVal, Is.EqualTo("testval"));

            found = _person.DynamicApi.TryGet<IList<RealmValue>>("propList", out var listVal);
            Assert.That(found, Is.True);
            Assert.That(listVal, Is.EqualTo(testList));

            found = _person.DynamicApi.TryGet<IList<RealmValue>>("unknonProp", out var listUnknonwVal);
            Assert.That(found, Is.False);
            Assert.That(listUnknonwVal, Is.EqualTo(default(IList<RealmValue>)));
        }

        [Test]
        public void Set_OnSameProperty_WorksWithSameType()
        {
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", "testval");
            });
            Assert.That(_person.DynamicApi.Get<string>("prop"), Is.EqualTo("testval"));

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", "testval2");
            });
            Assert.That(_person.DynamicApi.Get<string>("prop"), Is.EqualTo("testval2"));
        }

        [Test]
        public void Set_OnSameProperty_WorksWithDifferentType()
        {
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", "testval");
            });
            Assert.That(_person.DynamicApi.Get<string>("prop"), Is.EqualTo("testval"));

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", 23);
            });
            Assert.That(_person.DynamicApi.Get<int>("prop"), Is.EqualTo(23));

            var testList = new List<RealmValue> { 1, "test", true };

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", testList);
            });
            Assert.That(_person.DynamicApi.Get<IList<RealmValue>>("prop"), Is.EqualTo(testList));
        }

        [Test]
        public void Set_OnSameProperty_WorksWithCollectionOfSameType()
        {
            var testList1 = new List<RealmValue> { 1, "test", true };
            var testList2 = new List<RealmValue> { false, 50, "st" };

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", testList1);
            });
            Assert.That(_person.DynamicApi.Get<IList<RealmValue>>("prop"), Is.EqualTo(testList1));

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", testList2);
            });
            Assert.That(_person.DynamicApi.Get<IList<RealmValue>>("prop"), Is.EqualTo(testList2));
        }

        [Test]
        public void Unset_OnExtraProperty_RemovesProperty()
        {
            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", "testval");
            });
            Assert.That(_person.DynamicApi.Get<string>("prop"), Is.EqualTo("testval"));

            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop");
            });
            Assert.That(_person.DynamicApi.TryGet("prop", out _), Is.False);
        }

        [Test]
        public void Unset_OnUnknownProperty_DoesNotThrow()
        {
            Assert.That(() => _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop");
            }), Throws.Nothing);
        }

        [Test]
        public void Unset_OnSchemaProperty_Throws()
        {
            Assert.That(() => _realm.Write(() =>
            {
                _person.DynamicApi.Unset("FirstName");
            }), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Could not erase property: FirstName"));
        }

        [Test]
        public void ObjectSchema_HasProperty_ReturnsCorrectBoolean()
        {
            Assert.That(_person.ObjectSchema.HasProperty("prop"), Is.False);

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop", "testval");
            });
            Assert.That(_person.ObjectSchema.HasProperty("prop"), Is.True);

            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop");
            });
            Assert.That(_person.ObjectSchema.HasProperty("prop"), Is.False);
        }

        [Test]
        public void ObjectSchema_Enumerator_EnumeratesExtraProperties()
        {
            Assert.That(_person.ObjectSchema.Where(p => p.IsExtraProperty), Is.Empty);

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop1", "testval");
                _person.DynamicApi.Set("prop2", 10);
            });

            Assert.That(_person.ObjectSchema.Where(p => p.IsExtraProperty).Select(p => p.Name),
                Is.EquivalentTo(new[] { "prop1", "prop2" }));

            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop1");
            });

            Assert.That(_person.ObjectSchema.Where(p => p.IsExtraProperty).Select(p => p.Name),
                Is.EquivalentTo(new[] { "prop2" }));

            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop2");
            });

            Assert.That(_person.ObjectSchema.Where(p => p.IsExtraProperty), Is.Empty);
        }

        [Test]
        public void ObjectSchema_TryFindProperty_ReturnsExtraProperties()
        {
            bool foundProperty;
            Schema.Property property;

            _realm.Write(() =>
            {
                _person.DynamicApi.Set("prop1", "testval");
            });

            foundProperty = _person.ObjectSchema.TryFindProperty("prop1", out property);
            Assert.That(foundProperty, Is.True);
            Assert.That(property.IsExtraProperty, Is.True);
            Assert.That(property.Name, Is.EqualTo("prop1"));

            _realm.Write(() =>
            {
                _person.DynamicApi.Unset("prop1");
            });

            foundProperty = _person.ObjectSchema.TryFindProperty("prop1", out property);
            Assert.That(foundProperty, Is.False);
        }

        /* Missing tests:
         * - extended schema with schema property not in data model (need sync for this)
         * - open realm with/without relaxed schema config
         * - subscribeForNotifications/property changes tests
         * - keypath filtering
         * - queries support using extra properties
         * - support for asymmetric objects
         * - all sync tests
         */


    }
}
