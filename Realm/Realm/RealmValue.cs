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
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Bson;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// A type that can represent any valid Realm data type. It is a valid type in and of itself,
    /// which means that it can be used to declare a property of type <see cref="RealmValue"/> that
    /// can hold any type.
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
    public struct RealmValue
    {
        private PrimitiveValue _primitiveValue;
        private string _stringValue;
        private byte[] _dataValue;

        private ObjectHandle _objectHandle;
        private IntPtr _propertyIndex;

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

        internal RealmValue(PrimitiveValue primitive, ObjectHandle handle = default, IntPtr propertyIndex = default)
        {
            Type = primitive.Type;
            _objectHandle = handle;
            _propertyIndex = propertyIndex;

            switch (Type)
            {
                case RealmValueType.Data:
                    _primitiveValue = default;
                    _dataValue = primitive.AsBinary();
                    _stringValue = default;
                    break;
                case RealmValueType.String:
                    _primitiveValue = default;
                    _dataValue = default;
                    _stringValue = primitive.AsString();
                    break;
                default:
                    _primitiveValue = primitive;
                    _dataValue = default;
                    _stringValue = default;
                    break;
            }
        }

        internal RealmValue(byte[] data)
        {
            Type = data == null ? RealmValueType.Null : RealmValueType.Data;
            _dataValue = data;
            _primitiveValue = default;
            _stringValue = default;
            _objectHandle = default;
            _propertyIndex = default;
        }

        internal RealmValue(string value)
        {
            Type = value == null ? RealmValueType.Null : RealmValueType.String;
            _stringValue = value;
            _dataValue = default;
            _primitiveValue = default;
            _objectHandle = default;
            _propertyIndex = default;
        }

        internal static RealmValue Null() => new RealmValue(PrimitiveValue.Null());

        private static RealmValue Bool(bool value) => new RealmValue(PrimitiveValue.Bool(value));

        private static RealmValue Int(long value) => new RealmValue(PrimitiveValue.Int(value));

        private static RealmValue Float(float value) => new RealmValue(PrimitiveValue.Float(value));

        private static RealmValue Double(double value) => new RealmValue(PrimitiveValue.Double(value));

        private static RealmValue Date(DateTimeOffset value) => new RealmValue(PrimitiveValue.Date(value));

        private static RealmValue Decimal(Decimal128 value) => new RealmValue(PrimitiveValue.Decimal(value));

        private static RealmValue ObjectId(ObjectId value) => new RealmValue(PrimitiveValue.ObjectId(value));

        private static RealmValue Data(byte[] value) => new RealmValue(value);

        private static RealmValue String(string value) => new RealmValue(value);

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
            return (char)_primitiveValue.AsInt();
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
            return (byte)_primitiveValue.AsInt();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="short"/> (Int16).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 16-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsShortRealmInteger"/>
        public short AsShort()
        {
            EnsureType("short", RealmValueType.Int);
            return (short)_primitiveValue.AsInt();
        }

        /// <summary>
        /// Returns the stored value as an <see cref="int"/> (Int32).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 32-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsIntRealmInteger"/>
        public int AsInt()
        {
            EnsureType("int", RealmValueType.Int);
            return (int)_primitiveValue.AsInt();
        }

        /// <summary>
        /// Returns the stored value as a <see cref="long"/> (Int64).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns>A 64-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsLongRealmInteger"/>
        public long AsLong()
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
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns> An 8-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsByte"/>
        public RealmInteger<byte> AsByteRealmInteger() => new RealmInteger<byte>(AsByte(), _objectHandle, _propertyIndex);

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns> An 16-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsShort"/>
        public RealmInteger<short> AsShortRealmInteger() => new RealmInteger<short>(AsShort(), _objectHandle, _propertyIndex);

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns> An 32-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsInt"/>
        public RealmInteger<int> AsIntRealmInteger() => new RealmInteger<int>(AsInt(), _objectHandle, _propertyIndex);

        /// <summary>
        /// Returns the stored value as a <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/>.</exception>
        /// <returns> An 64-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsLong"/>
        public RealmInteger<long> AsLongRealmInteger() => new RealmInteger<long>(AsLong(), _objectHandle, _propertyIndex);

        /// <summary>
        /// Returns the stored value as a nullable <see cref="char"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable UTF-16 code unit representing the value stored in the database.</returns>
        public char? AsNullableChar() => Type == RealmValueType.Null ? null : (char?)AsChar();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="byte"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 8-bit unsigned integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableByteRealmInteger"/>
        public byte? AsNullableByte() => Type == RealmValueType.Null ? null : (byte?)AsByte();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="short"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 16-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableShortRealmInteger"/>
        public short? AsNullableShort() => Type == RealmValueType.Null ? null : (short?)AsShort();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="int"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 32-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableIntRealmInteger"/>
        public int? AsNullableInt() => Type == RealmValueType.Null ? null : (int?)AsInt();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="long"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 64-bit integer representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableLongRealmInteger"/>
        public long? AsNullableLong() => Type == RealmValueType.Null ? null : (long?)AsLong();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns> A nullable 8-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableByte"/>
        public RealmInteger<byte>? AsNullableByteRealmInteger() => Type == RealmValueType.Null ? null : (RealmInteger<byte>?)AsByteRealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns> A nullable 16-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableShort"/>
        public RealmInteger<short>? AsNullableShortRealmInteger() => Type == RealmValueType.Null ? null : (RealmInteger<short>?)AsShortRealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns> A nullable 32-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableInt"/>
        public RealmInteger<int>? AsNullableIntRealmInteger() => Type == RealmValueType.Null ? null : (RealmInteger<int>?)AsIntRealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="RealmInteger{T}"/>. It offers Increment/Decrement API that preserve intent when merging
        /// conflicts.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Int"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns> A nullable 64-bit <see cref="RealmInteger{T}"/> representing the value stored in the database.</returns>
        /// <seealso cref="AsNullableLong"/>
        public RealmInteger<long>? AsNullableLongRealmInteger() => Type == RealmValueType.Null ? null : (RealmInteger<long>?)AsLongRealmInteger();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="float"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Float"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 32-bit floating point number representing the value stored in the database.</returns>
        public float? AsNullableFloat() => Type == RealmValueType.Null ? null : (float?)AsFloat();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="double"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Double"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 64-bit floating point number representing the value stored in the database.</returns>
        public double? AsNullableDouble() => Type == RealmValueType.Null ? null : (double?)AsDouble();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="bool"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Bool"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable boolean representing the value stored in the database.</returns>
        public bool? AsNullableBool() => Type == RealmValueType.Null ? null : (bool?)AsBool();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Date"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable DateTimeOffset value representing the value stored in the database.</returns>
        public DateTimeOffset? AsNullableDate() => Type == RealmValueType.Null ? null : (DateTimeOffset?)AsDate();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="decimal"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Decimal128"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 96-bit decimal number representing the value stored in the database.</returns>
        public decimal? AsNullableDecimal() => Type == RealmValueType.Null ? null : (decimal?)AsDecimal();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="Decimal128"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Date"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable 128-bit decimal number representing the value stored in the database.</returns>
        public Decimal128? AsNullableDecimal128() => Type == RealmValueType.Null ? null : (Decimal128?)AsDecimal128();

        /// <summary>
        /// Returns the stored value as a nullable <see cref="MongoDB.Bson.ObjectId"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.ObjectId"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>A nullable ObjectId representing the value stored in the database.</returns>
        public ObjectId? AsNullableObjectId() => Type == RealmValueType.Null ? null : (ObjectId?)AsObjectId();

        /// <summary>
        /// Returns the stored value as an array of bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.Data"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// An array of bytes representing the value stored in the database. It will be <c>null</c> if <see cref="Type"/> is <see cref="RealmValueType.Null"/>.
        /// </returns>
        public byte[] AsData()
        {
            if (Type == RealmValueType.Null)
            {
                return null;
            }

            EnsureType("byte[]", RealmValueType.Data);
            return _dataValue;
        }

        /// <summary>
        /// Returns the stored value as a string.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying value is not of type <see cref="RealmValueType.String"/> or <see cref="RealmValueType.Null"/>.
        /// </exception>
        /// <returns>
        /// A string representing the value stored in the database. It will be <c>null</c> if <see cref="Type"/> is <see cref="RealmValueType.Null"/>.
        /// </returns>
        public string AsString()
        {
            if (Type == RealmValueType.Null)
            {
                return null;
            }

            EnsureType("string", RealmValueType.String);
            return _stringValue;
        }

        /// <summary>
        /// Returns the stored value converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to which to convert the value.</typeparam>
        /// <exception cref="InvalidCastException">Thrown if the type is not convertible to <typeparamref name="T"/>.</exception>
        /// <returns>The underlying value converted to <typeparamref name="T"/>.</returns>
        public T As<T>()
        {
            if (Type == RealmValueType.Int)
            {
                return Operator.Convert<long, T>(AsLong());
            }

            if (Type == RealmValueType.Decimal128)
            {
                return Operator.Convert<Decimal128, T>(AsDecimal128());
            }

            return (T)AsAny();
        }

        /// <summary>
        /// Returns the stored value boxed in <see cref="object"/>.
        /// </summary>
        /// <returns>The underlying value.</returns>
        public object AsAny()
        {
            return Type switch
            {
                RealmValueType.Null => null,
                RealmValueType.Int => AsLong(),
                RealmValueType.Bool => AsBool(),
                RealmValueType.String => AsString(),
                RealmValueType.Data => AsData(),
                RealmValueType.Date => AsDate(),
                RealmValueType.Float => AsFloat(),
                RealmValueType.Double => AsDouble(),
                RealmValueType.Decimal128 => AsDecimal128(),
                RealmValueType.ObjectId => AsObjectId(),
                RealmValueType.Object => throw new NotImplementedException(),
                _ => throw new NotSupportedException($"RealmValue of type {Type} is not supported."),
            };
        }

        /// <summary>
        /// Returns the string representation of this <see cref="RealmValue"/>.
        /// </summary>
        /// <returns>A string describing the value.</returns>
        public override string ToString() => AsAny()?.ToString() ?? "<null>";

        public static implicit operator char(RealmValue val) => val.AsChar();

        public static implicit operator byte(RealmValue val) => val.AsByte();

        public static implicit operator short(RealmValue val) => val.AsShort();

        public static implicit operator int(RealmValue val) => val.AsInt();

        public static implicit operator long(RealmValue val) => val.AsLong();

        public static implicit operator float(RealmValue val) => val.AsFloat();

        public static implicit operator double(RealmValue val) => val.AsDouble();

        public static implicit operator bool(RealmValue val) => val.AsBool();

        public static implicit operator DateTimeOffset(RealmValue val) => val.AsDate();

        public static implicit operator decimal(RealmValue val) => val.AsDecimal();

        public static implicit operator Decimal128(RealmValue val) => val.AsDecimal128();

        public static implicit operator ObjectId(RealmValue val) => val.AsObjectId();

        public static implicit operator char?(RealmValue val) => val.AsNullableChar();

        public static implicit operator byte?(RealmValue val) => val.AsNullableByte();

        public static implicit operator short?(RealmValue val) => val.AsNullableShort();

        public static implicit operator int?(RealmValue val) => val.AsNullableInt();

        public static implicit operator long?(RealmValue val) => val.AsNullableLong();

        public static implicit operator float?(RealmValue val) => val.AsNullableFloat();

        public static implicit operator double?(RealmValue val) => val.AsNullableDouble();

        public static implicit operator bool?(RealmValue val) => val.AsNullableBool();

        public static implicit operator DateTimeOffset?(RealmValue val) => val.AsNullableDate();

        public static implicit operator decimal?(RealmValue val) => val.AsNullableDecimal();

        public static implicit operator Decimal128?(RealmValue val) => val.AsNullableDecimal128();

        public static implicit operator ObjectId?(RealmValue val) => val.AsNullableObjectId();

        public static implicit operator RealmInteger<byte>(RealmValue val) => val.AsByteRealmInteger();

        public static implicit operator RealmInteger<short>(RealmValue val) => val.AsShortRealmInteger();

        public static implicit operator RealmInteger<int>(RealmValue val) => val.AsIntRealmInteger();

        public static implicit operator RealmInteger<long>(RealmValue val) => val.AsLongRealmInteger();

        public static implicit operator RealmInteger<byte>?(RealmValue val) => val.AsNullableByteRealmInteger();

        public static implicit operator RealmInteger<short>?(RealmValue val) => val.AsNullableShortRealmInteger();

        public static implicit operator RealmInteger<int>?(RealmValue val) => val.AsNullableIntRealmInteger();

        public static implicit operator RealmInteger<long>?(RealmValue val) => val.AsNullableLongRealmInteger();

        public static implicit operator byte[](RealmValue val) => val.AsData();

        public static implicit operator string(RealmValue val) => val.AsString();

        public static implicit operator RealmValue(char val) => Int(val);

        public static implicit operator RealmValue(byte val) => Int(val);

        public static implicit operator RealmValue(short val) => Int(val);

        public static implicit operator RealmValue(int val) => Int(val);

        public static implicit operator RealmValue(long val) => Int(val);

        public static implicit operator RealmValue(float val) => Float(val);

        public static implicit operator RealmValue(double val) => Double(val);

        public static implicit operator RealmValue(bool val) => Bool(val);

        public static implicit operator RealmValue(DateTimeOffset val) => Date(val);

        public static implicit operator RealmValue(decimal val) => Decimal(val);

        public static implicit operator RealmValue(Decimal128 val) => Decimal(val);

        public static implicit operator RealmValue(ObjectId val) => ObjectId(val);

        public static implicit operator RealmValue(char? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(byte? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(short? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(int? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(long? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(float? val) => val == null ? Null() : Float(val.Value);

        public static implicit operator RealmValue(double? val) => val == null ? Null() : Double(val.Value);

        public static implicit operator RealmValue(bool? val) => val == null ? Null() : Bool(val.Value);

        public static implicit operator RealmValue(DateTimeOffset? val) => val == null ? Null() : Date(val.Value);

        public static implicit operator RealmValue(decimal? val) => val == null ? Null() : Decimal(val.Value);

        public static implicit operator RealmValue(Decimal128? val) => val == null ? Null() : Decimal(val.Value);

        public static implicit operator RealmValue(ObjectId? val) => val == null ? Null() : ObjectId(val.Value);

        public static implicit operator RealmValue(RealmInteger<byte> val) => Int(val);

        public static implicit operator RealmValue(RealmInteger<short> val) => Int(val);

        public static implicit operator RealmValue(RealmInteger<int> val) => Int(val);

        public static implicit operator RealmValue(RealmInteger<long> val) => Int(val);

        public static implicit operator RealmValue(RealmInteger<byte>? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(RealmInteger<short>? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(RealmInteger<int>? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(RealmInteger<long>? val) => val == null ? Null() : Int(val.Value);

        public static implicit operator RealmValue(byte[] val) => Data(val);

        public static implicit operator RealmValue(string val) => String(val);

        private void EnsureType(string target, RealmValueType type)
        {
            if (Type != type)
            {
                throw new InvalidOperationException($"Can't cast to {target} since the underlying value is {Type}");
            }
        }

        internal struct HandlesToCleanup
        {
            private readonly GCHandle _handle;
            private readonly byte[] _buffer;

            public HandlesToCleanup(GCHandle handle, byte[] buffer = null)
            {
                _handle = handle;
                _buffer = buffer;
            }

            public void Dispose()
            {
                _handle.Free();
                if (_buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                }
            }
        }
    }
}
