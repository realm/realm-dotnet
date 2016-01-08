using System;
using Realms;

namespace AssemblyToProcess
{
    public class AllTypesObject : RealmObject
    {
        public int Int32Property { get; set; }
        public long Int64Property { get; set; }
        public float SingleProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BooleanProperty { get; set; }

        public string StringProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public int? NullableInt32Property { get; set; }
        public long? NullableInt64Property { get; set; }
        public float? NullableSingleProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }
        public bool? NullableBooleanProperty { get; set; }
    }

    public class IndexedInt32Object : RealmObject
    {
        [Indexed] public int Int32Property { get; set; }
    }

    public class IndexedInt64Object : RealmObject
    {
        [Indexed] public long Int64Property { get; set; }
    }

    public class IndexedStringObject : RealmObject
    {
        [Indexed] public string StringProperty { get; set; }
    }

    //public class IndexedNullableInt32Object : RealmObject
    //{
    //    [Indexed] public int? Int32Property { get; set; }
    //}

    //public class IndexedNullableInt64Object : RealmObject
    //{
    //    [Indexed] public long? Int64Property { get; set; }
    //}
}
