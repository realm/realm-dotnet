﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Realms.Schema;

namespace Realms.Serialization;

#pragma warning disable SA1600 // Elements should be documented
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RealmObjectSerializer
{
    private static readonly ConcurrentDictionary<Type, RealmObjectSerializerBase> SerializersByType = new();
    private static readonly ConcurrentDictionary<string, RealmObjectSerializerBase> SerializersByName = new();

    public static void Register(RealmObjectSerializerBase serializer)
    {
        SerializersByType.TryAdd(serializer.ValueType, serializer);
        SerializersByName.TryAdd(serializer.SchemaName, serializer);
    }

    public static RealmObjectSerializerBase? LookupSerializer(Type type)
    {
        if (SerializersByType.TryGetValue(type, out var serializer))
        {
            return serializer;
        }

        return null;
    }

    public static RealmObjectSerializerBase<T>? LookupSerializer<T>()
        where T : class?, IRealmObjectBase?
    {
        if (SerializersByType.TryGetValue(typeof(T), out var serializer))
        {
            return (RealmObjectSerializerBase<T>)serializer;
        }

        return null;
    }

    public static RealmObjectSerializerBase? LookupSerializer(string schemaName)
    {
        if (SerializersByName.TryGetValue(schemaName, out var serializer))
        {
            return serializer;
        }

        return null;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class RealmObjectSerializerBase : IBsonSerializer
{
    public abstract Type ValueType { get; }

    public abstract string SchemaName { get; }

    public abstract void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object? value);

    public abstract void SerializeId(BsonSerializationContext context, BsonSerializationArgs args, object? value);

    public abstract object? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args);

    public abstract IRealmObjectBase? DeserializeById(BsonDeserializationContext context, BsonDeserializationArgs args);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class RealmObjectSerializerBase<T> : RealmObjectSerializerBase, IBsonSerializer<T>
    where T : class?, IRealmObjectBase?
{
    public override Type ValueType => typeof(T);

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object? value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else if (value is T tValue)
        {
            SerializeValue(context, args, tValue);
        }
        else
        {
            throw new NotSupportedException($"Invalid value provided for serializer. Expected {ValueType} but got {value.GetType()}");
        }
    }

    public override object? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        if (reader.GetCurrentBsonType() == BsonType.Null)
        {
            reader.ReadNull();
            return null;
        }

        var result = CreateInstance();

        reader.ReadStartDocument();
        while (reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = reader.ReadName();

            try
            {
                ReadValue(result, name, context);
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Error while deserializing property {name}: {ex.Message}");
            }
        }

        reader.ReadEndDocument();

        return result;
    }

    // The methods "SerializeId" and "DeserializeById" are used when serialising/deserialising links, as
    // this values are represented just by the primary key value in the JSON
    public override void SerializeId(BsonSerializationContext context, BsonSerializationArgs args, object? value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else if (value is T tValue)
        {
            var pkValue = tValue.Accessor.GetValue(RealmObjectSerializerBase<T>.GetPKProperty(tValue).Name);
            switch (pkValue.Type)
            {
                case RealmValueType.Null:
                    context.Writer.WriteNull();
                    break;
                case RealmValueType.Int:
                    BsonSerializer.LookupSerializer(typeof(long)).Serialize(context, pkValue.AsInt64());
                    break;
                case RealmValueType.String:
                    BsonSerializer.LookupSerializer(typeof(string)).Serialize(context, pkValue.AsString());
                    break;
                case RealmValueType.ObjectId:
                    BsonSerializer.LookupSerializer(typeof(ObjectId)).Serialize(context, pkValue.AsObjectId());
                    break;
                case RealmValueType.Guid:
                    BsonSerializer.LookupSerializer(typeof(Guid)).Serialize(context, pkValue.AsGuid());
                    break;
            }
        }
        else
        {
            throw new NotSupportedException($"Invalid value provided for serializer. Expected {ValueType} but got {value.GetType()}");
        }
    }

    public virtual T? DeserializeById(BsonDeserializationContext context)
    {
        if (context.Reader.CurrentBsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null;
        }

        var result = CreateInstance()!;

        var pk = RealmObjectSerializerBase<T>.GetPKProperty(result);
        var pkValue = pk.Type.UnderlyingType() switch
        {
            PropertyType.Int => (RealmValue)context.Reader.ReadInt64(),
            PropertyType.String => (RealmValue)context.Reader.ReadString(),
            PropertyType.Guid => (RealmValue)(Guid)BsonSerializer.LookupSerializer(typeof(Guid)).Deserialize(context),
            PropertyType.ObjectId => (RealmValue)context.Reader.ReadObjectId(),
            _ => throw new NotSupportedException($"Unexpected primary key type: {pk.Type}"),
        };

        result.Accessor.SetValueUnique(pk.Name, pkValue);

        return result;
    }

    protected abstract T CreateInstance();

    // Read scalar values (including objects/links)
    protected abstract void ReadValue(T instance, string name, BsonDeserializationContext context);

    // Read list or set element
    protected abstract void ReadArrayElement(T instance, string name, BsonDeserializationContext context);

    // Read dictionary element
    protected abstract void ReadDocumentField(T instance, string name, string fieldName, BsonDeserializationContext context);

    protected abstract void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, T value);

    protected void ReadArray(T instance, string name, BsonDeserializationContext context)
    {
        var reader = context.Reader;

        if (reader.CurrentBsonType != BsonType.Array)
        {
            throw new SerializationException("BSON element is not an array");
        }

        reader.ReadStartArray();
        if (reader.State == BsonReaderState.Type)
        {
            reader.ReadBsonType();
        }

        while (reader.State != BsonReaderState.EndOfArray)
        {
            ReadArrayElement(instance, name, context);

            if (reader.State == BsonReaderState.Type)
            {
                reader.ReadBsonType();
            }
        }

        reader.ReadEndArray();
    }

    protected void ReadDictionary(T instance, string name, BsonDeserializationContext context)
    {
        var reader = context.Reader;

        if (reader.CurrentBsonType != BsonType.Document)
        {
            throw new SerializationException("BSON element is not a document");
        }

        reader.ReadStartDocument();
        if (reader.State == BsonReaderState.Type)
        {
            reader.ReadBsonType();
        }

        while (reader.State != BsonReaderState.EndOfDocument)
        {
            var fieldName = reader.ReadName();
            ReadDocumentField(instance, name, fieldName, context);

            if (reader.State == BsonReaderState.Type)
            {
                reader.ReadBsonType();
            }
        }

        reader.ReadEndDocument();
    }

    protected void WriteValue<TValue>(BsonSerializationContext context, BsonSerializationArgs args, string name, TValue value)
    {
        context.Writer.WriteName(name);
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else if (value is IRealmObject)
        {
            RealmObjectSerializer.LookupSerializer(typeof(TValue))!.SerializeId(context, args, value);
        }
        else
        {
            BsonSerializer.LookupSerializer(typeof(TValue)).Serialize(context, value);
        }
    }

    protected void WriteList<TValue>(BsonSerializationContext context, BsonSerializationArgs args, string name, IEnumerable<TValue> values)
        => WriteArray(context, args, name, values);

    protected void WriteSet<TValue>(BsonSerializationContext context, BsonSerializationArgs args, string name, IEnumerable<TValue> values)
        => WriteArray(context, args, name, values);

    private static void WriteArray<TValue>(BsonSerializationContext context, BsonSerializationArgs args, string name, IEnumerable<TValue> values)
    {
        context.Writer.WriteName(name);
        context.Writer.WriteStartArray();

        var type = typeof(TValue);
        Action<BsonSerializationContext, BsonSerializationArgs, object?> serialize = type switch
        {
            _ when type.IsRealmObject() || type.IsAsymmetricObject() => RealmObjectSerializer.LookupSerializer(type)!.SerializeId,
            _ when type.IsEmbeddedObject() => RealmObjectSerializer.LookupSerializer(type)!.Serialize,
            _ => BsonSerializer.LookupSerializer<TValue>().Serialize
        };

        foreach (var item in values)
        {
            serialize(context, args, item);
        }

        context.Writer.WriteEndArray();
    }

    protected void WriteDictionary<TValue>(BsonSerializationContext context, BsonSerializationArgs args, string name, IDictionary<string, TValue> values)
    {
        context.Writer.WriteName(name);
        context.Writer.WriteStartDocument();

        var type = typeof(TValue);
        Action<BsonSerializationContext, BsonSerializationArgs, object?> serialize = type switch
        {
            _ when type.IsRealmObject() || type.IsAsymmetricObject() => RealmObjectSerializer.LookupSerializer(type)!.SerializeId,
            _ when type.IsEmbeddedObject() => RealmObjectSerializer.LookupSerializer(type)!.Serialize,
            _ => BsonSerializer.LookupSerializer<TValue>().Serialize
        };

        foreach (var kvp in values)
        {
            context.Writer.WriteName(kvp.Key);
            serialize(context, args, kvp.Value);
        }

        context.Writer.WriteEndDocument();
    }

    T IBsonSerializer<T>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return (T)Deserialize(context, args)!;
    }

    void IBsonSerializer<T>.Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
    {
        Serialize(context, args, value);
    }

    public override IRealmObjectBase? DeserializeById(BsonDeserializationContext context, BsonDeserializationArgs args)
        => DeserializeById(context);

    private static Property GetPKProperty(T instance)
    {
        var pk = instance!.ObjectSchema!.PrimaryKeyProperty;
        if (pk == null)
        {
            throw new NotSupportedException($"A primary key property must be defined on {typeof(T)}.");
        }

        return pk.Value;
    }
}

#pragma warning restore SA1600 // Elements should be documented
