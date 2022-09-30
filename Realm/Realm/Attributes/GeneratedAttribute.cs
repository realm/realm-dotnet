////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.ComponentModel;

namespace Realms
{
    /// <summary>
    /// An attribute that indicates that a class has been generated. It is applied automatically by the Source Generator and should not be used manually.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class)]
    public class GeneratedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedAttribute"/> class.
        /// </summary>
        public GeneratedAttribute()
        {
        }
    }
}
