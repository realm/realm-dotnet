using SourceGeneratorPlayground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace SourceGeneratorPlayground
{
    [Generated]
    [Woven(typeof(AllTypesClassObjectHelper))]
    public partial class AllTypesClass : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("AllTypesClass", isEmbedded: false)
        {
            Property.Primitive("CharProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IAllTypesClassAccessor _accessor;
        
        public IRealmAccessor Accessor
        {
            get
            {
                if (_accessor == null)
                {
                    _accessor = new AllTypesClassUnmanagedAccessor(typeof(AllTypesClassObjectHelper));
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
            var newAccessor = (IAllTypesClassAccessor)managedAccessor;
        
            if (helper != null)
            {
                var oldAccessor = (IAllTypesClassAccessor)Accessor;
                
                newAccessor.CharProperty = oldAccessor.CharProperty;
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
        
        public static explicit operator AllTypesClass(RealmValue val) => val.AsRealmObject<AllTypesClass>();
        
        public static implicit operator RealmValue(AllTypesClass val) => RealmValue.Object(val);
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class AllTypesClassObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new AllTypesClassManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new AllTypesClass();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IAllTypesClassAccessor : IRealmAccessor
    {
        char CharProperty { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class AllTypesClassManagedAccessor : ManagedAccessor, IAllTypesClassAccessor
    {
        public char CharProperty
        {
            get => (char)GetValue("CharProperty");
            set => SetValue("CharProperty", value);
        }
    }

    internal class AllTypesClassUnmanagedAccessor : UnmanagedAccessor, IAllTypesClassAccessor
    {
        private char _charProperty;
        public char CharProperty
        {
            get => _charProperty;
            set
            {
                _charProperty = value;
                RaisePropertyChanged("CharProperty");
            }
        }
    
        public AllTypesClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "CharProperty" => _charProperty,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "CharProperty":
                    CharProperty = (char)val;
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
