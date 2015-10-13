using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealmNet
{
    //public class RealmObjectAttribute : Attribute
    //{
    //}

    public class PrimaryKeyAttribute : Attribute
    {
    }

    public class IndexedAttribute : Attribute
    {
    }

    public class IgnoreAttribute : Attribute
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
}
