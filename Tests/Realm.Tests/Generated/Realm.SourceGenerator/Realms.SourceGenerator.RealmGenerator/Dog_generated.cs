﻿// <auto-generated />
using Realms.Tests.Generated;
using Realms.Tests;
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

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(DogObjectHelper))]
    public partial class Dog : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Dog", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.Primitive("Color", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Color"),
            Property.Primitive("Vaccinated", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Vaccinated"),
            Property.Primitive("Age", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Age"),
            Property.Backlinks("Owners", "Owner", "ListOfDogs", managedName: "Owners"),
        }.Build();

        #region IRealmObject implementation

        private IDogAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IDogAccessor Accessor => _accessor = _accessor ?? new DogUnmanagedAccessor(typeof(Dog));

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
            var newAccessor = (IDogAccessor)managedAccessor;
            var oldAccessor = _accessor as IDogAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {

                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(!skipDefaults || oldAccessor.Color != default(string))
                {
                    newAccessor.Color = oldAccessor.Color;
                }
                if(!skipDefaults || oldAccessor.Vaccinated != default(bool))
                {
                    newAccessor.Vaccinated = oldAccessor.Vaccinated;
                }
                if(!skipDefaults || oldAccessor.Age != default(int))
                {
                    newAccessor.Age = oldAccessor.Age;
                }
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

        public static explicit operator Dog(RealmValue val) => val.AsRealmObject<Dog>();

        public static implicit operator RealmValue(Dog val) => RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

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

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class DogObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new DogManagedAccessor();

            public IRealmObjectBase CreateInstance() => new Dog();

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
    internal interface IDogAccessor : IRealmAccessor
    {
        string Name { get; set; }

        string Color { get; set; }

        bool Vaccinated { get; set; }

        int Age { get; set; }

        IQueryable<Owner> Owners { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DogManagedAccessor : ManagedAccessor, IDogAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }

        public string Color
        {
            get => (string)GetValue("Color");
            set => SetValue("Color", value);
        }

        public bool Vaccinated
        {
            get => (bool)GetValue("Vaccinated");
            set => SetValue("Vaccinated", value);
        }

        public int Age
        {
            get => (int)GetValue("Age");
            set => SetValue("Age", value);
        }

        private IQueryable<Owner> _owners;
        public IQueryable<Owner> Owners
        {
            get
            {
                if (_owners == null)
                {
                    _owners = GetBacklinks<Owner>("Owners");
                }

                return _owners;
            }
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

        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }

        private bool _vaccinated;
        public bool Vaccinated
        {
            get => _vaccinated;
            set
            {
                _vaccinated = value;
                RaisePropertyChanged("Vaccinated");
            }
        }

        private int _age;
        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                RaisePropertyChanged("Age");
            }
        }

        public IQueryable<Owner> Owners => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public DogUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "Color" => _color,
                "Vaccinated" => _vaccinated,
                "Age" => _age,
                "Owners" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
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
                case "Color":
                    Color = (string)val;
                    return;
                case "Vaccinated":
                    Vaccinated = (bool)val;
                    return;
                case "Age":
                    Age = (int)val;
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