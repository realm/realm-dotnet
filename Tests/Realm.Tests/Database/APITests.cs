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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class APITests
    {
        [TestCase(typeof(IRealmCollection<Person>))]
        [TestCase(typeof(RealmResults<Person>))]
        [TestCase(typeof(RealmList<Person>))]
        [TestCase(typeof(RealmSet<Person>))]
        public void RealmCollectionContravariance(Type type)
        {
            Assert.That(typeof(IRealmCollection<IRealmObjectBase>).IsAssignableFrom(type));
        }

        [Test]
        public void RealmIntegerApi()
        {
            var five = new RealmInteger<long>(5);
            Assert.That(five == 5);
            Assert.That(five != 10);
            Assert.That(five.Equals(5));
            Assert.That(five + 5, Is.EqualTo(10));
            Assert.That(five > 5, Is.False);
            Assert.That(five < 5, Is.False);
            Assert.That(five > 3);
            Assert.That(five < 7);
            Assert.That(five / 5, Is.EqualTo(1));
            Assert.That(five * 5, Is.EqualTo(25));
            Assert.That(five - 3, Is.EqualTo(2));

            Assert.That(() => five.Increment(), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => five.Decrement(), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => five.Increment(5), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => five++, Throws.TypeOf<NotSupportedException>());
            Assert.That(() => five--, Throws.TypeOf<NotSupportedException>());

            var ten = new RealmInteger<long>(10);
            Assert.That(five == ten, Is.False);
            Assert.That(five != ten);
            Assert.That(five, Is.Not.EqualTo(10));
            Assert.That(ten > five);
            Assert.That(ten < five, Is.False);
            Assert.That(five < ten);
            Assert.That(five > ten, Is.False);
            Assert.That(ten + five, Is.EqualTo(15));
            Assert.That(ten - five, Is.EqualTo(5));
            Assert.That(five - ten, Is.EqualTo(-5));
            Assert.That(five * ten, Is.EqualTo(50));
            Assert.That(ten * five, Is.EqualTo(50));
            Assert.That(ten / five, Is.EqualTo(2));
            Assert.That((double)five / ten, Is.EqualTo(0.5));

            var fifteen = new RealmInteger<long>(15);

            var integers = new[]
            {
                fifteen,
                five,
                ten
            };

            Assert.That(integers.OrderBy(i => i), Is.EqualTo(new[] { five, ten, fifteen }));
            Assert.That(integers.OrderByDescending(i => i), Is.EqualTo(new[] { fifteen, ten, five }));
        }

        [Test]
        public void TestTaskTimeout()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.AssertThrows<TimeoutException>(() => GetVoidTask().Timeout(10));

                var ex = await TestHelpers.AssertThrows<TimeoutException>(() => GetVoidTask().Timeout(10, detail: "some detail"));
                Assert.That(ex.Message, Does.Contain("some detail"));

                var errorTask = Task.FromException(new ArgumentException("invalid argument"));
                var ex2 = await TestHelpers.AssertThrows<ArgumentException>(() => GetVoidTask().Timeout(10, errorTask));
                Assert.That(ex2.Message, Does.Contain("invalid argument"));

                await TestHelpers.AssertThrows<TimeoutException>(() => GetIntTask().Timeout(10));

                var ex3 = await TestHelpers.AssertThrows<TimeoutException>(() => GetIntTask().Timeout(10, detail: "another detail"));
                Assert.That(ex3.Message, Does.Contain("another detail"));

                var ex4 = await TestHelpers.AssertThrows<ArgumentException>(() => GetFaultedIntTask().Timeout(1000));
                Assert.That(ex4.Message, Does.Contain("super invalid"));

                static async Task<int> GetIntTask()
                {
                    await Task.Delay(100);
                    return 5;
                }

                static Task GetVoidTask() => Task.Delay(100);

                static async Task<int> GetFaultedIntTask()
                {
                    await Task.Delay(1);
                    throw new ArgumentException("super invalid");
                }
            });
        }

        [Test]
        public void TestTaskCancellation()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var cts = new CancellationTokenSource(10);

                await TestHelpers.AssertThrows<TaskCanceledException>(() => Task.Delay(2000).AddCancellation(cts.Token));

                // Null token should just await the original task
                await Task.Delay(10).AddCancellation(token: null);

                // Cancelling after the task has completed should be a no-op
                var cts2 = new CancellationTokenSource(50);
                await Task.Delay(10).AddCancellation(cts2.Token);

                var cts3 = new CancellationTokenSource();
                cts3.Cancel();

                await TestHelpers.AssertThrows<TaskCanceledException>(() => Task.Delay(10).AddCancellation(cts3.Token));
            });
        }

        [Test]
        public void TestQueryMethods()
        {
            Assert.Throws<NotSupportedException>(() => QueryMethods.Contains("foo", "bar", StringComparison.Ordinal));
            Assert.Throws<NotSupportedException>(() => QueryMethods.Like("foo", "bar"));
            Assert.Throws<NotSupportedException>(() => QueryMethods.FullTextSearch("foo", "bar"));
            Assert.Throws<NotSupportedException>(() => QueryMethods.GeoWithin(null, new GeoCircle((0, 0), 10)));

            // Sanity check that we've covered all methods above.
            Assert.That(typeof(QueryMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Length, Is.EqualTo(4));
        }
    }
}
