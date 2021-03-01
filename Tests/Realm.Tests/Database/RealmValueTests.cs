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
        #region TestCaseSources

        private static readonly char[] _charValues = new char[] { (char)0, 'a', char.MaxValue, char.MinValue };
        private static readonly byte[] _byteValues = new byte[] { 0, 1, byte.MaxValue, byte.MinValue };
        private static readonly int[] _intValues = new int[] { 0, 1, -1, int.MaxValue, int.MinValue };
        private static readonly short[] _shortValues = new short[] { 0, 1, -1, short.MaxValue, short.MinValue };
        private static readonly long[] _longValues = new long[] { 0, 1, -1, long.MaxValue, long.MinValue };
        private static readonly float[] _floatValues = new float[] { 0, 1, -1, float.MaxValue, float.MinValue };
        private static readonly double[] _doubleValues = new double[] { 0, 1, -1, float.MaxValue, float.MinValue };
        private static readonly Decimal128[] _decimal128Values = new Decimal128[] { 0, 1, -1, Decimal128.MaxValue, Decimal128.MinValue };
        private static readonly decimal[] _decimalValues = new decimal[] { 0, 1, -1, decimal.MaxValue, decimal.MinValue };
        private static readonly bool[] _boolValues = new bool[] { false, true };
        private static readonly DateTimeOffset[] _dateValues = new DateTimeOffset[] { DateTimeOffset.Now, DateTimeOffset.MaxValue, DateTimeOffset.MinValue };
        private static readonly Guid[] _guidValues = new Guid[] { Guid.NewGuid(), Guid.Empty };
        private static readonly ObjectId[] _objectIdValues = new ObjectId[] { ObjectId.GenerateNewId(), ObjectId.Empty };
        private static readonly string[] _stringValues = new string[] { "abc", string.Empty};
        private static readonly byte[][] _dataValues = new byte[][] { new byte[] { 0, 1, 2 }, new byte[] { } };
        private static readonly RealmObject[] _objectValues = new RealmObject[] { new InternalObject { IntProperty = 10, StringProperty = "brown" } }; //TODO add new test cases

        public static IEnumerable<object> CharTestCases() => GenerateTestCases(_charValues);

        public static IEnumerable<object> ByteTestCases() => GenerateTestCases(_byteValues);

        public static IEnumerable<object> IntTestCases() => GenerateTestCases(_intValues);

        public static IEnumerable<object> ShortTestCases() => GenerateTestCases(_shortValues);

        public static IEnumerable<object> LongTestCases() => GenerateTestCases(_longValues);

        public static IEnumerable<object> FloatTestCases() => GenerateTestCases(_floatValues);

        public static IEnumerable<object> DoubleTestCases() => GenerateTestCases(_doubleValues);

        public static IEnumerable<object> Decimal128TestCases() => GenerateTestCases(_decimal128Values);

        public static IEnumerable<object> DecimalTestCases() => GenerateTestCases(_decimalValues);

        public static IEnumerable<object> BoolTestCases() => GenerateTestCases(_boolValues);

        public static IEnumerable<object> DateTestCases() => GenerateTestCases(_dateValues);

        public static IEnumerable<object> GuidTestCases() => GenerateTestCases(_guidValues);

        public static IEnumerable<object> ObjectIdTestCases() => GenerateTestCases(_objectIdValues);

        public static IEnumerable<object> StringTestCases() => GenerateTestCases(_stringValues);

        public static IEnumerable<object> DataTestCases() => GenerateTestCases(_dataValues);

        public static IEnumerable<object> ObjectTestCases() => GenerateTestCases(_objectValues);

        private static IEnumerable<object> GenerateTestCases<T>(IEnumerable<T> values)
        {
            foreach (var val in values)
            {
                yield return new object[] { val, false };
                yield return new object[] { val, true };
            }
        }

        #endregion

        [TestCaseSource(nameof(CharTestCases))]
        public void RealmValue_CharTests(char value, bool isManaged)
        {
            RunNumericTests(value, value, isManaged);
        }

        [TestCaseSource(nameof(ByteTestCases))]
        public void RealmValue_ByteTests(byte value, bool isManaged)
        {
            RunNumericTests(value, value, isManaged);
        }

        [TestCaseSource(nameof(IntTestCases))]
        public void RealmValue_IntTests(int value, bool isManaged)
        {
            RunNumericTests(value, value, isManaged);
        }

        [TestCaseSource(nameof(ShortTestCases))]
        public void RealmValue_ShortTests(short value, bool isManaged)
        {
            RunNumericTests(value, value, isManaged);
        }

        [TestCaseSource(nameof(LongTestCases))]
        public void RealmValue_LongTests(long value, bool isManaged)
        {
            RunNumericTests(value, value, isManaged);
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
            var byteValue = (byte)value;
            Assert.That((byte)rv == byteValue);
            Assert.That(rv.As<byte>() == byteValue);
            Assert.That((byte?)rv == byteValue);
            Assert.That(rv.As<byte?>() == byteValue);
            Assert.That(rv.AsByte() == byteValue);
            Assert.That(rv.AsNullableByte() == byteValue);
            Assert.That(rv.AsByteRealmInteger() == byteValue);
            Assert.That(rv.AsNullableByteRealmInteger() == byteValue);

            // 16 - short
            var shortValue = (short)value;
            Assert.That((short)rv == shortValue);
            Assert.That(rv.As<short>() == shortValue);
            Assert.That((short?)rv == shortValue);
            Assert.That(rv.As<short?>() == shortValue);
            Assert.That(rv.AsInt16() == shortValue);
            Assert.That(rv.AsNullableInt16() == shortValue);
            Assert.That(rv.AsInt16RealmInteger() == shortValue);
            Assert.That(rv.AsNullableInt16RealmInteger() == shortValue);

            // 32 - int
            var intValue = (int)value;
            Assert.That((int)rv == intValue);
            Assert.That(rv.As<int>() == intValue);
            Assert.That((int?)rv == intValue);
            Assert.That(rv.As<int?>() == intValue);
            Assert.That(rv.AsInt32() == intValue);
            Assert.That(rv.AsNullableInt32() == intValue);
            Assert.That(rv.AsInt32RealmInteger() == intValue);
            Assert.That(rv.AsNullableInt32RealmInteger() == intValue);

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

        [TestCaseSource(nameof(FloatTestCases))]
        public void RealmValue_FloatTests(float value, bool isManaged)
        {
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

        [TestCaseSource(nameof(DoubleTestCases))]
        public void RealmValue_DoubleTests(double value, bool isManaged)
        {
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

        [TestCaseSource(nameof(Decimal128TestCases))]
        public void RealmValue_Decimal128Tests(Decimal128 value, bool isManaged)
        {
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

        [TestCaseSource(nameof(DecimalTestCases))]
        public void RealmValue_DecimalTests(decimal value, bool isManaged)
        {
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

        [TestCaseSource(nameof(BoolTestCases))]
        public void RealmValue_BoolTests(bool value, bool isManaged)
        {
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

        [TestCaseSource(nameof(DateTestCases))]
        public void RealmValue_DateTests(DateTimeOffset value, bool isManaged)
        {
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

        [TestCaseSource(nameof(ObjectIdTestCases))]
        public void RealmValue_ObjectIdTests(ObjectId value, bool isManaged)
        {
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

        [TestCaseSource(nameof(GuidTestCases))]
        public void RealmValue_GuidTests(Guid value, bool isManaged)
        {
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

        [TestCaseSource(nameof(StringTestCases))]
        public void RealmValue_StringTests(string value, bool isManaged)
        {
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

        [TestCaseSource(nameof(DataTestCases))]
        public void RealmValue_DataTests(byte[] value, bool isManaged)
        {
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

        [TestCaseSource(nameof(ObjectTestCases))]
        public void RealmValue_ObjectTests(RealmObjectBase value, bool isManaged)
        {
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
        public void RealmValue_WhenManaged_ObjectGetsPersisted()
        {
            var value = new InternalObject { IntProperty = 10, StringProperty = "brown" };
            RealmValue rv = value;

            _realm.Write(() =>
            {
                _realm.Add(new RealmValueObject { RealmValueProperty = rv });
            });

            var objs = _realm.All<InternalObject>().ToList();

            Assert.That(objs.Count, Is.EqualTo(1));
            Assert.That(objs[0], Is.EqualTo(value));
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
            //Once #4459 is merged, need to add some tests for index_of (probably in a different method though...)
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

        [Test]
        public void AAARealmValue_QueryTests()
        {
            // TODO Can we put this in another method...?
            
            /**What to test
             * 
             *  - Equivalence between numerical values
             *  - Equivalence between Where and Filter values
             *  - Filter on RealmValueType (equal/not-equal)
             * 
             * 
             * 
             */

            var rvo1 = new RealmValueObject { Id = 1, RealmValueProperty = 1 };
            var rvo2 = new RealmValueObject { Id = 2, RealmValueProperty = 1.0 };
            var rvo3 = new RealmValueObject { Id = 3, RealmValueProperty = true };
            var rvo4 = new RealmValueObject { Id = 4, RealmValueProperty = "1" };
            var rvo5 = new RealmValueObject { Id = 5, RealmValueProperty = "abc" };
            var rvo6 = new RealmValueObject { Id = 6, RealmValueProperty = new InternalObject { IntProperty = 10, StringProperty = "brown" } };

            _realm.Write(() =>
            {
                _realm.Add(new[] { rvo1, rvo2, rvo3, rvo4, rvo5, rvo6 });
            });

            //var f1 = _realm.All<RealmValueObject>().Filter("RealmValueProperty.@type == 'int'").ToList();

            //Assert.That(f1, Is.EquivalentTo(new List<RealmValueObject> { rvo1 }));

            var t1 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty.Type != RealmValueType.String).ToList();

            Assert.That(t1, Is.EquivalentTo(new List<RealmValueObject> { rvo4, rvo5 }));

            // Numeric values are converted when possible
            var n1 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == 1).OrderBy(r => r.Id).ToList();
            var n2 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == 1.0).OrderBy(r => r.Id).ToList();
            var n3 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == true).OrderBy(r => r.Id).ToList();
            var n4 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == 1.1).OrderBy(r => r.Id).ToList();

            Assert.That(n1, Is.EquivalentTo(n2));
            Assert.That(n1, Is.EquivalentTo(n3));
            Assert.That(n1, Is.EquivalentTo(new List<RealmValueObject> { rvo1, rvo2, rvo3 }));
            Assert.That(n4.Count, Is.EqualTo(0));

            var s1 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == "1").OrderBy(r => r.Id).ToList();
            var s2 = _realm.All<RealmValueObject>().Where(r => r.RealmValueProperty == "abc").OrderBy(r => r.Id).ToList();

            Assert.That(s1, Is.EquivalentTo(new List<RealmValueObject> { rvo4 }));
            Assert.That(s2, Is.EquivalentTo(new List<RealmValueObject> { rvo5 }));
        }

        [Test]
        public void AAARealmValue_DynamicTests()
        {

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
            public int Id { get; set; }

            public RealmValue RealmValueProperty { get; set; }

            public IList<RealmValue> RealmValueList { get; }

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
