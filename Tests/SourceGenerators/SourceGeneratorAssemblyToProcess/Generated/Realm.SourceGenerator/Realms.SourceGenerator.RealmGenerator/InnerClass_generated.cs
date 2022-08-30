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
    public partial class OuterClass
    {
        public partial class MediumClass
        {
            [Generated]
            [Woven(typeof(InnerClassObjectHelper))]
            private partial class InnerClass : IRealmObject, INotifyPropertyChanged
            {
                public static ObjectSchema RealmSchema = new ObjectSchema.Builder("InnerClass", isEmbedded: false)
                {
                
                }.Build();
                
                #region IRealmObject implementation
                
                private IInnerClassAccessor _accessor;
                
                public IRealmAccessor Accessor
                {
                    get
                    {
                        if (_accessor == null)
                        {
                            _accessor = new InnerClassUnmanagedAccessor(typeof(InnerClassObjectHelper));
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
                    var newAccessor = (IInnerClassAccessor)managedAccessor;
                
                    if (helper != null)
                    {
                        var oldAccessor = (IInnerClassAccessor)Accessor;
                        
                
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
                
                public static explicit operator InnerClass(RealmValue val) => val.AsRealmObject<InnerClass>();
                
                public static implicit operator RealmValue(InnerClass val) => RealmValue.Object(val);
            
                [EditorBrowsable(EditorBrowsableState.Never)]
                private class InnerClassObjectHelper : IRealmObjectHelper
                {
                    public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
                    {
                        throw new InvalidOperationException("This method should not be called for source generated classes.");
                    }
                
                    public ManagedAccessor CreateAccessor() => new InnerClassManagedAccessor();
                
                    public IRealmObjectBase CreateInstance()
                    {
                        return new InnerClass();
                    }
                
                    public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
                    {
                        value = null;
                        return false;
                    }
                }
            }
        }
        
    }
    
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IInnerClassAccessor : IRealmAccessor
    {
    
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class InnerClassManagedAccessor : ManagedAccessor, IInnerClassAccessor
    {
    
    }

    internal class InnerClassUnmanagedAccessor : UnmanagedAccessor, IInnerClassAccessor
    {
    
    
        public InnerClassUnmanagedAccessor(Type objectType) : base(objectType)
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
