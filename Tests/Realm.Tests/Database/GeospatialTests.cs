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
using Realms.Exceptions;

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
            new object[] { new GeoCircle((55.67, 12.56), 0.001), new[] { "Realm" } },
            new object[] { new GeoCircle((55.67, 12.56), Distance.FromKilometers(10)), new[] { "Realm" } },
            new object[] { new GeoCircle((55.67, 12.56), Distance.FromKilometers(100)), new[] { "Realm", "Ragnarock" } },
            new object[] { new GeoCircle((45, -20), Distance.FromKilometers(5000)), new[] { "Realm", "Ragnarock", "MongoDB" } },
            new object[] { new GeoBox((55.6281, 12.0826), (55.6761, 12.5683)), new[] { "Realm" } },
            new object[] { new GeoBox((55.6280, 12.0826), (55.6761, 12.5683)), new[] { "Realm", "Ragnarock" } },
            new object[] { new GeoBox((0, -75), (60, 15)), new[] { "Realm", "Ragnarock", "MongoDB" } },
            new object[] { new GeoPolygon(new GeoPoint(55.6281, 12.0826), (55.6761, 12.0826), (55.6761, 12.5684), (55.6281, 12.5684), (55.6281, 12.0826)), new[] { "Realm" } },
            new object[] { new GeoPolygon(new GeoPoint(55, 12), (55.67, 12.5), (55.67, 11.5), (55, 12)), new[] { "Ragnarock" } },
            new object[] { new GeoPolygon(new GeoPoint(40.0096192, -75.5175781), (60, 20), (20, 20), (40.0096192, -75.5175781)), new[] { "MongoDB", "Realm", "Ragnarock" } },
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

            var matches = _realm.All<Company>().Where(c => QueryMethods.GeoWithin(c.Location, shape));
            Assert.That(matches.ToArray().Select(m => m.Name), Is.EquivalentTo(expectedMatches));
        }

        [Test]
        public void Filter_InvalidPropertyType_Throws()
        {
            PopulateCompanies();
            _realm.Write(() =>
            {
                var company = _realm.All<Company>().First();

                // We're making the point into a polygon, which is not supported
                company.Location!.DynamicApi.Set("type", "Polygon");

                _realm.Add(new ObjectWithInvalidGeoPoints
                {
                    CoordinatesEmbedded = new()
                    {
                        Coordinates = { 1, 2 }
                    },
                    TypeEmbedded = new(),
                    TopLevelGeoPoint = new()
                    {
                        Coordinates = { 1, 2 },
                    }
                });
            });

            AssertInvalidGeoData<Company>(nameof(Company.Location), expectedError: "The only Geospatial type currently supported is 'point'");

            // TODO: this is currently supported, but maybe it shouldn't?
            // AssertInvalidGeoData<ObjectWithInvalidGeoPoints>(nameof(ObjectWithInvalidGeoPoints.TopLevelGeoPoint));
            AssertInvalidGeoData<ObjectWithInvalidGeoPoints>(nameof(ObjectWithInvalidGeoPoints.TypeEmbedded));
            AssertInvalidGeoData<ObjectWithInvalidGeoPoints>(nameof(ObjectWithInvalidGeoPoints.CoordinatesEmbedded));

            void AssertInvalidGeoData<T>(string property, string expectedError = "wrong format")
                where T : IRealmObject
            {
                var shape = new GeoCircle((0, 0), 10);

                var ex = Assert.Throws<RealmException>(() => _realm.All<T>().Filter($"{property} geowithin $0", shape).ToArray(), $"Expected an error when querying {typeof(T).Name}.{property}")!;
                Assert.That(ex.Message, Does.Contain(expectedError));
            }
        }

        public static object[] GeoPointTests =
        {
            new object?[] { 0, 0, null },
            new object?[] { 90.000000001, 0, nameof(GeoPoint.Latitude) },
            new object?[] { -90.000000001, 0, nameof(GeoPoint.Latitude) },
            new object?[] { 9999999, 0, nameof(GeoPoint.Latitude) },
            new object?[] { -9999999, 0, nameof(GeoPoint.Latitude) },
            new object?[] { 90, 0, null },
            new object?[] { -90, 0, null },
            new object?[] { 12.3456789, 0, null },
            new object?[] { 0, 180.000000001, nameof(GeoPoint.Longitude) },
            new object?[] { 0, -180.000000001, nameof(GeoPoint.Longitude) },
            new object?[] { 0, 9999999, nameof(GeoPoint.Longitude) },
            new object?[] { 0, -9999999, nameof(GeoPoint.Longitude) },
            new object?[] { 0, 180, null },
            new object?[] { 0, -180, null },
            new object?[] { 0, 12.3456789, null },
        };

        [TestCaseSource(nameof(GeoPointTests))]
        public void GeoPoint_ArgumentValidation(double latitude, double longitude, string? expectedParamError)
        {
            if (expectedParamError != null)
            {
                var ex = Assert.Throws<ArgumentException>(() => new GeoPoint(latitude, longitude))!;
                Assert.That(ex.ParamName, Is.EqualTo(expectedParamError.ToLower()));
            }
            else
            {
                Assert.DoesNotThrow(() => new GeoPoint(latitude, longitude));
            }
        }

        public static object[] GeoPolygonTests =
        {
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0) }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (0, 0) }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2) }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (3, 3) }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (0, 0) }, new[] { new GeoPoint[] { (0, 0) } }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (0, 0) }, new[] { new GeoPoint[] { (0, 0), (1, 1), (0, 0) } }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (0, 0) }, new[] { new GeoPoint[] { (0, 0), (1, 1), (2, 2) } }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (0, 0) }, new[] { new GeoPoint[] { (0, 0), (1, 1), (2, 2), (3, 3) } }) },
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (1, 1), (2, 2), (0, 0) }, new[] { new GeoPoint[] { (0, 0), (1, 1), (2, 2), (3, 3) }, new GeoPoint[] { (0, 0) } }) },
        };

        [TestCaseSource(nameof(GeoPolygonTests))]
        public void GeoPolygon_ArgumentValidation(GeoPolygonValidationData testData)
        {
            Assert.Throws<ArgumentException>(() => testData.CreatePolygon());
        }

        public static object[] GeoPolygonQueryTests =
        {
            // Square (0, 0), (1, 1) and a square (2, 2), (3, 3) - outer ring doesn't contain hole
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (0, 1), (1, 1), (1, 0), (0, 0) }, new[] { new GeoPoint[] { (2, 2), (2, 3), (3, 3), (3, 2), (2, 2) } }) },

            // Square (0, 0), (1, 1) and a square (0, 0.1), (0.5, 0.5) - they share an edge (0, 0.1 - 0, 0.5)
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (0, 1), (1, 1), (1, 0), (0, 0) }, new[] { new GeoPoint[] { (0, 0.1), (0.5, 0.1), (0.5, 0.5), (0, 0.5), (0, 0.1) } }) },

            // Square (0, 0), (1, 1) and a square (0.25, 0.5), (0.75, 1.5) - they intersect
            new object?[] { new GeoPolygonValidationData(new GeoPoint[] { (0, 0), (0, 1), (1, 1), (1, 0), (0, 0) }, new[] { new GeoPoint[] { (0.25, 0.5), (0.75, 0.5), (0.75, 1.5), (0.25, 1.5), (0.25, 0.5) } }) },
        };

        [TestCaseSource(nameof(GeoPolygonQueryTests))]
        public void GeoPolygon_QueryArgumentValidation(GeoPolygonValidationData testData)
        {
            // These polygons are invalid, but not validated by the SDK. They'll only show
            // up as errors when we use them in a query
            var polygon = testData.CreatePolygon();
            Assert.Throws<ArgumentException>(() => _realm.All<Company>().Where(c => QueryMethods.GeoWithin(c.Location, polygon)).ToArray());
        }

        public static object[] GeospatialCollectionTestCases =
        {
            new object[] { new GeoPoint(1, 1), new[] { "1,2" } },
            new object[] { new GeoPoint(2, 2), new[] { "1,2", "2,3" } },
            new object[] { new GeoPoint(3, 3), new[] { "2,3" } },
        };

        [TestCaseSource(nameof(GeospatialCollectionTestCases))]
        public void Filter_ListOfPoints(GeoPoint point, string[] expectedMatches)
        {
            _realm.Write(() =>
            {
                _realm.Add(new Company
                {
                    Name = "1,2",
                    Offices =
                    {
                        new(1, 1),
                        new(2, 2),
                    }
                });

                _realm.Add(new Company
                {
                    Name = "2,3",
                    Offices =
                    {
                        new(2, 2),
                        new(3, 3)
                    }
                });
            });

            var circle = new GeoCircle(point, Distance.FromDegrees(0.5));
            var query = _realm.All<Company>().Filter("ANY Offices GEOWITHIN $0", circle);
            Assert.That(query.ToArray().Select(c => c.Name), Is.EquivalentTo(expectedMatches));
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

                _realm.Add(new Company
                {
                    Name = "Internet company",
                    Location = null
                });
            });
        }

        public partial class Company : TestRealmObject
        {
            [MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public string Name { get; set; } = null!;

            public CustomGeoPoint? Location { get; set; }

            public IList<CustomGeoPoint> Offices { get; } = null!;

            public BsonDocument ToBsonDocument()
            {
                return new BsonDocument
                {
                    ["Name"] = Name,
                    ["Location"] = Location?.ToBsonDocument() ?? new BsonDocument(),
                    ["Offices"] = new BsonArray(Offices.Select(o => o.ToBsonDocument()))
                };
            }
        }

        public partial class CustomGeoPoint : TestEmbeddedObject
        {
            [MapTo("coordinates")]
            private IList<double> Coordinates { get; } = null!;

            [MapTo("type")]
            public string Type { get; set; } = "Point";

            public double Latitude => Coordinates.Count > 1 ? Coordinates[1] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

            public double Longitude => Coordinates.Count > 1 ? Coordinates[0] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");

            public double? Altitude => Coordinates.Count > 2 ? Coordinates[2] : null;

            public CustomGeoPoint(double latitude, double longitude, double? altitude = null)
            {
                Coordinates.Add(longitude);
                Coordinates.Add(latitude);

                if (altitude != null)
                {
                    Coordinates.Add(altitude.Value);
                }
            }

#if TEST_WEAVER
            private CustomGeoPoint()
            {
            }
#endif

            public BsonDocument ToBsonDocument()
            {
                return new BsonDocument
                {
                    ["type"] = Type,
                    ["coordinates"] = new BsonArray(Coordinates)
                };
            }
        }

        public partial class ObjectWithInvalidGeoPoints : TestRealmObject
        {
            public CoordinatesEmbeddedObject? CoordinatesEmbedded { get; set; }

            public TypeEmbeddedObject? TypeEmbedded { get; set; }

            public TopLevelGeoPoint? TopLevelGeoPoint { get; set; }
        }

        public partial class CoordinatesEmbeddedObject : TestEmbeddedObject
        {
            [MapTo("coordinate")]
            public IList<double> Coordinates { get; } = null!;
        }

        public partial class TypeEmbeddedObject : TestEmbeddedObject
        {
            [MapTo("type")]
            public string Type { get; set; } = "Point";
        }

        public partial class TopLevelGeoPoint : TestRealmObject
        {
            [MapTo("coordinates")]
            public IList<double> Coordinates { get; } = null!;

            [MapTo("type")]
            public string Type { get; set; } = "Point";
        }

        // This class is only needed in order to override .ToString, otherwise NUnit can't differentiate between tests
        // that have the same number of elements in the holes array.
        public class GeoPolygonValidationData
        {
            public GeoPoint[] OuterRing { get; }

            public GeoPoint[][] Holes { get; }

            public GeoPolygonValidationData(GeoPoint[] outerRing, GeoPoint[][]? holes = null)
            {
                OuterRing = outerRing;
                Holes = holes ?? Array.Empty<GeoPoint[]>();
            }

            public GeoPolygon CreatePolygon() => new GeoPolygon(OuterRing, Holes);

            public override string ToString()
            {
                return $"{GeoPolygon.LinearRingToString(OuterRing)}"
                    + (Holes.Length == 0 ? string.Empty : $", [ {string.Join(", ", Holes.Select(GeoPolygon.LinearRingToString))} ]");
            }
        }
    }
}
