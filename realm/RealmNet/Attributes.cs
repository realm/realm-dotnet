using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealmNet
{
    //public class RealmObjectAttribute : Attribute
    //{
    //}

    public class PrimaryKeyAttribute : Attribute
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
