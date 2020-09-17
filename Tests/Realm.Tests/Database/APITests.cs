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
#if NETCOREAPP1_1 || WINDOWS_UWP
using System.Reflection;
#endif
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
        public void RealmCollectionContravariance(Type type)
        {
            Assert.That(typeof(IRealmCollection<RealmObjectBase>).IsAssignableFrom(type));
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
                try
                {
                    await Task.Delay(100).Timeout(10);
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<TimeoutException>());
                }
            });
        }
    }
}
