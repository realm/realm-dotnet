﻿// <auto-generated />
using Realms.Tests.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(PersonObjectHelper))]
    public partial class Person : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Person", isEmbedded: false)
        {
            Property.Primitive("FirstName", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("LastName", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("Score", RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Latitude", RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Longitude", RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Salary", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("IsAmbivalent", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("Birthday", RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("PublicCertificateBytes", RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("OptionalAddress", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("Email", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("IsInteresting", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.ObjectList("Friends", "Person"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IPersonAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IPersonAccessor Accessor => _accessor = _accessor ?? new PersonUnmanagedAccessor(typeof(Person));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IPersonAccessor)managedAccessor;
            var oldAccessor = _accessor as IPersonAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Friends.Clear();
                }
                
                newAccessor.FirstName = oldAccessor.FirstName;
                newAccessor.LastName = oldAccessor.LastName;
                newAccessor.Score = oldAccessor.Score;
                newAccessor.Latitude = oldAccessor.Latitude;
                newAccessor.Longitude = oldAccessor.Longitude;
                newAccessor.Salary = oldAccessor.Salary;
                newAccessor.IsAmbivalent = oldAccessor.IsAmbivalent;
                newAccessor.Birthday = oldAccessor.Birthday;
                newAccessor.PublicCertificateBytes = oldAccessor.PublicCertificateBytes;
                newAccessor.OptionalAddress = oldAccessor.OptionalAddress;
                newAccessor.Email_ = oldAccessor.Email_;
                newAccessor.IsInteresting = oldAccessor.IsInteresting;
                foreach(var val in oldAccessor.Friends)
                {
                    newAccessor.Realm.Add(val, update);
                    newAccessor.Friends.Add(val);
                }
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
        
        public static explicit operator Person(RealmValue val) => val.AsRealmObject<Person>();
        
        public static implicit operator RealmValue(Person val) => RealmValue.Object(val);
        
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
        
        /***
        public override string ToString()
        {
            return Accessor.ToString();
        }
        **/
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class PersonObjectHelper : IRealmObjectHelper
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
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPersonAccessor : IRealmAccessor
    {
        string FirstName { get; set; }
        
        string LastName { get; set; }
        
        float Score { get; set; }
        
        double Latitude { get; set; }
        
        double Longitude { get; set; }
        
        long Salary { get; set; }
        
        bool? IsAmbivalent { get; set; }
        
        DateTimeOffset Birthday { get; set; }
        
        byte[] PublicCertificateBytes { get; set; }
        
        string OptionalAddress { get; set; }
        
        string Email_ { get; set; }
        
        bool IsInteresting { get; set; }
        
        IList<Person> Friends { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PersonManagedAccessor : ManagedAccessor, IPersonAccessor
    {
        public string FirstName
        {
            get => (string)GetValue("FirstName");
            set => SetValue("FirstName", value);
        }
        
        public string LastName
        {
            get => (string)GetValue("LastName");
            set => SetValue("LastName", value);
        }
        
        public float Score
        {
            get => (float)GetValue("Score");
            set => SetValue("Score", value);
        }
        
        public double Latitude
        {
            get => (double)GetValue("Latitude");
            set => SetValue("Latitude", value);
        }
        
        public double Longitude
        {
            get => (double)GetValue("Longitude");
            set => SetValue("Longitude", value);
        }
        
        public long Salary
        {
            get => (long)GetValue("Salary");
            set => SetValue("Salary", value);
        }
        
        public bool? IsAmbivalent
        {
            get => (bool?)GetValue("IsAmbivalent");
            set => SetValue("IsAmbivalent", value);
        }
        
        public DateTimeOffset Birthday
        {
            get => (DateTimeOffset)GetValue("Birthday");
            set => SetValue("Birthday", value);
        }
        
        public byte[] PublicCertificateBytes
        {
            get => (byte[])GetValue("PublicCertificateBytes");
            set => SetValue("PublicCertificateBytes", value);
        }
        
        public string OptionalAddress
        {
            get => (string)GetValue("OptionalAddress");
            set => SetValue("OptionalAddress", value);
        }
        
        public string Email_
        {
            get => (string)GetValue("Email");
            set => SetValue("Email", value);
        }
        
        public bool IsInteresting
        {
            get => (bool)GetValue("IsInteresting");
            set => SetValue("IsInteresting", value);
        }
        
        private IList<Person> _friends;
        public IList<Person> Friends
        {
            get
            {
                if (_friends == null)
                {
                    _friends = GetListValue<Person>("Friends");
                }
        
                return _friends;
            }
        }
    }

    internal class PersonUnmanagedAccessor : UnmanagedAccessor, IPersonAccessor
    {
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged("FirstName");
            }
        }
        
        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged("LastName");
            }
        }
        
        private float _score;
        public float Score
        {
            get => _score;
            set
            {
                _score = value;
                RaisePropertyChanged("Score");
            }
        }
        
        private double _latitude;
        public double Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                RaisePropertyChanged("Latitude");
            }
        }
        
        private double _longitude;
        public double Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
                RaisePropertyChanged("Longitude");
            }
        }
        
        private long _salary;
        public long Salary
        {
            get => _salary;
            set
            {
                _salary = value;
                RaisePropertyChanged("Salary");
            }
        }
        
        private bool? _isAmbivalent;
        public bool? IsAmbivalent
        {
            get => _isAmbivalent;
            set
            {
                _isAmbivalent = value;
                RaisePropertyChanged("IsAmbivalent");
            }
        }
        
        private DateTimeOffset _birthday;
        public DateTimeOffset Birthday
        {
            get => _birthday;
            set
            {
                _birthday = value;
                RaisePropertyChanged("Birthday");
            }
        }
        
        private byte[] _publicCertificateBytes;
        public byte[] PublicCertificateBytes
        {
            get => _publicCertificateBytes;
            set
            {
                _publicCertificateBytes = value;
                RaisePropertyChanged("PublicCertificateBytes");
            }
        }
        
        private string _optionalAddress;
        public string OptionalAddress
        {
            get => _optionalAddress;
            set
            {
                _optionalAddress = value;
                RaisePropertyChanged("OptionalAddress");
            }
        }
        
        private string _email_;
        public string Email_
        {
            get => _email_;
            set
            {
                _email_ = value;
                RaisePropertyChanged("Email");
            }
        }
        
        private bool _isInteresting;
        public bool IsInteresting
        {
            get => _isInteresting;
            set
            {
                _isInteresting = value;
                RaisePropertyChanged("IsInteresting");
            }
        }
        
        public IList<Person> Friends { get; } = new List<Person>();
    
        public PersonUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "FirstName" => _firstName,
                "LastName" => _lastName,
                "Score" => _score,
                "Latitude" => _latitude,
                "Longitude" => _longitude,
                "Salary" => _salary,
                "IsAmbivalent" => _isAmbivalent,
                "Birthday" => _birthday,
                "PublicCertificateBytes" => _publicCertificateBytes,
                "OptionalAddress" => _optionalAddress,
                "Email" => _email_,
                "IsInteresting" => _isInteresting,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "FirstName":
                    FirstName = (string)val;
                    return;
                case "LastName":
                    LastName = (string)val;
                    return;
                case "Score":
                    Score = (float)val;
                    return;
                case "Latitude":
                    Latitude = (double)val;
                    return;
                case "Longitude":
                    Longitude = (double)val;
                    return;
                case "Salary":
                    Salary = (long)val;
                    return;
                case "IsAmbivalent":
                    IsAmbivalent = (bool?)val;
                    return;
                case "Birthday":
                    Birthday = (DateTimeOffset)val;
                    return;
                case "PublicCertificateBytes":
                    PublicCertificateBytes = (byte[])val;
                    return;
                case "OptionalAddress":
                    OptionalAddress = (string)val;
                    return;
                case "Email":
                    Email_ = (string)val;
                    return;
                case "IsInteresting":
                    IsInteresting = (bool)val;
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
            "Friends" => (IList<T>)Friends,
            
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

