/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using Realms;

namespace IntegrationTests
{
    public class PeopleTestsBase
    {
        protected Realm _realm;

        // Override this method in your test class instead of adding another <code>[SetUp]</code> because then NUnit will invoke both.
        [SetUp]
        public virtual void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        // Override this method in your test class instead of adding another <code>[TearDown]</code> because then NUnit will invoke both.
        [TearDown]
        public virtual void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }


        protected void MakeThreePeople()
        {
            Person p1, p2, p3;
            using (var transaction = _realm.BeginWrite())
            {
                p1 = _realm.CreateObject<Person>();
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
                p1.Score = -0.9907f;
                p1.Latitude = 51.508530;
                p1.Longitude = 0.076132;
                transaction.Commit();
            }
            Debug.WriteLine("p1 is named " + p1.FullName);

            using (var transaction = _realm.BeginWrite())
            {
                p2 = _realm.CreateObject<Person>();
                p2.FullName = "John Doe"; // uses our setter which splits and maps to First/Lastname
                p2.IsInteresting = false;
                p2.Email = "john@doe.com";
                p2.Score = 100;
                p2.Latitude = 40.7637286;
                p2.Longitude = -73.9748113;
                transaction.Commit();
            }
            Debug.WriteLine("p2 is named " + p2.FullName);

            using (var transaction = _realm.BeginWrite())
            {
                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.com";
                p3.IsInteresting = true;
                p3.Score = 42.42f;
                p3.Latitude = 37.7798657;
                p3.Longitude = -122.394179;
                transaction.Commit();
            }
            Debug.WriteLine("p3 is named " + p3.FullName);
        }
    }
}
