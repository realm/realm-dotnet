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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    internal record ClassInfo
    {
        public string Name { get; set; } = null!;

        public ObjectType ObjectType { get; set; }

        public string? MapTo { get; set; }

        public NamespaceInfo NamespaceInfo { get; set; } = null!;

        public Accessibility Accessibility { get; set; }

        public ITypeSymbol TypeSymbol { get; set; } = null!;

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
        public string? OriginalName { get; }

        [MemberNotNullWhen(returnValue: false, member: nameof(OriginalName))]
        public bool IsGlobal { get; }

        public static NamespaceInfo Global() => new(originalName: null, isGlobal: true);

        public static NamespaceInfo Local(string originalName) => new(originalName: originalName, isGlobal: false);

        private NamespaceInfo(string? originalName, bool isGlobal)
        {
            OriginalName = originalName;
            IsGlobal = isGlobal;
        }

        public string ComputedName => IsGlobal ? "Global" : OriginalName;
    }

    internal record EnclosingClassInfo(string Name, Accessibility Accessibility);

    internal record PropertyInfo(string Name)
    {
        public bool IsIndexed { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimaryKey { get; set; }

        public string? MapTo { get; set; }

        public string? Backlink { get; set; }

        public string? BacklinkMapTo { get; set; }

        public PropertyTypeInfo TypeInfo { get; set; } = null!;

        public Accessibility Accessibility { get; set; }

        public string? Initializer { get; set; }

        public string GetMappedOrOriginalName() => MapTo ?? Name;

        public string? GetMappedOrOriginalBacklink() => BacklinkMapTo ?? Backlink;
    }

    internal abstract record PropertyTypeInfo
    {
        private static readonly HashSet<ScalarType> _indexableTypes = new()
        {
            ScalarType.Int,
            ScalarType.Bool,
            ScalarType.String,
            ScalarType.ObjectId,
            ScalarType.Guid,
            ScalarType.Date,
        };

        private static readonly HashSet<ScalarType> _primaryKeyTypes = new()
        {
            ScalarType.Int,
            ScalarType.String,
            ScalarType.ObjectId,
            ScalarType.Guid,
        };

        private static readonly HashSet<ScalarType> _requiredTypes = new()
        {
            ScalarType.String,
            ScalarType.Data,
        };

        public virtual ScalarType ScalarType { get; set; }

        public virtual CollectionType CollectionType { get; }

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public virtual bool IsRealmInteger { get; set; }

        // NullabilityAnnotation != None for all value types and for ref types with nullability annotations enabled
        public NullableAnnotation NullableAnnotation { get; set; } = NullableAnnotation.None;

        public bool IsNullable => NullableAnnotation == NullableAnnotation.None || NullableAnnotation == NullableAnnotation.Annotated;

        // Only valid if ScalarType == Object;
        public string? MapTo { get; set; }

        public string? Namespace { get; set; }

        public ObjectType ObjectType { get; set; }

        public ITypeSymbol TypeSymbol { get; set; } = null!;

        // This includes the eventual nullability annotation ("?")
        public ITypeSymbol CompleteTypeSymbol { get; set; } = null!;

        public PropertyTypeInfo? InternalType { get; set; }

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsCollection => CollectionType != CollectionType.None;

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsListOrSet => IsList || IsSet;

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsSet => CollectionType == CollectionType.Set;

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsList => CollectionType == CollectionType.List;

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsDictionary => CollectionType == CollectionType.Dictionary;

        [MemberNotNullWhen(returnValue: true, member: nameof(InternalType))]
        public bool IsBacklink => CollectionType == CollectionType.Backlink;

        public virtual bool IsUnsupported => false;

        public virtual string TypeString => TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // This includes nullability annotation and fully qualified name (with namespaces)
        public virtual string CompleteFullyQualifiedString => CompleteTypeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

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

        public bool HasCorrectNullabilityAnnotation(bool ignoreObjectsNullability)
        {
            if (NullableAnnotation == NullableAnnotation.Annotated &&
                (IsCollection || ScalarType == ScalarType.RealmValue))
            {
                return false;
            }

            if (!ignoreObjectsNullability)
            {
                if (!IsNullable && ScalarType == ScalarType.Object)
                {
                    return false;
                }

                if (IsCollection && InternalType.ScalarType == ScalarType.Object)
                {
                    if (IsDictionary && InternalType.NullableAnnotation == NullableAnnotation.NotAnnotated)
                    {
                        return false;
                    }

                    if ((IsList || IsSet || IsBacklink) && InternalType.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /** This method returns the type with the type string with correct nullability annotation wether they were enabled or not in the original file,
         * together with the same annotation for the internal type (for collections). In some cases we were just getting the internal type string on his own, and this would be wrong for some collections.
         * Simplified example from a generated unmanaged accessor:
         * <code>
         * public IList≪AllTypesClass≫ ObjectCollectionProperty
         * {
         *      get => GetListValue≪AllTypesClass≫("ObjectCollectionProperty")
         * }.
         * </code>
         * If we would take the nullability of AllTypesClass on his own, this would be nullable. This can't be nullable though, because it can't be nullable in a list.
         */
        public (string CompleteType, string? InternalType, bool NeedsNullForgiving) GetCorrectlyAnnotatedTypeName(bool isRequired)
        {
            var internalTypeString = InternalType?.CompleteFullyQualifiedString;
            var nullableInternalTypeString = $"{internalTypeString}?";

            if (!isRequired)
            {
                if (NullableAnnotation == NullableAnnotation.None && (ScalarType == ScalarType.Data || ScalarType == ScalarType.String))
                {
                    return (CompleteFullyQualifiedString + "?", null, false);
                }

                if (IsCollection && InternalType.NullableAnnotation == NullableAnnotation.None
                    && (InternalType.ScalarType == ScalarType.Data || InternalType.ScalarType == ScalarType.String))
                {
                    return CollectionType switch
                    {
                        CollectionType.List => ($"System.Collections.Generic.IList<{nullableInternalTypeString}>", nullableInternalTypeString, true),
                        CollectionType.Set => ($"System.Collections.Generic.ISet<{nullableInternalTypeString}>", nullableInternalTypeString, true),
                        CollectionType.Dictionary => ($"System.Collections.Generic.IDictionary<string, {nullableInternalTypeString}>", nullableInternalTypeString, true),
                        _ => throw new NotImplementedException($"Collection type {CollectionType} with string or byte array argument is not supported yet"),
                    };
                }
            }

            if (ScalarType == ScalarType.Object && NullableAnnotation == NullableAnnotation.None)
            {
                return (CompleteFullyQualifiedString + "?", null, false);
            }

            if (IsDictionary && InternalType.ScalarType == ScalarType.Object && InternalType.NullableAnnotation == NullableAnnotation.None)
            {
                return ($"System.Collections.Generic.IDictionary<string, {nullableInternalTypeString}>", nullableInternalTypeString, true);
            }

            var nullForgiving = false;

            if (NullableAnnotation != NullableAnnotation.Annotated &&
                (ScalarType == ScalarType.Data || ScalarType == ScalarType.String || ScalarType == ScalarType.Object || IsCollection))
            {
                nullForgiving = true;
            }

            return (CompleteFullyQualifiedString, internalTypeString, nullForgiving);
        }
    }

    internal sealed record UnsupportedTypeInfo : PropertyTypeInfo
    {
        public override bool IsUnsupported => true;
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

    internal sealed record ScalarTypeInfo : PropertyTypeInfo
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
