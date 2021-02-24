////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmValueTests : RealmInstanceTest
    {
        [Test]
        public void RealmValue_CharTests([Values] bool isManaged)
        {
            RunNumericTests((char)10, 10, isManaged);
        }

        [Test]
        public void RealmValue_ByteTests([Values] bool isManaged)
        {
            RunNumericTests((byte)10, 10, isManaged);
        }

        [Test]
        public void RealmValue_IntTests([Values] bool isManaged)
        {
            RunNumericTests(10, 10, isManaged);
        }

        [Test]
        public void RealmValue_ShortTests([Values] bool isManaged)
        {
            RunNumericTests((short)10, 10, isManaged);
        }

        [Test]
        public void RealmValue_LongTests([Values] bool isManaged)
        {
            RunNumericTests(10L, 10, isManaged);
        }

        public void RunNumericTests(RealmValue rv, long value, bool isManaged)
        {
            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Int));
            Assert.That(rv != RealmValue.Null);

            // 8 - byte
            Assert.That((byte)rv == value);
            Assert.That(rv.As<byte>() == value);
            Assert.That((byte?)rv == value);
            Assert.That(rv.As<byte?>() == value);
            Assert.That(rv.AsByte() == value);
            Assert.That(rv.AsNullableByte() == value);
            Assert.That(rv.AsByteRealmInteger() == value);
            Assert.That(rv.AsNullableByteRealmInteger() == value);

            // 16 - short
            Assert.That((short)rv == value);
            Assert.That(rv.As<short>() == value);
            Assert.That((short?)rv == value);
            Assert.That(rv.As<short?>() == value);
            Assert.That(rv.AsInt16() == value);
            Assert.That(rv.AsNullableInt16() == value);
            Assert.That(rv.AsInt16RealmInteger() == value);
            Assert.That(rv.AsNullableInt16RealmInteger() == value);

            // 32 - int
            Assert.That((int)rv == value);
            Assert.That(rv.As<int>() == value);
            Assert.That((int?)rv == value);
            Assert.That(rv.As<int?>() == value);
            Assert.That(rv.AsInt32() == value);
            Assert.That(rv.AsNullableInt32() == value);
            Assert.That(rv.AsInt32RealmInteger() == value);
            Assert.That(rv.AsNullableInt32RealmInteger() == value);

            // 64 - long
            Assert.That((long)rv == value);
            Assert.That(rv.As<long>() == value);
            Assert.That((long?)rv == value);
            Assert.That(rv.As<long?>() == value);
            Assert.That(rv.AsInt64() == value);
            Assert.That(rv.AsNullableInt64() == value);
            Assert.That(rv.AsInt64RealmInteger() == value);
            Assert.That(rv.AsNullableInt64RealmInteger() == value);
        }

        [Test]
        public void RealmValue_FloatTests([Values] bool isManaged)
        {
            float value = 10F;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Float));

            Assert.That((float)rv == value);
            Assert.That(rv.As<float>() == value);
            Assert.That((float?)rv == value);
            Assert.That(rv.As<float?>() == value);
            Assert.That(rv.AsFloat() == value);
            Assert.That(rv.AsNullableFloat() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_DoubleTests([Values] bool isManaged)
        {
            double value = 10;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Double));

            Assert.That((double)rv == value);
            Assert.That(rv.As<double>() == value);
            Assert.That((double?)rv == value);
            Assert.That(rv.As<double?>() == value);
            Assert.That(rv.AsDouble() == value);
            Assert.That(rv.AsNullableDouble() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_Decimal128Tests([Values] bool isManaged)
        {
            Decimal128 value = 10;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Decimal128));

            Assert.That((Decimal128)rv == value);
            Assert.That(rv.As<Decimal128>() == value);
            Assert.That((Decimal128?)rv == value);
            Assert.That(rv.As<Decimal128?>() == value);
            Assert.That(rv.AsDecimal128() == value);
            Assert.That(rv.AsNullableDecimal128() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_DecimalTests([Values] bool isManaged)
        {
            decimal value = 10;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Decimal128));

            Assert.That((decimal)rv == value);
            Assert.That(rv.As<decimal>() == value);
            Assert.That((decimal?)rv == value);
            Assert.That(rv.As<decimal?>() == value);
            Assert.That(rv.AsDecimal() == value);
            Assert.That(rv.AsNullableDecimal() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_DateTests([Values] bool isManaged)
        {
            DateTimeOffset value = DateTimeOffset.Now;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Date));

            Assert.That((DateTimeOffset)rv == value);
            Assert.That(rv.As<DateTimeOffset>() == value);
            Assert.That((DateTimeOffset?)rv == value);
            Assert.That(rv.As<DateTimeOffset?>() == value);
            Assert.That(rv.AsDate() == value);
            Assert.That(rv.AsNullableDate() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_ObjectIdTests([Values] bool isManaged)
        {
            ObjectId value = ObjectId.GenerateNewId();
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.ObjectId));

            Assert.That((ObjectId)rv == value);
            Assert.That(rv.As<ObjectId>() == value);
            Assert.That((ObjectId?)rv == value);
            Assert.That(rv.As<ObjectId?>() == value);
            Assert.That(rv.AsObjectId() == value);
            Assert.That(rv.AsNullableObjectId() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_GuidTests([Values] bool isManaged)
        {
            Guid value = Guid.NewGuid();
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Guid));

            Assert.That((Guid)rv == value);
            Assert.That(rv.As<Guid>() == value);
            Assert.That((Guid?)rv == value);
            Assert.That(rv.As<Guid?>() == value);
            Assert.That(rv.AsGuid() == value);
            Assert.That(rv.AsNullableGuid() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_BoolTests([Values] bool isManaged)
        {
            bool value = true;
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Bool));

            Assert.That((bool)rv == value);
            Assert.That(rv.As<bool>() == value);
            Assert.That((bool?)rv == value);
            Assert.That(rv.As<bool?>() == value);
            Assert.That(rv.AsBool() == value);
            Assert.That(rv.AsNullableBool() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_StringTests([Values] bool isManaged)
        {
            string value = "abc";
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.String));

            Assert.That((string)rv == value);
            Assert.That(rv.As<string>() == value);
            Assert.That(rv.AsString() == value);
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_DataTests([Values] bool isManaged)
        {
            byte[] value = new byte[] { 0, 1, 2 };
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Data));

            Assert.That((byte[])rv, Is.EqualTo(value));
            Assert.That(rv.As<byte[]>(), Is.EqualTo(value));
            Assert.That(rv.AsData(), Is.EqualTo(value));
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_ObjectTests([Values] bool isManaged)
        {
            var value = new InternalObject { IntProperty = 10, StringProperty = "brown" };
            RealmValue rv = value;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Object));

            Assert.That((RealmObjectBase)rv, Is.EqualTo(value));
            Assert.That(rv.As<RealmObjectBase>(), Is.EqualTo(value));
            Assert.That(rv.AsRealmObject(), Is.EqualTo(value));
            Assert.That(rv != RealmValue.Null);
        }

        [Test]
        public void RealmValue_NullTests([Values] bool isManaged)
        {
            RealmValue rv = RealmValue.Null;

            if (isManaged)
            {
                var retrievedObject = PersistAndFind(rv);
                rv = retrievedObject.RealmValueProperty;
            }

            Assert.That(rv == RealmValue.Null);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Null));

            Assert.That(rv.AsNullableBool() == null);
            Assert.That(rv.AsNullableChar() == null);
            Assert.That(rv.AsNullableDate() == null);
            Assert.That(rv.AsNullableDecimal() == null);
            Assert.That(rv.AsNullableDecimal128() == null);
            Assert.That(rv.AsNullableDouble() == null);
            Assert.That(rv.AsNullableFloat() == null);
            Assert.That(rv.AsNullableGuid() == null);
            Assert.That(rv.AsNullableObjectId() == null);
            Assert.That(rv.AsNullableByte() == null);
            Assert.That(rv.AsNullableByteRealmInteger() == null);
            Assert.That(rv.AsNullableInt16() == null);
            Assert.That(rv.AsNullableInt16RealmInteger() == null);
            Assert.That(rv.AsNullableInt32() == null);
            Assert.That(rv.AsNullableInt32RealmInteger() == null);
            Assert.That(rv.AsNullableInt64() == null);
            Assert.That(rv.AsNullableInt64RealmInteger() == null);

            Assert.That((bool?)rv == null);
            Assert.That((DateTimeOffset?)rv == null);
            Assert.That((decimal?)rv == null);
            Assert.That((Decimal128?)rv == null);
            Assert.That((double?)rv == null);
            Assert.That((float?)rv == null);
            Assert.That((Guid?)rv == null);
            Assert.That((ObjectId?)rv == null);
            Assert.That((byte?)rv == null);
            Assert.That((RealmInteger<byte>?)rv == null);
            Assert.That((short?)rv == null);
            Assert.That((RealmInteger<short>?)rv == null);
            Assert.That((int?)rv == null);
            Assert.That((RealmInteger<int>?)rv == null);
            Assert.That((long?)rv == null);
            Assert.That((RealmInteger<long>?)rv == null);
        }

        [Test]
        public void RealmValue_WithRealmInteger_Increments()
        {
            // This fails because it's unsupported in Core, for now
            RealmValue rv = 10;
            var retrievedObject = PersistAndFind(rv);

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 10);

            _realm.Write(() =>
            {
                retrievedObject.RealmValueProperty.AsInt32RealmInteger().Increment();
            });

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 11);

            _realm.Write(() =>
            {
                retrievedObject.RealmValueProperty.AsInt32RealmInteger().Decrement();
            });

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 10);
        }

        [Test]
        public void RealmValue_WhenCastingIsWrong_ThrowsException()
        {
            RealmValue rv = 10;

            Assert.That(() => rv.AsString(), Throws.InvalidOperationException);
            Assert.That(() => rv.AsFloat(), Throws.InvalidOperationException);

            rv = Guid.NewGuid().ToString();

            Assert.That(() => rv.AsInt16(), Throws.InvalidOperationException);
            Assert.That(() => rv.AsGuid(), Throws.InvalidOperationException);

            rv = true;

            Assert.That(() => rv.AsInt16(), Throws.InvalidOperationException);
        }

        [Test]
        public void RealmValue_Reference_IsChangedCorrectly()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = 10;

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            var savedValue = rvo.RealmValueProperty;

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = "abc";
            });

            Assert.That(rvo.RealmValueProperty != savedValue);
            Assert.That(savedValue == 10);
        }

        [Test]
        public void RealmValue_WhenManaged_CanChangeType()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = 10;

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            Assert.That(rvo.RealmValueProperty == 10);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = "abc";
            });

            Assert.That(rvo.RealmValueProperty == "abc");

            var guidValue = Guid.NewGuid();

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = guidValue;
            });

            Assert.That(rvo.RealmValueProperty == guidValue);

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = RealmValue.Null;
            });

            Assert.That(rvo.RealmValueProperty == RealmValue.Null);
        }

        [Test]
        public void RealmValue_WhenManaged_NotificationTests()
        {
            var notifiedPropertyNames = new List<string>();

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            var rvo = new RealmValueObject();

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            rvo.PropertyChanged += handler;

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = "abc";
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(RealmValueObject.RealmValueProperty) }));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = 10;
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(RealmValueObject.RealmValueProperty),
                nameof(RealmValueObject.RealmValueProperty) }));
        }

        [TestCase(1, true)]
        [TestCase(1, false)]
        [TestCase(0, true)]
        [TestCase(0, false)]
        public void RealmValue_WhenManaged_BoolNotificationTests(int intValue, bool boolValue)
        {
            var notifiedPropertyNames = new List<string>();

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            var rvo = new RealmValueObject();

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            rvo.PropertyChanged += handler;

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = intValue;
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(RealmValueObject.RealmValueProperty) }));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = boolValue;
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(RealmValueObject.RealmValueProperty),
                nameof(RealmValueObject.RealmValueProperty) }));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = intValue;
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(RealmValueObject.RealmValueProperty),
                nameof(RealmValueObject.RealmValueProperty), 
                nameof(RealmValueObject.RealmValueProperty) }));
        }

        [Test]
        public void RealmValue_ListTests()
        {
            var rvo = new RealmValueObject();

            var intValue = 5;
            var stringValue = "abc";
            var guidValue = Guid.NewGuid();
            var nullValue = RealmValue.Null;
            var objectValue = new InternalObject { IntProperty = 10, StringProperty = "brown" };
            var notAddedValue = "notAdded";

            _realm.Write(() => _realm.Add(rvo));

            var changeSetList = new List<ChangeSet>();
            using var token = rvo.RealmValueList.SubscribeForNotifications((sender, changes, error) =>
            {
                if (changes != null)
                {
                    changeSetList.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueList.Add(intValue);
                rvo.RealmValueList.Add(stringValue);
                rvo.RealmValueList.Add(guidValue);
                rvo.RealmValueList.Add(nullValue);
                rvo.RealmValueList.Add(objectValue);
            });

            Assert.That(rvo.RealmValueList.Count, Is.EqualTo(5));
            Assert.That(rvo.RealmValueList[0] == intValue);
            Assert.That(rvo.RealmValueList[1] == stringValue);
            Assert.That(rvo.RealmValueList[2] == guidValue);
            Assert.That(rvo.RealmValueList[3] == RealmValue.Null);
            Assert.That(rvo.RealmValueList[4].As<RealmObjectBase>(), Is.EqualTo(objectValue));

            Assert.That(rvo.RealmValueList.Contains(intValue));
            Assert.That(rvo.RealmValueList.Contains(stringValue));
            Assert.That(rvo.RealmValueList.Contains(guidValue));
            Assert.That(rvo.RealmValueList.Contains(nullValue));
            Assert.That(rvo.RealmValueList.Contains(objectValue));
            Assert.That(!rvo.RealmValueList.Contains(notAddedValue));

            Assert.That(rvo.RealmValueList.IndexOf(intValue), Is.EqualTo(0));
            Assert.That(rvo.RealmValueList.IndexOf(stringValue), Is.EqualTo(1));
            Assert.That(rvo.RealmValueList.IndexOf(guidValue), Is.EqualTo(2));
            Assert.That(rvo.RealmValueList.IndexOf(nullValue), Is.EqualTo(3));
            Assert.That(rvo.RealmValueList.IndexOf(objectValue), Is.EqualTo(4));
            Assert.That(rvo.RealmValueList.IndexOf(notAddedValue), Is.EqualTo(-1));

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].InsertedIndices, Is.EquivalentTo(Enumerable.Range(0, rvo.RealmValueList.Count)));
            });

            _realm.Write(() =>
            {
                rvo.RealmValueList.Remove(stringValue);
            });

            Assert.That(rvo.RealmValueList.Count, Is.EqualTo(4));
            Assert.That(rvo.RealmValueList[0] == intValue);
            Assert.That(rvo.RealmValueList[1] == guidValue);
            Assert.That(rvo.RealmValueList[2] == RealmValue.Null);
            Assert.That(rvo.RealmValueList[3].As<RealmObjectBase>(), Is.EqualTo(objectValue));

            Assert.That(rvo.RealmValueList.IndexOf(intValue), Is.EqualTo(0));
            Assert.That(rvo.RealmValueList.IndexOf(guidValue), Is.EqualTo(1));
            Assert.That(rvo.RealmValueList.IndexOf(nullValue), Is.EqualTo(2));
            Assert.That(rvo.RealmValueList.IndexOf(objectValue), Is.EqualTo(3));
            Assert.That(rvo.RealmValueList.IndexOf(stringValue), Is.EqualTo(-1));
            Assert.That(rvo.RealmValueList.IndexOf(notAddedValue), Is.EqualTo(-1));

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].DeletedIndices, Is.EquivalentTo(new[] { 1 }));
            });

            _realm.Write(() =>
            {
                rvo.RealmValueList.Clear();
            });

            Assert.That(rvo.RealmValueList.Count, Is.EqualTo(0));

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].DeletedIndices, Is.EquivalentTo(Enumerable.Range(0, 4)));
            });
        }

        [Test]
        public void RealmValue_DictionaryTests()
        {
            var rvo = new RealmValueObject();

            var intKey = "intKey";
            var stringKey = "stringKey";
            var guidKey = "guidKey";
            var nullKey = "nullKey";
            var objectKey = "objectKey";

            var intValue = 5;
            var stringValue = "abc";
            var guidValue = Guid.NewGuid();
            var nullValue = RealmValue.Null;
            var objectValue = new InternalObject { IntProperty = 10, StringProperty = "brown" };

            _realm.Write(() => _realm.Add(rvo));

            var changeSetList = new List<ChangeSet>();
            using var token = rvo.RealmValueDictionary.SubscribeForNotifications((sender, changes, error) =>
            {
                if (changes != null)
                {
                    changeSetList.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueDictionary[intKey] = intValue;
                rvo.RealmValueDictionary[stringKey] = stringValue;
                rvo.RealmValueDictionary[guidKey] = guidValue;
                rvo.RealmValueDictionary[nullKey] = nullValue;
                rvo.RealmValueDictionary[objectKey] = objectValue;
            });

            Assert.That(rvo.RealmValueDictionary.Count, Is.EqualTo(5));
            Assert.That(rvo.RealmValueDictionary[intKey] == intValue);
            Assert.That(rvo.RealmValueDictionary[stringKey] == stringValue);
            Assert.That(rvo.RealmValueDictionary[guidKey] == guidValue);
            Assert.That(rvo.RealmValueDictionary[nullKey] == nullValue);
            Assert.That(rvo.RealmValueDictionary[objectKey].As<RealmObjectBase>(), Is.EqualTo(objectValue));

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].InsertedIndices.Count, Is.EqualTo(rvo.RealmValueDictionary.Count));
            });

            _realm.Write(() =>
            {
                rvo.RealmValueDictionary.Remove(stringKey);
            });

            Assert.That(rvo.RealmValueDictionary.Count, Is.EqualTo(4));
            Assert.That(rvo.RealmValueDictionary[intKey] == intValue);
            Assert.That(rvo.RealmValueDictionary[guidKey] == guidValue);
            Assert.That(rvo.RealmValueDictionary[nullKey] == nullValue);
            Assert.That(rvo.RealmValueDictionary[objectKey].As<RealmObjectBase>(), Is.EqualTo(objectValue));
            Assert.That(rvo.RealmValueDictionary.ContainsKey(stringKey), Is.False);

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].DeletedIndices.Count, Is.EqualTo(1));
            });

            _realm.Write(() =>
            {
                rvo.RealmValueDictionary.Clear();
            });

            Assert.That(rvo.RealmValueDictionary.Count, Is.EqualTo(0));

            VerifyNotifications(_realm, changeSetList, () =>
            {
                Assert.That(changeSetList[0].DeletedIndices.Count, Is.EqualTo(4));
            });
        }

        private static void VerifyNotifications(Realm realm, List<ChangeSet> notifications, Action verifier)
        {
            realm.Refresh();
            Assert.That(notifications.Count, Is.EqualTo(1));
            verifier();
            notifications.Clear();
        }

        private RealmValueObject PersistAndFind(RealmValue rv)
        {
            _realm.Write(() =>
            {
                _realm.Add(new RealmValueObject { RealmValueProperty = rv });
            });

            return _realm.All<RealmValueObject>().First();
        }

        private class RealmValueObject : RealmObject
        {
            public RealmValue RealmValueProperty { get; set; }

            public IList<RealmValue> RealmValueList { get; }

            public ISet<RealmValue> RealmValueSet { get; } //TODO Need to add test for those when Set is ready

            public IDictionary<string, RealmValue> RealmValueDictionary { get; }
        }

        private class InternalObject : RealmObject, IEquatable<InternalObject>
        {
            public int IntProperty { get; set; }

            public string StringProperty { get; set; }

            public override bool Equals(object obj) => Equals(obj as InternalObject);

            public bool Equals(InternalObject other) => other != null &&
                       IntProperty == other.IntProperty &&
                       StringProperty == other.StringProperty;
        }
    }
}
