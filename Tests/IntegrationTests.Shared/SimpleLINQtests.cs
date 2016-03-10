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
    class SimpleLINQtests : PeopleTestsBase
    {
        // see comment on base method why this isn't decorated with [SetUp]
        public override void Setup()
        {
            base.Setup();
            MakeThreePeople();
        }

        [Test]
        public void CreateList()
        {
            var s0 = _realm.All<Person>().Where(p => p.Score == 42.42f).ToList();
            Assert.That(s0.Count(), Is.EqualTo(1));
            Assert.That(s0[0].Score, Is.EqualTo(42.42f));


            var s1 = _realm.All<Person>().Where(p => p.Longitude < -70.0 && p.Longitude > -90.0).ToList();
            Assert.That(s1[0].Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Where(p => p.Longitude < 0).ToList();
            Assert.That(s2.Count(), Is.EqualTo(2));
            Assert.That(s2[0].Email, Is.EqualTo("john@doe.com"));
            Assert.That(s2[1].Email, Is.EqualTo("peter@jameson.com"));

            var s3 = _realm.All<Person>().Where(p => p.Email != "");
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

            var countNotComplex = _realm.All<Person>().Where( p => !(p.Longitude < -70.0 && p.Longitude > -90.0)).Count();
            Assert.That(countNotComplex, Is.EqualTo(2));
        }


        [Test]
        public void CountFoundItems()
        {
            var c0 = _realm.All<Person>().Where(p => p.Score == 42.42f).Count();
            Assert.That(c0, Is.EqualTo(1));

            var c1 = _realm.All<Person>().Where(p => p.Latitude <= 50).Count();
            Assert.That(c1, Is.EqualTo(2));

            var c2 = _realm.All<Person>().Where(p => p.IsInteresting).Count();
            Assert.That(c2, Is.EqualTo(2));

            var c3 = _realm.All<Person>().Where(p => p.FirstName=="John").Count();
            Assert.That(c3, Is.EqualTo(2));

            var c4 = _realm.All<Person>().Count(p => p.FirstName=="John");
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
        public void AnySucceeds()
        {
            Assert.That( _realm.All<Person>().Where(p => p.Latitude > 50).Any());
            Assert.That( _realm.All<Person>().Where(p => p.Score > 0).Any());
            Assert.That( _realm.All<Person>().Where(p => p.IsInteresting == false).Any());
            Assert.That( _realm.All<Person>().Where(p => p.FirstName == "John").Any());
        }


        [Test]
        public void AnyFails()
        {
            Assert.False( _realm.All<Person>().Where(p => p.Latitude > 100).Any());
            Assert.False( _realm.All<Person>().Where(p => p.Score > 50000).Any());
            Assert.False( _realm.All<Person>().Where(p => p.FirstName == "Samantha").Any());
        }


        [Test]
        public void SingleFailsToFind()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Latitude > 100) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Latitude > 100) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Score > 50000) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.FirstName == "Samantha") );
        }


        [Test]
        public void SingleFindsTooMany()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Latitude == 50) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.Score != 100.0f) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().Single(p => p.FirstName == "John") );
        }


        [Test]
        public void SingleWorks()
        {
            var s0 = _realm.All<Person>().Single(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().Single(p => p.Score == 100.0f);
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().Single(p => p.FirstName == "Peter");
            Assert.That(s2.FirstName, Is.EqualTo("Peter"));
        }


        [Test]
        public void FirstFailsToFind()
        {
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.Latitude > 100) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.Latitude > 100) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.Score > 50000) );
            Assert.Throws<InvalidOperationException>(() => _realm.All<Person>().First(p => p.FirstName == "Samantha") );
        }

        [Test]
        public void FirstWorks()
        {
            var s0 = _realm.All<Person>().First(p => p.Longitude < -70.0 && p.Longitude > -90.0);
            Assert.That(s0.Email, Is.EqualTo("john@doe.com"));

            var s1 = _realm.All<Person>().First(p => p.Score == 100.0f);
            Assert.That(s1.Email, Is.EqualTo("john@doe.com"));

            var s2 = _realm.All<Person>().First(p => p.FirstName == "John");
            Assert.That(s2.FirstName, Is.EqualTo("John"));
        }

    } // SimpleLINQtests
}
