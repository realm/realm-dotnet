using SourceGeneratorAssemblyToProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using OtherNamespace;

namespace SourceGeneratorAssemblyToProcess
{
    [Generated]
    [Woven(typeof(NamespaceObjObjectHelper))]
    public partial class NamespaceObj : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("NamespaceObj", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Object("OtherNamespaceObj", "OtherNamespaceObj"),
        }.Build();
        
        #region IRealmObject implementation
        
        private INamespaceObjAccessor _accessor;
        
        public IRealmAccessor Accessor
        {
            get
            {
                if (_accessor == null)
                {
                    _accessor = new NamespaceObjUnmanagedAccessor(typeof(NamespaceObjObjectHelper));
                }
        
                return _accessor;
            }
        }
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (INamespaceObjAccessor)managedAccessor;
        
            if (helper != null)
            {
                var oldAccessor = (INamespaceObjAccessor)Accessor;
                
                newAccessor.Id = oldAccessor.Id;
                newAccessor.OtherNamespaceObj = oldAccessor.OtherNamespaceObj;
            }
        
            _accessor = newAccessor;
        
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
        
        public static explicit operator NamespaceObj(RealmValue val) => val.AsRealmObject<NamespaceObj>();
        
        public static implicit operator RealmValue(NamespaceObj val) => RealmValue.Object(val);
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class NamespaceObjObjectHelper : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }
    
        public ManagedAccessor CreateAccessor() => new NamespaceObjManagedAccessor();
    
        public IRealmObjectBase CreateInstance()
        {
            return new NamespaceObj();
        }
    
        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            value = null;
            return false;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface INamespaceObjAccessor : IRealmAccessor
    {
        int Id { get; set; }
        
        OtherNamespaceObj OtherNamespaceObj { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class NamespaceObjManagedAccessor : ManagedAccessor, INamespaceObjAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
        
        public OtherNamespaceObj OtherNamespaceObj
        {
            get => (OtherNamespaceObj)GetValue("OtherNamespaceObj");
            set => SetValue("OtherNamespaceObj", value);
        }
    }

    internal class NamespaceObjUnmanagedAccessor : UnmanagedAccessor, INamespaceObjAccessor
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
        
        private OtherNamespaceObj _otherNamespaceObj;
        public OtherNamespaceObj OtherNamespaceObj
        {
            get => _otherNamespaceObj;
            set
            {
                _otherNamespaceObj = value;
                RaisePropertyChanged("OtherNamespaceObj");
            }
        }
    
        public NamespaceObjUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "OtherNamespaceObj" => _otherNamespaceObj,
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
                case "OtherNamespaceObj":
                    OtherNamespaceObj = (OtherNamespaceObj)val;
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
            throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}");
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
