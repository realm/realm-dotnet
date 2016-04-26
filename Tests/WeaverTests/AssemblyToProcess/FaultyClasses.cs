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
}
