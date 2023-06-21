using System;
using MongoDB.Bson;

namespace Realms
{
    public readonly struct RealmValue
    {
        public RealmValueType Type { get; }

        public static explicit operator char(RealmValue val) => default;

        public static explicit operator byte(RealmValue val) => default;

        public static explicit operator short(RealmValue val) => default;

        public static explicit operator int(RealmValue val) => default;

        public static explicit operator long(RealmValue val) => default;

        public static explicit operator float(RealmValue val) => default;

        public static explicit operator double(RealmValue val) => default;

        public static explicit operator bool(RealmValue val) => default;

        public static explicit operator DateTimeOffset(RealmValue val) => default;

        public static explicit operator decimal(RealmValue val) => default;

        public static explicit operator Decimal128(RealmValue val) => default;

        public static explicit operator ObjectId(RealmValue val) => default;

        public static explicit operator Guid(RealmValue val) => default;

        public static explicit operator char?(RealmValue val) => default;

        public static explicit operator byte?(RealmValue val) => default;

        public static explicit operator short?(RealmValue val) => default;

        public static explicit operator int?(RealmValue val) => default;

        public static explicit operator long?(RealmValue val) => default;

        public static explicit operator float?(RealmValue val) => default;

        public static explicit operator double?(RealmValue val) => default;

        public static explicit operator bool?(RealmValue val) => default;

        public static explicit operator DateTimeOffset?(RealmValue val) => default;

        public static explicit operator decimal?(RealmValue val) => default;

        public static explicit operator Decimal128?(RealmValue val) => default;

        public static explicit operator ObjectId?(RealmValue val) => default;

        public static explicit operator Guid?(RealmValue val) => default;

        public static explicit operator RealmInteger<byte>(RealmValue val) => default;

        public static explicit operator RealmInteger<short>(RealmValue val) => default;

        public static explicit operator RealmInteger<int>(RealmValue val) => default;

        public static explicit operator RealmInteger<long>(RealmValue val) => default;

        public static explicit operator RealmInteger<byte>?(RealmValue val) => default;

        public static explicit operator RealmInteger<short>?(RealmValue val) => default;

        public static explicit operator RealmInteger<int>?(RealmValue val) => default;

        public static explicit operator RealmInteger<long>?(RealmValue val) => default;

        public static explicit operator byte[]?(RealmValue val) => default;

        public static explicit operator string?(RealmValue val) => default;

        public static explicit operator RealmObjectBase?(RealmValue val) => default;

        public static implicit operator RealmValue(char val) => default;

        public static implicit operator RealmValue(byte val) => default;

        public static implicit operator RealmValue(short val) => default;

        public static implicit operator RealmValue(int val) => default;

        public static implicit operator RealmValue(long val) => default;

        public static implicit operator RealmValue(float val) => default;

        public static implicit operator RealmValue(double val) => default;

        public static implicit operator RealmValue(bool val) => default;

        public static implicit operator RealmValue(DateTimeOffset val) => default;

        public static implicit operator RealmValue(decimal val) => default;

        public static implicit operator RealmValue(Decimal128 val) => default;

        public static implicit operator RealmValue(ObjectId val) => default;

        public static implicit operator RealmValue(Guid val) => default;

        public static implicit operator RealmValue(char? val) => default;

        public static implicit operator RealmValue(byte? val) => default;

        public static implicit operator RealmValue(short? val) => default;

        public static implicit operator RealmValue(int? val) => default;

        public static implicit operator RealmValue(long? val) => default;

        public static implicit operator RealmValue(float? val) => default;

        public static implicit operator RealmValue(double? val) => default;

        public static implicit operator RealmValue(bool? val) => default;

        public static implicit operator RealmValue(DateTimeOffset? val) => default;

        public static implicit operator RealmValue(decimal? val) => default;

        public static implicit operator RealmValue(Decimal128? val) => default;

        public static implicit operator RealmValue(ObjectId? val) => default;

        public static implicit operator RealmValue(Guid? val) => default;

        public static implicit operator RealmValue(RealmInteger<byte> val) => default;

        public static implicit operator RealmValue(RealmInteger<short> val) => default;

        public static implicit operator RealmValue(RealmInteger<int> val) => default;

        public static implicit operator RealmValue(RealmInteger<long> val) => default;

        public static implicit operator RealmValue(RealmInteger<byte>? val) => default;

        public static implicit operator RealmValue(RealmInteger<short>? val) => default;

        public static implicit operator RealmValue(RealmInteger<int>? val) => default;

        public static implicit operator RealmValue(RealmInteger<long>? val) => default;

        public static implicit operator RealmValue(byte[]? val) => default;

        public static implicit operator RealmValue(string? val) => default;

        public static implicit operator RealmValue(RealmObjectBase val) => default;
    }
}
