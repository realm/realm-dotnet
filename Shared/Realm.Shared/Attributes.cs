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
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IgnoredAttribute : Attribute
    {
    }

    /// <summary>
    /// Delays persisting the value in data-binding scenarios. This is useful when you have string fields, or other UI controls where values change rapidly.
    /// If a transaction is already opened, the value is persisted as soon as the transaction is committed. Otherwise, all changes will be throttled and the final value will be persisted <c>milliseconds</c> ms after the last change.
    /// /// NOTE: When using [Throttle], make sure that the realm instance on the UI thread will remain open for at least the throttle period after the last change of a throttled property.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class ThrottleSetterAttribute : Attribute
    {
        public int Milliseconds { get; set; }

        public ThrottleSetterAttribute(int milliseconds)
        {
            this.Milliseconds = milliseconds;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class MapToAttribute : Attribute
    {
        public string Mapping { get; set; }

        public MapToAttribute(string mapping)
        {
            this.Mapping = mapping;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Class)]
    public class WovenAttribute : Attribute
    {
        internal Type HelperType { get; private set; }

        public WovenAttribute(Type helperType)
        {
            this.HelperType = helperType;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    [AttributeUsage(AttributeTargets.Property)]
    public class WovenPropertyAttribute : Attribute
    {
    }
}
