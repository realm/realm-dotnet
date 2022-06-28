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

namespace Realm.SourceGenerator
{
    internal record ClassInfo
    {
        public string Name { get; set; }

        public bool IsEmbedded { get; set; }

        public string MapTo { get; set; }

        public string Namespace { get; set; }

        public Accessibility Accessibility { get; set; }

        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();

        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

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

        public Accessibility Accessibility { get; set; }  //TODO At the end check if this is needed
    }

    internal abstract record PropertyTypeInfo
    {
        private static HashSet<ScalarTypeEnum> _indexableTypes = new()
        {
            ScalarTypeEnum.Int,
            ScalarTypeEnum.Bool,
            ScalarTypeEnum.String,
            ScalarTypeEnum.ObjectId,
            ScalarTypeEnum.Guid,
            ScalarTypeEnum.Date,
        };

        public bool IsCollection => CollectionType != null;

        public bool IsScalar => ScalarType != null;

        public bool IsSet => CollectionType == CollectionTypeEnum.Set;

        public bool IsList => CollectionType == CollectionTypeEnum.List;

        public bool IsDictionary => CollectionType == CollectionTypeEnum.Dictionary;

        public virtual ScalarTypeEnum? ScalarType { get; set; } = null;

        public virtual CollectionTypeEnum? CollectionType { get; set; } = null;

        public virtual bool IsRealmInteger { get; set; } = false;

        public virtual bool IsNullable { get; set; } = false;

        public virtual string TypeString { get; set; } = null;

        public virtual PropertyTypeInfo InternalType { get; set; } = null;

        public ITypeSymbol TypeSymbol { get; set; }

        public sealed override string ToString()
        {
            if (this is UnsupportedTypeInfo)
            {
                return "Unsupported";
            }

            var nullabilityString = IsNullable ? "?" : "";
            string desc;
            if (IsCollection)
            {
                desc = $"{CollectionType}{nullabilityString} of {InternalType}";
            }
            else if (IsRealmInteger)
            {
                desc = $"RealmInteger{nullabilityString} of {InternalType}";
            }
            else if(ScalarType == ScalarTypeEnum.Object)
            {
                desc = $"Object of type {TypeSymbol.Name}{nullabilityString} of";
            }
            else
            {
                desc = $"Scalar of type {ScalarType}{nullabilityString}";
            }

            return $"{TypeSymbol.ToReadableName()} ({desc})";
        }

        public static PropertyTypeInfo Unsupported = new UnsupportedTypeInfo();

        public static PropertyTypeInfo List => new ListTypeInfo();

        public static PropertyTypeInfo Set => new SetTypeInfo();

        public static PropertyTypeInfo Dictionary => new DictionaryTypeInfo();

        public static PropertyTypeInfo Int => new ScalarTypeInfo(ScalarTypeEnum.Int);

        public static PropertyTypeInfo Bool => new ScalarTypeInfo(ScalarTypeEnum.Bool);

        public static PropertyTypeInfo String => new ScalarTypeInfo(ScalarTypeEnum.String);

        public static PropertyTypeInfo Data => new ScalarTypeInfo(ScalarTypeEnum.Data);

        public static PropertyTypeInfo Date => new ScalarTypeInfo(ScalarTypeEnum.Date);

        public static PropertyTypeInfo Float => new ScalarTypeInfo(ScalarTypeEnum.Float);

        public static PropertyTypeInfo Double => new ScalarTypeInfo(ScalarTypeEnum.Double);

        public static PropertyTypeInfo Object => new ScalarTypeInfo(ScalarTypeEnum.Object);

        public static PropertyTypeInfo LinkingObjects => new ScalarTypeInfo(ScalarTypeEnum.LinkingObjects);

        public static PropertyTypeInfo RealmValue => new ScalarTypeInfo(ScalarTypeEnum.RealmValue);

        public static PropertyTypeInfo ObjectId => new ScalarTypeInfo(ScalarTypeEnum.ObjectId);

        public static PropertyTypeInfo Decimal => new ScalarTypeInfo(ScalarTypeEnum.Decimal);

        public static PropertyTypeInfo Guid => new ScalarTypeInfo(ScalarTypeEnum.Guid);

        public static PropertyTypeInfo RealmInteger => new RealmIntegerTypeInfo();

        public bool IsSupportedIndexType()
        {
            if (IsNullable)
            {
                return false;
            }

            if (IsRealmInteger)
            {
                return InternalType.IsSupportedIndexType();
            }

            if (!IsScalar)
            {
                return false;
            }

            return _indexableTypes.Contains(ScalarType.Value);
        }

        public bool IsUnsupported => this is UnsupportedTypeInfo;
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

        public override string TypeString => $"IList<{InternalType.TypeString}>";
    }

    internal record SetTypeInfo : CollectionTypeInfo
    {
        public override CollectionTypeEnum? CollectionType => CollectionTypeEnum.Set;

        public override string TypeString => $"ISet<{InternalType.TypeString}>";
    }

    internal record DictionaryTypeInfo : CollectionTypeInfo
    {
        public override CollectionTypeEnum? CollectionType => CollectionTypeEnum.Dictionary;

        public override string TypeString => $"IDictionary<string,{InternalType.TypeString}>";
    }

    internal record RealmIntegerTypeInfo : PropertyTypeInfo
    {
        public override bool IsRealmInteger => true;

        public override string TypeString => $"RealmInteger<{InternalType.TypeString}>";  //TODO Need to check for nullability

    }

    internal record ScalarTypeInfo : PropertyTypeInfo
    {
        public ScalarTypeInfo(ScalarTypeEnum type)
        {
            ScalarType = type;
        }
    }

    internal enum CollectionTypeEnum
    {
        List,
        Set,
        Dictionary
    }

    internal enum ScalarTypeEnum
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Data = 3,
        Date = 4,
        Float = 5,
        Double = 6,
        Object = 7,
        LinkingObjects = 8,
        RealmValue = 9,
        ObjectId = 10,
        Decimal = 11,
        Guid = 12,
    }
}
