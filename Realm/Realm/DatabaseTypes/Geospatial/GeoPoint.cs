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
    /// Represents a point geometry.
    /// </summary>
    public readonly struct GeoPoint
    {
        /// <summary>
        /// Gets the latitude of the point.
        /// </summary>
        /// <value>The point's latutide.</value>
        public double Latitude { get; }

        /// <summary>
        /// Gets the longitude of the point.
        /// </summary>
        /// <value>The point's longitude.</value>
        public double Longitude { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPoint"/> struct with the provided coordinates.
        /// </summary>
        /// <param name="latitude">The latitude of the point.</param>
        /// <param name="longitude">The longitude of the point.</param>
        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        internal NativeGeoPoint ToNative() => new(Latitude, Longitude);
    }
}
