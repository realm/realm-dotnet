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

using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// Represents a circle on the surface of a sphere for a geospatial <see cref="QueryMethods.GeoWithin"/> query.
    /// </summary>
    public class GeoCircle : GeoShapeBase
    {
        /// <summary>
        /// Gets the center of the circle.
        /// </summary>
        /// <value>The circle's center.</value>
        public GeoPoint Center { get; }

        /// <summary>
        /// Gets the radius of the circle in radians.
        /// </summary>
        /// <value>The circle's radius.</value>
        public double Radius { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCircle"/> class.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radiusInRadians">The radius of the circle in radians.</param>
        public GeoCircle(GeoPoint center, double radiusInRadians)
        {
            Argument.Ensure(radiusInRadians >= 0, $"Cannot construct a circle with a negative radius: {radiusInRadians}", nameof(radiusInRadians));

            Center = center;
            Radius = radiusInRadians;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCircle"/> class.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public GeoCircle(GeoPoint center, Distance radius)
        {
            Center = center;
            Radius = radius.Radians;
        }

        internal NativeGeoCircle ToNative() => new(Center.ToNative(), Radius);

        /// <inheritdoc/>
        public override string ToString() => $"Circle: {{ center: {Center}, radius: {Radius} }}";
    }
}
