using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace AssemblyToProcess
{
    public class IllegalObjectId : RealmObject
    {
        [ObjectId]
        public float FloatObjectId { get; set; }
    }

    public class RealmListWithSetter : RealmObject
    {
        public RealmList<Person> People { get; set; } 
    }

    public class IndexedProperties : RealmObject
    {
        // These should be allowed:

        [Indexed]
        public int IntProperty { get; set; }

        [Indexed]
        public string StringProperty { get; set; }


        // This should cause errors:

        [Indexed]
        public float SingleProperty { get; set; }
    }
}
