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

namespace Realms.Tests.Database
{
    public abstract class PeopleTestsBase : RealmInstanceTest
    {
        protected void MakeThreePeople()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FirstName = "John",
                    LastName = "Smith",
                    IsInteresting = true,
                    Email = "john@smith.com",
                    Salary = 30000,
                    Score = -0.9907f,
                    Latitude = 51.508530,
                    Longitude = 0.076132,
                    Birthday = new DateTimeOffset(1959, 3, 13, 0, 0, 0, TimeSpan.Zero),
                    PublicCertificateBytes = new byte[] { 0xca, 0xfe, 0xba, 0xbe },
                    OptionalAddress = "12 Cosgrove St.",
                    IsAmbivalent = true
                });
            });

            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FullName = "John Doe", // uses our setter which splits and maps to First/Lastname
                    IsInteresting = false,
                    Email = "john@doe.com",
                    Salary = 60000,
                    Score = 100,
                    Latitude = 40.7637286,
                    Longitude = -73.9748113,
                    Birthday = new DateTimeOffset(1963, 4, 14, 0, 0, 0, TimeSpan.Zero),
                    PublicCertificateBytes = new byte[] { 0xde, 0xad, 0xbe, 0xef },
                    OptionalAddress = string.Empty,
                    IsAmbivalent = false
                });
            });

            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FullName = "Peter Jameson",
                    Email = "peter@jameson.net",
                    Salary = 87000,
                    IsInteresting = true,
                    Score = 42.42f,
                    Latitude = 37.7798657,
                    Longitude = -122.394179,
                    Birthday = new DateTimeOffset(1989, 2, 25, 0, 0, 0, TimeSpan.Zero)
                });
            });
        }
    }
}
