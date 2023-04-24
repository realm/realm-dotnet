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
        [Test]
        public void Geospatial_SupportsQuerying()
        {
            PopulateCompanies();

            var matches = _realm.All<Company>().Filter("Location geoWithin geoSphere([74.0060, 40.7128], 0.01)");
            Assert.That(matches.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Geospatial_SupportsQuerying_WithArgs()
        {
            PopulateCompanies();

            var sphere = new GeoSphere(new(40.7128, 74.0060), 0.01);
            var matches = _realm.All<Company>().Filter("Location geoWithin $0", sphere);
            Assert.That(matches.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Geospatial_SupportsLINQ()
        {
            PopulateCompanies();

            var sphere = new GeoSphere(new(40.7128, 74.0060), 0.01);
            var matches = _realm.All<Company>().Where(c => c.Location.GeoWithin(sphere));
            Assert.That(matches.Count(), Is.EqualTo(1));
        }

        private void PopulateCompanies()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Company
                {
                    Name = "MongoDB",
                    Location = new(40.7128, 74.0060)
                });

                _realm.Add(new Company
                {
                    Name = "Realm",
                    Location = new(55.6761, 12.5683)
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
