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
using System.Linq;
using System.Text.RegularExpressions;

namespace Realms
{
    /// <summary>
    /// Provides methods that get translated into native Realm queries when using LINQ.
    /// </summary>
    /// <remarks>
    /// These methods don't have implementation and are only usable in a LINQ query
    /// performed on a Realm query result - e.g. the IQueryable returned by <see cref="Realm.All{T}"/>.
    /// </remarks>
    public static class QueryMethods
    {
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how this string and value are compared.</param>
        /// <returns><c>true</c> if the value parameter occurs within this string, or if value is the empty string (""); otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>str</c> or <c>value</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <c>comparisonType</c> is not a valid <see cref="StringComparison"/> value.</exception>
        public static bool Contains(string? str, string value, StringComparison comparisonType)
        {
            throw new NotSupportedException("This method can only be used in queries and cannot be invoked directly on strings.");
        }

        /// <summary>
        /// Performs a 'like' comparison between the specified string and pattern.
        /// </summary>
        /// <remarks>
        /// <c>?</c> and <c>*</c> are allowed where <c>?</c> matches a single character and <c>*</c> matches zero or
        /// more characters, such that <c>?bc*</c> matches <c>abcde</c> and <c>bbc</c>, but does not match <c>bcd</c>.
        /// <para/>
        /// This extension method can be used in LINQ queries against the <see cref="IQueryable"/> returned from
        /// <see cref="Realm.All{T}"/>. If used outside of a query context, it will use a <see cref="Regex"/> to perform
        /// the comparison using the same rules.
        /// </remarks>
        /// <param name="str">The string to compare against the pattern.</param>
        /// <param name="pattern">The pattern to compare against.</param>
        /// <param name="caseSensitive">If set to <c>true</c> performs a case sensitive comparison.</param>
        /// <returns><c>true</c>  if the string matches the pattern, <c>false</c> otherwise.</returns>
        public static bool Like(string? str, string pattern, bool caseSensitive = true)
        {
            throw new NotSupportedException("This method can only be used in queries and cannot be invoked directly on strings.");
        }

        /// <summary>
        /// Performs a 'simple term' Full-Text search on a string property.
        /// </summary>
        /// <param name="str">The string to compare against the terms.</param>
        /// <param name="terms">The terms to look for in <paramref name="str"/>.</param>
        /// <returns><c>true</c> if the string matches the terms; <c>false</c> otherwise.</returns>
        /// <example>
        /// <code>
        /// var matches = realm.All&lt;Book&gt;().Where(b => b.Summary.FullTextSearch("fantasy novel"));
        /// </code>
        /// </example>
        /// <remarks>
        /// When this method is used outside of a Realm query, a <see cref="NotSupportedException"/> will be thrown.
        /// </remarks>
        public static bool FullTextSearch(string? str, string terms)
        {
            throw new NotSupportedException("This method can only be used in queries and cannot be invoked directly on strings.");
        }

        /// <summary>
        /// Checks whether <paramref name="geoShape"/> contains the point represented by an embedded object.
        /// </summary>
        /// <param name="point">
        /// An embedded object that has the shape of a GeoJson point. It must have at least two fields - <c>IList&lt;double&gt; coordinates</c>
        /// and <c>string type</c>. The shape of the object will be verified at runtime.
        /// </param>
        /// <param name="geoShape">
        /// One of <see cref="GeoBox"/>, <see cref="GeoCircle"/>, or <see cref="GeoPolygon"/> representing the shape that <paramref name="point"/>
        /// will be checked against.
        /// </param>
        /// <returns><c>true</c> if <paramref name="point"/> is contained in <paramref name="geoShape"/>; <c>false</c> otherwise.</returns>
        /// <example>
        /// An example embedded object that can be used in geospatial queries is:
        /// <code>
        /// public partial class MyGeoPoint : IEmbeddedObject
        /// {
        ///     [MapTo("coordinates")]
        ///     private IList&lt;double&gt; Coordinates { get; } = null!;
        ///
        ///     [MapTo("type")]
        ///     private string Type { get; set; } = "Point";
        ///
        ///     public double Latitude => Coordinates.Count > 1 ? Coordinates[1] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");
        ///
        ///     public double Longitude => Coordinates.Count > 1 ? Coordinates[0] : throw new Exception($"Invalid coordinate array. Expected at least 2 elements, but got: {Coordinates.Count}");
        ///
        ///     public MyGeoPoint(double latitude, double longitude)
        ///     {
        ///         Coordinates.Add(longitude);
        ///         Coordinates.Add(latitude);
        ///     }
        /// }
        /// </code>
        ///
        /// Note that if you're using Sync, the name of the embedded object type must match exactly the <c>title</c> of
        /// the embedded object defined in the GeoJson Schema on the server.
        /// </example>
        public static bool GeoWithin(IEmbeddedObject? point, GeoShapeBase geoShape)
        {
            throw new NotSupportedException("This method can only be used in queries and should not be used directly.");
        }
    }
}
