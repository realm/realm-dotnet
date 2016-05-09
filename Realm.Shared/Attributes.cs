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
        public Type HelperType { get; private set; }

        public WovenAttribute(Type helperType)
        {
            this.HelperType = helperType;
        }
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
