using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using MongoDB.Bson;
using SourceGeneratorPlayground;

namespace SourceGeneratorPlayground
{

    [Generated]
    [Woven(typeof(BacklinkClassObjectHelper))]
    public partial class BacklinkClass : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("BacklinkClass", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Object("InverseLink", "UnsupportedBacklink"),

        }.Build();

        #region IRealmObject implementation

        private IBacklinkClassAccessor _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public BacklinkClass()
        {
            _accessor = new BacklinkClassUnmanagedAccessor(typeof(BacklinkClassObjectHelper));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (BacklinkClassManagedAccessor)managedAccessor;

            if (helper != null)
            {


                Id = unmanagedAccessor.Id;
                InverseLink = unmanagedAccessor.InverseLink;

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
            _accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            _accessor.UnsubscribeFromNotifications();
        }

        public static explicit operator BacklinkClass(RealmValue val) => val.AsRealmObject<BacklinkClass>();

        public static implicit operator RealmValue(BacklinkClass val) => RealmValue.Object(val);
    }
}

namespace Realms.Generated
{

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class BacklinkClassObjectHelper : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public ManagedAccessor CreateAccessor() => new BacklinkClassManagedAccessor();

        public IRealmObjectBase CreateInstance()
        {
            return new BacklinkClass();
        }

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            value = null;
            return false;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IBacklinkClassAccessor : IRealmAccessor
    {
        int Id { get; set; }

        UnsupportedBacklink InverseLink { get; set; }


    }

    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class BacklinkClassManagedAccessor : ManagedAccessor, IBacklinkClassAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
        public UnsupportedBacklink InverseLink
        {
            get => (UnsupportedBacklink)GetValue("InverseLink");
            set => SetValue("InverseLink", value);
        }

    }

    
    internal class BacklinkClassUnmanagedAccessor : UnmanagedAccessor, IBacklinkClassAccessor
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
        private UnsupportedBacklink _inverseLink;
        public UnsupportedBacklink InverseLink
        {
            get => _inverseLink;
            set
            {
                _inverseLink = value;
                RaisePropertyChanged("InverseLink");
            }
        }


        public BacklinkClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "InverseLink" => _inverseLink,

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
                case "InverseLink":
                    InverseLink = (UnsupportedBacklink)val;
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

        public IQueryable<T> GetBacklinks<T>(string propertyName) where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed(persisted) objects.");

    }
}
