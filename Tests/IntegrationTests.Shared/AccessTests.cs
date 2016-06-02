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

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class AccessTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [TestCaseSource(nameof(SetAndGetValueCases))]
        public void SetAndGetValue(string propertyName, object propertyValue)
        {
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));
        }

        public IEnumerable<object[]> SetAndGetValueCases()
        {
            yield return new object[] { "CharProperty", '0' };
            yield return new object[] { "ByteProperty", (byte)100 };
            yield return new object[] { "Int16Property", (short)100 };
            yield return new object[] { "Int32Property", 100 };
            yield return new object[] { "Int64Property", 100L };
            yield return new object[] { "SingleProperty", 123.123f };
            yield return new object[] { "DoubleProperty", 123.123 };
            yield return new object[] { "BooleanProperty", true };
            yield return new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } };
            yield return new object[] { "ByteArrayProperty", new byte[0] };
            yield return new object[] { "StringProperty", "hello" };
            yield return new object[] { "DateTimeOffsetProperty", new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero) };
        }

        [TestCaseSource(nameof(SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull(string propertyName, object propertyValue)
        {
            AllTypesObject ato;
            using (var transaction = _realm.BeginWrite())
            {
                ato = _realm.CreateObject<AllTypesObject>();

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));

            using (var transaction = _realm.BeginWrite())
            {
                TestHelpers.SetPropertyValue(ato, propertyName, null);
                transaction.Commit();
            }

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(null));
        }

        public IEnumerable<object[]> SetAndReplaceWithNullCases()
        {
            yield return new object[] { "NullableCharProperty", '0' };
            yield return new object[] { "NullableByteProperty", (byte)100 };
            yield return new object[] { "NullableInt16Property", (short)100 };
            yield return new object[] { "NullableInt32Property", 100 };
            yield return new object[] { "NullableInt64Property", 100L };
            yield return new object[] { "NullableSingleProperty", 123.123f };
            yield return new object[] { "NullableDoubleProperty", 123.123 };
            yield return new object[] { "NullableBooleanProperty", true };
            yield return new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } };
            yield return new object[] { "ByteArrayProperty", new byte[0] };
            yield return new object[] { "StringProperty", "hello" };
            yield return new object[] { "NullableDateTimeOffsetProperty", new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero) };
        }
    }
}
