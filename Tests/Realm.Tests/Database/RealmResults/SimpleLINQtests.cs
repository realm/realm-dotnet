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

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Realms.Exceptions;
using TestExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    internal class SimpleLINQtests : PeopleTestsBase
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            MakeThreePeople();
        }

        [Test]
        public void CreateList()
        {
            var s0 = _realm.All<Person>().Where(p => p.Score == 42.42f).ToList();
            Assert.That(s0.Count, Is.EqualTo(1));
            Assert.That(s0[0].Score, Is.EqualTo(42.42f));

            var s1 = _realm.All<Person>().Where(p => p.Longitude < -70.0 && p.Longitude > -90.0).ToList();
            Assert.That(s1[0].Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Where(p => p.Longitude < 0).ToList();
            Assert.That(s2.Count, Is.EqualTo(2));
            Assert.That(s2[0].Email, Is.EqualTo("john@doe.com"));
            Assert.That(s2[1].Email, Is.EqualTo("peter@jameson.net"));

            var s3 = _realm.All<Person>().Where(p => p.Email != string.Empty);
            Assert.That(s3.Count(), Is.EqualTo(3));
        }

        [Test]
        public void CountWithNot()
        {
            var countSimpleNot = _realm.All<Person>().Where(p => !p.IsInteresting).Count();
            Assert.That(countSimpleNot, Is.EqualTo(1));

            var countSimpleNot2 = _realm.All<Person>().Count(p => !p.IsInteresting);
            Assert.That(countSimpleNot2, Is.EqualTo(1));

            var countNotEqual = _realm.All<Person>().Where(p => !(p.Score == 42.42f)).Count();
            Assert.That(countNotEqual, Is.EqualTo(2));

            var countNotComplex = _realm.All<Person>().Where(p => !(p.Longitude < -70.0 && p.Longitude > -90.0)).Count();
            Assert.That(countNotComplex, Is.EqualTo(2));
        }

        [Test]
        public void CountFoundItems()
        {
            var r0 = _realm.All<Person>().Where(p => p.Score == 42.42f);
            var c0 = r0.Count();  // defer so can check in debugger if RealmResults.Count() evaluated correctly
            Assert.That(c0, Is.EqualTo(1));

            var c1 = _realm.All<Person>().Where(p => p.Latitude <= 50).Count();
            Assert.That(c1, Is.EqualTo(2));

            var c2 = _realm.All<Person>().Where(p => p.IsInteresting).Count();
            Assert.That(c2, Is.EqualTo(2));

            var c3 = _realm.All<Person>().Where(p => p.FirstName == "John").Count();
            Assert.That(c3, Is.EqualTo(2));

            var c4 = _realm.All<Person>().Count(p => p.FirstName == "John");
            Assert.That(c4, Is.EqualTo(2));
        }

        [Test]
        public void CountFails()
        {
            var c0 = _realm.All<Person>().Where(p => p.Score == 3.14159f).Count();
            Assert.That(c0, Is.EqualTo(0));

            var c1 = _realm.All<Person>().Where(p => p.Latitude > 88).Count();
            Assert.That(c1, Is.EqualTo(0));

            var c3 = _realm.All<Person>().Where(p => p.FirstName == "Samantha").Count();
            Assert.That(c3, Is.EqualTo(0));
        }

        // Extension method rather than SQL-style LINQ
        // Also tests the Count on results, ElementOf, First and Single methods
        [Test]
        public void SearchComparingFloat()
        {
            var s0 = _realm.All<Person>().Where(p => p.Score == 42.42f);
            var s0l = s0.ToList();
            Assert.That(s0.Count(), Is.EqualTo(1));
            Assert.That(s0l[0].Score, Is.EqualTo(42.42f));

            var s1 = _realm.All<Person>().Where(p => p.Score != 100.0f).ToList();
            Assert.That(s1.Count, Is.EqualTo(2));
            Assert.That(s1[0].Score, Is.EqualTo(-0.9907f));
            Assert.That(s1[1].Score, Is.EqualTo(42.42f));

            var s2 = _realm.All<Person>().Where(p => p.Score < 0).ToList();
            Assert.That(s2.Count, Is.EqualTo(1));
            Assert.That(s2[0].Score, Is.EqualTo(-0.9907f));

            var s3 = _realm.All<Person>().Where(p => p.Score <= 42.42f).ToList();
            Assert.That(s3.Count, Is.EqualTo(2));
            Assert.That(s3[0].Score, Is.EqualTo(-0.9907f));
            Assert.That(s3[1].Score, Is.EqualTo(42.42f));

            var s4 = _realm.All<Person>().Where(p => p.Score > 99.0f).ToList();
            Assert.That(s4.Count, Is.EqualTo(1));
            Assert.That(s4[0].Score, Is.EqualTo(100.0f));

            var s5 = _realm.All<Person>().Where(p => p.Score >= 100).ToList();
            Assert.That(s5.Count, Is.EqualTo(1));
            Assert.That(s5[0].Score, Is.EqualTo(100.0f));
        }

        [Test]
        public void SearchComparingDouble()
        {
            var s0 = _realm.All<Person>().Where(p => p.Latitude == 40.7637286);
            Assert.That(s0.Count, Is.EqualTo(1));
            Assert.That(s0.ToList()[0].Latitude, Is.EqualTo(40.7637286));

            var s1 = _realm.All<Person>().Where(p => p.Latitude != 40.7637286).ToList();
            Assert.That(s1.Count, Is.EqualTo(2));
            Assert.That(s1[0].Latitude, Is.EqualTo(51.508530));
            Assert.That(s1[1].Latitude, Is.EqualTo(37.7798657));

            var s2 = _realm.All<Person>().Where(p => p.Latitude < 40).ToList();
            Assert.That(s2.Count, Is.EqualTo(1));
            Assert.That(s2[0].Latitude, Is.EqualTo(37.7798657));

            var s3 = _realm.All<Person>().Where(p => p.Latitude <= 40.7637286).ToList();
            Assert.That(s3.Count, Is.EqualTo(2));
            Assert.That(s3[0].Latitude, Is.EqualTo(40.7637286));
            Assert.That(s3[1].Latitude, Is.EqualTo(37.7798657));

            var s4 = _realm.All<Person>().Where(p => p.Latitude > 50).ToList();
            Assert.That(s4.Count, Is.EqualTo(1));
            Assert.That(s4[0].Latitude, Is.EqualTo(51.508530));

            var s5 = _realm.All<Person>().Where(p => p.Latitude >= 51.508530).ToList();
            Assert.That(s5.Count, Is.EqualTo(1));
            Assert.That(s5[0].Latitude, Is.EqualTo(51.508530));
        }

        [Test]
        public void SearchComparingLong()
        {
            var equality = _realm.All<Person>().Where(p => p.Salary == 60000).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));

            var lessThan = _realm.All<Person>().Where(p => p.Salary < 50000).ToArray();
            Assert.That(lessThan.Length, Is.EqualTo(1));
            Assert.That(lessThan[0].FullName, Is.EqualTo("John Smith"));

            var lessOrEqualThan = _realm.All<Person>().Where(p => p.Salary <= 60000).ToArray();
            Assert.That(lessOrEqualThan.Length, Is.EqualTo(2));
            Assert.That(lessOrEqualThan.All(p => p.FirstName == "John"), Is.True);

            var greaterThan = _realm.All<Person>().Where(p => p.Salary > 80000).ToArray();
            Assert.That(greaterThan.Length, Is.EqualTo(1));
            Assert.That(greaterThan[0].FullName, Is.EqualTo("Peter Jameson"));

            var greaterOrEqualThan = _realm.All<Person>().Where(p => p.Salary >= 60000).ToArray();
            Assert.That(greaterOrEqualThan.Length, Is.EqualTo(2));
            Assert.That(greaterOrEqualThan.Any(p => p.FullName == "John Doe") && greaterOrEqualThan.Any(p => p.FullName == "Peter Jameson"), Is.True);

            var between = _realm.All<Person>().Where(p => p.Salary > 30000 && p.Salary < 87000).ToArray();
            Assert.That(between.Length, Is.EqualTo(1));
            Assert.That(between[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingString()
        {
            var equality = _realm.All<Person>().Where(p => p.LastName == "Smith").ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Smith"));

            var contains = _realm.All<Person>().Where(p => p.FirstName.Contains("et")).ToArray();
            Assert.That(contains.Length, Is.EqualTo(1));
            Assert.That(contains[0].FullName, Is.EqualTo("Peter Jameson"));

            var startsWith = _realm.All<Person>().Where(p => p.Email.StartsWith("john@")).ToArray();
            Assert.That(startsWith.Length, Is.EqualTo(2));
            Assert.That(startsWith.All(p => p.FirstName == "John"), Is.True);

            var endsWith = _realm.All<Person>().Where(p => p.Email.EndsWith(".net")).ToArray();
            Assert.That(endsWith.Length, Is.EqualTo(1));
            Assert.That(endsWith[0].FullName, Is.EqualTo("Peter Jameson"));

            var @null = _realm.All<Person>().Where(p => p.OptionalAddress == null).ToArray();
            Assert.That(@null[0].FullName, Is.EqualTo("Peter Jameson"));

            var empty = _realm.All<Person>().Where(p => p.OptionalAddress == string.Empty).ToArray();
            Assert.That(empty[0].FullName, Is.EqualTo("John Doe"));

            var null_or_empty = _realm.All<Person>().Where(p => string.IsNullOrEmpty(p.OptionalAddress));
            Assert.That(null_or_empty.Count(), Is.EqualTo(2));
        }

        [Test]
        public void SearchComparingDateTimeOffset()
        {
            var d1960 = new DateTimeOffset(1960, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var d1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var bdayJohnDoe = new DateTimeOffset(1963, 4, 14, 0, 0, 0, TimeSpan.Zero);
            var bdayPeterJameson = new DateTimeOffset(1989, 2, 25, 0, 0, 0, TimeSpan.Zero);

            var equality = _realm.All<Person>().Where(p => p.Birthday == bdayPeterJameson).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("Peter Jameson"));

            var lessThan = _realm.All<Person>().Where(p => p.Birthday < d1960).ToArray();
            Assert.That(lessThan.Length, Is.EqualTo(1));
            Assert.That(lessThan[0].FullName, Is.EqualTo("John Smith"));

            var lessOrEqualThan = _realm.All<Person>().Where(p => p.Birthday <= bdayJohnDoe).ToArray();
            Assert.That(lessOrEqualThan.Length, Is.EqualTo(2));
            Assert.That(lessOrEqualThan.All(p => p.FirstName == "John"), Is.True);

            var greaterThan = _realm.All<Person>().Where(p => p.Birthday > d1970).ToArray();
            Assert.That(greaterThan.Length, Is.EqualTo(1));
            Assert.That(greaterThan[0].FullName, Is.EqualTo("Peter Jameson"));

            var greaterOrEqualThan = _realm.All<Person>().Where(p => p.Birthday >= bdayJohnDoe).ToArray();
            Assert.That(greaterOrEqualThan.Length, Is.EqualTo(2));
            Assert.That(greaterOrEqualThan.Any(p => p.FullName == "John Doe") && greaterOrEqualThan.Any(p => p.FullName == "Peter Jameson"), Is.True);

            var between = _realm.All<Person>().Where(p => p.Birthday > d1960 && p.Birthday < d1970).ToArray();
            Assert.That(between.Length, Is.EqualTo(1));
            Assert.That(between[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingNullable()
        {
            var @null = _realm.All<Person>().Where(p => p.IsAmbivalent == null);
            Assert.That(@null.Single().FullName, Is.EqualTo("Peter Jameson"));

            var not_null = _realm.All<Person>().Where(p => p.IsAmbivalent != null);
            Assert.That(not_null.Count(), Is.EqualTo(2));

            var @true = _realm.All<Person>().Where(p => p.IsAmbivalent == true);
            Assert.That(@true.Single().FullName, Is.EqualTo("John Smith"));

            var @false = _realm.All<Person>().Where(p => p.IsAmbivalent == false);
            Assert.That(@false.Single().FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingByteArrays()
        {
            var deadbeef = new byte[] { 0xde, 0xad, 0xbe, 0xef };
            var cafebabe = new byte[] { 0xca, 0xfe, 0xba, 0xbe };
            var empty = Array.Empty<byte>();

            var equality = _realm.All<Person>().Where(p => p.PublicCertificateBytes == cafebabe);
            Assert.That(equality.Single().PublicCertificateBytes, Is.EqualTo(cafebabe));

            var unequality = _realm.All<Person>().Where(p => p.PublicCertificateBytes != deadbeef);
            Assert.That(unequality.Count(), Is.EqualTo(2));

            var emptyness = _realm.All<Person>().Where(p => p.PublicCertificateBytes == empty);
            Assert.That(emptyness, Is.Empty);

            // we should support this as well - see #570
            // var @null = _realm.All<Person>().Where(p => p.PublicCertificateBytes == null);
            // Assert.That(@null.Count(), Is.EqualTo(1));
        }

        [Test]
        public void SearchComparingChar()
        {
            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyCharObject { CharProperty = 'A' });
                _realm.Add(new PrimaryKeyCharObject { CharProperty = 'B' });
                _realm.Add(new PrimaryKeyCharObject { CharProperty = 'c' });
                _realm.Add(new PrimaryKeyCharObject { CharProperty = 'a' });
            });

            var A = 'A';
            var B = 'B';
            var c = 'c';
            var a = 'a';
            var X = 'X';

            var equality = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty == 'A').ToArray();
            var varEquality = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty == A).ToArray();

            // Assert.That(equality.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A' }));
            Assert.That(varEquality.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A' }));

            var inequality = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty != 'c').ToArray();
            var varInequality = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty != c).ToArray();
            Assert.That(inequality.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a' }));
            Assert.That(varInequality.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a' }));

            var lessThan = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty < 'c').ToArray();
            var varLessThan = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty < c).ToArray();
            Assert.That(lessThan.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a' }));
            Assert.That(varLessThan.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a' }));

            var lessThanOrEqual = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty <= 'c').ToArray();
            var varLessThanOrEqual = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty <= c).ToArray();
            Assert.That(lessThanOrEqual.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a', 'c' }));
            Assert.That(varLessThanOrEqual.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'A', 'B', 'a', 'c' }));

            var greaterThan = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty > 'a').ToArray();
            var varGreaterThan = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty > a).ToArray();
            Assert.That(greaterThan.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'c' }));
            Assert.That(varGreaterThan.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'c' }));

            var greaterThanOrEqual = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty >= 'B').ToArray();
            var varGreaterThanOrEqual = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty >= B).ToArray();
            Assert.That(greaterThanOrEqual.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'B', 'a', 'c' }));
            Assert.That(varGreaterThanOrEqual.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'B', 'a', 'c' }));

            var between = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty > 'A' && p.CharProperty < 'a').ToArray();
            var varBetween = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty > A && p.CharProperty < a).ToArray();
            Assert.That(between.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'B' }));
            Assert.That(varBetween.Select(p => p.CharProperty), Is.EquivalentTo(new[] { 'B' }));

            var missing = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty == 'X').ToArray();
            var varMissing = _realm.All<PrimaryKeyCharObject>().Where(p => p.CharProperty == X).ToArray();
            Assert.That(missing.Length, Is.EqualTo(0));
            Assert.That(varMissing.Length, Is.EqualTo(0));
        }

        [Test]
        public void SearchComparingShort()
        {
            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyInt16Object { Int16Property = 0 });
                _realm.Add(new PrimaryKeyInt16Object { Int16Property = 1 });
                _realm.Add(new PrimaryKeyInt16Object { Int16Property = 2 });
            });

            short zero = 0;
            short one = 1;
            short two = 2;

            var equality = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property == 0).ToArray().Select(o => o.Int16Property);
            var varEquality = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property == zero).ToArray().Select(o => o.Int16Property);
            Assert.That(equality, Is.EquivalentTo(new short[] { 0 }));
            Assert.That(varEquality, Is.EquivalentTo(new short[] { 0 }));

            var inequality = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property != 0).ToArray().Select(o => o.Int16Property);
            var varInequality = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property != zero).ToArray().Select(o => o.Int16Property);
            Assert.That(inequality, Is.EquivalentTo(new short[] { 1, 2 }));
            Assert.That(varInequality, Is.EquivalentTo(new short[] { 1, 2 }));

            var lessThan = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property < 1).ToArray().Select(o => o.Int16Property);
            var varLessThan = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property < one).ToArray().Select(o => o.Int16Property);
            Assert.That(lessThan, Is.EquivalentTo(new short[] { 0 }));
            Assert.That(varLessThan, Is.EquivalentTo(new short[] { 0 }));

            var lessThanOrEqual = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property <= 1).ToArray().Select(o => o.Int16Property);
            var varLessThanOrEqual = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property <= one).ToArray().Select(o => o.Int16Property);
            Assert.That(lessThanOrEqual, Is.EquivalentTo(new short[] { 0, 1 }));
            Assert.That(varLessThanOrEqual, Is.EquivalentTo(new short[] { 0, 1 }));

            var greaterThan = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property > 1).ToArray().Select(o => o.Int16Property);
            var varGreaterThan = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property > one).ToArray().Select(o => o.Int16Property);
            Assert.That(greaterThan, Is.EquivalentTo(new short[] { 2 }));
            Assert.That(varGreaterThan, Is.EquivalentTo(new short[] { 2 }));

            var greaterThanOrEqual = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property >= 1).ToArray().Select(o => o.Int16Property);
            var varGreaterThanOrEqual = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property >= one).ToArray().Select(o => o.Int16Property);
            Assert.That(greaterThanOrEqual, Is.EquivalentTo(new short[] { 1, 2 }));
            Assert.That(varGreaterThanOrEqual, Is.EquivalentTo(new short[] { 1, 2 }));

            var between = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property > 0 && o.Int16Property < 2).ToArray().Select(o => o.Int16Property);
            var varBetween = _realm.All<PrimaryKeyInt16Object>().Where(o => o.Int16Property > zero && o.Int16Property < two).ToArray().Select(o => o.Int16Property);
            Assert.That(between, Is.EquivalentTo(new short[] { 1 }));
            Assert.That(varBetween, Is.EquivalentTo(new short[] { 1 }));
        }

        [Test]
        public void SearchComparingByte()
        {
            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyByteObject { ByteProperty = 0 });
                _realm.Add(new PrimaryKeyByteObject { ByteProperty = 1 });
                _realm.Add(new PrimaryKeyByteObject { ByteProperty = 2 });
            });

            byte zero = 0;
            byte one = 1;
            byte two = 2;

            var equality = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty == 0).ToArray().Select(o => o.ByteProperty);
            var varEquality = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty == zero).ToArray().Select(o => o.ByteProperty);
            Assert.That(equality, Is.EquivalentTo(new byte[] { 0 }));
            Assert.That(varEquality, Is.EquivalentTo(new byte[] { 0 }));

            var inequality = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty != 0).ToArray().Select(o => o.ByteProperty);
            var varInequality = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty != zero).ToArray().Select(o => o.ByteProperty);
            Assert.That(inequality, Is.EquivalentTo(new byte[] { 1, 2 }));
            Assert.That(varInequality, Is.EquivalentTo(new byte[] { 1, 2 }));

            var lessThan = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty < 1).ToArray().Select(o => o.ByteProperty);
            var varLessThan = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty < one).ToArray().Select(o => o.ByteProperty);
            Assert.That(lessThan, Is.EquivalentTo(new byte[] { 0 }));
            Assert.That(varLessThan, Is.EquivalentTo(new byte[] { 0 }));

            var lessThanOrEqual = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty <= 1).ToArray().Select(o => o.ByteProperty);
            var varLessThanOrEqual = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty <= one).ToArray().Select(o => o.ByteProperty);
            Assert.That(lessThanOrEqual, Is.EquivalentTo(new byte[] { 0, 1 }));
            Assert.That(varLessThanOrEqual, Is.EquivalentTo(new byte[] { 0, 1 }));

            var greaterThan = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty > 1).ToArray().Select(o => o.ByteProperty);
            var varGreaterThan = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty > one).ToArray().Select(o => o.ByteProperty);
            Assert.That(greaterThan, Is.EquivalentTo(new byte[] { 2 }));
            Assert.That(varGreaterThan, Is.EquivalentTo(new byte[] { 2 }));

            var greaterThanOrEqual = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty >= 1).ToArray().Select(o => o.ByteProperty);
            var varGreaterThanOrEqual = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty >= one).ToArray().Select(o => o.ByteProperty);
            Assert.That(greaterThanOrEqual, Is.EquivalentTo(new byte[] { 1, 2 }));
            Assert.That(varGreaterThanOrEqual, Is.EquivalentTo(new byte[] { 1, 2 }));

            var between = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty > 0 && o.ByteProperty < 2).ToArray().Select(o => o.ByteProperty);
            var varBetween = _realm.All<PrimaryKeyByteObject>().Where(o => o.ByteProperty > zero && o.ByteProperty < two).ToArray().Select(o => o.ByteProperty);
            Assert.That(between, Is.EquivalentTo(new byte[] { 1 }));
            Assert.That(varBetween, Is.EquivalentTo(new byte[] { 1 }));
        }

        [Test]
        public void SearchComparingConstants()
        {
            // Verify that constants in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandConstant).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingStaticFields()
        {
            // Verify that static field in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandField).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingStaticProperties()
        {
            // Verify that static properties in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandProperty).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingInstanceFields()
        {
            var constants = new InstanceConstants();

            // Verify that instance fields in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.SixtyThousandField).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingInstanceProperties()
        {
            var constants = new InstanceConstants();

            // Verify that instance properties in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.SixtyThousandProperty).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingNestedInstanceFields()
        {
            var constants = new NestedConstants();

            // Verify that nested instance fields in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.InstanceConstants.SixtyThousandField).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingNestedInstanceProperties()
        {
            var constants = new NestedConstants();

            // Verify that nested instance properties in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.InstanceConstants.SixtyThousandProperty).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingObjects()
        {
            var rex = new Dog
            {
                Name = "Rex"
            };

            var peter = new Owner
            {
                Name = "Peter",
                TopDog = rex
            };

            var george = new Owner
            {
                Name = "George",
                TopDog = rex
            };

            var sharo = new Dog
            {
                Name = "Sharo"
            };

            var ivan = new Owner
            {
                Name = "Ivan",
                TopDog = sharo
            };

            _realm.Write(() =>
            {
                _realm.Add(peter);
                _realm.Add(george);
                _realm.Add(ivan);
            });

            var rexOwners = _realm.All<Owner>().Where(o => o.TopDog == rex);
            Assert.That(rexOwners.Count(), Is.EqualTo(2));

            var sharoOwners = _realm.All<Owner>().Where(o => o.TopDog != rex);
            Assert.That(sharoOwners.Count(), Is.EqualTo(1));
            Assert.That(sharoOwners.Single().Name, Is.EqualTo("Ivan"));
        }

        [Test]
        public void SearchComparingObjects_WhenObjectIsUnmanaged_ShouldFail()
        {
            var rex = new Dog { Name = "Rex" };
            Assert.That(
                () => _realm.All<Owner>().Where(o => o.TopDog == rex).ToArray(),
                Throws.TypeOf<NotSupportedException>().And.Message.Contains("should be a managed RealmObjectBase"));
        }

        [Test]
        public void SearchComparingObjects_WhenObjectIsDeleted_ShouldFail()
        {
            var rex = new Dog { Name = "Rex" };
            _realm.Write(() =>
            {
                _realm.Add(rex);
            });

            Assert.That(
                () => _realm.All<Owner>().Where(o => o.TopDog == rex).ToArray(),
                Throws.Nothing);

            _realm.Write(() =>
            {
                _realm.Remove(rex);
            });

            Assert.That(
                () => _realm.All<Owner>().Where(o => o.TopDog == rex).ToArray(),
                Throws.TypeOf<NotSupportedException>().And.Message.Contains("should be a managed RealmObjectBase"));
        }

        [Test]
        public void StringSearch_Equals_CaseSensitivityTests()
        {
            MakeThreePatricks();

            // case sensitive
            var equalequal_patrick = _realm.All<Person>().Where(p => p.FirstName == "patrick").Count();
            Assert.That(equalequal_patrick, Is.EqualTo(2));

            // case sensitive
            var notequal_patrick = _realm.All<Person>().Where(p => p.FirstName != "patrick").Count();
            Assert.That(notequal_patrick, Is.EqualTo(1));

            // case sensitive
            var equals_patrick = _realm.All<Person>().Where(p => p.FirstName.Equals("patrick")).Count();
            Assert.That(equals_patrick, Is.EqualTo(2));

            // case sensitive
            var notequals_patrick = _realm.All<Person>().Where(p => !p.FirstName.Equals("patrick")).Count();
            Assert.That(notequals_patrick, Is.EqualTo(1));

            // ignore case
            var equals_ignorecase_patrick = _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.OrdinalIgnoreCase)).Count();
            Assert.That(equals_ignorecase_patrick, Is.EqualTo(3));

            // ignore case
            var notequals_ignorecase_patrick = _realm.All<Person>().Where(p => !p.FirstName.Equals("patrick", StringComparison.OrdinalIgnoreCase)).Count();
            Assert.That(notequals_ignorecase_patrick, Is.EqualTo(0));

            // case sensitive
            var equals_ordinal_patrick = _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.Ordinal)).Count();
            Assert.That(equals_ordinal_patrick, Is.EqualTo(2));

            // case sensitive
            var equals_ordinal_Patrick = _realm.All<Person>().Where(p => p.FirstName.Equals("Patrick", StringComparison.Ordinal)).Count();
            Assert.That(equals_ordinal_Patrick, Is.EqualTo(0));

            // case sensitive
            var equals_ordinal_patRick = _realm.All<Person>().Where(p => p.FirstName.Equals("patRick", StringComparison.Ordinal)).Count();
            Assert.That(equals_ordinal_patRick, Is.EqualTo(1));

            // case sensitive
            var notequals_ordinal_patrick = _realm.All<Person>().Where(p => !p.FirstName.Equals("patrick", StringComparison.Ordinal)).Count();
            Assert.That(notequals_ordinal_patrick, Is.EqualTo(1));
        }

        [Test]
        public void StringSearch_StartsWith_CaseSensitivityTests()
        {
            MakeThreePatricks();

            // case sensitive
            var startswith_patr = _realm.All<Person>().Where(p => p.FirstName.StartsWith("patr")).Count();
            Assert.That(startswith_patr, Is.EqualTo(2));

            // case sensitive
            var startswith_ordinal_patr = _realm.All<Person>().Where(p => p.FirstName.StartsWith("patr", StringComparison.Ordinal)).Count();
            Assert.That(startswith_ordinal_patr, Is.EqualTo(2));

            // ignore case
            var startswith_ignorecase_patr = _realm.All<Person>().Where(p => p.FirstName.StartsWith("patr", StringComparison.OrdinalIgnoreCase)).Count();
            Assert.That(startswith_ignorecase_patr, Is.EqualTo(3));
        }

        [Test]
        public void StringSearch_EndsWith_CaseSensitivityTests()
        {
            MakeThreePatricks();

            // case sensitive
            var endswith_rick = _realm.All<Person>().Where(p => p.FirstName.EndsWith("rick")).Count();
            Assert.That(endswith_rick, Is.EqualTo(2));

            // case sensitive
            var endswith_ordinal_rick = _realm.All<Person>().Where(p => p.FirstName.EndsWith("rick", StringComparison.Ordinal)).Count();
            Assert.That(endswith_ordinal_rick, Is.EqualTo(2));

            // ignore case
            var endswith_ignorecase_rick = _realm.All<Person>().Where(p => p.FirstName.EndsWith("rick", StringComparison.OrdinalIgnoreCase)).Count();
            Assert.That(endswith_ignorecase_rick, Is.EqualTo(3));
        }

        [Test]
        public void StringSearch_Contains_CaseSensitivityTests()
        {
            MakeThreePatricks();

            // case sensitive
            var contains_atri = _realm.All<Person>().Where(p => p.FirstName.Contains("atri")).Count();
            Assert.That(contains_atri, Is.EqualTo(2));

            // case sensitive
            var contains_ordinal_atri = _realm.All<Person>().Where(p => p.FirstName.Contains("atri", StringComparison.Ordinal)).Count();
            Assert.That(contains_ordinal_atri, Is.EqualTo(2));

            // ignore case
            var contains_ignorecase_atri = _realm.All<Person>().Where(p => p.FirstName.Contains("atri", StringComparison.OrdinalIgnoreCase)).Count();
            Assert.That(contains_ignorecase_atri, Is.EqualTo(3));
        }

        [Test]
        public void StringSearch_InvalidStringComparisonTests()
        {
            MakeThreePatricks();

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.CurrentCulture)).Count();
            }, Throws.TypeOf<NotSupportedException>());

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.CurrentCultureIgnoreCase)).Count();
            }, Throws.TypeOf<NotSupportedException>());

