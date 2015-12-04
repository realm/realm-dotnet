/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Reflection;

namespace Realms
{
    public class ObjectIdAttribute : Attribute
    {
    }

    public class IndexedAttribute : Attribute
    {
    }

    public class IgnoredAttribute : Attribute
    {
    }

    public class MapToAttribute : Attribute
    {
        public string Mapping { get; set; }
        public MapToAttribute(string mapping)
        {
            this.Mapping = mapping;
        }
    }

    internal class WovenAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class WovenPropertyAttribute : Attribute
    {
        internal string BackingFieldName { get; private set; }

        public WovenPropertyAttribute(string backingFieldName)
        {
            this.BackingFieldName = backingFieldName;
        }
    }
}
