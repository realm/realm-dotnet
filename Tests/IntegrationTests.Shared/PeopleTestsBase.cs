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

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using Realms;

namespace IntegrationTests
{
    [Preserve(AllMembers = true)]
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
                p1.Salary = 30000;
                p1.Score = -0.9907f;
                p1.Latitude = 51.508530;
                p1.Longitude = 0.076132;
                p1.Birthday = new DateTimeOffset(1959, 3, 13, 0, 0, 0, TimeSpan.Zero);
                p1.PublicCertificateBytes = new byte[] { 0xca, 0xfe, 0xba, 0xbe };
                transaction.Commit();
            }
            Debug.WriteLine("p1 is named " + p1.FullName);

            using (var transaction = _realm.BeginWrite())
            {
                p2 = _realm.CreateObject<Person>();
                p2.FullName = "John Doe"; // uses our setter whcih splits and maps to First/Lastname
                p2.IsInteresting = false;
                p2.Email = "john@doe.com";
                p2.Salary = 60000;
                p2.Score = 100;
                p2.Latitude = 40.7637286;
                p2.Longitude = -73.9748113;
                p2.Birthday = new DateTimeOffset(1963, 4, 14, 0, 0, 0, TimeSpan.Zero);
                p2.PublicCertificateBytes = new byte[] { 0xde, 0xad, 0xbe, 0xef };
                transaction.Commit();
            }
            Debug.WriteLine("p2 is named " + p2.FullName);

            using (var transaction = _realm.BeginWrite())
            {
                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.net";
                p3.Salary = 87000;
                p3.IsInteresting = true;
                p3.Score = 42.42f;
                p3.Latitude = 37.7798657;
                p3.Longitude = -122.394179;
                p3.Birthday = new DateTimeOffset(1989, 2, 25, 0, 0, 0, TimeSpan.Zero);
                transaction.Commit();
            }
            Debug.WriteLine("p3 is named " + p3.FullName);
        }
    }
}
