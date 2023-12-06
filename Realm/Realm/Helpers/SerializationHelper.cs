////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using Realms.Serialization;
using Realms.Sync;

namespace Realms.Helpers
{
    internal static class SerializationHelper
    {
        private static readonly JsonWriterSettings _jsonSettings = new()
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson,
            Indent = false,
        };

        private static int _isInitialized;

        static SerializationHelper()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                BsonSerializer.RegisterSerializationProvider(new RealmSerializationProvider());
            }
        }

        [Preserve]
        internal static void PreserveSerializers()
        {
            _ = new BooleanSerializer();
            _ = new ByteSerializer();
            _ = new CharSerializer();
            _ = new Int16Serializer();
            _ = new Int32Serializer();
            _ = new Int64Serializer();
            _ = new SingleSerializer();
            _ = new DoubleSerializer();
            _ = new DecimalSerializer();
            _ = new Decimal128Serializer();
            _ = new ObjectIdSerializer();
            _ = new GuidSerializer();
            _ = new DateTimeSerializer();
            _ = new DateTimeOffsetSerializer();
            _ = new StringSerializer();
            _ = new ByteArraySerializer();

            _ = new EnumSerializer<Credentials.AuthProvider>();

            _ = new ArraySerializer<bool>();
            _ = new ArraySerializer<byte>();
            _ = new ArraySerializer<char>();
            _ = new ArraySerializer<short>();
            _ = new ArraySerializer<int>();
            _ = new ArraySerializer<long>();
            _ = new ArraySerializer<float>();
            _ = new ArraySerializer<double>();
            _ = new ArraySerializer<decimal>();
            _ = new ArraySerializer<Decimal128>();
            _ = new ArraySerializer<ObjectId>();
            _ = new ArraySerializer<Guid>();
            _ = new ArraySerializer<DateTime>();
            _ = new ArraySerializer<DateTimeOffset>();
            _ = new ArraySerializer<string>();
            _ = new ArraySerializer<byte[]>();

            _ = new NullableSerializer<bool>();
            _ = new NullableSerializer<byte>();
            _ = new NullableSerializer<char>();
            _ = new NullableSerializer<short>();
            _ = new NullableSerializer<int>();
            _ = new NullableSerializer<long>();
            _ = new NullableSerializer<float>();
            _ = new NullableSerializer<double>();
            _ = new NullableSerializer<decimal>();
            _ = new NullableSerializer<Decimal128>();
            _ = new NullableSerializer<ObjectId>();
            _ = new NullableSerializer<Guid>();
            _ = new NullableSerializer<DateTime>();
            _ = new NullableSerializer<DateTimeOffset>();

            _ = new BsonDocumentSerializer();
            _ = new BsonArraySerializer();

            _ = new ObjectSerializer();

            _ = new EnumerableInterfaceImplementerSerializer<IEnumerable<object>, object>();
            _ = new ExpandoObjectSerializer();
        }

        public static string ToNativeJson(this object? value)
        {
            if (value is RealmValue rv)
            {
                return rv.AsAny().ToNativeJson();
            }

            if (value is object?[] arr)
            {
                var elements = arr.Select(ToNativeJson);
                return $"[{string.Join(",", elements)}]";
            }

            if (value is null)
            {
                return value.ToJson(_jsonSettings);
            }

            return value.ToJson(value.GetType(), _jsonSettings);
        }

        private class RealmSerializationProvider : IBsonSerializationProvider
        {
            public IBsonSerializer? GetSerializer(Type type) => type switch
            {
                _ when type == typeof(decimal) => new DecimalSerializer(BsonType.Decimal128, new RepresentationConverter(allowOverflow: false, allowTruncation: false)),
                _ when type == typeof(Decimal128) => new Decimal128Serializer(BsonType.Decimal128),
                _ when type == typeof(Guid) => new GuidSerializer(GuidRepresentation.Standard),
                _ when type == typeof(DateTimeOffset) => new DateTimeOffsetSerializer(BsonType.DateTime),
                _ when type == typeof(object) => new ObjectSerializer(
                    BsonSerializer.LookupDiscriminatorConvention(typeof(object)),
                    GuidRepresentation.Standard,
                    allowedSerializationTypes: _ => true,
                    allowedDeserializationTypes: t => ObjectSerializer.DefaultAllowedTypes(t) || IsAnonymousType(t)),
                _ when type.IsClosedGeneric(typeof(RealmInteger<>), out var typeArgs) => CreateRealmIntegerSerializer(typeArgs.Single()),
                _ when type == typeof(RealmValue) => new RealmValueSerializer(),
                _ => RealmObjectSerializer.LookupSerializer(type)
            };

            // TODO: remove this when https://github.com/mongodb/mongo-csharp-driver/commit/e0c14c80e6c31f337439d2915b5dd90fe38f9562
            // is released
            private static bool IsAnonymousType(Type type) =>
                type.GetCustomAttributes(false).Any(x => x is CompilerGeneratedAttribute) &&
                type.IsGenericType &&
                type.Name.Contains("Anon");

            private static IBsonSerializer CreateRealmIntegerSerializer(Type type)
            {
                var serializerType = typeof(RealmIntegerSerializer<>).MakeGenericType(type);
                return (IBsonSerializer)Activator.CreateInstance(serializerType)!;
            }
        }

        [Preserve(AllMembers = true)]
        private class RealmIntegerSerializer<T> :
            IBsonSerializer<RealmInteger<T>>
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            public Type ValueType => typeof(RealmInteger<T>);

            public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, RealmInteger<T> value)
            {
                BsonSerializer.LookupSerializer<T>().Serialize(context, args, value);
            }

            void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
            {
                var intValue = value switch
                {
                    RealmInteger<T> ri => (T)ri,
                    T => value,
                    _ => throw new NotSupportedException($"Unexpected RealmInteger type: got {value.GetType()} but expected {ValueType}"),
                };

                BsonSerializer.LookupSerializer<T>().Serialize(context, args, intValue);
            }

            public RealmInteger<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
                => BsonSerializer.LookupSerializer<T>().Deserialize(context, args);

            object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
                => Deserialize(context, args);
        }

        [Preserve(AllMembers = true)]
        private class RealmValueSerializer : IBsonSerializer<RealmValue>
        {
            public Type ValueType => typeof(RealmValue);

            public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, RealmValue value)
            {
                if (value.Type == RealmValueType.Null)
                {
                    context.Writer.WriteNull();
                }
                else if (value.Type == RealmValueType.Object)
                {
                    SerializeDbRef(context, args, value);
                }
                else
                {
                    var boxed = value.AsAny()!;
                    BsonSerializer.LookupSerializer(boxed.GetType()).Serialize(context, args, boxed);
                }
            }

            void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
                => Serialize(context, args, (RealmValue)value);

            public RealmValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var reader = context.Reader;
                switch (reader.CurrentBsonType)
                {
                    case BsonType.Null:
                        reader.ReadNull();
                        return RealmValue.Null;
                    case BsonType.Double:
                        return reader.ReadDouble();
                    case BsonType.String:
                        var value = reader.ReadString();
                        if (DateTimeOffset.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                        {
                            return date;
                        }

                        return value;
                    case BsonType.Document:
                        return DeseriealizeDBRef(context, args);
                    case BsonType.Binary:
                        var binary = reader.ReadBinaryData();
                        if (binary.SubType == BsonBinarySubType.UuidStandard)
                        {
                            return GuidConverter.FromBytes(binary.Bytes, GuidRepresentation.Standard);
                        }

                        return binary.Bytes;
                    case BsonType.ObjectId:
                        return reader.ReadObjectId();
                    case BsonType.Boolean:
                        return reader.ReadBoolean();
                    case BsonType.DateTime:
                        return DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadDateTime());
                    case BsonType.Int32:
                        return reader.ReadInt32();
                    case BsonType.Int64:
                        return reader.ReadInt64();
                    case BsonType.Decimal128:
                        return reader.ReadDecimal128();
                    default:
                        throw new NotSupportedException($"Can't deserialize RealmValue from json type: {reader.CurrentBsonType}.");
                }
            }

            object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
                => Deserialize(context, args);

            private void SerializeDbRef(BsonSerializationContext context, BsonSerializationArgs args, RealmValue value)
            {
                var obj = value.AsIRealmObject()!;

                var schemaName = obj.ObjectSchema!.Name;

                context.Writer.WriteStartDocument();

                context.Writer.WriteName("$ref");
                BsonSerializer.LookupSerializer(typeof(string)).Serialize(context, schemaName);

                context.Writer.WriteName("$id");
                RealmObjectSerializer.LookupSerializer(obj.GetType())!.SerializeId(context, args, obj);

                context.Writer.WriteEndDocument();
            }

            private RealmValue DeseriealizeDBRef(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                string schemaName = null!;
                IRealmObjectBase obj = null!;

                var reader = context.Reader;
                reader.ReadStartDocument();
                if (reader.State == BsonReaderState.Type)
                {
                    reader.ReadBsonType();
                }

                while (reader.State != BsonReaderState.EndOfDocument)
                {
                    var fieldName = reader.ReadName();

                    if (fieldName == "$ref")
                    {
                        schemaName = reader.ReadString();
                    }
                    else if (fieldName == "$id")
                    {
                        obj = RealmObjectSerializer.LookupSerializer(schemaName)!.DeserializeById(context, args)!;
                    }

                    if (reader.State == BsonReaderState.Type)
                    {
                        reader.ReadBsonType();
                    }
                }

                reader.ReadEndDocument();
                return RealmValue.Object(obj);
            }
        }
    }
}
