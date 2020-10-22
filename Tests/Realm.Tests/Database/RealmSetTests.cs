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
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmSetTests : RealmInstanceTest
    {
        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenUnmanaged_Int64(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64Set, testData);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenUnmanaged_Int64Counter(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt64(TestCaseData<long?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt64Set, testData);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt64Counter(TestCaseData<long?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt64CounterSet, ToInteger(testData));
        }

        private static void RunUnmanagedTests<T>(Func<SetsObject, ISet<T>> accessor, TestCaseData<T> testData)
        {
            var testObject = new SetsObject();
            var set = accessor(testObject);

            testData.AssertCount(set);
            testData.AssertExceptWith(set);
            testData.AssertIntersectWith(set);
            testData.AssertIsProperSubsetOf(set);
            testData.AssertIsProperSupersetOf(set);
            testData.AssertIsSubsetOf(set);
            testData.AssertIsSupersetOf(set);
            testData.AssertOverlaps(set);
            testData.AssertSymmetricExceptWith(set);
            testData.AssertUnionWith(set);
        }

        private static TestCaseData<RealmInteger<T>> ToInteger<T>(TestCaseData<T> data)
            where T : struct, IComparable<T>, IFormattable
        {
            return new TestCaseData<RealmInteger<T>>(data.InitialValues.ToInteger(), data.OtherCollection.ToInteger());
        }

        private static TestCaseData<RealmInteger<T>?> ToInteger<T>(TestCaseData<T?> data)
            where T : struct, IComparable<T>, IFormattable
        {
            return new TestCaseData<RealmInteger<T>?>(data.InitialValues.ToInteger(), data.OtherCollection.ToInteger());
        }

        public static IEnumerable<TestCaseData<long>> Int64TestValues()
        {
            yield return new TestCaseData<long>(new long[] { 1, 2, 3 }, new long[] { 4, 5, 6 });
            yield return new TestCaseData<long>(new long[] { 1, 2, 3 }, new long[] { 1, 2, 3 });
            yield return new TestCaseData<long>(new long[] { long.MinValue, long.MaxValue }, new long[] { 0 });
            yield return new TestCaseData<long>(new long[] { -1, 0, 1 }, Array.Empty<long>());
            yield return new TestCaseData<long>(Array.Empty<long>(), new long[] { 0 });
            yield return new TestCaseData<long>(new long[] { 4, 6, 8 }, new long[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<long>(new long[] { 1, 1, 1, 1, 1, 1, 1 }, new long[] { 1, 1, 1 });
            yield return new TestCaseData<long>(new long[] { 1, 1, 1, 1, 1, 1, 1 }, new long[] { 1, 2, 1 });
            yield return new TestCaseData<long>(new long[] { 1, 2, 2, 1, 1, 1, 1 }, new long[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<long?>> NullableInt64TestValues()
        {
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 3 }, new long?[] { 4, 5, 6 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 3, null }, new long?[] { 1, 2, 3, null });
            yield return new TestCaseData<long?>(new long?[] { long.MinValue, long.MaxValue }, new long?[] { 0 });
            yield return new TestCaseData<long?>(new long?[] { -1, 0, 1 }, Array.Empty<long?>());
            yield return new TestCaseData<long?>(Array.Empty<long?>(), new long?[] { 0 });
            yield return new TestCaseData<long?>(new long?[] { 4, 6, 8 }, new long?[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<long?>(new long?[] { 1, 1, 1, 1, 1, 1, 1 }, new long?[] { 1, 1, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 1, 1, 1, 1, 1, 1 }, new long?[] { 1, 2, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 2, 1, 1, 1, 1 }, new long?[] { 1, 1, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 2, 1, null, null, null }, new long?[] { 1, 1, 1, null });
            yield return new TestCaseData<long?>(new long?[] { null }, new long?[] { null, null });
            yield return new TestCaseData<long?>(new long?[] { null, null }, new long?[] { null, 6 });
        }

        public class TestCaseData<T>
        {
            public T[] InitialValues { get; }

            public T[] OtherCollection { get; }

            public TestCaseData(IEnumerable<T> initialValues, IEnumerable<T> otherCollection)
            {
                InitialValues = initialValues.ToArray();
                OtherCollection = otherCollection.ToArray();
            }

            public override string ToString()
            {
                return $"{typeof(T).Name}: {{{string.Join(",", InitialValues)}}} - {{{string.Join(",", OtherCollection)}}}";
            }

            public void AssertCount(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                Assert.That(target.Count, Is.EqualTo(reference.Count));
                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertUnionWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.UnionWith(OtherCollection);
                reference.UnionWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertExceptWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.ExceptWith(OtherCollection);
                reference.ExceptWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertIntersectWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.IntersectWith(OtherCollection);
                reference.IntersectWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertIsProperSubsetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsProperSubsetOf(OtherCollection);
                var referenceResult = reference.IsProperSubsetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsProperSupersetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsProperSupersetOf(OtherCollection);
                var referenceResult = reference.IsProperSupersetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsSubsetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsSubsetOf(OtherCollection);
                var referenceResult = reference.IsSubsetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsSupersetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsSupersetOf(OtherCollection);
                var referenceResult = reference.IsSupersetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertOverlaps(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.Overlaps(OtherCollection);
                var referenceResult = reference.Overlaps(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertSymmetricExceptWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.SymmetricExceptWith(OtherCollection);
                reference.SymmetricExceptWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            private void Seed(ISet<T> target)
            {
                target.Clear();
                foreach (var item in InitialValues)
                {
                    target.Add(item);
                }
            }

            private ISet<T> GetReferenceSet() => new HashSet<T>(InitialValues);
        }
    }
}
