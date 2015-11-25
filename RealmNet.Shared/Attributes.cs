/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Reflection;

namespace RealmNet
{
    public class IdentifierAttribute : Attribute
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

    public class WovenAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class WovenPropertyAttribute : Attribute
    {
        internal string BackingFieldName { get; private set; }

        public WovenPropertyAttribute(string backingFieldName)
        {
            this.BackingFieldName = backingFieldName;
        }
    }
}
