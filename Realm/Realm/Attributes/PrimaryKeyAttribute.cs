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
using MongoDB.Bson;

namespace Realms
{
    /// <summary>
    /// An attribute that indicates the primary key property. It allows quick lookup of objects and enforces uniqueness of the values stored. It may only be applied to a single property in a class.
    /// </summary>
    /// <remarks>
    /// Valid primary key types are <see cref="char"/>, <see cref="byte"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/>,
    /// <see cref="string"/>, <see cref="ObjectId"/>, <see cref="Guid"/>, as well as their nullable versions.
    /// Once an object with a Primary Key has been added to the Realm, that property may not be changed.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.
        /// </summary>
        public PrimaryKeyAttribute()
        {
        }
    }
}
