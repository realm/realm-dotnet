﻿// <auto-generated />
using Realms.Tests.Database;
using Realms.Tests.Database.Generated;
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

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(ProductObjectHelper))]
    public partial class Product : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Product", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Id"),
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.Primitive("Date", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Date"),
            Property.ObjectList("Reports", "Report", managedName: "Reports"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IProductAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IProductAccessor Accessor => _accessor = _accessor ?? new ProductUnmanagedAccessor(typeof(Product));
        
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
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IProductAccessor)managedAccessor;
            var oldAccessor = _accessor as IProductAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Reports.Clear();
                }
                
                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(!skipDefaults || oldAccessor.Date != default(string))
                {
                    newAccessor.Date = oldAccessor.Date;
                }
                
                CollectionExtensions.PopulateCollection(oldAccessor.Reports, newAccessor.Reports, update, skipDefaults);
            }
        
            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }
        
            OnManaged();
        }
        
        #endregion
        
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
        
        partial void OnManaged();
        
        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications(RaisePropertyChanged);
        }
        
        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }
        
        public static explicit operator Product(RealmValue val) => val.AsRealmObject<Product>();
        
        public static implicit operator RealmValue(Product val) => RealmValue.Object(val);
        
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
        private class ProductObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new ProductManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new Product();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Database.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IProductAccessor : IRealmAccessor
    {
        int Id { get; set; }
        
        string Name { get; set; }
        
        string Date { get; set; }
        
        IList<Report> Reports { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ProductManagedAccessor : ManagedAccessor, IProductAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
        
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        public string Date
        {
            get => (string)GetValue("Date");
            set => SetValue("Date", value);
        }
        
        private IList<Report> _reports;
        public IList<Report> Reports
        {
            get
            {
                if (_reports == null)
                {
                    _reports = GetListValue<Report>("Reports");
                }
        
                return _reports;
            }
        }
    }

    internal class ProductUnmanagedAccessor : UnmanagedAccessor, IProductAccessor
    {
        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        private string _date;
        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                RaisePropertyChanged("Date");
            }
        }
        
        public IList<Report> Reports { get; } = new List<Report>();
    
        public ProductUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "Name" => _name,
                "Date" => _date,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    Id = (int)val;
                    return;
                case "Name":
                    Name = (string)val;
                    return;
                case "Date":
                    Date = (string)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
        }
    
        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Reports" => (IList<T>)Reports,
            
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

