using System;
using Realms;

namespace AssemblyToProcess
{
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

        [Indexed]
        public bool BooleanProperty { get; set; }

        [Indexed]
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        // This should cause an error:

        [Indexed]
        public float SingleProperty { get; set; }
    }

    public class ObjectIdProperties : RealmObject
    {
        // These should be allowed:

        [ObjectId]
        public int IntProperty { get; set; }

        [ObjectId]
        public string StringProperty { get; set; }

        // These should cause errors:

        [ObjectId]
        public bool BooleanProperty { get; set; }

        [ObjectId]
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        [ObjectId]
        public float SingleProperty { get; set; }
    }
}
