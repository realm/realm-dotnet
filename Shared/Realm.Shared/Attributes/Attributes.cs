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

namespace Realms
{
    /// <summary>
    /// An attribute that indicates that a class has been woven. It is applied automatically by the RealmWeaver and should not be used manually.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class)]
    public class WovenAttribute : Attribute
    {
        internal Type HelperType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WovenAttribute"/> class.
        /// </summary>
        /// <param name="helperType">The type of the generated RealmObjectHelper for that class.</param>
        public WovenAttribute(Type helperType)
        {
            this.HelperType = helperType;
        }
    }

    /// <summary>
    /// An attribute that indicates that a property has been woven. It is applied automatically by the RealmWeaver and should not be used manually.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class WovenPropertyAttribute : Attribute
    {
    }

    /// <summary>
    /// Do not implicitly add the type decorated by this attribute to a Realm's schema unless it has been explicitly set.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class ExplicitAttribute : Attribute
    {
    }
}