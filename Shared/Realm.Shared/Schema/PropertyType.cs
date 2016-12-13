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

namespace Realms.Schema
{
    /// <summary>
    /// An enum, containing the possible property types.
    /// </summary>
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
        /// 32 bit floating point property.
        /// </summary>
        Float = 9,

        /// <summary>
        /// 64 bit floating point property.
        /// </summary>
        Double = 10,

        /// <summary>
        /// String property.
        /// </summary>
        String = 2,

        /// <summary>
        /// Binary data (byte[]) property.
        /// </summary>
        Data = 4,

        /// <summary>
        /// Any property type.
        /// </summary>
        Any = 6,

        /// <summary>
        /// DateTimeOffset property.
        /// </summary>
        Date = 8,

        /// <summary>
        /// Related object property, representing a one-to-one or many-to-one relationship.
        /// </summary>
        Object = 12,

        /// <summary>
        /// A collection of related objects property, representing one-to-many relationship.
        /// </summary>
        Array = 13,

        /// <summary>
        /// A collection of objects linking to the model owning this property.
        /// </summary>
        LinkingObjects = 14
    }
}