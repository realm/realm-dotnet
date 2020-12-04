////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

namespace Realms
{
    /// <summary>
    /// Represents the type of a value stored in a <see cref="RealmValue"/> property.
    /// </summary>
    public enum RealmValueType : byte
    {
        /// <summary>
        /// The value is <c>null</c>.
        /// </summary>
        Null,

        /// <summary>
        /// The value is a <see cref="long"/>.
        /// </summary>
        /// <remarks>
        /// For performance reasons, all integers, as well as <see cref="char"/>, in Realm are stored as 64-bit values.
        /// You can still cast it to the narrower types using <see cref="RealmValue.AsByte"/>, <see cref="RealmValue.AsInt16"/>,
        /// <see cref="RealmValue.AsInt32"/>, or <see cref="RealmValue.AsChar"/>.
        /// </remarks>
        Int,

        /// <summary>
        /// The value represents a <see cref="bool"/>.
        /// </summary>
        Bool,

        /// <summary>
        /// The value represents a non-null <see cref="string"/>.
        /// </summary>
        String,

        /// <summary>
        /// The value represents a non-null byte array.
        /// </summary>
        Data,

        /// <summary>
        /// The value represents a <see cref="DateTimeOffset"/>.
        /// </summary>
        Date,

        /// <summary>
        /// The value represents a <see cref="float"/>.
        /// </summary>
        Float,

        /// <summary>
        /// The value represents a <see cref="double"/>.
        /// </summary>
        Double,

        /// <summary>
        /// The value represents a <see cref="MongoDB.Bson.Decimal128"/>.
        /// </summary>
        /// <remarks>
        /// For performance reasons, all decimals in Realm are stored as 128-bit values.
        /// You can still cast it to the 96-bit <see cref="decimal"/> using <see cref="RealmValue.AsDecimal"/>.
        /// </remarks>
        Decimal128,

        /// <summary>
        /// The value represents a <see cref="MongoDB.Bson.ObjectId"/>.
        /// </summary>
        ObjectId,

        /// <summary>
        /// The value represents a link to another object.
        /// </summary>
        Object,

        /// <summary>
        /// The value represents a <see cref="Guid"/>.
        /// </summary>
        Guid,
    }
}
