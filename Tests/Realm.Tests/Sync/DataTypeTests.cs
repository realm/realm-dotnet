////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DataTypeSynchronizationTests : SyncTestBase
    {
        [Test]
        public void Set_Boolean() => TestSetCore(o => o.BooleanSet, true, false);

        [Test]
        public void Set_Byte() => TestSetCore(o => o.ByteSet, (byte)9, (byte)255);

        [Test]
        public void Set_Int16() => TestSetCore(o => o.Int16Set, (short)55, (short)987);

        [Test]
        public void Set_Int32() => TestSetCore(o => o.Int32Set, 987, 123);

        [Test]
        public void Set_Int64() => TestSetCore(o => o.Int64Set, 12345678910111213, 987654321);

        [Test]
        public void Set_Double() => TestSetCore(o => o.DoubleSet, 123.456, 789.123);

        // TODO: use more precise numbers once https://jira.mongodb.org/browse/REALMC-8475 is done.
        [Test]
        public void Set_Decimal() => TestSetCore(o => o.DecimalSet, 123.7777777777777m, 999.99999999999m);

        // TODO: use more precise numbers once https://jira.mongodb.org/browse/REALMC-8475 is done.
        [Test]
        public void Set_Decimal128() => TestSetCore(o => o.Decimal128Set, 123.7777777777777m, 999.99999999999m);

        [Test]
        public void Set_ObjectId() => TestSetCore(o => o.ObjectIdSet, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public void Set_DateTimeOffset() => TestSetCore(o => o.DateTimeOffsetSet, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public void Set_String() => TestSetCore(o => o.StringSet, "abc", "cde");

        [Test]
        public void Set_Binary() => TestSetCore(o => o.ByteArraySet, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public void Set_Object() => TestSetCore(o => o.ObjectSet, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        private void TestSetCore<T>(Func<SyncSetsObject, ISet<T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncSetsObject());
                });

                await WaitForUploadAsync(realm1);
                await WaitForDownloadAsync(realm2);

                var obj2 = realm2.Find<SyncSetsObject>(obj1.Id);

                var set1 = getter(obj1);
                var set2 = getter(obj2);

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    set1.Add(item1);
                });

                await WaitForCollectionChangeAsync(set2.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                realm2.Write(() =>
                {
                    set2.Add(item2);
                });

                await WaitForCollectionChangeAsync(set1.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                // Assert Remove works
                realm2.Write(() =>
                {
                    set2.Remove(set2.First());
                });

                await WaitForCollectionChangeAsync(set1.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                // Assert Clear works
                realm1.Write(() =>
                {
                    set1.Clear();
                });

                await WaitForCollectionChangeAsync(set2.AsRealmCollection());

                Assert.That(set1, Is.Empty);
                Assert.That(set2, Is.Empty);
            }, ensureNoSessionErrors: true);
        }

        private async Task WaitForCollectionChangeAsync<T>(IRealmCollection<T> collection, int timeout = 10 * 1000)
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                collection.CollectionChanged += Collection_CollectionChanged;
                await tcs.Task.Timeout(timeout);
            }
            finally
            {
                collection.CollectionChanged -= Collection_CollectionChanged;
            }

            void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                tcs.TrySetResult(null);
            }
        }
    }
}
