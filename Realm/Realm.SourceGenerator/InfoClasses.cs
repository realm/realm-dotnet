////////////////////////////////////////////////////////////////////////////
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

        public ObjectType ObjectType { get; set; }

        public string MapTo { get; set; }

        public NamespaceInfo NamespaceInfo { get; set; }

        public Accessibility Accessibility { get; set; }

        public ITypeSymbol TypeSymbol { get; set; }

        public List<PropertyInfo> Properties { get; } = new();

        public List<Diagnostic> Diagnostics { get; } = new();

        public List<string> Usings { get; } = new();

        public List<EnclosingClassInfo> EnclosingClasses { get; } = new();

        public bool HasParameterlessConstructor { get; set; }

        public bool OverridesToString { get; set; }

        public bool OverridesEquals { get; set; }

        public bool OverridesGetHashCode { get; set; }

        public bool HasPropertyChangedEvent { get; set; }

        public bool HasDuplicatedName { get; set; }

        public PropertyInfo PrimaryKey => Properties.FirstOrDefault(p => p.IsPrimaryKey);
    }

    internal record NamespaceInfo
    {
        public string OriginalName { get; set; }

        public bool IsGlobal { get; set; }

        public string ComputedName => IsGlobal ? "Global" : OriginalName;
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

        public string BacklinkMapTo { get; set; }

        public PropertyTypeInfo TypeInfo { get; set; }

        public Accessibility Accessibility { get; set; }

        public string Initializer { get; set; }

        public string GetMappedOrOriginalName() => MapTo ?? Name;

        public string GetMappedOrOriginalBacklink() => BacklinkMapTo ?? Backlink;
    }

    internal abstract record PropertyTypeInfo
    {
        private static HashSet<ScalarType> _indexableTypes = new()
        {
            ScalarType.Int,
            ScalarType.Bool,
            ScalarType.String,
            ScalarType.ObjectId,
            ScalarType.Guid,
            ScalarType.Date,
        };

        private static HashSet<ScalarType> _primaryKeyTypes = new()
        {
            ScalarType.Int,
            ScalarType.String,
            ScalarType.ObjectId,
            ScalarType.Guid,
        };

        private static HashSet<ScalarType> _requiredTypes = new()
        {
            ScalarType.String,
            ScalarType.Data,
        };

        public virtual ScalarType ScalarType { get; set; }

        public virtual CollectionType CollectionType { get; }

        public virtual bool IsRealmInteger { get; set; }

        // NullabilityAnnotation != None for all value types and for ref types with nullability annotations enabled
        public NullableAnnotation NullableAnnotation { get; set; } = NullableAnnotation.None;

        public bool IsNullable => NullableAnnotation == NullableAnnotation.None || NullableAnnotation == NullableAnnotation.Annotated;

        // Only valid if ScalarType == Object;
        public string MapTo { get; set; }

        public string Namespace { get; set; }

        public ObjectType ObjectType { get; set; }

        public ITypeSymbol TypeSymbol { get; set; }

        // This includes the eventual nullability annotation ("?")
        public ITypeSymbol CompleteTypeSymbol { get; set; }

        public PropertyTypeInfo InternalType { get; set; }

        public bool IsCollection => CollectionType != CollectionType.None;

        public bool IsSimpleType => ScalarType != ScalarType.None;

        public bool IsListOrSet => IsList || IsSet;

        public bool IsSet => CollectionType == CollectionType.Set;

        public bool IsList => CollectionType == CollectionType.List;

        public bool IsDictionary => CollectionType == CollectionType.Dictionary;

        public bool IsBacklink => CollectionType == CollectionType.Backlink;

        public bool IsUnsupported => this is UnsupportedTypeInfo;

        public virtual string TypeString => TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public virtual string CompleteTypeString => CompleteTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public static PropertyTypeInfo Unsupported => new UnsupportedTypeInfo();

        public static PropertyTypeInfo List => new ListTypeInfo();

        public static PropertyTypeInfo Set => new SetTypeInfo();

        public static PropertyTypeInfo Dictionary => new DictionaryTypeInfo();

        public static PropertyTypeInfo Int => new ScalarTypeInfo(ScalarType.Int);

        public static PropertyTypeInfo Bool => new ScalarTypeInfo(ScalarType.Bool);

        public static PropertyTypeInfo String => new ScalarTypeInfo(ScalarType.String);

        public static PropertyTypeInfo Data => new ScalarTypeInfo(ScalarType.Data);

        public static PropertyTypeInfo Date => new ScalarTypeInfo(ScalarType.Date);

        public static PropertyTypeInfo Float => new ScalarTypeInfo(ScalarType.Float);

        public static PropertyTypeInfo Double => new ScalarTypeInfo(ScalarType.Double);

        public static PropertyTypeInfo Object => new ScalarTypeInfo(ScalarType.Object);

        public static PropertyTypeInfo RealmValue => new ScalarTypeInfo(ScalarType.RealmValue);

        public static PropertyTypeInfo ObjectId => new ScalarTypeInfo(ScalarType.ObjectId);

        public static PropertyTypeInfo Decimal => new ScalarTypeInfo(ScalarType.Decimal);

        public static PropertyTypeInfo Guid => new ScalarTypeInfo(ScalarType.Guid);

        public static PropertyTypeInfo RealmInteger => new RealmIntegerTypeInfo();

        public static PropertyTypeInfo Backlink => new BacklinkTypeInfo();

        public bool IsSupportedIndexType()
        {
            if (IsRealmInteger)
            {
                if (IsNullable)
                {
                    return false;
                }

                return InternalType.IsSupportedIndexType();
            }

            return _indexableTypes.Contains(ScalarType);
        }

        public bool IsSupportedPrimaryKeyType()
        {
            return _primaryKeyTypes.Contains(ScalarType);
        }

        public bool IsSupportedRequiredType()
        {
            if (IsCollection)
            {
                return InternalType.IsSupportedRequiredType();
            }

            return _requiredTypes.Contains(ScalarType);
        }

        public bool HasCorrectNullabilityAnnotation()
        {
            if (NullableAnnotation == NullableAnnotation.Annotated &&
                (IsCollection || ScalarType == ScalarType.RealmValue))
            {
                return false;
            }

            if (!IsNullable &&
                (ScalarType == ScalarType.Object))
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
        public override CollectionType CollectionType => CollectionType.List;
    }

    internal record SetTypeInfo : CollectionTypeInfo
    {
        public override CollectionType CollectionType => CollectionType.Set;
    }

    internal record DictionaryTypeInfo : CollectionTypeInfo
    {
        public override CollectionType CollectionType => CollectionType.Dictionary;
    }

    internal record BacklinkTypeInfo : PropertyTypeInfo
    {
        public override CollectionType CollectionType => CollectionType.Backlink;
    }

    internal record RealmIntegerTypeInfo : PropertyTypeInfo
    {
        public override bool IsRealmInteger => true;
    }

    internal record ScalarTypeInfo : PropertyTypeInfo
    {
        public ScalarTypeInfo(ScalarType type)
        {
            ScalarType = type;
        }
    }

    internal enum ObjectType
    {
        None,
        RealmObject,
        EmbeddedObject,
        AsymmetricObject
    }

    internal enum CollectionType
    {
        None,
        List,
        Set,
        Dictionary,
        Backlink
    }

    internal enum ScalarType
    {
        None,
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
