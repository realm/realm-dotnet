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
    [Woven(typeof(DynamicOwnerObjectHelper))]
    public partial class DynamicOwner : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("DynamicOwner", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.Object("TopDog", "DynamicDog", managedName: "TopDog"),
            Property.ObjectList("Dogs", "DynamicDog", managedName: "Dogs"),
            Property.PrimitiveList("Tags", RealmValueType.String, areElementsNullable: true, managedName: "Tags"),
            Property.ObjectDictionary("DogsDictionary", "DynamicDog", managedName: "DogsDictionary"),
            Property.PrimitiveDictionary("TagsDictionary", RealmValueType.String, areElementsNullable: true, managedName: "TagsDictionary"),
            Property.ObjectSet("DogsSet", "DynamicDog", managedName: "DogsSet"),
            Property.PrimitiveSet("TagsSet", RealmValueType.String, areElementsNullable: true, managedName: "TagsSet"),
        }.Build();

        #region IRealmObject implementation

        private IDynamicOwnerAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IDynamicOwnerAccessor Accessor => _accessor = _accessor ?? new DynamicOwnerUnmanagedAccessor(typeof(DynamicOwner));

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
            var newAccessor = (IDynamicOwnerAccessor)managedAccessor;
            var oldAccessor = _accessor as IDynamicOwnerAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Dogs.Clear();
                    newAccessor.Tags.Clear();
                    newAccessor.DogsDictionary.Clear();
                    newAccessor.TagsDictionary.Clear();
                    newAccessor.DogsSet.Clear();
                    newAccessor.TagsSet.Clear();
                }

                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(oldAccessor.TopDog != null)
                {
                    newAccessor.Realm.Add(oldAccessor.TopDog, update);
                }
                newAccessor.TopDog = oldAccessor.TopDog;

                CollectionExtensions.PopulateCollection(oldAccessor.Dogs, newAccessor.Dogs, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.Tags, newAccessor.Tags, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.DogsDictionary, newAccessor.DogsDictionary, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.TagsDictionary, newAccessor.TagsDictionary, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.DogsSet, newAccessor.DogsSet, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.TagsSet, newAccessor.TagsSet, update, skipDefaults);
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

        public static explicit operator DynamicOwner(RealmValue val) => val.AsRealmObject<DynamicOwner>();

        public static implicit operator RealmValue(DynamicOwner val) => RealmValue.Object(val);

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
        private class DynamicOwnerObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new DynamicOwnerManagedAccessor();

            public IRealmObjectBase CreateInstance()
            {
                return new DynamicOwner();
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
    internal interface IDynamicOwnerAccessor : IRealmAccessor
    {
        string Name { get; set; }

        DynamicDog TopDog { get; set; }

        IList<DynamicDog> Dogs { get; }

        IList<string> Tags { get; }

        IDictionary<string, DynamicDog> DogsDictionary { get; }

        IDictionary<string, string> TagsDictionary { get; }

        ISet<DynamicDog> DogsSet { get; }

        ISet<string> TagsSet { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DynamicOwnerManagedAccessor : ManagedAccessor, IDynamicOwnerAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }

        public DynamicDog TopDog
        {
            get => (DynamicDog)GetValue("TopDog");
            set => SetValue("TopDog", value);
        }

        private IList<DynamicDog> _dogs;
        public IList<DynamicDog> Dogs
        {
            get
            {
                if (_dogs == null)
                {
                    _dogs = GetListValue<DynamicDog>("Dogs");
                }

                return _dogs;
            }
        }

        private IList<string> _tags;
        public IList<string> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = GetListValue<string>("Tags");
                }

                return _tags;
            }
        }

        private IDictionary<string, DynamicDog> _dogsDictionary;
        public IDictionary<string, DynamicDog> DogsDictionary
        {
            get
            {
                if (_dogsDictionary == null)
                {
                    _dogsDictionary = GetDictionaryValue<DynamicDog>("DogsDictionary");
                }

                return _dogsDictionary;
            }
        }

        private IDictionary<string, string> _tagsDictionary;
        public IDictionary<string, string> TagsDictionary
        {
            get
            {
                if (_tagsDictionary == null)
                {
                    _tagsDictionary = GetDictionaryValue<string>("TagsDictionary");
                }

                return _tagsDictionary;
            }
        }

        private ISet<DynamicDog> _dogsSet;
        public ISet<DynamicDog> DogsSet
        {
            get
            {
                if (_dogsSet == null)
                {
                    _dogsSet = GetSetValue<DynamicDog>("DogsSet");
                }

                return _dogsSet;
            }
        }

        private ISet<string> _tagsSet;
        public ISet<string> TagsSet
        {
            get
            {
                if (_tagsSet == null)
                {
                    _tagsSet = GetSetValue<string>("TagsSet");
                }

                return _tagsSet;
            }
        }
    }

    internal class DynamicOwnerUnmanagedAccessor : UnmanagedAccessor, IDynamicOwnerAccessor
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

        private DynamicDog _topDog;
        public DynamicDog TopDog
        {
            get => _topDog;
            set
            {
                _topDog = value;
                RaisePropertyChanged("TopDog");
            }
        }

        public IList<DynamicDog> Dogs { get; } = new List<DynamicDog>();

        public IList<string> Tags { get; } = new List<string>();

        public IDictionary<string, DynamicDog> DogsDictionary { get; } = new Dictionary<string, DynamicDog>();

        public IDictionary<string, string> TagsDictionary { get; } = new Dictionary<string, string>();

        public ISet<DynamicDog> DogsSet { get; } = new HashSet<DynamicDog>(RealmSet<DynamicDog>.Comparer);

        public ISet<string> TagsSet { get; } = new HashSet<string>(RealmSet<string>.Comparer);

        public DynamicOwnerUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "TopDog" => _topDog,
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
                case "TopDog":
                    TopDog = (DynamicDog)val;
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
            "Dogs" => (IList<T>)Dogs,
            "Tags" => (IList<T>)Tags,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "DogsSet" => (ISet<T>)DogsSet,
            "TagsSet" => (ISet<T>)TagsSet,

                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "DogsDictionary" => (IDictionary<string, TValue>)DogsDictionary,
                "TagsDictionary" => (IDictionary<string, TValue>)TagsDictionary,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}