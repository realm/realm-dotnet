using SourceGeneratorAssemblyToProcess.TestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace SourceGeneratorAssemblyToProcess.TestClasses
{
    [Generated]
    [Woven(typeof(ClassWithoutParameterlessConstructorObjectHelper))]
    public partial class ClassWithoutParameterlessConstructor : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ClassWithoutParameterlessConstructor", isEmbedded: false)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private IClassWithoutParameterlessConstructorAccessor _accessor;
        
        public IRealmAccessor Accessor
        {
            get
            {
                if (_accessor == null)
                {
                    _accessor = new ClassWithoutParameterlessConstructorUnmanagedAccessor(typeof(ClassWithoutParameterlessConstructorObjectHelper));
                }
        
                return _accessor;
            }
        }
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        private ClassWithoutParameterlessConstructor() {}
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IClassWithoutParameterlessConstructorAccessor)managedAccessor;
        
            if (helper != null)
            {
                var oldAccessor = (IClassWithoutParameterlessConstructorAccessor)Accessor;
                
                newAccessor.Name = oldAccessor.Name;
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
        
        public static explicit operator ClassWithoutParameterlessConstructor(RealmValue val) => val.AsRealmObject<ClassWithoutParameterlessConstructor>();
        
        public static implicit operator RealmValue(ClassWithoutParameterlessConstructor val) => RealmValue.Object(val);
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class ClassWithoutParameterlessConstructorObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new ClassWithoutParameterlessConstructorManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new ClassWithoutParameterlessConstructor();
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
    internal interface IClassWithoutParameterlessConstructorAccessor : IRealmAccessor
    {
        string Name { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ClassWithoutParameterlessConstructorManagedAccessor : ManagedAccessor, IClassWithoutParameterlessConstructorAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
    }

    internal class ClassWithoutParameterlessConstructorUnmanagedAccessor : UnmanagedAccessor, IClassWithoutParameterlessConstructorAccessor
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
    
        public ClassWithoutParameterlessConstructorUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
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
