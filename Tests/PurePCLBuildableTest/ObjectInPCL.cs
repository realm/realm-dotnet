using System;
using System.Collections.Generic;
using Realms;

namespace PurePCLBuildableTest
{
    public class ObjectInPCL : RealmObject
    {
    // all the simple properties
        public char CharProperty { get; set; }
        public byte ByteProperty { get; set; }
        public short Int16Property { get; set; }
        public int Int32Property { get; set; }
        public long Int64Property { get; set; }
        public float SingleProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BooleanProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public string StringProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }

        public char? NullableCharProperty { get; set; }
        public byte? NullableByteProperty { get; set; }
        public short? NullableInt16Property { get; set; }
        public int? NullableInt32Property { get; set; }
        public long? NullableInt64Property { get; set; }
        public float? NullableSingleProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }
        public bool? NullableBooleanProperty { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

    // relationship properties
        public ObjectInPCL OneAndOnly {get; set;}
        public IList<ObjectInPCL> Siblings { get; }

    }
}
