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

namespace Realms
{
    /// <summary>
    /// Prevents the MonoTouch linker from linking the target.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class PreserveAttribute : Attribute
    {
#if WINDOWS
        public bool AllMembers { get; set; }
        public bool Conditional { get; set; }
#else
        /// <summary>
        /// Ensures that all members of this type are preserved.
        /// </summary>
        public bool AllMembers;

        /// <summary>
        /// Flags the method as a method to preserve during linking if the container class is pulled in.
        /// </summary>
        public bool Conditional;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Realms.PreserveAttribute"/> class.
        /// </summary>
        /// <param name="allMembers">If set to <c>true</c> all members will be preserved.</param>
        /// <param name="conditional">If set to <c>true</c>, the method will only be preserved if the container class is preserved.</param>
        public PreserveAttribute(bool allMembers, bool conditional)
        {
            AllMembers = allMembers;
            Conditional = conditional;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Realms.PreserveAttribute"/> class.
        /// </summary>
        public PreserveAttribute()
        {
        }
    }
}