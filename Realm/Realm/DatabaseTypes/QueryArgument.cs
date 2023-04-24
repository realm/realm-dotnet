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
using MongoDB.Bson;
using Realms.Exceptions;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// A type that can represent any valid query argument type. It is typically used when filtering
    /// a Realm collection using the string-based query language - e.g. in
    /// <see cref="CollectionExtensions.Filter{T}(IQueryable{T}, string, QueryArgument[])"/>.
    /// </summary>
    public readonly struct QueryArgument
    {
        internal readonly RealmValue? RealmValue;
        internal readonly GeoShapeBase? GeoValue;

        private QueryArgument(RealmValue? realmValue = null, GeoShapeBase? geoValue = null)
        {
            RealmValue = realmValue;
            GeoValue = geoValue;
        }

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="char"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(char value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(byte value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="short"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(short value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(int value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="long"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(long value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(float value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(double value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(bool value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(DateTimeOffset value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(decimal value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(Decimal128 value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="ObjectId"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(ObjectId value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(Guid value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="char">char?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(char? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="byte">byte?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(byte? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="short">short?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(short? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="int">int?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(int? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="long">long?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(long? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="float">float?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(float? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="double">double?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(double? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="bool">bool?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(bool? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="DateTimeOffset">DateTimeOffset?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(DateTimeOffset? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="decimal">decimal?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(decimal? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="Decimal128">Decimal128?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(Decimal128? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="ObjectId">ObjectId?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(ObjectId? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="Guid">Guid?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(Guid? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<byte> value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<short> value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<int> value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<long> value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<byte>? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<short>? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<int>? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmInteger<long>? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="byte">byte[]?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(byte[]? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="string">string?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(string? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmObjectBase">RealmObjectBase?</see>.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmObjectBase? value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="RealmValue" />.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(RealmValue value) => new(value);

        /// <summary>
        /// Implicitly constructs a <see cref="QueryArgument"/> from <see cref="GeoShapeBase" />.
        /// </summary>
        /// <param name="value">The value to store in the <see cref="QueryArgument"/>.</param>
        /// <returns>A <see cref="QueryArgument"/> containing the supplied <paramref name="value"/>.</returns>
        public static implicit operator QueryArgument(GeoShapeBase? value) => new(geoValue: value);

        internal (NativeQueryArgument Value, RealmValue.HandlesToCleanup? Handles) ToNative()
        {
            if (RealmValue != null)
            {
                var primitive = RealmValue.Value;
                if (primitive.Type == RealmValueType.Object && !primitive.AsIRealmObject().IsManaged)
                {
                    throw new RealmException("Can't use unmanaged object as argument of Filter");
                }

                var (primitiveValue, handles) = primitive.ToNative();
                return (NativeQueryArgument.Primitive(primitiveValue), handles);
            }

            // We're dealing with a geo value
            return GeoValue switch
            {
                GeoBox box => (NativeQueryArgument.GeoBox(box.ToNative()), null),
                GeoSphere sphere => (NativeQueryArgument.GeoSphere(sphere.ToNative()), null),
                GeoPolygon polygon => polygon.ToNativeQueryArgument(),
                _ => throw new NotSupportedException($"Unsupported GeoShapeBase type: {GeoValue?.GetType().FullName}")
            };
        }

        /// <inheritdoc/>
        public override string ToString() => RealmValue?.ToString() ?? GeoValue?.ToString();
    }
}