#if !WINDOWS_UWP && !NETCOREAPP1_1

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.InvariantCulture)).Count();
            }, Throws.TypeOf<NotSupportedException>());

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.Equals("patrick", StringComparison.InvariantCultureIgnoreCase)).Count();
            }, Throws.TypeOf<NotSupportedException>());

#endif

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.StartsWith("pat", StringComparison.CurrentCulture)).Count();
            }, Throws.TypeOf<NotSupportedException>());

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.EndsWith("rick", StringComparison.CurrentCulture)).Count();
            }, Throws.TypeOf<NotSupportedException>());

            Assert.That(() =>
            {
                _realm.All<Person>().Where(p => p.FirstName.Contains("atri", StringComparison.CurrentCulture)).Count();
            }, Throws.TypeOf<NotSupportedException>());
        }

        [TestCaseSource(nameof(LikeTestValues))]
        public void StringSearch_LikeTests(string str, string pattern, bool caseSensitive, bool expected)
        {
            _realm.Write(() => _realm.Add(new IntPrimaryKeyWithValueObject { Id = 1, StringValue = str }));

            var regularQuery = _realm.All<IntPrimaryKeyWithValueObject>().Where(o => o.StringValue.Like(pattern, caseSensitive));
            var negatedQuery = _realm.All<IntPrimaryKeyWithValueObject>().Where(o => !o.StringValue.Like(pattern, caseSensitive));

            if (expected)
            {
                Assert.That(regularQuery.Count(), Is.EqualTo(1));
                Assert.That(regularQuery.Single().Id, Is.EqualTo(1));

                Assert.That(negatedQuery.Count(), Is.EqualTo(0));
            }
            else
            {
                Assert.That(regularQuery.Count(), Is.EqualTo(0));

                Assert.That(negatedQuery.Count(), Is.EqualTo(1));
                Assert.That(negatedQuery.Single().Id, Is.EqualTo(1));
            }
        }

        public static object[] LikeTestValues =
        {
            new object[] { "abc", "ab", false, false },
            new object[] { "abc", string.Empty, false, false },
            new object[] { string.Empty, string.Empty, true, true },
            new object[] { string.Empty, string.Empty, false, true },
            new object[] { null, string.Empty, true, false },
            new object[] { null, string.Empty, false, false },
            new object[] { string.Empty, null, true, false },
            new object[] { string.Empty, null, false, false },
            new object[] { null, null, true, true },
            new object[] { null, null, false, true },
            new object[] { null, "*", true, false },
            new object[] { null, "*", false, false },
            new object[] { string.Empty, "*", true, true },
            new object[] { string.Empty, "*", false, true },
            new object[] { string.Empty, "?", true, false },
            new object[] { string.Empty, "?", false, false },

            new object[] { "abc", "*a*", true, true },
            new object[] { "abc", "*b*", true, true },
            new object[] { "abc", "*c", true, true },
            new object[] { "abc", "ab*", true, true },
            new object[] { "abc", "*bc", true, true },
            new object[] { "abc", "a*bc", true, true },
            new object[] { "abc", "*abc*", true, true },
            new object[] { "abc", "*d*", true, false },
            new object[] { "abc", "aabc", true, false },
            new object[] { "abc", "b*bc", true, false },

            new object[] { "abc", "a??", true, true },
            new object[] { "abc", "?b?", true, true },
            new object[] { "abc", "*?c", true, true },
            new object[] { "abc", "ab?", true, true },
            new object[] { "abc", "?bc", true, true },
            new object[] { "abc", "?d?", true, false },
            new object[] { "abc", "?abc", true, false },
            new object[] { "abc", "b?bc", true, false },

            new object[] { "abc", "*C*", true, false },
            new object[] { "abc", "*c*", false, true },
            new object[] { "abc", "*C*", false, true },
        };

        [Test]
        public void AnySucceeds()
        {
            Assert.That(_realm.All<Person>().Where(p => p.Latitude > 50).Any());
            Assert.That(_realm.All<Person>().Where(p => p.Score > 0).Any());
            Assert.That(_realm.All<Person>().Where(p => p.IsInteresting == false).Any());
            Assert.That(_realm.All<Person>().Where(p => p.FirstName == "John").Any());
        }

        [Test]
        public void AnyFails()
        {
            Assert.False(_realm.All<Person>().Where(p => p.Latitude > 100).Any());
            Assert.False(_realm.All<Person>().Where(p => p.Score > 50000).Any());
            Assert.False(_realm.All<Person>().Where(p => p.FirstName == "Samantha").Any());
        }

        [Test]
        public void SingleFailsToFind()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Latitude > 100));
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Where(p => p.Latitude > 100).Single());
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Score > 50000));
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.FirstName == "Samantha"));
        }

        [Test]
        public void SingleFindsTooMany()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Where(p => p.Latitude == 50).Single());
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Score != 100.0f));
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.FirstName == "John"));
        }

        [Test]
        public void SingleWorks()
        {
            var s0 = _realm.All<Person>().Single(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).Single();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Single(p => p.FirstName == "Peter");
            Assert.That(s2.FirstName, Is.EqualTo("Peter"));
        }

        [Test]
        public void SingleOrDefaultWorks()
        {
            var s0 = _realm.All<Person>().SingleOrDefault(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).SingleOrDefault();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().SingleOrDefault(p => p.FirstName == "Peter");
            Assert.That(s2.FirstName, Is.EqualTo("Peter"));
        }

        [Test]
        public void SingleOrDefaultReturnsDefault()
        {
            var expectedDef = _realm.All<Person>().SingleOrDefault(p => p.FirstName == "Zaphod");
            Assert.That(expectedDef, Is.Null);

            expectedDef = _realm.All<Person>().Where(p => p.FirstName == "Just Some Guy").SingleOrDefault();
            Assert.That(expectedDef, Is.Null);
        }

        [Test]
        public void FirstFailsToFind()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.Latitude > 100));
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Where(p => p.Latitude > 100).First());
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.FirstName == "Samantha"));
        }

        [Test]
        public void FirstWorks()
        {
            var s0 = _realm.All<Person>().First(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).First();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().First(p => p.FirstName == "John");
            Assert.That(s2.FirstName, Is.EqualTo("John"));
        }

        [Test]
        public void FirstOrDefaultWorks()
        {
            var s0 = _realm.All<Person>().FirstOrDefault(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).FirstOrDefault();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().FirstOrDefault(p => p.FirstName == "John");
            Assert.That(s2.FirstName, Is.EqualTo("John"));
        }

        [Test]
        public void FirstOrDefaultReturnsDefault()
        {
            var expectedDef = _realm.All<Person>().FirstOrDefault(p => p.FirstName == "Zaphod");
            Assert.That(expectedDef, Is.Null);

            expectedDef = _realm.All<Person>().Where(p => p.FirstName == "Just Some Guy").FirstOrDefault();
            Assert.That(expectedDef, Is.Null);

            expectedDef = _realm.All<Person>()
                                .Where(p => p.FirstName == "Just Some Guy")
                                .OrderByDescending(p => p.FirstName)
                                .FirstOrDefault();

            Assert.That(expectedDef, Is.Null);
        }

        [Test]
        public void LastFailsToFind()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Last(p => p.Latitude > 100));
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Where(p => p.Latitude > 100).Last());
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Last(p => p.LastName == "Samantha"));
        }

        [Test]
        public void LastWorks()
        {
            var s0 = _realm.All<Person>().Last(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));  // Last same as First when one match

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).Last();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Last(p => p.FirstName == "John");
            Assert.That(s2.FirstName, Is.EqualTo("John"));  // order not guaranteed in two items but know they match this
        }

        [Test]
        public void LastOrDefaultWorks()
        {
            var s0 = _realm.All<Person>().LastOrDefault(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));  // Last same as First when one match

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).LastOrDefault();
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().LastOrDefault(p => p.FirstName == "John");
            Assert.That(s2.FirstName, Is.EqualTo("John"));  // order not guaranteed in two items but know they match this
        }

        [Test]
        public void LastOrDefaultReturnsDefault()
        {
            var expectedDef = _realm.All<Person>().LastOrDefault(p => p.FirstName == "Zaphod");
            Assert.That(expectedDef, Is.Null);

            expectedDef = _realm.All<Person>().Where(p => p.FirstName == "Just Some Guy").LastOrDefault();
            Assert.That(expectedDef, Is.Null);
        }

        [Test]
        public void ElementAtOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _realm.All<Person>().Where(p => p.Latitude > 140).ElementAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _realm.All<Person>().Where(p => p.FirstName == "Samantha").ElementAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _realm.All<Person>().Where(p => p.FirstName == "Peter").ElementAt(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => _realm.All<Person>().ElementAt(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => _realm.All<Person>().ElementAt(-1));
        }

        [Test]
        public void ElementAtInRange()
        {
            var s0 = _realm.All<Person>().Where(p => p.Longitude < -70.0 && p.Longitude > -90.0).ElementAt(0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).ElementAt(0);
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Where(p => p.FirstName == "John").ElementAt(1);
            Assert.That(s2.FirstName, Is.EqualTo("John"));

            var s3 = _realm.All<Person>().ElementAt(2);
            Assert.That(s3.FirstName, Is.EqualTo("Peter"));
        }

        [Test]
        public void ElementAtOrDefaultInRange()
        {
            var s0 = _realm.All<Person>().Where(p => p.Longitude < -70.0 && p.Longitude > -90.0).ElementAtOrDefault(0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Where(p => p.Score == 100.0f).ElementAtOrDefault(0);
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Where(p => p.FirstName == "John").ElementAtOrDefault(1);
            Assert.That(s2.FirstName, Is.EqualTo("John"));

            var s3 = _realm.All<Person>().ElementAtOrDefault(2);
            Assert.That(s3.FirstName, Is.EqualTo("Peter"));
        }

        [Test]
        public void ElementAtOrDefaultReturnsDefault()
        {
            var expectedDef = _realm.All<Person>().Where(p => p.FirstName == "Just Some Guy").ElementAtOrDefault(0);
            Assert.That(expectedDef, Is.Null);
        }

        [Test]
        public void ElementAt_SortedViaLINQ()
        {
            var query = _realm.All<Person>().OrderByDescending(p => p.FirstName).ThenBy(p => p.LastName);
            var arr = query.ToArray();
            Assert.That(query.ElementAt(0).FullName, Is.EqualTo(arr[0].FullName));
            Assert.That(query.ElementAt(1).FullName, Is.EqualTo(arr[1].FullName));
            Assert.That(query.ElementAt(2).FullName, Is.EqualTo(arr[2].FullName));
        }

        [Test]
        public void ElementAt_SortedViaStringQuery()
        {
            var query = _realm.All<Person>().Filter("TRUEPREDICATE SORT(FirstName DESC, LastName ASC)");
            var arr = query.ToArray();
            Assert.That(query.ElementAt(0).FullName, Is.EqualTo(arr[0].FullName));
            Assert.That(query.ElementAt(1).FullName, Is.EqualTo(arr[1].FullName));
            Assert.That(query.ElementAt(2).FullName, Is.EqualTo(arr[2].FullName));
        }

        // note that DefaultIfEmpty returns a collection of one item
        [Test, TestExplicit("Currently broken and hard to implement")]
        public void DefaultIfEmptyReturnsDefault()
        {
            /*
             * This is comprehensively broken and awkward to fix in our current architecture.
             * Looking at the test code below, the Count is invoked on a RealmResults and directly
             * invokes its query handle, which of course has zero elements.
             * One posible approach is to toggle the RealmResults into a special state where
             * it acts as a generator for a single null pointer.*
             */
            var expectCollectionOfOne = _realm.All<Person>().Where(p => p.FirstName == "Just Some Guy").DefaultIfEmpty();
            Assert.That(expectCollectionOfOne.Count(), Is.EqualTo(1));
            var expectedDef = expectCollectionOfOne.Single();
            Assert.That(expectedDef, Is.Null);
        }

        [Test]
        public void ChainedWhereFirst()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var johnScorer = moderateScorers.Where(p => p.FirstName == "John").First();
            Assert.That(johnScorer, Is.Not.Null);
            Assert.That(johnScorer.Score, Is.EqualTo(100.0f));
            Assert.That(johnScorer.FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void ChainedWhereFirstWithPredicate()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var johnScorer = moderateScorers.First(p => p.FirstName == "John");
            Assert.That(johnScorer, Is.Not.Null);
            Assert.That(johnScorer.Score, Is.EqualTo(100.0f));
            Assert.That(johnScorer.FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void ChainedWhereAnyTrue()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var hasJohn = moderateScorers.Where(p => p.FirstName == "John").Any();
            Assert.That(hasJohn, Is.True);
        }

        [Test]
        public void ChainedWhereAnyFalse()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var hasJohnSmith = moderateScorers.Where(p => p.LastName == "Smith").Any();
            Assert.That(hasJohnSmith, Is.False);
        }

        [Test]
        public void ChainedWhereAnyWithPredicateTrue()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var hasJohn = moderateScorers.Any(p => p.FirstName == "John");
            Assert.That(hasJohn, Is.True);
        }

        [Test]
        public void ChainedWhereAnyWithPredicateFalse()
        {
            var moderateScorers = _realm.All<Person>().Where(p => p.Score >= 20.0f && p.Score <= 100.0f);
            var hasJohnSmith = moderateScorers.Any(p => p.LastName == "Smith");
            Assert.That(hasJohnSmith, Is.False);
        }

        #region [MapTo] Test cases

        [Test]
        public void Where_WhenPropertyIsMappedTo_FiltersCorrectly()
        {
            var objs = MakeThreeMappedObjects();

            Assert.That(objs.Count(o => o.Name.StartsWith("p", StringComparison.OrdinalIgnoreCase)), Is.EqualTo(2));
            Assert.That(objs.Count(o => o.Id > 2), Is.EqualTo(1));
        }

        [Test]
        public void OrderBy_WhenPropertyIsMappedTo_OrderdsCorrectly()
        {
            var objs = MakeThreeMappedObjects();

            var objsById = objs.OrderByDescending(o => o.Id).ToArray().Select(o => o.Id);

            Assert.That(objsById, Is.EqualTo(new[] { 3, 2, 1 }));

            var objsByName = objs.OrderBy(o => o.Name).ToArray().Select(o => o.Id);

            Assert.That(objsByName, Is.EqualTo(new[] { 3, 2, 1 }));
        }

        [Test]
        public void Where_WhenTypeIsMappedTo_FiltersCorrectly()
        {
            RemappedTypeObject first = null;
            _realm.Write(() =>
            {
                first = _realm.Add(new RemappedTypeObject
                {
                    Id = 1,
                    StringValue = "1"
                });

                _realm.Add(new RemappedTypeObject
                {
                    Id = 2,
                    StringValue = "2",
                    MappedLink = first,
                    NormalLink = first
                });

                _realm.Add(new RemappedTypeObject
                {
                    Id = 3,
                    StringValue = "3",
                    MappedLink = first,
                    NormalLink = first
                });
            });

            var normalLinks = _realm.All<RemappedTypeObject>()
                                    .Where(o => o.NormalLink == first)
                                    .ToArray()
                                    .Select(n => n.Id);
            Assert.That(normalLinks, Is.EquivalentTo(new[] { 2, 3 }));

            var mappedLinks = _realm.All<RemappedTypeObject>()
                                    .Where(o => o.MappedLink == first)
                                    .ToArray()
                                    .Select(n => n.Id);
            Assert.That(mappedLinks, Is.EquivalentTo(new[] { 2, 3 }));

            var mappedLinksAndStringValue = _realm.All<RemappedTypeObject>()
                                                  .Where(o => o.MappedLink == first && o.StringValue.StartsWith("2"))
                                                  .ToArray()
                                                  .Select(n => n.Id);
            Assert.That(mappedLinksAndStringValue, Is.EquivalentTo(new[] { 2 }));

            var normalBacklinks = first.NormalBacklink
                                       .Where(m => m.StringValue.StartsWith("2"))
                                       .ToArray()
                                       .Select(n => n.Id);
            Assert.That(normalBacklinks, Is.EquivalentTo(new[] { 2 }));

            var mappedBacklinks = first.MappedBacklink
                                       .Where(m => m.StringValue.StartsWith("3"))
                                       .ToArray()
                                       .Select(n => n.Id);
            Assert.That(mappedBacklinks, Is.EquivalentTo(new[] { 3 }));
        }

        #endregion

        [Test]
        public void ObjectComparison_WhenUsingNull_ShouldWork()
        {
            var ivar = new Owner
            {
                Name = "Ivar the Dogless"
            };

            _realm.Write(() =>
            {
                _realm.Add(ivar);
                _realm.Add(new Owner
                {
                    Name = "John the Dogful",
                    TopDog = new Dog
                    {
                        Name = "Little Johny"
                    }
                });
            });

            var nullTopDogs = _realm.All<Owner>()
                                    .Where(x => x.TopDog == null)
                                    .ToArray();

            Assert.That(nullTopDogs.Length, Is.EqualTo(1));
            Assert.That(nullTopDogs.Single(), Is.EqualTo(ivar));

            var notNullTopDogs = _realm.All<Owner>()
                                       .Where(x => x.TopDog != null)
                                       .ToArray();

            Assert.That(notNullTopDogs.Length, Is.EqualTo(1));
            Assert.That(notNullTopDogs.Single(), Is.Not.EqualTo(ivar));
        }

        [Test]
        public void Queryable_IndexOf_ShouldWork()
        {
            var item1 = new IntPrimaryKeyWithValueObject { Id = 1, StringValue = "BBB" };
            var item2 = new IntPrimaryKeyWithValueObject { Id = 2, StringValue = "AAA" };
            _realm.Write(() =>
            {
                _realm.RemoveAll<IntPrimaryKeyWithValueObject>();

                _realm.Add(item1);
                _realm.Add(item2);
            });

            var query = _realm.All<IntPrimaryKeyWithValueObject>().AsRealmCollection();
            Assert.That(query.IndexOf(item1), Is.Zero);
            Assert.That(query.IndexOf(item2), Is.EqualTo(1));

            var sortedQuery = _realm.All<IntPrimaryKeyWithValueObject>().OrderBy(i => i.StringValue).AsRealmCollection();
            Assert.That(sortedQuery.IndexOf(item2), Is.Zero);
            Assert.That(sortedQuery.IndexOf(item1), Is.EqualTo(1));
        }

        [Test]
        public void Queryable_IndexOf_WhenObjectIsNotManaged_ShouldThrow()
        {
            var query = _realm.All<IntPrimaryKeyWithValueObject>().AsRealmCollection();
            var nonManagedItem = new IntPrimaryKeyWithValueObject();
            Assert.That(() => query.IndexOf(nonManagedItem), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Queryable_IndexOf_WhenObjectBelongsToADifferentRealm_ShouldThrow()
        {
            var config = new RealmConfiguration(Path.GetTempFileName());
            try
            {
                using (var otherRealm = Realm.GetInstance(config))
                {
                    var otherRealmItem = new IntPrimaryKeyWithValueObject { Id = 1 };
                    otherRealm.Write(() =>
                    {
                        otherRealm.Add(otherRealmItem);
                    });

                    var query = _realm.All<IntPrimaryKeyWithValueObject>().AsRealmCollection();
                    Assert.That(() => query.IndexOf(otherRealmItem), Throws.InstanceOf<RealmObjectManagedByAnotherRealmException>());
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void Queryable_IndexOf_WhenNull_ShouldThrow()
        {
            var query = _realm.All<IntPrimaryKeyWithValueObject>().AsRealmCollection();
            Assert.That(() => query.IndexOf(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void Queryable_IndexOf_WhenDifferentType_ShouldThrow()
        {
            var query = _realm.All<IntPrimaryKeyWithValueObject>().AsRealmCollection();
            Assert.That(() => query.IndexOf(123), Throws.InstanceOf<ArgumentException>());
        }

        private IQueryable<RemappedPropertiesObject> MakeThreeMappedObjects()
        {
            _realm.Write(() =>
            {
                _realm.RemoveAll<RemappedPropertiesObject>();

                _realm.Add(new RemappedPropertiesObject
                {
                    Id = 1,
                    Name = "Peter"
                });

                _realm.Add(new RemappedPropertiesObject
                {
                    Id = 2,
                    Name = "Patrick"
                });

                _realm.Add(new RemappedPropertiesObject
                {
                    Id = 3,
                    Name = "George"
                });
            });

            return _realm.All<RemappedPropertiesObject>();
        }

        private void MakeThreePatricks()
        {
            _realm.Write(() =>
            {
                _realm.RemoveAll<Person>();

                _realm.Add(new Person
                {
                    FirstName = "patRick"
                });

                _realm.Add(new Person
                {
                    FirstName = "patrick"
                });

                _realm.Add(new Person
                {
                    FirstName = "patrick"
                });
            });
        }

        private static class Constants
        {
            public const long SixtyThousandConstant = 60000;

            public static readonly long SixtyThousandField = 60000;

            public static long SixtyThousandProperty { get; } = 60000;
        }

        private class NestedConstants
        {
            public InstanceConstants InstanceConstants { get; } = new InstanceConstants();
        }

        private class InstanceConstants
        {
            public readonly long SixtyThousandField = 60000;

            public long SixtyThousandProperty { get; } = 60000;
        }
    }
}