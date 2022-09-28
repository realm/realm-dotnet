﻿// <auto-generated />
using Realms.Tests;
using Realms.Tests.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Schema;
using MongoDB.Bson;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(ListsObjectObjectHelper))]
    public partial class ListsObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ListsObject", isEmbedded: false)
        {
            Property.PrimitiveList("CharList", RealmValueType.Int, areElementsNullable: false, managedName: "CharList"),
            Property.PrimitiveList("ByteList", RealmValueType.Int, areElementsNullable: false, managedName: "ByteList"),
            Property.PrimitiveList("Int16List", RealmValueType.Int, areElementsNullable: false, managedName: "Int16List"),
            Property.PrimitiveList("Int32List", RealmValueType.Int, areElementsNullable: false, managedName: "Int32List"),
            Property.PrimitiveList("Int64List", RealmValueType.Int, areElementsNullable: false, managedName: "Int64List"),
            Property.PrimitiveList("SingleList", RealmValueType.Float, areElementsNullable: false, managedName: "SingleList"),
            Property.PrimitiveList("DoubleList", RealmValueType.Double, areElementsNullable: false, managedName: "DoubleList"),
            Property.PrimitiveList("BooleanList", RealmValueType.Bool, areElementsNullable: false, managedName: "BooleanList"),
            Property.PrimitiveList("DecimalList", RealmValueType.Decimal128, areElementsNullable: false, managedName: "DecimalList"),
            Property.PrimitiveList("Decimal128List", RealmValueType.Decimal128, areElementsNullable: false, managedName: "Decimal128List"),
            Property.PrimitiveList("ObjectIdList", RealmValueType.ObjectId, areElementsNullable: false, managedName: "ObjectIdList"),
            Property.PrimitiveList("GuidList", RealmValueType.Guid, areElementsNullable: false, managedName: "GuidList"),
            Property.PrimitiveList("StringList", RealmValueType.String, areElementsNullable: false, managedName: "StringList"),
            Property.PrimitiveList("ByteArrayList", RealmValueType.Data, areElementsNullable: false, managedName: "ByteArrayList"),
            Property.PrimitiveList("DateTimeOffsetList", RealmValueType.Date, areElementsNullable: false, managedName: "DateTimeOffsetList"),
            Property.PrimitiveList("NullableCharList", RealmValueType.Int, areElementsNullable: true, managedName: "NullableCharList"),
            Property.PrimitiveList("NullableByteList", RealmValueType.Int, areElementsNullable: true, managedName: "NullableByteList"),
            Property.PrimitiveList("NullableInt16List", RealmValueType.Int, areElementsNullable: true, managedName: "NullableInt16List"),
            Property.PrimitiveList("NullableInt32List", RealmValueType.Int, areElementsNullable: true, managedName: "NullableInt32List"),
            Property.PrimitiveList("NullableInt64List", RealmValueType.Int, areElementsNullable: true, managedName: "NullableInt64List"),
            Property.PrimitiveList("NullableSingleList", RealmValueType.Float, areElementsNullable: true, managedName: "NullableSingleList"),
            Property.PrimitiveList("NullableDoubleList", RealmValueType.Double, areElementsNullable: true, managedName: "NullableDoubleList"),
            Property.PrimitiveList("NullableBooleanList", RealmValueType.Bool, areElementsNullable: true, managedName: "NullableBooleanList"),
            Property.PrimitiveList("NullableDateTimeOffsetList", RealmValueType.Date, areElementsNullable: true, managedName: "NullableDateTimeOffsetList"),
            Property.PrimitiveList("NullableDecimalList", RealmValueType.Decimal128, areElementsNullable: true, managedName: "NullableDecimalList"),
            Property.PrimitiveList("NullableDecimal128List", RealmValueType.Decimal128, areElementsNullable: true, managedName: "NullableDecimal128List"),
            Property.PrimitiveList("NullableObjectIdList", RealmValueType.ObjectId, areElementsNullable: true, managedName: "NullableObjectIdList"),
            Property.PrimitiveList("NullableGuidList", RealmValueType.Guid, areElementsNullable: true, managedName: "NullableGuidList"),
            Property.PrimitiveList("NullableStringList", RealmValueType.String, areElementsNullable: true, managedName: "NullableStringList"),
            Property.PrimitiveList("NullableByteArrayList", RealmValueType.Data, areElementsNullable: true, managedName: "NullableByteArrayList"),
            Property.RealmValueList("RealmValueList", managedName: "RealmValueList"),
        }.Build();

        #region IRealmObject implementation

        private IListsObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IListsObjectAccessor Accessor => _accessor = _accessor ?? new ListsObjectUnmanagedAccessor(typeof(ListsObject));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IListsObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IListsObjectAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.CharList.Clear();
                    newAccessor.ByteList.Clear();
                    newAccessor.Int16List.Clear();
                    newAccessor.Int32List.Clear();
                    newAccessor.Int64List.Clear();
                    newAccessor.SingleList.Clear();
                    newAccessor.DoubleList.Clear();
                    newAccessor.BooleanList.Clear();
                    newAccessor.DecimalList.Clear();
                    newAccessor.Decimal128List.Clear();
                    newAccessor.ObjectIdList.Clear();
                    newAccessor.GuidList.Clear();
                    newAccessor.StringList.Clear();
                    newAccessor.ByteArrayList.Clear();
                    newAccessor.DateTimeOffsetList.Clear();
                    newAccessor.NullableCharList.Clear();
                    newAccessor.NullableByteList.Clear();
                    newAccessor.NullableInt16List.Clear();
                    newAccessor.NullableInt32List.Clear();
                    newAccessor.NullableInt64List.Clear();
                    newAccessor.NullableSingleList.Clear();
                    newAccessor.NullableDoubleList.Clear();
                    newAccessor.NullableBooleanList.Clear();
                    newAccessor.NullableDateTimeOffsetList.Clear();
                    newAccessor.NullableDecimalList.Clear();
                    newAccessor.NullableDecimal128List.Clear();
                    newAccessor.NullableObjectIdList.Clear();
                    newAccessor.NullableGuidList.Clear();
                    newAccessor.NullableStringList.Clear();
                    newAccessor.NullableByteArrayList.Clear();
                    newAccessor.RealmValueList.Clear();
                }

                CollectionExtensions.PopulateCollection(oldAccessor.CharList, newAccessor.CharList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.ByteList, newAccessor.ByteList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.Int16List, newAccessor.Int16List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.Int32List, newAccessor.Int32List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.Int64List, newAccessor.Int64List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.SingleList, newAccessor.SingleList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.DoubleList, newAccessor.DoubleList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.BooleanList, newAccessor.BooleanList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.DecimalList, newAccessor.DecimalList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.Decimal128List, newAccessor.Decimal128List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.ObjectIdList, newAccessor.ObjectIdList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.GuidList, newAccessor.GuidList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.StringList, newAccessor.StringList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.ByteArrayList, newAccessor.ByteArrayList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.DateTimeOffsetList, newAccessor.DateTimeOffsetList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableCharList, newAccessor.NullableCharList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableByteList, newAccessor.NullableByteList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableInt16List, newAccessor.NullableInt16List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableInt32List, newAccessor.NullableInt32List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableInt64List, newAccessor.NullableInt64List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableSingleList, newAccessor.NullableSingleList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableDoubleList, newAccessor.NullableDoubleList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableBooleanList, newAccessor.NullableBooleanList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableDateTimeOffsetList, newAccessor.NullableDateTimeOffsetList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableDecimalList, newAccessor.NullableDecimalList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableDecimal128List, newAccessor.NullableDecimal128List, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableObjectIdList, newAccessor.NullableObjectIdList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableGuidList, newAccessor.NullableGuidList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableStringList, newAccessor.NullableStringList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.NullableByteArrayList, newAccessor.NullableByteArrayList, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.RealmValueList, newAccessor.RealmValueList, update, skipDefaults);
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

        partial void OnManaged();

        private event PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    UnsubscribeFromNotifications();
                }
            }
        }

        partial void OnPropertyChanged(string propertyName);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }

        public static explicit operator ListsObject(RealmValue val) => val.AsRealmObject<ListsObject>();

        public static implicit operator RealmValue(ListsObject val) => RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return Accessor.GetTypeInfo(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is InvalidObject)
            {
                return !IsValid;
            }

            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode()
        {
            return IsManaged ? Accessor.GetHashCode() : base.GetHashCode();
        }

        public override string ToString()
        {
            return Accessor.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class ListsObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new ListsObjectManagedAccessor();

            public IRealmObjectBase CreateInstance()
            {
                return new ListsObject();
            }

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IListsObjectAccessor : IRealmAccessor
    {
        IList<char> CharList { get; }

        IList<byte> ByteList { get; }

        IList<short> Int16List { get; }

        IList<int> Int32List { get; }

        IList<long> Int64List { get; }

        IList<float> SingleList { get; }

        IList<double> DoubleList { get; }

        IList<bool> BooleanList { get; }

        IList<decimal> DecimalList { get; }

        IList<Decimal128> Decimal128List { get; }

        IList<ObjectId> ObjectIdList { get; }

        IList<Guid> GuidList { get; }

        IList<string> StringList { get; }

        IList<byte[]> ByteArrayList { get; }

        IList<DateTimeOffset> DateTimeOffsetList { get; }

        IList<char?> NullableCharList { get; }

        IList<byte?> NullableByteList { get; }

        IList<short?> NullableInt16List { get; }

        IList<int?> NullableInt32List { get; }

        IList<long?> NullableInt64List { get; }

        IList<float?> NullableSingleList { get; }

        IList<double?> NullableDoubleList { get; }

        IList<bool?> NullableBooleanList { get; }

        IList<DateTimeOffset?> NullableDateTimeOffsetList { get; }

        IList<decimal?> NullableDecimalList { get; }

        IList<Decimal128?> NullableDecimal128List { get; }

        IList<ObjectId?> NullableObjectIdList { get; }

        IList<Guid?> NullableGuidList { get; }

        IList<string> NullableStringList { get; }

        IList<byte[]> NullableByteArrayList { get; }

        IList<RealmValue> RealmValueList { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ListsObjectManagedAccessor : ManagedAccessor, IListsObjectAccessor
    {
        private IList<char> _charList;
        public IList<char> CharList
        {
            get
            {
                if (_charList == null)
                {
                    _charList = GetListValue<char>("CharList");
                }

                return _charList;
            }
        }

        private IList<byte> _byteList;
        public IList<byte> ByteList
        {
            get
            {
                if (_byteList == null)
                {
                    _byteList = GetListValue<byte>("ByteList");
                }

                return _byteList;
            }
        }

        private IList<short> _int16List;
        public IList<short> Int16List
        {
            get
            {
                if (_int16List == null)
                {
                    _int16List = GetListValue<short>("Int16List");
                }

                return _int16List;
            }
        }

        private IList<int> _int32List;
        public IList<int> Int32List
        {
            get
            {
                if (_int32List == null)
                {
                    _int32List = GetListValue<int>("Int32List");
                }

                return _int32List;
            }
        }

        private IList<long> _int64List;
        public IList<long> Int64List
        {
            get
            {
                if (_int64List == null)
                {
                    _int64List = GetListValue<long>("Int64List");
                }

                return _int64List;
            }
        }

        private IList<float> _singleList;
        public IList<float> SingleList
        {
            get
            {
                if (_singleList == null)
                {
                    _singleList = GetListValue<float>("SingleList");
                }

                return _singleList;
            }
        }

        private IList<double> _doubleList;
        public IList<double> DoubleList
        {
            get
            {
                if (_doubleList == null)
                {
                    _doubleList = GetListValue<double>("DoubleList");
                }

                return _doubleList;
            }
        }

        private IList<bool> _booleanList;
        public IList<bool> BooleanList
        {
            get
            {
                if (_booleanList == null)
                {
                    _booleanList = GetListValue<bool>("BooleanList");
                }

                return _booleanList;
            }
        }

        private IList<decimal> _decimalList;
        public IList<decimal> DecimalList
        {
            get
            {
                if (_decimalList == null)
                {
                    _decimalList = GetListValue<decimal>("DecimalList");
                }

                return _decimalList;
            }
        }

        private IList<Decimal128> _decimal128List;
        public IList<Decimal128> Decimal128List
        {
            get
            {
                if (_decimal128List == null)
                {
                    _decimal128List = GetListValue<Decimal128>("Decimal128List");
                }

                return _decimal128List;
            }
        }

        private IList<ObjectId> _objectIdList;
        public IList<ObjectId> ObjectIdList
        {
            get
            {
                if (_objectIdList == null)
                {
                    _objectIdList = GetListValue<ObjectId>("ObjectIdList");
                }

                return _objectIdList;
            }
        }

        private IList<Guid> _guidList;
        public IList<Guid> GuidList
        {
            get
            {
                if (_guidList == null)
                {
                    _guidList = GetListValue<Guid>("GuidList");
                }

                return _guidList;
            }
        }

        private IList<string> _stringList;
        public IList<string> StringList
        {
            get
            {
                if (_stringList == null)
                {
                    _stringList = GetListValue<string>("StringList");
                }

                return _stringList;
            }
        }

        private IList<byte[]> _byteArrayList;
        public IList<byte[]> ByteArrayList
        {
            get
            {
                if (_byteArrayList == null)
                {
                    _byteArrayList = GetListValue<byte[]>("ByteArrayList");
                }

                return _byteArrayList;
            }
        }

        private IList<DateTimeOffset> _dateTimeOffsetList;
        public IList<DateTimeOffset> DateTimeOffsetList
        {
            get
            {
                if (_dateTimeOffsetList == null)
                {
                    _dateTimeOffsetList = GetListValue<DateTimeOffset>("DateTimeOffsetList");
                }

                return _dateTimeOffsetList;
            }
        }

        private IList<char?> _nullableCharList;
        public IList<char?> NullableCharList
        {
            get
            {
                if (_nullableCharList == null)
                {
                    _nullableCharList = GetListValue<char?>("NullableCharList");
                }

                return _nullableCharList;
            }
        }

        private IList<byte?> _nullableByteList;
        public IList<byte?> NullableByteList
        {
            get
            {
                if (_nullableByteList == null)
                {
                    _nullableByteList = GetListValue<byte?>("NullableByteList");
                }

                return _nullableByteList;
            }
        }

        private IList<short?> _nullableInt16List;
        public IList<short?> NullableInt16List
        {
            get
            {
                if (_nullableInt16List == null)
                {
                    _nullableInt16List = GetListValue<short?>("NullableInt16List");
                }

                return _nullableInt16List;
            }
        }

        private IList<int?> _nullableInt32List;
        public IList<int?> NullableInt32List
        {
            get
            {
                if (_nullableInt32List == null)
                {
                    _nullableInt32List = GetListValue<int?>("NullableInt32List");
                }

                return _nullableInt32List;
            }
        }

        private IList<long?> _nullableInt64List;
        public IList<long?> NullableInt64List
        {
            get
            {
                if (_nullableInt64List == null)
                {
                    _nullableInt64List = GetListValue<long?>("NullableInt64List");
                }

                return _nullableInt64List;
            }
        }

        private IList<float?> _nullableSingleList;
        public IList<float?> NullableSingleList
        {
            get
            {
                if (_nullableSingleList == null)
                {
                    _nullableSingleList = GetListValue<float?>("NullableSingleList");
                }

                return _nullableSingleList;
            }
        }

        private IList<double?> _nullableDoubleList;
        public IList<double?> NullableDoubleList
        {
            get
            {
                if (_nullableDoubleList == null)
                {
                    _nullableDoubleList = GetListValue<double?>("NullableDoubleList");
                }

                return _nullableDoubleList;
            }
        }

        private IList<bool?> _nullableBooleanList;
        public IList<bool?> NullableBooleanList
        {
            get
            {
                if (_nullableBooleanList == null)
                {
                    _nullableBooleanList = GetListValue<bool?>("NullableBooleanList");
                }

                return _nullableBooleanList;
            }
        }

        private IList<DateTimeOffset?> _nullableDateTimeOffsetList;
        public IList<DateTimeOffset?> NullableDateTimeOffsetList
        {
            get
            {
                if (_nullableDateTimeOffsetList == null)
                {
                    _nullableDateTimeOffsetList = GetListValue<DateTimeOffset?>("NullableDateTimeOffsetList");
                }

                return _nullableDateTimeOffsetList;
            }
        }

        private IList<decimal?> _nullableDecimalList;
        public IList<decimal?> NullableDecimalList
        {
            get
            {
                if (_nullableDecimalList == null)
                {
                    _nullableDecimalList = GetListValue<decimal?>("NullableDecimalList");
                }

                return _nullableDecimalList;
            }
        }

        private IList<Decimal128?> _nullableDecimal128List;
        public IList<Decimal128?> NullableDecimal128List
        {
            get
            {
                if (_nullableDecimal128List == null)
                {
                    _nullableDecimal128List = GetListValue<Decimal128?>("NullableDecimal128List");
                }

                return _nullableDecimal128List;
            }
        }

        private IList<ObjectId?> _nullableObjectIdList;
        public IList<ObjectId?> NullableObjectIdList
        {
            get
            {
                if (_nullableObjectIdList == null)
                {
                    _nullableObjectIdList = GetListValue<ObjectId?>("NullableObjectIdList");
                }

                return _nullableObjectIdList;
            }
        }

        private IList<Guid?> _nullableGuidList;
        public IList<Guid?> NullableGuidList
        {
            get
            {
                if (_nullableGuidList == null)
                {
                    _nullableGuidList = GetListValue<Guid?>("NullableGuidList");
                }

                return _nullableGuidList;
            }
        }

        private IList<string> _nullableStringList;
        public IList<string> NullableStringList
        {
            get
            {
                if (_nullableStringList == null)
                {
                    _nullableStringList = GetListValue<string>("NullableStringList");
                }

                return _nullableStringList;
            }
        }

        private IList<byte[]> _nullableByteArrayList;
        public IList<byte[]> NullableByteArrayList
        {
            get
            {
                if (_nullableByteArrayList == null)
                {
                    _nullableByteArrayList = GetListValue<byte[]>("NullableByteArrayList");
                }

                return _nullableByteArrayList;
            }
        }

        private IList<RealmValue> _realmValueList;
        public IList<RealmValue> RealmValueList
        {
            get
            {
                if (_realmValueList == null)
                {
                    _realmValueList = GetListValue<RealmValue>("RealmValueList");
                }

                return _realmValueList;
            }
        }
    }

    internal class ListsObjectUnmanagedAccessor : UnmanagedAccessor, IListsObjectAccessor
    {
        public IList<char> CharList { get; } = new List<char>();

        public IList<byte> ByteList { get; } = new List<byte>();

        public IList<short> Int16List { get; } = new List<short>();

        public IList<int> Int32List { get; } = new List<int>();

        public IList<long> Int64List { get; } = new List<long>();

        public IList<float> SingleList { get; } = new List<float>();

        public IList<double> DoubleList { get; } = new List<double>();

        public IList<bool> BooleanList { get; } = new List<bool>();

        public IList<decimal> DecimalList { get; } = new List<decimal>();

        public IList<Decimal128> Decimal128List { get; } = new List<Decimal128>();

        public IList<ObjectId> ObjectIdList { get; } = new List<ObjectId>();

        public IList<Guid> GuidList { get; } = new List<Guid>();

        public IList<string> StringList { get; } = new List<string>();

        public IList<byte[]> ByteArrayList { get; } = new List<byte[]>();

        public IList<DateTimeOffset> DateTimeOffsetList { get; } = new List<DateTimeOffset>();

        public IList<char?> NullableCharList { get; } = new List<char?>();

        public IList<byte?> NullableByteList { get; } = new List<byte?>();

        public IList<short?> NullableInt16List { get; } = new List<short?>();

        public IList<int?> NullableInt32List { get; } = new List<int?>();

        public IList<long?> NullableInt64List { get; } = new List<long?>();

        public IList<float?> NullableSingleList { get; } = new List<float?>();

        public IList<double?> NullableDoubleList { get; } = new List<double?>();

        public IList<bool?> NullableBooleanList { get; } = new List<bool?>();

        public IList<DateTimeOffset?> NullableDateTimeOffsetList { get; } = new List<DateTimeOffset?>();

        public IList<decimal?> NullableDecimalList { get; } = new List<decimal?>();

        public IList<Decimal128?> NullableDecimal128List { get; } = new List<Decimal128?>();

        public IList<ObjectId?> NullableObjectIdList { get; } = new List<ObjectId?>();

        public IList<Guid?> NullableGuidList { get; } = new List<Guid?>();

        public IList<string> NullableStringList { get; } = new List<string>();

        public IList<byte[]> NullableByteArrayList { get; } = new List<byte[]>();

        public IList<RealmValue> RealmValueList { get; } = new List<RealmValue>();

        public ListsObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "CharList" => (IList<T>)CharList,
            "ByteList" => (IList<T>)ByteList,
            "Int16List" => (IList<T>)Int16List,
            "Int32List" => (IList<T>)Int32List,
            "Int64List" => (IList<T>)Int64List,
            "SingleList" => (IList<T>)SingleList,
            "DoubleList" => (IList<T>)DoubleList,
            "BooleanList" => (IList<T>)BooleanList,
            "DecimalList" => (IList<T>)DecimalList,
            "Decimal128List" => (IList<T>)Decimal128List,
            "ObjectIdList" => (IList<T>)ObjectIdList,
            "GuidList" => (IList<T>)GuidList,
            "StringList" => (IList<T>)StringList,
            "ByteArrayList" => (IList<T>)ByteArrayList,
            "DateTimeOffsetList" => (IList<T>)DateTimeOffsetList,
            "NullableCharList" => (IList<T>)NullableCharList,
            "NullableByteList" => (IList<T>)NullableByteList,
            "NullableInt16List" => (IList<T>)NullableInt16List,
            "NullableInt32List" => (IList<T>)NullableInt32List,
            "NullableInt64List" => (IList<T>)NullableInt64List,
            "NullableSingleList" => (IList<T>)NullableSingleList,
            "NullableDoubleList" => (IList<T>)NullableDoubleList,
            "NullableBooleanList" => (IList<T>)NullableBooleanList,
            "NullableDateTimeOffsetList" => (IList<T>)NullableDateTimeOffsetList,
            "NullableDecimalList" => (IList<T>)NullableDecimalList,
            "NullableDecimal128List" => (IList<T>)NullableDecimal128List,
            "NullableObjectIdList" => (IList<T>)NullableObjectIdList,
            "NullableGuidList" => (IList<T>)NullableGuidList,
            "NullableStringList" => (IList<T>)NullableStringList,
            "NullableByteArrayList" => (IList<T>)NullableByteArrayList,
            "RealmValueList" => (IList<T>)RealmValueList,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}");
        }
    }
}