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
    [Woven(typeof(DogObjectHelper))]
    public partial class Dog : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Dog", isEmbedded: false)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Object("Owner", "Person"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IDogAccessor _accessor;
        
        public IRealmAccessor Accessor
        {
            get
            {
                if (_accessor == null)
                {
                    _accessor = new DogUnmanagedAccessor(typeof(DogObjectHelper));
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
            var newAccessor = (IDogAccessor)managedAccessor;
        
            if (helper != null)
            {
                var oldAccessor = (IDogAccessor)Accessor;
                
                newAccessor.Name = oldAccessor.Name;
                newAccessor.Owner = oldAccessor.Owner;
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
        
        public static explicit operator Dog(RealmValue val) => val.AsRealmObject<Dog>();
        
        public static implicit operator RealmValue(Dog val) => RealmValue.Object(val);
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class DogObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new DogManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new Dog();
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
    internal interface IDogAccessor : IRealmAccessor
    {
        string Name { get; set; }
        
        Person Owner { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DogManagedAccessor : ManagedAccessor, IDogAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        public Person Owner
        {
            get => (Person)GetValue("Owner");
            set => SetValue("Owner", value);
        }
    }

    internal class DogUnmanagedAccessor : UnmanagedAccessor, IDogAccessor
    {
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
        
        private Person _owner;
        public Person Owner
        {
            get => _owner;
            set
            {
                _owner = value;
                RaisePropertyChanged("Owner");
            }
        }
    
        public DogUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "Owner" => _owner,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = (string)val;
                    return;
                case "Owner":
                    Owner = (Person)val;
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
