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
    [Woven(typeof(PersonObjectHelper))]
    public partial class Person : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Person", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Backlinks("Dogs", "Dog", "Owner"),

        }.Build();

        #region IRealmObject implementation

        private IPersonAccessor _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public Person()
        {
            _accessor = new PersonUnmanagedAccessor(typeof(PersonObjectHelper));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (PersonManagedAccessor)managedAccessor;

            if (helper != null)
            {


                Id = unmanagedAccessor.Id;
                Name = unmanagedAccessor.Name;

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

        public static explicit operator Person(RealmValue val) => val.AsRealmObject<Person>();

        public static implicit operator RealmValue(Person val) => RealmValue.Object(val);
    }
}

namespace Realms.Generated
{

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PersonObjectHelper : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public ManagedAccessor CreateAccessor() => new PersonManagedAccessor();

        public IRealmObjectBase CreateInstance()
        {
            return new Person();
        }

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            value = ((IPersonAccessor)instance.Accessor).Id;
            return true;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPersonAccessor : IRealmAccessor
    {
        int Id { get; set; }

        string Name { get; set; }

        IQueryable<Dog> Dogs { get; }


    }

    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PersonManagedAccessor : ManagedAccessor, IPersonAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValueUnique("Id", value);
        }
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        private IQueryable<Dog> _dogs;
        public IQueryable<Dog> Dogs
        {
            get
            {
                if(_dogs == null)
                {
                    _dogs = GetBacklinks<Dog>("Dogs");
                }

                return _dogs;
            }
        }


    }

    
    internal class PersonUnmanagedAccessor : UnmanagedAccessor, IPersonAccessor
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
        public IQueryable<Dog> Dogs => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");



        public PersonUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "Name" => _name,
                "Dogs" =>  throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),

                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "Name":
                    Name = (string)val;
                    return;

                default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "Id")
            {
                throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
            }

            Id = (int)val;
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
