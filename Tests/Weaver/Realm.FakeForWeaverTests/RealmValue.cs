using System;
using MongoDB.Bson;

namespace Realms
{
    public readonly struct RealmValue
    {
        public RealmValueType Type { get; }

        public static implicit operator char(RealmValue val) => default;

        public static implicit operator byte(RealmValue val) => default;

        public static implicit operator short(RealmValue val) => default;

        public static implicit operator int(RealmValue val) => default;

        public static implicit operator long(RealmValue val) => default;

        public static implicit operator float(RealmValue val) => default;

        public static implicit operator double(RealmValue val) => default;

        public static implicit operator bool(RealmValue val) => default;

        public static implicit operator DateTimeOffset(RealmValue val) => default;

        public static implicit operator decimal(RealmValue val) => default;

        public static implicit operator Decimal128(RealmValue val) => default;

        public static implicit operator ObjectId(RealmValue val) => default;

        public static implicit operator char?(RealmValue val) => default;

        public static implicit operator byte?(RealmValue val) => default;

        public static implicit operator short?(RealmValue val) => default;

        public static implicit operator int?(RealmValue val) => default;

        public static implicit operator long?(RealmValue val) => default;

        public static implicit operator float?(RealmValue val) => default;

        public static implicit operator double?(RealmValue val) => default;

        public static implicit operator bool?(RealmValue val) => default;

        public static implicit operator DateTimeOffset?(RealmValue val) => default;

        public static implicit operator decimal?(RealmValue val) => default;

        public static implicit operator Decimal128?(RealmValue val) => default;

        public static implicit operator ObjectId?(RealmValue val) => default;

        public static implicit operator RealmInteger<byte>(RealmValue val) => default;

        public static implicit operator RealmInteger<short>(RealmValue val) => default;

        public static implicit operator RealmInteger<int>(RealmValue val) => default;

        public static implicit operator RealmInteger<long>(RealmValue val) => default;

        public static implicit operator RealmInteger<byte>?(RealmValue val) => default;

        public static implicit operator RealmInteger<short>?(RealmValue val) => default;

        public static implicit operator RealmInteger<int>?(RealmValue val) => default;

        public static implicit operator RealmInteger<long>?(RealmValue val) => default;

        public static implicit operator byte[](RealmValue val) => default;

        public static implicit operator string(RealmValue val) => default;

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

        public static implicit operator RealmValue(RealmInteger<byte> val) => default;

        public static implicit operator RealmValue(RealmInteger<short> val) => default;

        public static implicit operator RealmValue(RealmInteger<int> val) => default;

        public static implicit operator RealmValue(RealmInteger<long> val) => default;

        public static implicit operator RealmValue(RealmInteger<byte>? val) => default;

        public static implicit operator RealmValue(RealmInteger<short>? val) => default;

        public static implicit operator RealmValue(RealmInteger<int>? val) => default;

        public static implicit operator RealmValue(RealmInteger<long>? val) => default;

        public static implicit operator RealmValue(byte[] val) => default;

        public static implicit operator RealmValue(string val) => default;
    }
}
