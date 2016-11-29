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
    /// An attribute that indicates the primary key property. It allows quick lookup of objects and enforces uniqueness of the values stored. It may only be applied to a single property in a class.
    /// </summary>
    /// <remarks>
    /// Only char, integral types, and strings can be used as primary keys.
    /// Once an object with a Primary Key has been added to the Realm, that property may not be changed.
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// An attribute that indicates an indexed property. Indexed properties slow down insertions, but can greatly speed up queries.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
    }

    /// <summary>
    /// An attribute that indicates an ignored property. Ignored properties will not be persisted in the Realm.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IgnoredAttribute : Attribute
    {
    }

    /// <summary>
    /// An attribute that indicates a required property. When persisting, the Realm will validate that the value of the property is not null.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// An attribute that indicates that a property should be persisted under a different name.
    /// </summary>
    /// <remarks>
    /// This is useful when opening a Realm across different bindings where code style conventions might differ.
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class MapToAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the property in the database.
        /// </summary>
        public string Mapping { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Realms.MapToAttribute"/> class.
        /// </summary>
        /// <param name="mapping">The name of the property in the database.</param>
        public MapToAttribute(string mapping)
        {
            this.Mapping = mapping;
        }
    }

    /// <summary>
    /// An attribute that indicates that a class has been woven. It is applied automatically by the RealmWeaver and should not be used manually.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class)]
    public class WovenAttribute : Attribute
    {
        internal Type HelperType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Realms.WovenAttribute"/> class.
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
