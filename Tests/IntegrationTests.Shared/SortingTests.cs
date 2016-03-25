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
            _realm.Write(() =>
            {
                // add an entry like John Doe but interesting
                var jd = _realm.CreateObject<Person>();
                jd.FullName = "John Doesmore"; 
                jd.IsInteresting = true;
                jd.Email = "john@doe.com";
                jd.Score = 100;
                jd.Latitude = 40.7637286;
                jd.Longitude = -73.9748113;
            });
        }


        [Test, Explicit("disable until work out how to do Results with sort")]
        public void AllSortOneLevel()
        {
            var s0 = _realm.All<Person>().OrderBy(p => p.Score).ToList().Select(p => p.Score);
            Assert.That(s0, Is.EqualTo( new float[] {-0.9907f, 42.42f, 100}) );
        }


        [Test, Explicit("disable until work out how to do Results with sort")]
        public void AllSortTwoLevel()
        {
            var sortAA = _realm.All<Person>().OrderBy(p => p.FirstName).ThenBy(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(sortAA, Is.EqualTo( new double[] {40.7637286, 51.508530, 37.7798657}) );

            var sortDA = _realm.All<Person>().OrderByDescending(p => p.FirstName).ThenBy(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(sortDA, Is.EqualTo( new double[] {37.7798657, 40.7637286, 51.508530}) );

            var sortAD = _realm.All<Person>().OrderByDescending(p => p.FirstName).ThenBy(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(sortAD, Is.EqualTo( new double[] {51.508530, 40.7637286, 37.7798657}) );

            var sortDD = _realm.All<Person>().OrderByDescending(p => p.FirstName).ThenByDescending(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(sortDD, Is.EqualTo( new double[] {37.7798657, 51.508530, 40.7637286}) );
        }



        [Test]
        public void QuerySortOneLevelNumbers()
        {
            var s0 = _realm.All<Person>().Where(p => p.IsInteresting).OrderBy(p => p.Score).ToList().Select(p => p.Score);
            Assert.That(s0, Is.EqualTo( new float[] {-0.9907f, 42.42f, 100}) );
        }


        [Test]
        public void QuerySortOneLevelStrings()
        {
            var s0 = _realm.All<Person>().Where(p => p.IsInteresting).OrderBy(p => p.LastName).ToList().Select(p => p.LastName);
            Assert.That(s0, Is.EqualTo( new [] {"Doesmore", "Jameson", "Smith"}) );
        }


        [Test]
        public void QuerySortUpUp()
        {
            var sortAA = _realm.All<Person>().Where(p => p.IsInteresting).
                OrderBy(p => p.FirstName).
                ThenBy(p => p.Latitude).ToList();
            var sortAAname = sortAA.Select(p => p.FirstName);
            Assert.That(sortAAname, Is.EqualTo( new [] {"John", "John", "Peter"}) );
            var sortAAlat = sortAA.Select(p => p.Latitude);
            Assert.That(sortAAlat, Is.EqualTo( new double[] {40.7637286, 51.508530, 37.7798657}) );
        }


        [Test]
        public void QuerySortDownUp()
        {
            var sortDA = _realm.All<Person>().Where(p => p.IsInteresting).
                OrderByDescending(p => p.FirstName).
                ThenBy(p => p.Latitude).ToList();
            var sortDAname = sortDA.Select(p => p.FirstName);
            Assert.That(sortDAname, Is.EqualTo( new [] {"Peter", "John", "John"}) );
            var sortDAlat = sortDA.Select(p => p.Latitude);
            Assert.That(sortDA, Is.EqualTo( new double[] {37.7798657, 40.7637286, 51.508530}) );
        }


        [Test]
        public void QuerySortUpDown()
        {
            var sortAD = _realm.All<Person>().Where(p => p.IsInteresting).
                OrderBy(p => p.FirstName).
                ThenByDescending(p => p.Latitude).ToList();
            var sortADname = sortAD.Select(p => p.FirstName);
            Assert.That(sortADname, Is.EqualTo( new [] {"John", "John", "Peter"}) );
            var sortADlat = sortAD.Select(p => p.Latitude);
            Assert.That(sortADlat, Is.EqualTo( new double[] {51.508530, 40.7637286, 37.7798657}) );
        }


        [Test]
        public void QuerySortDownDown()
        {
            var sortDD = _realm.All<Person>().Where(p => p.IsInteresting).
                OrderByDescending(p => p.FirstName).
                ThenByDescending(p => p.Latitude).ToList();
            var sortDDname = sortDD.Select(p => p.FirstName);
            Assert.That(sortDDname, Is.EqualTo( new [] {"Peter", "John", "John"}) );
            var sortDDlat = sortDD.Select(p => p.Latitude);
            Assert.That(sortDDlat, Is.EqualTo( new double[] {37.7798657, 51.508530, 40.7637286}) );
        }

        //TODO some exception tests for misuse of clauses

    } // SortingTests
}
