////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;

namespace Realms.Schema
{
    /// <summary>
    /// An enum, containing the possible property types.
    /// </summary>
    [Flags]
    [SuppressMessage("Naming", "CA1714:Flags enums should have plural names", Justification = "Would be a breaking change to rename.")]
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "This is by design - the enum represents types.")]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "The native values are duplicated.")]
    public enum PropertyType : byte
    {
        /// <summary>
        /// Integer property, combining all integral types.
        /// </summary>
        Int = 0,

        /// <summary>
        /// Boolean property.
        /// </summary>
        Bool = 1,

        /// <summary>
        /// String property.
        /// </summary>
        String = 2,

        /// <summary>
        /// Binary data (byte[]) property.
        /// </summary>
        Data = 3,

        /// <summary>
        /// DateTimeOffset property.
        /// </summary>
        Date = 4,

        /// <summary>
        /// 32 bit floating point property.
        /// </summary>
        Float = 5,

        /// <summary>
        /// 64 bit floating point property.
        /// </summary>
        Double = 6,

        /// <summary>
        /// Related object property, representing a one-to-one or many-to-one relationship.
        /// </summary>
        Object = 7,

        /// <summary>
        /// A collection of objects linking to the model owning this property.
        /// </summary>
        LinkingObjects = 8,

        /// <summary>
        /// 96 bit ObjectID property.
        /// </summary>
        ObjectId = 10,

        /// <summary>
        /// 128 bit decimal property.
        /// </summary>
        Decimal = 11,

        /// <summary>
        /// A required property. Can be combined with other values.
        /// </summary>
        Required = 0,

        /// <summary>
        /// A nullable (optional) property. Can be combined with other values.
        /// </summary>
        Nullable = 64,

        /// <summary>
        /// A collection. Can be combined with other values.
        /// </summary>
        Array = 128,

        /// <summary>
        /// Metadata flags.
        /// </summary>
        Flags = Nullable | Array,

        /// <summary>
        /// A shorthand for PropertyType.Int | PropertyType.Nullable.
        /// </summary>
        NullableInt = Int | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.Bool | PropertyType.Nullable.
        /// </summary>
        NullableBool = Bool | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.Float | PropertyType.Nullable.
        /// </summary>
        NullableFloat = Float | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.Double | PropertyType.Nullable.
        /// </summary>
        NullableDouble = Double | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.Date | PropertyType.Nullable.
        /// </summary>
        NullableDate = Date | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.ObjectId | PropertyType.Nullable.
        /// </summary>
        NullableObjectId = ObjectId | Nullable,

        /// <summary>
        /// A shorthand for PropertyType.Decimal | PropertyType.Nullable.
        /// </summary>
        NullableDecimal = Decimal | Nullable,
    }
}