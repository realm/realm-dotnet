// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    internal record ClassInfo
    {
        public string Name { get; set; }

        public bool IsEmbedded { get; set; }

        public string MapTo { get; set; }

        public string Namespace { get; set; }

        public Accessibility Accessibility { get; set; }

        public ITypeSymbol TypeSymbol { get; set; }

        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();

        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        public PropertyInfo PrimaryKey => Properties.FirstOrDefault(p => p.IsPrimaryKey);
    }

    internal record PropertyInfo
    {
        public string Name { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsNullable { get; set; }

        public string MapTo { get; set; }

        public string Backlink { get; set; }

        public PropertyTypeInfo TypeInfo { get; set; }

        public Accessibility Accessibility { get; set; }
    }

    internal abstract record PropertyTypeInfo
    {
        private static HashSet<SimpleTypeEnum?> _indexableTypes = new()
        {
            SimpleTypeEnum.Int,
            SimpleTypeEnum.Bool,
            SimpleTypeEnum.String,
            SimpleTypeEnum.ObjectId,
            SimpleTypeEnum.Guid,
            SimpleTypeEnum.Date,
        };

        private static HashSet<SimpleTypeEnum?> _primaryKeyTypes = new()
        {
            SimpleTypeEnum.Int,
            SimpleTypeEnum.String,
            SimpleTypeEnum.ObjectId,
            SimpleTypeEnum.Guid,
        };

        private static HashSet<SimpleTypeEnum?> _requiredTypes = new()
        {
            SimpleTypeEnum.String,
            SimpleTypeEnum.Data,
        };

        public virtual SimpleTypeEnum? SimpleType { get; set; } = null;

        public virtual CollectionTypeEnum? CollectionType { get; set; } = null;

        public virtual bool IsRealmInteger { get; set; } = false;

        public virtual bool IsIQueryable { get; set; } = false;

        // NullabilityAnnotation != None for all value types and for ref types with nullability annotations enabled
        public virtual NullableAnnotation NullableAnnotation { get; set; } = NullableAnnotation.None;

        public bool HasNullabilityAnnotation => NullableAnnotation == NullableAnnotation.Annotated;

        public ITypeSymbol TypeSymbol { get; set; }

        public virtual PropertyTypeInfo InternalType { get; set; } = null;

        public bool IsCollection => CollectionType != null;

        public bool IsListOrSet => IsList || IsSet;

        public bool IsSimpleType => SimpleType != null;

        public bool IsSet => CollectionType == CollectionTypeEnum.Set;

        public bool IsList => CollectionType == CollectionTypeEnum.List;

        public bool IsDictionary => CollectionType == CollectionTypeEnum.Dictionary;

        public bool IsUnsupported => this is UnsupportedTypeInfo;


        public virtual string TypeString => TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public static PropertyTypeInfo Unsupported => new UnsupportedTypeInfo();

        public static PropertyTypeInfo List => new ListTypeInfo();

        public static PropertyTypeInfo Set => new SetTypeInfo();

        public static PropertyTypeInfo Dictionary => new DictionaryTypeInfo();

        public static PropertyTypeInfo Int => new SimpleTypeInfo(SimpleTypeEnum.Int);

        public static PropertyTypeInfo Bool => new SimpleTypeInfo(SimpleTypeEnum.Bool);

        public static PropertyTypeInfo String => new SimpleTypeInfo(SimpleTypeEnum.String);

        public static PropertyTypeInfo Data => new SimpleTypeInfo(SimpleTypeEnum.Data);

        public static PropertyTypeInfo Date => new SimpleTypeInfo(SimpleTypeEnum.Date);

        public static PropertyTypeInfo Float => new SimpleTypeInfo(SimpleTypeEnum.Float);

        public static PropertyTypeInfo Double => new SimpleTypeInfo(SimpleTypeEnum.Double);

        public static PropertyTypeInfo Object => new SimpleTypeInfo(SimpleTypeEnum.Object);

        public static PropertyTypeInfo RealmValue => new SimpleTypeInfo(SimpleTypeEnum.RealmValue);

        public static PropertyTypeInfo ObjectId => new SimpleTypeInfo(SimpleTypeEnum.ObjectId);

        public static PropertyTypeInfo Decimal => new SimpleTypeInfo(SimpleTypeEnum.Decimal);

        public static PropertyTypeInfo Guid => new SimpleTypeInfo(SimpleTypeEnum.Guid);

        public static PropertyTypeInfo RealmInteger => new RealmIntegerTypeInfo();

        public static PropertyTypeInfo IQueryable => new IQueryableTypeInfo();

        public bool IsSupportedIndexType()
        {
            if (IsRealmInteger)
            {
                if (HasNullabilityAnnotation)
                {
                    return false;
                }

                return InternalType.IsSupportedIndexType();
            }

            if (SimpleType == SimpleTypeEnum.String)
            {
                return true;
            }

            if (_indexableTypes.Contains(SimpleType))
            {
                return !HasNullabilityAnnotation;
            }

            return false;
        }

        public bool IsSupportedPrimaryKeyType()
        {
            return _primaryKeyTypes.Contains(SimpleType);
        }

        public bool IsSupportedRequiredType()
        {
            if (IsListOrSet)
            {
                return InternalType.IsSupportedRequiredType();
            }

            return _requiredTypes.Contains(SimpleType);
        }

        public bool SupportsNullability()
        {
            if (NullableAnnotation == NullableAnnotation.Annotated &&
                (IsCollection || IsIQueryable || SimpleType == SimpleTypeEnum.RealmValue))
            {
                return false;
            }

            if (NullableAnnotation == NullableAnnotation.NotAnnotated &&
                (SimpleType == SimpleTypeEnum.Object))
            {
                return false;
            }

            return true;
        }
        

        public sealed override string ToString()
        {
            if (this is UnsupportedTypeInfo)
            {
                return "Unsupported";
            }

            var nullabilityString = NullableAnnotation == NullableAnnotation.Annotated ? "?" : "";
            string desc;
            if (IsCollection)
            {
                desc = $"{CollectionType}{nullabilityString} of {InternalType}";
            }
            else if (IsRealmInteger)
            {
                desc = $"RealmInteger{nullabilityString} of {InternalType}";
            }
            else if (SimpleType == SimpleTypeEnum.Object)
            {
                desc = $"Object of type {TypeSymbol.Name}{nullabilityString} of";
            }
            else
            {
                desc = $"Scalar of type {SimpleType}{nullabilityString}";
            }

            return $"{TypeSymbol.ToReadableName()} ({desc})";
        }
    }

    internal sealed record UnsupportedTypeInfo : PropertyTypeInfo
    {
    }

    internal abstract record CollectionTypeInfo : PropertyTypeInfo
    {
    }

    internal record ListTypeInfo : CollectionTypeInfo
    {
        public override CollectionTypeEnum? CollectionType => CollectionTypeEnum.List;
    }

    internal record SetTypeInfo : CollectionTypeInfo
    {
        public override CollectionTypeEnum? CollectionType => CollectionTypeEnum.Set;
    }

    internal record DictionaryTypeInfo : CollectionTypeInfo
    {
        public override CollectionTypeEnum? CollectionType => CollectionTypeEnum.Dictionary;
    }

    internal record RealmIntegerTypeInfo : PropertyTypeInfo
    {
        public override bool IsRealmInteger => true;
    }

    internal record IQueryableTypeInfo : PropertyTypeInfo
    {
        public override bool IsIQueryable => true;
    }

    internal record SimpleTypeInfo : PropertyTypeInfo
    {
        public SimpleTypeInfo(SimpleTypeEnum type)
        {
            SimpleType = type;
        }
    }

    internal enum CollectionTypeEnum
    {
        List,
        Set,
        Dictionary
    }

    internal enum SimpleTypeEnum
    {
        Int,
        Bool,
        String,
        Data,
        Date,
        Float,
        Double,
        Object,
        RealmValue,
        ObjectId,
        Decimal,
        Guid,
    }
}
