/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    class SortingTests : PeopleTestsBase
    {
        
    // see comment on base method why this isn't decorated with [SetUp]
    public override void Setup()
    {
        base.Setup();
        MakeThreePeople();
    }


        [Test]
        public void AllSortNumerically()
        {
            var s0 = _realm.All<Person>().ToList().Select(p => p.Score);
//            var s0 = _realm.All<Person>().OrderBy(p => p.Score).Select(p => p.Score).ToList();
            Assert.That(s0, Is.EquivalentTo( new float[] {-0.9907f, 42.42f, 100}) );
        }

    } // SortingTests
}
