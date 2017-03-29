using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace NUnit.Tests.Simple {

    class People : RealmObject
    {
        public string Name { get; set; }
    }


    [TestFixture]
    public class TestLocal {
        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            using (var realm = Realm.GetInstance())
            {
                var initialCount = realm.All<People>().Count();

                realm.Write(() =>
                {
                    realm.Add(new People() {Name = "Andy"});
                });
                Assert.Equals(initialCount + 1, realm.All<People>().Count());
            }
        }
    }
}
