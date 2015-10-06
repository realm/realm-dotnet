using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmNet;

namespace UnitTests
{
    [TestFixture]
    public class RealmTests
    {
        [Test]
        public void FirstTest()
        {
            var r = Realm.GetInstance("");
        }
    }
}
