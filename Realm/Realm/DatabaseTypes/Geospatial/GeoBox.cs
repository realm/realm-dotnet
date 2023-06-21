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

using Realms.Native;

namespace Realms
{
    /// <summary>
    /// Represents a rectangle for a geospatial <see cref="QueryMethods.GeoWithin"/> query.
    /// </summary>
    public class GeoBox : GeoShapeBase
    {
        /// <summary>
        /// Gets the longitude of the left edge of the rectangle.
        /// </summary>
        /// <value>The box's left edge.</value>
        public double Left { get; }

        /// <summary>
        /// Gets the latitude of the top edge of the rectangle.
        /// </summary>
        /// <value>The box's top edge.</value>
        public double Top { get; }

        /// <summary>
        /// Gets the longitude of the right edge of the rectangle.
        /// </summary>
        /// <value>The box's right edge.</value>
        public double Right { get; }

        /// <summary>
        /// Gets the latitude of the bottom edge of the rectangle.
        /// </summary>
        /// <value>The box's bottom edge.</value>
        public double Bottom { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoBox"/> class from the provided coordinates.
        /// </summary>
        /// <param name="bottomLeftCorner">The bottom left corner of the rectangle.</param>
        /// <param name="topRightCorner">The top right corner of the rectangle.</param>
        public GeoBox(GeoPoint bottomLeftCorner, GeoPoint topRightCorner)
        {
            Left = bottomLeftCorner.Longitude;
            Bottom = bottomLeftCorner.Latitude;
            Right = topRightCorner.Longitude;
            Top = topRightCorner.Latitude;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoBox"/> class from the provided coordinates.
        /// </summary>
        /// <param name="left">The longitude of the left edge of the rectangle.</param>
        /// <param name="top">The latitude of the top edge of the rectangle.</param>
        /// <param name="right">The longitude of the right edge of the rectangle.</param>
        /// <param name="bottom">The latitude of the bottom edge of the rectangle.</param>
        public GeoBox(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        internal NativeGeoBox ToNative() => new(Left, Top, Right, Bottom);

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString() => $"Box: {{ left: {Left}, top: {Top}, right: {Right}, bottom: {Bottom} }}";
    }
}
