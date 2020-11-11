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
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Bson;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    public struct RealmValue
    {
        private PrimitiveValue _primitiveValue;
        private string _stringValue;
        private byte[] _dataValue;

        private ObjectHandle _objectHandle;
        private IntPtr _propertyIndex;

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

        public static RealmValue Null() => new RealmValue(PrimitiveValue.Null());

        public static RealmValue Bool(bool value) => new RealmValue(PrimitiveValue.Bool(value));

        public static RealmValue NullableBool(bool? value) => new RealmValue(PrimitiveValue.NullableBool(value));

        public static RealmValue Int(long value) => new RealmValue(PrimitiveValue.Int(value));

        public static RealmValue NullableInt(long? value) => new RealmValue(PrimitiveValue.NullableInt(value));

        public static RealmValue Float(float value) => new RealmValue(PrimitiveValue.Float(value));

        public static RealmValue NullableFloat(float? value) => new RealmValue(PrimitiveValue.NullableFloat(value));

        public static RealmValue Double(double value) => new RealmValue(PrimitiveValue.Double(value));

        public static RealmValue NullableDouble(double? value) => new RealmValue(PrimitiveValue.NullableDouble(value));

        public static RealmValue Date(DateTimeOffset value) => new RealmValue(PrimitiveValue.Date(value));

        public static RealmValue NullableDate(DateTimeOffset? value) => new RealmValue(PrimitiveValue.NullableDate(value));

        public static RealmValue Decimal(Decimal128 value) => new RealmValue(PrimitiveValue.Decimal(value));

        public static RealmValue NullableDecimal(Decimal128? value) => new RealmValue(PrimitiveValue.NullableDecimal(value));

        public static RealmValue ObjectId(ObjectId value) => new RealmValue(PrimitiveValue.ObjectId(value));

        public static RealmValue NullableObjectId(ObjectId? value) => new RealmValue(PrimitiveValue.NullableObjectId(value));

        public static RealmValue Data(byte[] value) => new RealmValue(value);

        public static RealmValue String(string value) => new RealmValue(value);

        public static RealmValue Create<T>(T value, RealmValueType type)
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

        internal (PrimitiveValue Value, GCHandle? GCHandle) ToNative()
        {
            switch (Type)
            {
                case RealmValueType.String:
                    if (_stringValue == null)
                    {
                        return (PrimitiveValue.Null(), null);
                    }

                    // TODO-niki: use memory pool to avoid allocating/deallocating all the time
                    var bytes = Encoding.UTF8.GetBytes(_stringValue);
                    var stringHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    return (PrimitiveValue.String(stringHandle.AddrOfPinnedObject(), bytes.Length), stringHandle);
                case RealmValueType.Data:
                    if (_dataValue == null)
                    {
                        return (PrimitiveValue.Null(), null);
                    }

                    var handle = GCHandle.Alloc(_dataValue, GCHandleType.Pinned);
                    return (PrimitiveValue.Data(handle.AddrOfPinnedObject(), _dataValue?.Length ?? 0), handle);
                default:
                    return (_primitiveValue, null);
            }
        }

        public char AsChar()
        {
            EnsureType("char", RealmValueType.Int);
            return (char)_primitiveValue.AsInt();
        }

        public byte AsByte()
        {
            EnsureType("byte", RealmValueType.Int);
            return (byte)_primitiveValue.AsInt();
        }

        public short AsInt16()
        {
            EnsureType("short", RealmValueType.Int);
            return (short)_primitiveValue.AsInt();
        }

        public int AsInt32()
        {
            EnsureType("int", RealmValueType.Int);
            return (int)_primitiveValue.AsInt();
        }

        public long AsInt64()
        {
            EnsureType("long", RealmValueType.Int);
            return _primitiveValue.AsInt();
        }

        public float AsFloat()
        {
            EnsureType("float", RealmValueType.Float);
            return _primitiveValue.AsFloat();
        }

        public double AsDouble()
        {
            EnsureType("double", RealmValueType.Double);
            return _primitiveValue.AsDouble();
        }

        public bool AsBool()
        {
            EnsureType("bool", RealmValueType.Bool);
            return _primitiveValue.AsBool();
        }

        public DateTimeOffset AsDate()
        {
            EnsureType("date", RealmValueType.Date);
            return _primitiveValue.AsDate();
        }

        public decimal AsDecimal()
        {
            EnsureType("decimal", RealmValueType.Decimal128);
            return (decimal)_primitiveValue.AsDecimal();
        }

        public Decimal128 AsDecimal128()
        {
            EnsureType("Decimal128", RealmValueType.Decimal128);
            return _primitiveValue.AsDecimal();
        }

        public ObjectId AsObjectId()
        {
            EnsureType("ObjectId", RealmValueType.ObjectId);
            return _primitiveValue.AsObjectId();
        }

        public RealmInteger<byte> AsByteInteger() => new RealmInteger<byte>(AsByte(), _objectHandle, _propertyIndex);

        public T As<T>()
        {
            if (Type == RealmValueType.Int)
            {
                return Operator.Convert<long, T>(AsInt64());
            }

            if (Type == RealmValueType.Decimal128)
            {
                return Operator.Convert<Decimal128, T>(AsDecimal128());
            }

            return (T)AsObject();
        }

        public RealmInteger<short> AsInt16Integer() => new RealmInteger<short>(AsInt16(), _objectHandle, _propertyIndex);

        public RealmInteger<int> AsInt32Integer() => new RealmInteger<int>(AsInt32(), _objectHandle, _propertyIndex);

        public RealmInteger<long> AsInt64Integer() => new RealmInteger<long>(AsInt64(), _objectHandle, _propertyIndex);

        public char? AsNullableChar() => Type == RealmValueType.Null ? null : (char?)AsChar();

        public byte? AsNullableByte() => Type == RealmValueType.Null ? null : (byte?)AsByte();

        public short? AsNullableInt16() => Type == RealmValueType.Null ? null : (short?)AsInt16();

        public int? AsNullableInt32() => Type == RealmValueType.Null ? null : (int?)AsInt32();

        public long? AsNullableInt64() => Type == RealmValueType.Null ? null : (long?)AsInt64();

        public RealmInteger<byte>? AsNullableByteInteger() => Type == RealmValueType.Null ? null : (RealmInteger<byte>?)AsByteInteger();

        public RealmInteger<short>? AsNullableInt16Integer() => Type == RealmValueType.Null ? null : (RealmInteger<short>?)AsInt16Integer();

        public RealmInteger<int>? AsNullableInt32Integer() => Type == RealmValueType.Null ? null : (RealmInteger<int>?)AsInt32Integer();

        public RealmInteger<long>? AsNullableInt64Integer() => Type == RealmValueType.Null ? null : (RealmInteger<long>?)AsInt64Integer();

        public float? AsNullableFloat() => Type == RealmValueType.Null ? null : (float?)AsFloat();

        public double? AsNullableDouble() => Type == RealmValueType.Null ? null : (double?)AsDouble();

        public bool? AsNullableBool() => Type == RealmValueType.Null ? null : (bool?)AsBool();

        public DateTimeOffset? AsNullableDate() => Type == RealmValueType.Null ? null : (DateTimeOffset?)AsDate();

        public decimal? AsNullableDecimal() => Type == RealmValueType.Null ? null : (decimal?)AsDecimal();

        public Decimal128? AsNullableDecimal128() => Type == RealmValueType.Null ? null : (Decimal128?)AsDecimal128();

        public ObjectId? AsNullableObjectId() => Type == RealmValueType.Null ? null : (ObjectId?)AsObjectId();

        public byte[] AsData()
        {
            if (Type == RealmValueType.Null)
            {
                return null;
            }

            EnsureType("byte[]", RealmValueType.Data);
            return _dataValue;
        }

        public string AsString()
        {
            if (Type == RealmValueType.Null)
            {
                return null;
            }

            EnsureType("string", RealmValueType.String);
            return _stringValue;
        }

        public object AsObject()
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
                RealmValueType.Object => throw new NotImplementedException(),
                _ => throw new NotSupportedException($"RealmValue of type {Type} is not supported."),
            };
        }

        public static implicit operator char(RealmValue val) => val.AsChar();

        public static implicit operator byte(RealmValue val) => val.AsByte();

        public static implicit operator short(RealmValue val) => val.AsInt16();

        public static implicit operator int(RealmValue val) => val.AsInt32();

        public static implicit operator long(RealmValue val) => val.AsInt64();

        public static implicit operator float(RealmValue val) => val.AsFloat();

        public static implicit operator double(RealmValue val) => val.AsDouble();

        public static implicit operator bool(RealmValue val) => val.AsBool();

        public static implicit operator DateTimeOffset(RealmValue val) => val.AsDate();

        public static implicit operator decimal(RealmValue val) => val.AsDecimal();

        public static implicit operator Decimal128(RealmValue val) => val.AsDecimal128();

        public static implicit operator ObjectId(RealmValue val) => val.AsObjectId();

        public static implicit operator char?(RealmValue val) => val.AsNullableChar();

        public static implicit operator byte?(RealmValue val) => val.AsNullableByte();

        public static implicit operator short?(RealmValue val) => val.AsNullableInt16();

        public static implicit operator int?(RealmValue val) => val.AsNullableInt32();

        public static implicit operator long?(RealmValue val) => val.AsNullableInt64();

        public static implicit operator float?(RealmValue val) => val.AsNullableFloat();

        public static implicit operator double?(RealmValue val) => val.AsNullableDouble();

        public static implicit operator bool?(RealmValue val) => val.AsNullableBool();

        public static implicit operator DateTimeOffset?(RealmValue val) => val.AsNullableDate();

        public static implicit operator decimal?(RealmValue val) => val.AsNullableDecimal();

        public static implicit operator Decimal128?(RealmValue val) => val.AsNullableDecimal128();

        public static implicit operator ObjectId?(RealmValue val) => val.AsNullableObjectId();

        public static implicit operator RealmInteger<byte>(RealmValue val) => val.AsByteInteger();

        public static implicit operator RealmInteger<short>(RealmValue val) => val.AsInt16Integer();

        public static implicit operator RealmInteger<int>(RealmValue val) => val.AsInt32Integer();

        public static implicit operator RealmInteger<long>(RealmValue val) => val.AsInt64Integer();

        public static implicit operator RealmInteger<byte>?(RealmValue val) => val.AsNullableByteInteger();

        public static implicit operator RealmInteger<short>?(RealmValue val) => val.AsNullableInt16Integer();

        public static implicit operator RealmInteger<int>?(RealmValue val) => val.AsNullableInt32Integer();

        public static implicit operator RealmInteger<long>?(RealmValue val) => val.AsNullableInt64Integer();

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
    }
}
