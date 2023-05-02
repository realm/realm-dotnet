////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

#if TEST_WEAVER
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public partial class GeospatialTests : RealmInstanceTest
    {
        public static object[] GeospatialTestCases =
        {
            new object[] { new GeoSphere(new(55.67, 12.56), 0.001), new[] { "Realm" } },
            new object[] { new GeoSphere(new(55.67, 12.56), Distance.FromKilometers(10)), new[] { "Realm" } },
            new object[] { new GeoSphere(new(55.67, 12.56), Distance.FromKilometers(100)), new[] { "Realm", "Ragnarock" } },
            new object[] { new GeoSphere(new(45, -20), Distance.FromKilometers(5000)), new[] { "Realm", "Ragnarock", "MongoDB" } },
            new object[] { new GeoBox(new(55.6281, 12.0826), new(55.6761, 12.5683)), new[] { "Realm" } },
            new object[] { new GeoBox(new(55.6280, 12.0826), new(55.6761, 12.5683)), new[] { "Realm", "Ragnarock" } },
            new object[] { new GeoBox(new(0, -75), new(60, 15)), new[] { "Realm", "Ragnarock", "MongoDB" } },
        };

        [TestCaseSource(nameof(GeospatialTestCases))]
        public void FilterTests(GeoShapeBase shape, string[] expectedMatches)
        {
            PopulateCompanies();

            var matches = _realm.All<Company>().Filter("Location geoWithin $0", shape);
            Assert.That(matches.ToArray().Select(m => m.Name), Is.EquivalentTo(expectedMatches));
        }

        [TestCaseSource(nameof(GeospatialTestCases))]
        public void LinqTests(GeoShapeBase shape, string[] expectedMatches)
        {
            PopulateCompanies();

            var matches = _realm.All<Company>().Where(c => c.Location.GeoWithin(shape));
            Assert.That(matches.ToArray().Select(m => m.Name), Is.EquivalentTo(expectedMatches));
        }

        private void PopulateCompanies()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Company
                {
                    Name = "MongoDB",
                    Location = new(40.7128, -74.0060)
                });

                _realm.Add(new Company
                {
                    Name = "Realm",
                    Location = new(55.6761, 12.5683)
                });

                _realm.Add(new Company
                {
                    Name = "Ragnarock",
                    Location = new(55.6280, 12.0826)
                });
            });
        }

        public partial class Company : TestRealmObject
        {
            [MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public string Name { get; set; } = null!;

            public CustomGeoPoint? Location { get; set; }
        }

        public partial class CustomGeoPoint : TestEmbeddedObject
        {
            [MapTo("coordinates")]
            private IList<double> Coordinates { get; } = null!;

            [MapTo("type")]
            private string Type { get; set; } = "Point";

            public double Latitude => Coordinates.Count > 1 ? Coordinates[1] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

            public double Longitude => Coordinates.Count > 1 ? Coordinates[0] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

            public CustomGeoPoint(double latitude, double longitude)
            {
                Coordinates.Add(longitude);
                Coordinates.Add(latitude);
            }

#if TEST_WEAVER
            private CustomGeoPoint()
            {
            }
#endif
        }
    }
}
