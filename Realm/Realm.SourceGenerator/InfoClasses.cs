﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

        public List<PropertyInfo> Properties { get; } = new();

        public List<Diagnostic> Diagnostics { get; } = new();

        public List<string> Usings { get; } = new();

        public List<EnclosingClassInfo> EnclosingClasses { get; } = new();

        public bool HasParameterlessConstructor { get; set; }

        public PropertyInfo PrimaryKey => Properties.FirstOrDefault(p => p.IsPrimaryKey);
    }

    internal record EnclosingClassInfo
    {
        public string Name { get; set; }

        public Accessibility Accessibility { get; set; }
    }

    internal record PropertyInfo
    {
        public string Name { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimaryKey { get; set; }

        public string MapTo { get; set; }

        public string Backlink { get; set; }

        public PropertyTypeInfo TypeInfo { get; set; }

        public Accessibility Accessibility { get; set; }

        public string Initializer { get; set; }
    }

    internal abstract record PropertyTypeInfo
    {
        private static HashSet<ScalarType?> _indexableTypes = new()
        {
            SourceGenerator.ScalarType.Int,
            SourceGenerator.ScalarType.Bool,
            SourceGenerator.ScalarType.String,
            SourceGenerator.ScalarType.ObjectId,
            SourceGenerator.ScalarType.Guid,
            SourceGenerator.ScalarType.Date,
        };

        private static HashSet<ScalarType?> _primaryKeyTypes = new()
        {
            SourceGenerator.ScalarType.Int,
            SourceGenerator.ScalarType.String,
            SourceGenerator.ScalarType.ObjectId,
            SourceGenerator.ScalarType.Guid,
        };

        private static HashSet<ScalarType?> _requiredTypes = new()
        {
            SourceGenerator.ScalarType.String,
            SourceGenerator.ScalarType.Data,
        };

        public virtual ScalarType? ScalarType { get; set; }

        public virtual CollectionType? CollectionType { get; }

        public virtual bool IsRealmInteger { get; set; }

        public virtual bool IsIQueryable { get; set; }

        // NullabilityAnnotation != None for all value types and for ref types with nullability annotations enabled
        public NullableAnnotation NullableAnnotation { get; set; } = NullableAnnotation.None;

        public bool IsNullable => NullableAnnotation == NullableAnnotation.None || NullableAnnotation == NullableAnnotation.Annotated;

        public bool HasNullabilityAnnotation => NullableAnnotation == NullableAnnotation.Annotated;

        public string Namespace { get; set; }

        public ITypeSymbol TypeSymbol { get; set; }

        // This includes the eventual nullability annotation
        public ITypeSymbol CompleteTypeSymbol { get; set; }

        public PropertyTypeInfo InternalType { get; set; }

        public bool IsCollection => CollectionType != null;

        public bool IsListOrSet => IsList || IsSet;

        public bool IsSimpleType => ScalarType != null;

        public bool IsSet => CollectionType == SourceGenerator.CollectionType.Set;

        public bool IsList => CollectionType == SourceGenerator.CollectionType.List;

        public bool IsDictionary => CollectionType == SourceGenerator.CollectionType.Dictionary;

        public bool IsUnsupported => this is UnsupportedTypeInfo;

        public virtual string TypeString => TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public virtual string CompleteTypeString => CompleteTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public static PropertyTypeInfo Unsupported => new UnsupportedTypeInfo();

        public static PropertyTypeInfo List => new ListTypeInfo();

        public static PropertyTypeInfo Set => new SetTypeInfo();

        public static PropertyTypeInfo Dictionary => new DictionaryTypeInfo();

        public static PropertyTypeInfo Int => new ScalarTypeInfo(SourceGenerator.ScalarType.Int);

        public static PropertyTypeInfo Bool => new ScalarTypeInfo(SourceGenerator.ScalarType.Bool);

        public static PropertyTypeInfo String => new ScalarTypeInfo(SourceGenerator.ScalarType.String);

        public static PropertyTypeInfo Data => new ScalarTypeInfo(SourceGenerator.ScalarType.Data);

        public static PropertyTypeInfo Date => new ScalarTypeInfo(SourceGenerator.ScalarType.Date);

        public static PropertyTypeInfo Float => new ScalarTypeInfo(SourceGenerator.ScalarType.Float);

        public static PropertyTypeInfo Double => new ScalarTypeInfo(SourceGenerator.ScalarType.Double);

        public static PropertyTypeInfo Object => new ScalarTypeInfo(SourceGenerator.ScalarType.Object);

        public static PropertyTypeInfo RealmValue => new ScalarTypeInfo(SourceGenerator.ScalarType.RealmValue);

        public static PropertyTypeInfo ObjectId => new ScalarTypeInfo(SourceGenerator.ScalarType.ObjectId);

        public static PropertyTypeInfo Decimal => new ScalarTypeInfo(SourceGenerator.ScalarType.Decimal);

        public static PropertyTypeInfo Guid => new ScalarTypeInfo(SourceGenerator.ScalarType.Guid);

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

            if (ScalarType == SourceGenerator.ScalarType.String)
            {
                return true;
            }

            if (_indexableTypes.Contains(ScalarType))
            {
                return !HasNullabilityAnnotation;
            }

            return false;
        }

        public bool IsSupportedPrimaryKeyType()
        {
            return _primaryKeyTypes.Contains(ScalarType);
        }

        public bool IsSupportedRequiredType()
        {
            if (IsListOrSet)
            {
                return InternalType.IsSupportedRequiredType();
            }

            return _requiredTypes.Contains(ScalarType);
        }

        public bool HasCorrectNullabilityAnnotation()
        {
            if (NullableAnnotation == NullableAnnotation.Annotated &&
                (IsCollection || IsIQueryable || ScalarType == SourceGenerator.ScalarType.RealmValue))
            {
                return false;
            }

            if (NullableAnnotation == NullableAnnotation.NotAnnotated &&
                (ScalarType == SourceGenerator.ScalarType.Object))
            {
                return false;
            }

            return true;
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
        public override CollectionType? CollectionType => SourceGenerator.CollectionType.List;
    }

    internal record SetTypeInfo : CollectionTypeInfo
    {
        public override CollectionType? CollectionType => SourceGenerator.CollectionType.Set;
    }

    internal record DictionaryTypeInfo : CollectionTypeInfo
    {
        public override CollectionType? CollectionType => SourceGenerator.CollectionType.Dictionary;
    }

    internal record RealmIntegerTypeInfo : PropertyTypeInfo
    {
        public override bool IsRealmInteger => true;
    }

    internal record IQueryableTypeInfo : PropertyTypeInfo
    {
        public override bool IsIQueryable => true;
    }

    internal record ScalarTypeInfo : PropertyTypeInfo
    {
        public ScalarTypeInfo(ScalarType type)
        {
            ScalarType = type;
        }
    }

    internal enum CollectionType
    {
        List,
        Set,
        Dictionary
    }

    internal enum ScalarType
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
