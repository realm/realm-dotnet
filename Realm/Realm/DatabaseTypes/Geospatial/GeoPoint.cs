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
using System.Diagnostics.CodeAnalysis;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// Represents a point geometry.
    /// </summary>
    /// <remarks>
    /// This type cannot be used for persistence - i.e. you can't declare a Realm property
    /// that is of type <see cref="GeoPoint"/>. It is only used as a building block for the
    /// geospatial shape types, such as <see cref="GeoBox"/>, <see cref="GeoCircle"/>, and
    /// <see cref="GeoPolygon"/>.
    /// </remarks>
    public readonly struct GeoPoint : IEquatable<GeoPoint>
    {
        /// <summary>
        /// Gets the latitude of the point.
        /// </summary>
        /// <value>The point's latitude.</value>
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
            Argument.EnsureRange(latitude, -90, 90, nameof(latitude));
            Argument.EnsureRange(longitude, -180, 180, nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        internal NativeGeoPoint ToNative() => new(Latitude, Longitude);

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString() => $"[{Latitude}, {Longitude}]";

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is GeoPoint geoPoint)
            {
                return Equals(geoPoint);
            }

            return false;
        }

        /// <summary>
        /// Indicates whether the current <see cref="GeoPoint"/> is equal to another
        /// <see cref="GeoPoint"/>.</summary>
        /// <param name="other">An object to compare with this <see cref="GeoPoint"/>.</param>
        /// <returns>
        /// <see langword="true" />if the current point's latitude and longitude are
        /// equal to the <paramref name="other" /> point's latitude and longitude;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(GeoPoint other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = -1416534245;
            hashCode = (hashCode * -1521134295) + Latitude.GetHashCode();
            hashCode = (hashCode * -1521134295) + Longitude.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="GeoPoint"/> instances for equality.
        /// </summary>
        /// <param name="left">The first <see cref="GeoPoint"/>.</param>
        /// <param name="right">The second <see cref="GeoPoint"/>.</param>
        /// <returns>
        /// <c>true</c> if both points contain the same latitude and longitude; <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(GeoPoint left, GeoPoint right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="GeoPoint"/> instances for inequality.
        /// </summary>
        /// <param name="left">The first <see cref="GeoPoint"/>.</param>
        /// <param name="right">The second <see cref="GeoPoint"/>.</param>
        /// <returns>
        /// <c>true</c> if the points contain different latitude and longitude; <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(GeoPoint left, GeoPoint right) => !(left == right);

        /// <summary>
        /// Converts a tuple containing latitude and longitude to <see cref="GeoPoint"/>.
        /// </summary>
        /// <param name="tuple">The tuple consisting of two coordinates.</param>
        /// <returns>
        /// A <see cref="GeoPoint"/> with latitude equal to the first element of the tuple and longitude equal
        /// to the second element.
        /// </returns>
        public static implicit operator GeoPoint((double Latitude, double Longitude) tuple) => new(tuple.Latitude, tuple.Longitude);
    }
}
