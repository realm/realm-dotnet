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
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Bson;
using Realms.Extensions;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// A type that can represent any valid Realm data type. It is a valid type in and of itself, which
    /// means that it can be used to declare a property of type <see cref="RealmValue"/>.
    /// Please note that a <see cref="RealmValue"/> property in a managed <see cref="IRealmObjectBase">realm object</see>
    /// cannot contain an <see cref="IEmbeddedObject">embedded object</see> or an <see cref="IAsymmetricObject">asymmetric object</see>.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyClass : RealmObject
    /// {
    ///     public RealmValue MyValue { get; set; }
    /// }
    ///
    /// var obj = new MyClass();
    /// obj.MyValue = 123;
    /// obj.MyValue = "abc";
    ///
    /// if (obj.Type == RealmValueType.Int)
    /// {
    ///     var myInt = obj.MyValue.AsLong();
    /// }
    /// </code>
    /// </example>
    [Preserve(AllMembers = true)]
    [DebuggerDisplay("Type = {Type}, Value = {ToString(),nq}")]
    public readonly struct RealmValue : IEquatable<RealmValue>
    {
        private readonly PrimitiveValue _primitiveValue;
        private readonly string? _stringValue;
        private readonly byte[]? _dataValue;
        private readonly IRealmObjectBase? _objectValue;

        private readonly IList<RealmValue>? _listValue;
        private readonly ISet<RealmValue>? _setValue;
        private readonly IDictionary<string, RealmValue>? _dictionaryValue;

        private readonly ObjectHandle? _objectHandle;
        private readonly IntPtr _propertyIndex;

        /// <summary>
        /// Gets the <see cref="RealmValueType"/> stored in this value.
        /// </summary>
        /// <remarks>
        /// You can check the type of the Realm value and then use any of the AsXXX methods to convert it to correct C# type.
        /// <br/>
        /// For performance reasons, all integral types, i.e. <see cref="byte"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/>,
        /// as well as <see cref="char"/> are represented as <see cref="RealmValueType.Int"/>. Realm preserves no information about the original
        /// type of the integral value stored in a <see cref="RealmValue"/> field.
        /// </remarks>
        /// <value>The <see cref="RealmValueType"/> of the current value in the database.</value>
        public RealmValueType Type { get; }

        internal RealmValue(PrimitiveValue primitive, Realm? realm = null, ObjectHandle? handle = default, IntPtr propertyIndex = default) : this()
        {
            Type = primitive.Type;
            _objectHandle = handle;
            _propertyIndex = propertyIndex;

            switch (Type)
            {
                case RealmValueType.Data:
                    _dataValue = primitive.AsBinary();
                    break;
                case RealmValueType.String:
                    _stringValue = primitive.AsString();
                    break;
                case RealmValueType.Object:
                    Argument.NotNull(realm, nameof(realm));
                    _objectValue = primitive.AsObject(realm!);
                    break;
                case RealmValueType.List:
                    Argument.NotNull(realm, nameof(realm));
                    _listValue = primitive.AsList(realm!);
                    break;
                case RealmValueType.Set:
                    Argument.NotNull(realm, nameof(realm));
                    _setValue = primitive.AsSet(realm!);
                    break;
                case RealmValueType.Dictionary:
                    Argument.NotNull(realm, nameof(realm));
                    _dictionaryValue = primitive.AsDictionary(realm!);
                    break;
                default:
                    _primitiveValue = primitive;
                    break;
            }
        }

        private RealmValue(byte[] data) : this()
        {
            Type = RealmValueType.Data;
            _dataValue = data;
        }

        private RealmValue(string value) : this()
        {
            Type = RealmValueType.String;
            _stringValue = value;
        }

        private RealmValue(IRealmObjectBase obj) : this()
        {
            Type = RealmValueType.Object;
            _objectValue = obj;
        }

        private RealmValue(IList<RealmValue> list) : this()
        {
            Type = RealmValueType.List;
            _listValue = list;
        }

        private RealmValue(ISet<RealmValue> set) : this()
        {
            Type = RealmValueType.Set;
            _setValue = set;
        }

        private RealmValue(IDictionary<string, RealmValue> dict) : this()
        {
            Type = RealmValueType.Dictionary;
            _dictionaryValue = dict;
        }

        /// <summary>
        /// Gets a RealmValue representing <c>null</c>.
        /// </summary>
        /// <value>A new RealmValue instance of type <see cref="Null"/>.</value>
        public static RealmValue Null => new(PrimitiveValue.Null());

        private static RealmValue Bool(bool value) => new(PrimitiveValue.Bool(value));

        private static RealmValue Int(long value) => new(PrimitiveValue.Int(value));

        private static RealmValue Float(float value) => new(PrimitiveValue.Float(value));

        private static RealmValue Double(double value) => new(PrimitiveValue.Double(value));

        private static RealmValue Date(DateTimeOffset value) => new(PrimitiveValue.Date(value));

        private static RealmValue Decimal(Decimal128 value) => new(PrimitiveValue.Decimal(value));

        private static RealmValue ObjectId(ObjectId value) => new(PrimitiveValue.ObjectId(value));

        private static RealmValue Guid(Guid value) => new(PrimitiveValue.Guid(value));

        private static RealmValue Data(byte[] value) => new(value);

        private static RealmValue String(string value) => new(value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static RealmValue Object(IRealmObjectBase value) => new(value);

        /// <summary>
        /// Gets a RealmValue representing a list.
        /// </summary>
        /// <param name="value"> The input list to copy. </param>
        /// <returns> A new RealmValue representing the input list. </returns>
        /// <remarks> Once created, this RealmValue will just wrap the input collection.
        /// When the object containing this RealmValue gets managed, then this value will be a Realm list.</remarks>
        public static RealmValue List(IList<RealmValue> value) => new(value);

        /// <summary>
        /// Gets a RealmValue representing a set.
        /// </summary>
        /// <param name="value"> The input set to copy. </param>
        /// <returns> A new RealmValue representing the input set. </returns>
        /// <remarks> Once created, this RealmValue will just wrap the input collection.
        /// When the object containing this RealmValue gets managed, then this value will be a Realm set.</remarks>
        public static RealmValue Set(ISet<RealmValue> value) => new(value);

        /// <summary>
        /// Gets a RealmValue representing a dictionary.
        /// </summary>
        /// <param name="value"> The input dictionary to copy. </param>
        /// <returns> A new RealmValue representing the input dictionary. </returns>
        /// <remarks> Once created, this RealmValue will just wrap the input collection.
        /// When the object containing this RealmValue gets managed, then this value will be a Realm dictionary.</remarks>
        public static RealmValue Dictionary(IDictionary<string, RealmValue> value) => new(value);

        internal static RealmValue Create<T>(T value, RealmValueType type)
        {
            if (value is null)
            {
                return new RealmValue(PrimitiveValue.Null());
            }

            return type switch
            {
                RealmValueType.String => String(Operator.Convert<T, string>(value)),
                RealmValueType.Data => Data(Operator.Convert<T, byte[]>(value)),
                RealmValueType.Bool => Bool(Operator.Convert<T, bool>(value)),
                RealmValueType.Int => Int(Operator.Convert<T, long>(value)),
                RealmValueType.Float => Float(Operator.Convert<T, float>(value)),
                RealmValueType.Double => Double(Operator.Convert<T, double>(value)),
                RealmValueType.Date => Date(Operator.Convert<T, DateTimeOffset>(value)),
                RealmValueType.Decimal128 => Decimal(Operator.Convert<T, Decimal128>(value)),
                RealmValueType.ObjectId => ObjectId(Operator.Convert<T, ObjectId>(value)),
                RealmValueType.Guid => Guid(Operator.Convert<T, Guid>(value)),
                RealmValueType.Object => Object(Operator.Convert<T, IRealmObjectBase>(value)),
                RealmValueType.List => List(Operator.Convert<T, IList<RealmValue>>(value)),
                RealmValueType.Set => Set(Operator.Convert<T, ISet<RealmValue>>(value)),
                RealmValueType.Dictionary => Dictionary(Operator.Convert<T, IDictionary<string, RealmValue>>(value)),
                _ => throw new NotSupportedException($"RealmValueType {type} is not supported."),
            };
        }

        internal (PrimitiveValue Value, HandlesToCleanup? Handles) ToNative()
        {
            switch (Type)
            {
                case RealmValueType.String:
                    if (_stringValue == null)
                    {
                        return (PrimitiveValue.Null(), null);
                    }

                    var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(_stringValue.Length));
                    var bytes = Encoding.UTF8.GetBytes(_stringValue, 0, _stringValue.Length, buffer, 0);
                    var stringHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    return (PrimitiveValue.String(stringHandle.AddrOfPinnedObject(), bytes), new HandlesToCleanup(stringHandle, buffer));
                case RealmValueType.Data:
                    if (_dataValue == null)
                    {
                        return (PrimitiveValue.Null(), null);
                    }

                    var handle = GCHandle.Alloc(_dataValue, GCHandleType.Pinned);
                    return (PrimitiveValue.Data(handle.AddrOfPinnedObject(), _dataValue?.Length ?? 0), new HandlesToCleanup(handle));
                case RealmValueType.Object:
                    var obj = AsIRealmObject();
                    if (!obj.IsManaged)
                    {
                        throw new InvalidOperationException("Can't convert unmanaged object to native");
                    }

                    return (PrimitiveValue.Object(obj.GetObjectHandle()!), null);
                default:
                    return (_primitiveValue, null);
            }
        }

        /// <summary>
        /// Returns the stored value as a <see cref="char"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A UTF-16 code unit representing the value stored in the database.</returns>
        public char AsChar()
        {
            EnsureType("char", RealmValueType.Int);
            return (char)AsInt64();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="byte"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>An 8-bit unsigned integer representing the value stored in the database.</returns>
        /// <seealso cref="AsByteRealmInteger"/>
        public byte AsByte()
        {
            EnsureType("byte", RealmValueType.Int);
            return (byte)AsInt64();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="short"/> (Int16).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 16-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsInt16RealmInteger"/>
        public short AsInt16()
        {
            EnsureType("short", RealmValueType.Int);
            return (short)AsInt64();
        }

        /// <summary>
        /// Returns the stored value as an <see cref="int"/> (Int32).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 32-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsInt32RealmInteger"/>
        public int AsInt32()
        {
            EnsureType("int", RealmValueType.Int);
            return (int)AsInt64();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="long"/> (Int64).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 64-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsInt64RealmInteger"/>
        public long AsInt64()
        {
            EnsureType("long", RealmValueType.Int);
            return _primitiveValue.AsInt();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="float"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Float"/>.</exception>
        /// <returns>A 32-bit floating point number representing the value stored in the database.</returns>
        public float AsFloat()
        {
            EnsureType("float", RealmValueType.Float);
            return _primitiveValue.AsFloat();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="double"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Double"/>.</exception>
        /// <returns>A 64-bit floating point number representing the value stored in the database.</returns>
        public double AsDouble()
        {
            EnsureType("double", RealmValueType.Double);
            return _primitiveValue.AsDouble();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="bool"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Bool"/>.</exception>
        /// <returns>A boolean representing the value stored in the database.</returns>
        public bool AsBool()
        {
            EnsureType("bool", RealmValueType.Bool);
            return _primitiveValue.AsBool();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Date"/>.</exception>
        /// <returns>A DateTimeOffset value representing the value stored in the database.</returns>
        public DateTimeOffset AsDate()
        {
            EnsureType("date", RealmValueType.Date);
            return _primitiveValue.AsDate();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="decimal"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Decimal128"/>.</exception>
        /// <returns>A 96-bit decimal number representing the value stored in the database.</returns>
        public decimal AsDecimal()
        {
            EnsureType("decimal", RealmValueType.Decimal128);
            return (decimal)_primitiveValue.AsDecimal();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="Decimal128"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Decimal128"/>.</exception>
        /// <returns>A 128-bit decimal number representing the value stored in the database.</returns>
        public Decimal128 AsDecimal128()
        {
            EnsureType("Decimal128", RealmValueType.Decimal128);
            return _primitiveValue.AsDecimal();
        }

        /// <summary>
        /// Returns the stored value as an <see cref="MongoDB.Bson.ObjectId"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.ObjectId"/>.</exception>
        /// <returns>An ObjectId representing the value stored in the database.</returns>
        public ObjectId AsObjectId()
        {
            EnsureType("ObjectId", RealmValueType.ObjectId);
            return _primitiveValue.AsObjectId();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="System.Guid"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Guid"/>.</exception>
        /// <returns>A Guid representing the value stored in the database.</returns>
        public Guid AsGuid()
        {
            EnsureType("Guid", RealmValueType.Guid);
            return _primitiveValue.AsGuid();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>An 8-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsByte"/>
        public RealmInteger<byte> AsByteRealmInteger() => AsRealmInteger(AsByte());

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>An 16-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsInt16"/>
        public RealmInteger<short> AsInt16RealmInteger() => AsRealmInteger(AsInt16());

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>An 32-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsInt32"/>
        public RealmInteger<int> AsInt32RealmInteger() => AsRealmInteger(AsInt32());

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>An 64-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsInt64"/>
        public RealmInteger<long> AsInt64RealmInteger() => AsRealmInteger(AsInt64());

        private RealmInteger<T> AsRealmInteger<T>(T value)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            if (_objectHandle is null)
            {
                return new(value);
            }

            return new(value, _objectHandle, _propertyIndex);
        }

        /// <summary>
        /// Returns the stored value as an array of bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Data"/>.</exception>
        /// <returns>An array of bytes representing the value stored in the database.</returns>
        public byte[] AsData()
        {
            EnsureType("byte[]", RealmValueType.Data);
            return _dataValue!;
        }

        /// <summary>
        /// Returns the stored value as a string.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.String"/>.</exception>
        /// <returns> A string representing the value stored in the database.</returns>
        public string AsString()
        {
            EnsureType("string", RealmValueType.String);
            return _stringValue!;
        }

        /// <summary>
        /// Returns the stored value as a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.List"/>.</exception>
        /// <returns> A list representing the value stored in the database.</returns>
        public IList<RealmValue> AsList()
        {
            EnsureType("List", RealmValueType.List);
            return _listValue!;
        }

        /// <summary>
        /// Returns the stored value as a set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Set"/>.</exception>
        /// <returns> A set representing the value stored in the database.</returns>
        public ISet<RealmValue> AsSet()
        {
            EnsureType("Set", RealmValueType.Set);
            return _setValue!;
        }

        /// <summary>
        /// Returns the stored value as a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Dictionary"/>.</exception>
        /// <returns> A dictionary representing the value stored in the database.</returns>
        public IDictionary<string, RealmValue> AsDictionary()
        {
            EnsureType("Dictionary", RealmValueType.Dictionary);
            return _dictionaryValue!;
        }

        /// <summary>
        /// Returns the stored value as a <see cref="RealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/>.
        /// </exception>
        /// <returns>
        /// A <see cref="RealmObjectBase"/> instance representing the value stored in the database.
        /// </returns>
        public RealmObjectBase AsRealmObject() => AsRealmObject<RealmObjectBase>();

        /// <summary>
        /// Returns the stored value as a <see cref="IRealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/>.
        /// </exception>
        /// <returns>
        /// A <see cref="IRealmObjectBase"/> instance representing the value stored in the database.
        /// </returns>
        public IRealmObjectBase AsIRealmObject() => AsRealmObject<IRealmObjectBase>();

        /// <summary>
        /// Returns the stored value as a <typeparamref name="T"/> which inherits from <see cref="RealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/>.
        /// </exception>
        /// <typeparam name="T">The type of the object stored in the database.</typeparam>
        /// <returns>
        /// A <see cref="RealmObjectBase"/> instance representing the value stored in the database.
        /// </returns>
        public T AsRealmObject<T>()
            where T : IRealmObjectBase
        {
            EnsureType("object", RealmValueType.Object);
            return (T)_objectValue!;
        }

        /// <summary>
        /// Returns the stored value as a nullable <see cref="char"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable UTF-16 code unit representing the value stored in the database.</returns>
        public char? AsNullableChar() => Type == RealmValueType.Null ? null : AsChar();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="byte"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 8-bit unsigned integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableByteRealmInteger"/>
        public byte? AsNullableByte() => Type == RealmValueType.Null ? null : AsByte();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="short"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 16-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt16RealmInteger"/>
        public short? AsNullableInt16() => Type == RealmValueType.Null ? null : AsInt16();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="int"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 32-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt32RealmInteger"/>
        public int? AsNullableInt32() => Type == RealmValueType.Null ? null : AsInt32();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="long"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 64-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt64RealmInteger"/>
        public long? AsNullableInt64() => Type == RealmValueType.Null ? null : AsInt64();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 8-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableByte"/>
        public RealmInteger<byte>? AsNullableByteRealmInteger() => Type == RealmValueType.Null ? null : AsByteRealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 16-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt16"/>
        public RealmInteger<short>? AsNullableInt16RealmInteger() => Type == RealmValueType.Null ? null : AsInt16RealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 32-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt32"/>
        public RealmInteger<int>? AsNullableInt32RealmInteger() => Type == RealmValueType.Null ? null : AsInt32RealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 64-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt64"/>
        public RealmInteger<long>? AsNullableInt64RealmInteger() => Type == RealmValueType.Null ? null : AsInt64RealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="float"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Float"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 32-bit floating point number representing the value stored in the database.</returns>
        public float? AsNullableFloat() => Type == RealmValueType.Null ? null : AsFloat();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="double"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Double"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 64-bit floating point number representing the value stored in the database.</returns>
        public double? AsNullableDouble() => Type == RealmValueType.Null ? null : AsDouble();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="bool"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Bool"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable boolean representing the value stored in the database.</returns>
        public bool? AsNullableBool() => Type == RealmValueType.Null ? null : AsBool();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Date"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable DateTimeOffset value representing the value stored in the database.</returns>
        public DateTimeOffset? AsNullableDate() => Type == RealmValueType.Null ? null : AsDate();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="decimal"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Decimal128"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 96-bit decimal number representing the value stored in the database.</returns>
        public decimal? AsNullableDecimal() => Type == RealmValueType.Null ? null : AsDecimal();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="Decimal128"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Date"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 128-bit decimal number representing the value stored in the database.</returns>
        public Decimal128? AsNullableDecimal128() => Type == RealmValueType.Null ? null : AsDecimal128();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="MongoDB.Bson.ObjectId"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.ObjectId"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable ObjectId representing the value stored in the database.</returns>
        public ObjectId? AsNullableObjectId() => Type == RealmValueType.Null ? null : AsObjectId();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="System.Guid"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Guid"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable Guid representing the value stored in the database.</returns>
        public Guid? AsNullableGuid() => Type == RealmValueType.Null ? null : AsGuid();

        /// <summary>
        /// Returns the stored value as an array of bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Data"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// A nullable array of bytes representing the value stored in the database.
        /// </returns>
        public byte[]? AsNullableData() => Type == RealmValueType.Null ? null : AsData();

        /// <summary>
        /// Returns the stored value as a string.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.String"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// A nullable string representing the value stored in the database.
        /// </returns>
        public string? AsNullableString() => Type == RealmValueType.Null ? null : AsString();

        /// <summary>
        /// Returns the stored value as a <see cref="RealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// A nullable <see cref="RealmObjectBase"/> instance representing the value stored in the database. It will be <c>null</c> if <see cref="Type"/> is <see cref="RealmValueType.Null"/>.
        /// </returns>
        public RealmObjectBase? AsNullableRealmObject() => AsNullableRealmObject<RealmObjectBase>();

        /// <summary>
        /// Returns the stored value as a <see cref="IRealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// A nullable <see cref="IRealmObjectBase"/> instance representing the value stored in the database. It will be <c>null</c> if <see cref="Type"/> is <see cref="RealmValueType.Null"/>.
        /// </returns>
        public IRealmObjectBase? AsNullableIRealmObject() => AsNullableRealmObject<IRealmObjectBase>();

        /// <summary>
        /// Returns the stored value as a <typeparamref name="T"/> which inherits from <see cref="RealmObjectBase"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Object"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <typeparam name="T">The type of the object stored in the database.</typeparam>
        /// <returns>
        /// A nullable <see cref="RealmObjectBase"/> instance representing the value stored in the database. It will be <c>null</c> if <see cref="Type"/> is <see cref="RealmValueType.Null"/>.
        /// </returns>
        public T? AsNullableRealmObject<T>()
            where T : class, IRealmObjectBase
            => Type == RealmValueType.Null ? null : AsRealmObject<T>();

        /// <summary>
        /// Returns the stored value converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to which to convert the value.</typeparam>
        /// <exception cref="InvalidCastException">Thrown if the type is not convertible to <typeparamref name="T"/>.</exception>
        /// <returns>The underlying value converted to <typeparamref name="T"/>.</returns>
        public T As<T>()
        {
            if (typeof(T) == typeof(RealmValue))
            {
                return Operator.Convert<RealmValue, T>(this);
            }

            // This largely copies AsAny to avoid boxing the underlying value in an object
            return Type switch
            {
                RealmValueType.Null => Operator.Convert<T>(null)!,
                RealmValueType.Int => Operator.Convert<long, T>(AsInt64()),
                RealmValueType.Bool => Operator.Convert<bool, T>(AsBool()),
                RealmValueType.String => Operator.Convert<string, T>(AsString()),
                RealmValueType.Data => Operator.Convert<byte[], T>(AsData()),
                RealmValueType.Date => Operator.Convert<DateTimeOffset, T>(AsDate()),
                RealmValueType.Float => Operator.Convert<float, T>(AsFloat()),
                RealmValueType.Double => Operator.Convert<double, T>(AsDouble()),
                RealmValueType.Decimal128 => Operator.Convert<Decimal128, T>(AsDecimal128()),
                RealmValueType.ObjectId => Operator.Convert<ObjectId, T>(AsObjectId()),
                RealmValueType.Guid => Operator.Convert<Guid, T>(AsGuid()),
                RealmValueType.Object => Operator.Convert<IRealmObjectBase, T>(AsIRealmObject()),
                RealmValueType.List => Operator.Convert<IList<RealmValue>, T>(AsList()),
                RealmValueType.Set => Operator.Convert<ISet<RealmValue>, T>(AsSet()),
                RealmValueType.Dictionary => Operator.Convert<IDictionary<string, RealmValue>, T>(AsDictionary()),
                _ => throw new NotSupportedException($"RealmValue of type {Type} is not supported."),
            };
        }

        /// <summary>
        /// Returns the stored value boxed in <see cref="object"/>.
        /// </summary>
        /// <returns>The underlying value.</returns>
        public object? AsAny()
        {
            return Type switch
            {
                RealmValueType.Null => null,
                RealmValueType.Int => AsInt64(),
                RealmValueType.Bool => AsBool(),
                RealmValueType.String => AsString(),
                RealmValueType.Data => AsData(),
                RealmValueType.Date => AsDate(),
                RealmValueType.Float => AsFloat(),
                RealmValueType.Double => AsDouble(),
                RealmValueType.Decimal128 => AsDecimal128(),
                RealmValueType.ObjectId => AsObjectId(),
                RealmValueType.Guid => AsGuid(),
                RealmValueType.Object => AsIRealmObject(),
                RealmValueType.List => AsList(),
                RealmValueType.Set => AsSet(),
                RealmValueType.Dictionary => AsDictionary(),
                _ => throw new NotSupportedException($"RealmValue of type {Type} is not supported."),
            };
        }

        /// <summary>
        /// Gets the name of the type of the object contained in <see cref="RealmValue"/>.
        /// If it does not contain an object, it will return null.
        /// </summary>
        /// <returns>
        /// The name of the type stored in <see cref="RealmValue"/> if an object, null otherwise.
        /// </returns>
        public string? ObjectType
        {
            get
            {
                if (Type != RealmValueType.Object)
                {
                    return null;
                }

                var obj = AsIRealmObject();
                if (obj.IsManaged)
                {
                    return obj.ObjectSchema.Name;
                }

                return obj.GetType().Name;
            }
        }

        /// <summary>
        /// Returns the string representation of this <see cref="RealmValue"/>.
        /// </summary>
        /// <returns>A string describing the value.</returns>
        public override string ToString() => AsAny()?.ToString() ?? "<null>";

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not RealmValue val)
            {
                return false;
            }

            return Equals(val);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = -1285871140;
                hashCode = (hashCode * -1521134295) + Type.GetHashCode();

                var valueHashCode = Type switch
                {
                    RealmValueType.Int => AsInt64().GetHashCode(),
                    RealmValueType.Bool => AsBool().GetHashCode(),
                    RealmValueType.String => AsString().GetHashCode(),
                    RealmValueType.Data => AsData().Length,
                    RealmValueType.Date => AsDate().GetHashCode(),
                    RealmValueType.Float => AsFloat().GetHashCode(),
                    RealmValueType.Double => AsDouble().GetHashCode(),
                    RealmValueType.Decimal128 => AsDecimal128().GetHashCode(),
                    RealmValueType.ObjectId => AsObjectId().GetHashCode(),
                    RealmValueType.Object => AsIRealmObject().GetHashCode(),
                    RealmValueType.List => AsList().GetHashCode(),
                    RealmValueType.Set => AsSet().GetHashCode(),
                    RealmValueType.Dictionary => AsDictionary().GetHashCode(),
                    _ => 0,
                };

                hashCode = (hashCode * -1521134295) + valueHashCode;
                return hashCode;
            }
        }

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="char"/>. Equivalent to <see cref="AsChar"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="char"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator char(RealmValue val) => val.AsChar();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="byte"/>. Equivalent to <see cref="AsByte"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="byte"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator byte(RealmValue val) => val.AsByte();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="short"/>. Equivalent to <see cref="AsInt16"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="short"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator short(RealmValue val) => val.AsInt16();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="int"/>. Equivalent to <see cref="AsInt32"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="int"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator int(RealmValue val) => val.AsInt32();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="long"/>. Equivalent to <see cref="AsInt64"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="long"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator long(RealmValue val) => val.AsInt64();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="float"/>. Equivalent to <see cref="AsFloat"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="float"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator float(RealmValue val) => val.AsFloat();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="double"/>. Equivalent to <see cref="AsDouble"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="double"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator double(RealmValue val) => val.AsDouble();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="bool"/>. Equivalent to <see cref="AsBool"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="bool"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator bool(RealmValue val) => val.AsBool();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="DateTimeOffset"/>. Equivalent to <see cref="AsDate"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="DateTimeOffset"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator DateTimeOffset(RealmValue val) => val.AsDate();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="decimal"/>. Equivalent to <see cref="AsDecimal"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="decimal"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator decimal(RealmValue val) => val.AsDecimal();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="Decimal128"/>. Equivalent to <see cref="AsDecimal128"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="Decimal128"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator Decimal128(RealmValue val) => val.AsDecimal128();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="MongoDB.Bson.ObjectId"/>. Equivalent to <see cref="AsObjectId"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="MongoDB.Bson.ObjectId"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator ObjectId(RealmValue val) => val.AsObjectId();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="System.Guid"/>. Equivalent to <see cref="AsGuid"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="System.Guid"/> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator Guid(RealmValue val) => val.AsGuid();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="char">char?</see>. Equivalent to <see cref="AsNullableChar"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="char">char?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator char?(RealmValue val) => val.AsNullableChar();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="byte">byte?</see>. Equivalent to <see cref="AsNullableByte"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="byte">byte?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator byte?(RealmValue val) => val.AsNullableByte();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="short">short?</see>. Equivalent to <see cref="AsNullableInt16"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="short">short?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator short?(RealmValue val) => val.AsNullableInt16();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="int">int?</see>. Equivalent to <see cref="AsNullableInt32"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="int">int?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator int?(RealmValue val) => val.AsNullableInt32();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="long">long?</see>. Equivalent to <see cref="AsNullableInt64"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="long">long?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator long?(RealmValue val) => val.AsNullableInt64();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="float">float?</see>. Equivalent to <see cref="AsNullableFloat"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="float">float?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator float?(RealmValue val) => val.AsNullableFloat();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="double">double?</see>. Equivalent to <see cref="AsNullableDouble"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="double">double?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator double?(RealmValue val) => val.AsNullableDouble();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="bool">bool?</see>. Equivalent to <see cref="AsNullableBool"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="bool">bool?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator bool?(RealmValue val) => val.AsNullableBool();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="DateTimeOffset">DateTimeOffset?</see>. Equivalent to <see cref="AsNullableDate"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="DateTimeOffset">DateTimeOffset?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator DateTimeOffset?(RealmValue val) => val.AsNullableDate();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="decimal">decimal?</see>. Equivalent to <see cref="AsNullableDecimal"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="decimal">decimal?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator decimal?(RealmValue val) => val.AsNullableDecimal();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="Decimal128">Decimal128?</see>. Equivalent to <see cref="AsNullableDecimal128"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="Decimal128">Decimal128?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator Decimal128?(RealmValue val) => val.AsNullableDecimal128();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="MongoDB.Bson.ObjectId">ObjectId?</see>. Equivalent to <see cref="AsNullableObjectId"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="MongoDB.Bson.ObjectId">ObjectId?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator ObjectId?(RealmValue val) => val.AsNullableObjectId();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="System.Guid">Guid?</see>. Equivalent to <see cref="AsNullableGuid"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="System.Guid">Guid?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator Guid?(RealmValue val) => val.AsNullableGuid();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;</see>.
        /// Equivalent to <see cref="AsByteRealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<byte>(RealmValue val) => val.AsByteRealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;</see>.
        /// Equivalent to <see cref="AsInt16RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<short>(RealmValue val) => val.AsInt16RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;</see>.
        /// Equivalent to <see cref="AsInt32RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<int>(RealmValue val) => val.AsInt32RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;</see>.
        /// Equivalent to <see cref="AsInt64RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<long>(RealmValue val) => val.AsInt64RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;?</see>.
        /// Equivalent to <see cref="AsNullableByteRealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<byte>?(RealmValue val) => val.AsNullableByteRealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;?</see>.
        /// Equivalent to <see cref="AsNullableInt16RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<short>?(RealmValue val) => val.AsNullableInt16RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;?</see>.
        /// Equivalent to <see cref="AsNullableInt32RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<int>?(RealmValue val) => val.AsNullableInt32RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;?</see>.
        /// Equivalent to <see cref="AsNullableInt64RealmInteger"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmInteger<long>?(RealmValue val) => val.AsNullableInt64RealmInteger();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="byte">byte[]?</see>. Equivalent to <see cref="AsNullableData"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="byte">byte[]?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator byte[]?(RealmValue val) => val.AsNullableData();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="string">string?</see>. Equivalent to <see cref="AsNullableString"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="string">string?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator string?(RealmValue val) => val.AsNullableString();

        /// <summary>
        /// Converts a <see cref="RealmValue"/> to <see cref="RealmObjectBase">RealmObjectBase?</see>. Equivalent to <see cref="AsNullableRealmObject"/>.
        /// </summary>
        /// <param name="val">The <see cref="RealmValue"/> to convert.</param>
        /// <returns>The <see cref="RealmObjectBase">RealmObjectBase?</see> stored in the <see cref="RealmValue"/>.</returns>
        public static explicit operator RealmObjectBase?(RealmValue val) => val.AsNullableRealmObject();

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="char"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(char val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="byte"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(byte val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="short"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(short val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="int"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(int val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="long"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(long val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="float"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(float val) => Float(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="double"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(double val) => Double(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="bool"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(bool val) => Bool(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(DateTimeOffset val) => Date(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="decimal"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(decimal val) => Decimal(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="Decimal128"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(Decimal128 val) => Decimal(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="MongoDB.Bson.ObjectId"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(ObjectId val) => ObjectId(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(Guid val) => Guid(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="char">char?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(char? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="byte">byte?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(byte? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="short">short?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(short? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="int">int?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(int? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="long">long?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(long? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="float">float?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(float? val) => val == null ? Null : Float(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="double">double?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(double? val) => val == null ? Null : Double(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="bool">bool?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(bool? val) => val == null ? Null : Bool(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="DateTimeOffset">DateTimeOffset?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(DateTimeOffset? val) => val == null ? Null : Date(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="decimal">decimal?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(decimal? val) => val == null ? Null : Decimal(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="Decimal128">Decimal128?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(Decimal128? val) => val == null ? Null : Decimal(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="MongoDB.Bson.ObjectId">ObjectId?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(ObjectId? val) => val == null ? Null : ObjectId(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="System.Guid">Guid?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(Guid? val) => val == null ? Null : Guid(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<byte> val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<short> val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<int> val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<long> val) => Int(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;byte&gt;?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<byte>? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;short&gt;?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<short>? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;int&gt;?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<int>? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmInteger{T}">RealmInteger&lt;long&gt;?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmInteger<long>? val) => val == null ? Null : Int(val.Value);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="byte">byte[]?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(byte[]? val) => val == null ? Null : Data(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="string">string?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(string? val) => val == null ? Null : String(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="RealmObjectBase">RealmObjectBase?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(RealmObjectBase? val) => val == null ? Null : Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="System.Collections.Generic.List{T}">List&lt;RealmValue&gt;?</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(List<RealmValue>? val) => val == null ? Null : List(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="System.Collections.Generic.HashSet{T}">HashSet&lt;RealmValue&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(HashSet<RealmValue>? val) => val == null ? Null : Set(val);

        /// <summary>
        /// Implicitly constructs a <see cref="RealmValue"/> from <see cref="System.Collections.Generic.Dictionary{TKey, TValue}">Dictionary&lt;string, RealmValue&gt;</see>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="RealmValue"/>.</param>
        /// <returns>A <see cref="RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator RealmValue(Dictionary<string, RealmValue>? val) => val == null ? Null : Dictionary(val);

        private void EnsureType(string target, RealmValueType type)
        {
            if (Type != type)
            {
                throw new InvalidCastException($"Can't cast to {target} since the underlying value is {Type}");
            }
        }

        internal readonly struct HandlesToCleanup
        {
            private readonly GCHandle _handle;

            // This is only needed for GeoPolygon. We could make it into an array, but
            // that would mean we'd be allocating arrays even for a single handle argument
            // and this happens on a hot path (string/byte[] property access). While this
            // is quite ugly, it is cheaper and keeps all allocations on the stack, reducing
            // GC pressure.
            private readonly GCHandle? _handle2;
            private readonly byte[]? _buffer;

            public HandlesToCleanup(GCHandle handle, byte[]? buffer = null, GCHandle? handle2 = null)
            {
                _handle = handle;
                _buffer = buffer;
                _handle2 = handle2;
            }

            public void Dispose()
            {
                _handle.Free();
                _handle2?.Free();

                if (_buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                }
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(RealmValue other)
        {
            if (other.Type != Type)
            {
                return false;
            }

            return Type switch
            {
                RealmValueType.Int => AsInt64() == other.AsInt64(),
                RealmValueType.Bool => AsBool() == other.AsBool(),
                RealmValueType.String => AsString() == other.AsString(),
                RealmValueType.Data => AsData().SequenceEqual(other.AsData()),
                RealmValueType.Date => AsDate() == other.AsDate(),
                RealmValueType.Float => AsFloat() == other.AsFloat(),
                RealmValueType.Double => AsDouble() == other.AsDouble(),
                RealmValueType.Decimal128 => AsDecimal128() == other.AsDecimal128(),
                RealmValueType.ObjectId => AsObjectId() == other.AsObjectId(),
                RealmValueType.Guid => AsGuid() == other.AsGuid(),
                RealmValueType.Object => AsIRealmObject().Equals(other.AsIRealmObject()),
                RealmValueType.List => AsList().SequenceEqual(other.AsList()),
                RealmValueType.Set => AsSet().SequenceEqual(other.AsSet()),
                RealmValueType.Dictionary => AsDictionary().SequenceEqual(other.AsDictionary()),
                RealmValueType.Null => true,
                _ => false,
            };
        }

        /// <summary>
        /// Compares two <see cref="RealmValue"/> instances for equality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>
        /// <c>true</c> if the underlying values stored in both instances are equal; <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(RealmValue left, RealmValue right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="RealmValue"/> instances for inequality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>
        /// <c>true</c> if the underlying values stored in both instances are not equal; <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(RealmValue left, RealmValue right)
        {
            return !(left == right);
        }
    }
}
