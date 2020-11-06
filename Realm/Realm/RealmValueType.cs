using System;
using Realms.Schema;

namespace Realms
{
    public enum RealmValueType : byte
    {
        Null,
        Int,
        Bool,
        String,
        Data,
        Date,
        Float,
        Double,
        Decimal128,
        ObjectId,
        Object,
    }

    internal static class TempExtensions
    {
        public static RealmValueType ToRealmValueType(this PropertyType type)
        {
            switch (type.UnderlyingType())
            {
                case PropertyType.Int:
                    return RealmValueType.Int;
                case PropertyType.Bool:
                    return RealmValueType.Bool;
                case PropertyType.String:
                    return RealmValueType.String;
                case PropertyType.Data:
                    return RealmValueType.Data;
                case PropertyType.Date:
                    return RealmValueType.Date;
                case PropertyType.Float:
                    return RealmValueType.Float;
                case PropertyType.Double:
                    return RealmValueType.Double;
                case PropertyType.Object:
                    return RealmValueType.Object;
                case PropertyType.ObjectId:
                    return RealmValueType.ObjectId;
                case PropertyType.Decimal:
                    return RealmValueType.Decimal128;
                default:
                    throw new NotSupportedException($"The type {type} can't be mapped to RealmValueType.");
            }
        }
    }
}
