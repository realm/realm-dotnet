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
    /// Represents a rectangle for a geospatial geoWithin query.
    /// </summary>
    public class GeoBox : GeoShapeBase
    {
        /// <summary>
        /// Gets the bottom left corner of the rectangle.
        /// </summary>
        /// <value>The box's bottom left corner.</value>
        public GeoPoint BottomLeftCorner { get; }

        /// <summary>
        /// Gets the top right corner of the rectangle.
        /// </summary>
        /// <value>The box's top right corner.</value>
        public GeoPoint TopRightCorner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoBox"/> class from the provided coordinates.
        /// </summary>
        /// <param name="bottomLeftCorner">The bottom left corner of the rectangle.</param>
        /// <param name="topRightCorner">The top right corner of the rectangle.</param>
        public GeoBox(GeoPoint bottomLeftCorner, GeoPoint topRightCorner)
        {
            BottomLeftCorner = bottomLeftCorner;
            TopRightCorner = topRightCorner;
        }

        internal NativeGeoBox ToNative() => new(BottomLeftCorner.ToNative(), TopRightCorner.ToNative());

        /// <inheritdoc />
        public override string ToString() => $"Box: {{ {BottomLeftCorner}, {TopRightCorner} }}";
    }
}
