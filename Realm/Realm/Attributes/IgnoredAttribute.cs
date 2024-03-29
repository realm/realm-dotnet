﻿////////////////////////////////////////////////////////////////////////////
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

namespace Realms
{
    /// <summary>
    /// An attribute that indicates an ignored property. Ignored properties will not be persisted in the Realm.
    /// </summary>
    /// <remarks>
    /// Non-autoimplemented properties are automatically ignored, as are properties that only have a setter or
    /// a getter.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IgnoredAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoredAttribute"/> class.
        /// </summary>
        public IgnoredAttribute()
        {
        }
    }
}
