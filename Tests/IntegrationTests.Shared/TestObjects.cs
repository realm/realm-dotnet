using System;
using Realms;

namespace IntegrationTests.Shared
{
    public class AllTypesObject : RealmObject
    {
        public char CharProperty { get; set; }
        public byte ByteProperty { get; set; }
        public short Int16Property { get; set; }
        public int Int32Property { get; set; }
        public long Int64Property { get; set; }
        public float SingleProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BooleanProperty { get; set; }

        public string StringProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public char? NullableCharProperty { get; set; }
        public byte? NullableByteProperty { get; set; }
        public short? NullableInt16Property { get; set; }
        public int? NullableInt32Property { get; set; }
        public long? NullableInt64Property { get; set; }
        public float? NullableSingleProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }
        public bool? NullableBooleanProperty { get; set; }
    }

    public class ObjectIdCharObject : RealmObject
    {
        [ObjectId] public char CharProperty { get; set; }
    }

    public class ObjectIdByteObject : RealmObject
    {
        [ObjectId] public byte ByteProperty { get; set; }
    }

    public class ObjectIdInt16Object : RealmObject
    {
        [ObjectId] public short Int16Property { get; set; }
    }

    public class ObjectIdInt32Object : RealmObject
    {
        [ObjectId] public int Int32Property { get; set; }
    }

    public class ObjectIdInt64Object : RealmObject
    {
        [ObjectId] public long Int64Property { get; set; }
    }

    public class ObjectIdStringObject : RealmObject
    {
        [ObjectId] public string StringProperty { get; set; }
    }
}
