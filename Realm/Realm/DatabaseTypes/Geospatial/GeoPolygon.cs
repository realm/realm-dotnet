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
using System.Runtime.InteropServices;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// A polygon describes a shape comprised of 3 or more line segments for a geospatial <see cref="QueryMethods.GeoWithin"/> query.
    /// </summary>
    /// <remarks>
    /// A polygon comprises of one outer ring and 0 or more rings representing holes with the following restrictions:
    /// <list type="bullet">
    /// <item>
    /// Each ring must consist of at least 3 distinct points (vertices). The first and the last point must be the same to indicate a closed ring
    /// (meaning you need at least 4 points to define the polygon).
    /// </item>
    /// <item>Rings may not cross, i.e. the boundary of a ring may not intersect both the interior and exterior of any other ring.</item>
    /// <item>Rings may not share edges, i.e. if a ring contains an edge AB, then no other ring may contain AB or BA.</item>
    /// <item>Rings may share vertices, however no vertex may appear twice in a single ring.</item>
    /// <item>No ring may be empty.</item>
    /// </list>
    /// <br/>
    /// Holes may be nested inside each other, in which case a location will be considered "inside" the polygon if it is included in an odd number of rings.
    /// For example, a polygon representing a square with side 10 centered at (0,0) with holes representing squares with sides 5 and 2, centered at (0,0) will
    /// include the location (1, 1) because it is contained in 3 rings, but not (3, 3), because it is contained in 2.
    /// </remarks>
    public class GeoPolygon : GeoShapeBase
    {
        private readonly IReadOnlyList<GeoPoint>[] _linearRings;

        /// <summary>
        /// Gets the outer ring of the polygon.
        /// </summary>
        /// <value>The polygon's outer ring.</value>
        public IReadOnlyList<GeoPoint> OuterRing { get; }

        /// <summary>
        /// Gets the holes in the polygon.
        /// </summary>
        /// <value>The holes (if any) in the polygon.</value>
        public IReadOnlyList<IReadOnlyList<GeoPoint>> Holes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPolygon"/> class from a collection of <see cref="GeoPoint">GeoPoints</see>
        /// with no holes.
        /// </summary>
        /// <param name="outerRing">The points representing the outer ring of the polygon.</param>
        /// <remarks>
        /// <paramref name="outerRing"/> must contain at least 3 unique points. The first and the last point may be identical, but
        /// no other duplicates are allowed. Each subsequent pair of points represents an edge in the polygon with the first and the
        /// last points being implicitly connected.
        /// </remarks>
        public GeoPolygon(params GeoPoint[] outerRing)
            : this(new[] { outerRing })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPolygon"/> class with an outer ring and a collection of holes.
        /// </summary>
        /// <param name="outerRing">
        /// A collection of <see cref="GeoPoint">GeoPoints</see> representing the outer ring of the polygon.
        /// </param>
        /// <param name="holes">
        /// A collection of collections of <see cref="GeoPoint">GeoPoints</see> representing the inner rings of the polygon.
        /// </param>
        /// <remarks>
        /// <paramref name="outerRing"/> must contain at least 3 unique points. The first and the last point may be identical, but
        /// no other duplicates are allowed. Each subsequent pair of points represents an edge in the polygon with the first and the
        /// last points being implicitly connected.
        /// <br/>
        /// Each collection in <paramref name="holes"/> must contain at least 3 unique points with the same rules as for <paramref name="outerRing"/>.
        /// <br/>
        /// No two rings may intersect or share an edge, though they may share vertices.
        /// <br/>
        /// A point is considered "inside" the polygon if it is contained by an odd number of rings and "outside" if it's contained
        /// by an even number of rings.
        /// </remarks>
        public GeoPolygon(IEnumerable<GeoPoint> outerRing, params IEnumerable<GeoPoint>[] holes)
            : this(new[] { outerRing.ToList().AsReadOnly() }.Concat(holes.Select(h => h.ToList().AsReadOnly())).ToArray())
        {
        }

        private GeoPolygon(IReadOnlyList<GeoPoint>[] linearRings)
        {
            foreach (var polygon in linearRings)
            {
                Argument.Ensure(polygon.Count > 3, $"Each linear ring (both the outer one and any holes) must have at least 4 points, but {LinearRingToString(polygon)} only had {polygon.Count}.", nameof(linearRings));
                Argument.Ensure(polygon[0] == polygon[polygon.Count - 1], $"The first and the last points of the polygon {LinearRingToString(polygon)} must be the same.", nameof(linearRings));
            }

            _linearRings = linearRings;

            OuterRing = _linearRings[0];
            Holes = new ArraySegment<IReadOnlyList<GeoPoint>>(_linearRings, 1, _linearRings.Length - 1);
        }

        internal unsafe (NativeGeoPolygon NativePolygon, RealmValue.HandlesToCleanup? Handles) ToNative()
        {
            var points = new NativeGeoPoint[_linearRings.Sum(p => p.Count)];
            var pointsLengths = new nint[_linearRings.Length];

            var pointIndex = 0;
            var polygonIndex = 0;
            foreach (var polygon in _linearRings)
            {
                foreach (var point in polygon)
                {
                    points[pointIndex++] = point.ToNative();
                }

                pointsLengths[polygonIndex++] = polygon.Count;
            }

            var pointsHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
            var pointsLengthsHandle = GCHandle.Alloc(pointsLengths, GCHandleType.Pinned);

            return (new()
            {
                Points = (NativeGeoPoint*)pointsHandle.AddrOfPinnedObject(),
                PointsLengths = (nint*)pointsLengthsHandle.AddrOfPinnedObject(),
                PointsLengthsLength = pointsLengths.Length,
            }, new(pointsHandle, handle2: pointsLengthsHandle));
        }

        internal (NativeQueryArgument QueryArgument, RealmValue.HandlesToCleanup? Handles) ToNativeQueryArgument()
        {
            var (polygon, handles) = ToNative();
            return (NativeQueryArgument.GeoPolygon(polygon), handles);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Polygon: {LinearRingToString(OuterRing)}"
            + (Holes.Count == 0 ? string.Empty : $", Holes: [ {string.Join(", ", Holes.Select(LinearRingToString))} ]");

        internal static string LinearRingToString(IEnumerable<GeoPoint> points) => $"{{ {string.Join(",", points)} }}";
    }
}
