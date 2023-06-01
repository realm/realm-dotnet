﻿////////////////////////////////////////////////////////////////////////////
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
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// Represents equatorial distance.
    /// </summary>
    public readonly struct Distance
    {
        private const double EarthRadiusMeters = 6378100.0;
        private const double MetersPerMile = 1609.344;
        private const double RadiansPerDegree = Math.PI / 180;

        /// <summary>
        /// Gets the distance in radians.
        /// </summary>
        /// <value>The distance in radians.</value>
        public double Radians { get; }

        private Distance(double radians)
        {
            Radians = radians;
        }

        /// <summary>
        /// Constructs a <see cref="Distance"/> from a kilometer value.
        /// </summary>
        /// <param name="kilometers">The distance in kilometers.</param>
        /// <returns>A <see cref="Distance"/> value that represents the provided distance in radians.</returns>
        public static Distance FromKilometers(double kilometers)
        {
            Argument.Ensure(kilometers >= 0, $"Cannot construct a negative distance: {kilometers} < 0", nameof(kilometers));

            return new(kilometers * 1000 / EarthRadiusMeters);
        }

        /// <summary>
        /// Constructs a <see cref="Distance"/> from a miles value.
        /// </summary>
        /// <param name="miles">The distance in miles.</param>
        /// <returns>A <see cref="Distance"/> value that represents the provided distance in radians.</returns>
        public static Distance FromMiles(double miles)
        {
            Argument.Ensure(miles >= 0, $"Cannot construct a negative distance: {miles} < 0", nameof(miles));

            return new(miles * MetersPerMile / EarthRadiusMeters);
        }

        /// <summary>
        /// Constructs a <see cref="Distance"/> from a radians value.
        /// </summary>
        /// <param name="radians">The distance in radians.</param>
        /// <returns>A <see cref="Distance"/> value that represents the provided distance in radians.</returns>
        public static Distance FromRadians(double radians)
        {
            Argument.Ensure(radians >= 0, $"Cannot construct a negative radians: {radians} < 0", nameof(radians));

            return new(radians);
        }

        /// <summary>
        /// Constructs a <see cref="Distance"/> from a degrees value.
        /// </summary>
        /// <param name="degrees">The distance in degrees.</param>
        /// <returns>A <see cref="Distance"/> value that represents the provided distance in radians.</returns>
        public static Distance FromDegrees(double degrees) => new(degrees * RadiansPerDegree);
    }
}
