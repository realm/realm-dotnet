﻿// <auto-generated />
using Realms.Tests.Database.Generated;
using Realms.Tests.Database;
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
    [Woven(typeof(DynamicSubSubTaskObjectHelper))]
    public partial class DynamicSubSubTask : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("DynamicSubSubTask", ObjectSchema.ObjectType.EmbeddedObject)
        {
            Property.Primitive("Summary", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Summary"),
            Property.Backlinks("ParentSubTask", "DynamicSubTask", "SubSubTasks", managedName: "ParentSubTask"),
            Property.Backlinks("ParentTask", "DynamicTask", "SubSubTasks", managedName: "ParentTask"),
        }.Build();

        #region IEmbeddedObject implementation

        private IDynamicSubSubTaskAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IDynamicSubSubTaskAccessor Accessor => _accessor ?? (_accessor = new DynamicSubSubTaskUnmanagedAccessor(typeof(DynamicSubSubTask)));

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
            var newAccessor = (IDynamicSubSubTaskAccessor)managedAccessor;
            var oldAccessor = (IDynamicSubSubTaskAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if(!skipDefaults || oldAccessor.Summary != default(string))
                {
                    newAccessor.Summary = oldAccessor.Summary;
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

        public static explicit operator DynamicSubSubTask(RealmValue val) => val.AsRealmObject<DynamicSubSubTask>();

        public static implicit operator RealmValue(DynamicSubSubTask val) => RealmValue.Object(val);

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
        private class DynamicSubSubTaskObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new DynamicSubSubTaskManagedAccessor();

            public IRealmObjectBase CreateInstance() => new DynamicSubSubTask();

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
    internal interface IDynamicSubSubTaskAccessor : IRealmAccessor
    {
        string Summary { get; set; }

        IQueryable<DynamicSubTask> ParentSubTask { get; }

        IQueryable<DynamicTask> ParentTask { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DynamicSubSubTaskManagedAccessor : ManagedAccessor, IDynamicSubSubTaskAccessor
    {
        public string Summary
        {
            get => (string)GetValue("Summary");
            set => SetValue("Summary", value);
        }

        private IQueryable<DynamicSubTask> _parentSubTask;
        public IQueryable<DynamicSubTask> ParentSubTask
        {
            get
            {
                if (_parentSubTask == null)
                {
                    _parentSubTask = GetBacklinks<DynamicSubTask>("ParentSubTask");
                }

                return _parentSubTask;
            }
        }

        private IQueryable<DynamicTask> _parentTask;
        public IQueryable<DynamicTask> ParentTask
        {
            get
            {
                if (_parentTask == null)
                {
                    _parentTask = GetBacklinks<DynamicTask>("ParentTask");
                }

                return _parentTask;
            }
        }
    }

    internal class DynamicSubSubTaskUnmanagedAccessor : UnmanagedAccessor, IDynamicSubSubTaskAccessor
    {
        private string _summary;
        public string Summary
        {
            get => _summary;
            set
            {
                _summary = value;
                RaisePropertyChanged("Summary");
            }
        }

        public IQueryable<DynamicSubTask> ParentSubTask => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public IQueryable<DynamicTask> ParentTask => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public DynamicSubSubTaskUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Summary" => _summary,
                "ParentSubTask" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                "ParentTask" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Summary":
                    Summary = (string)val;
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