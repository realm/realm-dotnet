using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Realms;

namespace PurePCLBuildableTest
{
    public class Class1 : RealmObject
    {
        public string Name { get; set; }
        public IList<Class1> Siblings { get; }
    }
}
