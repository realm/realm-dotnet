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
    public class GeoPolygon : GeoShapeBase
    {
        private readonly IReadOnlyList<GeoPoint>[] _linearRings;

        public IReadOnlyList<GeoPoint> OuterRing { get; }

        public IReadOnlyList<IReadOnlyList<GeoPoint>> Holes { get; }

        public GeoPolygon(params GeoPoint[] outerRing)
            : this(new[] { outerRing })
        {
        }

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

        private static string LinearRingToString(IEnumerable<GeoPoint> points) => $"{{ {string.Join(",", points)} }}";
    }
}
