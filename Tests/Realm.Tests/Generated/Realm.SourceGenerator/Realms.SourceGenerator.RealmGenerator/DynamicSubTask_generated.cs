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
    [Woven(typeof(DynamicSubTaskObjectHelper))]
    public partial class DynamicSubTask : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("DynamicSubTask", ObjectSchema.ObjectType.EmbeddedObject)
        {
            Property.Primitive("Summary", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Summary"),
            Property.Object("CompletionReport", "CompletionReport", managedName: "CompletionReport"),
            Property.ObjectList("SubSubTasks", "DynamicSubSubTask", managedName: "SubSubTasks"),
        }.Build();

        ~DynamicSubTask()
        {
            UnsubscribeFromNotifications();
        }

        #region IEmbeddedObject implementation

        private IDynamicSubTaskAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IDynamicSubTaskAccessor Accessor => _accessor = _accessor ?? new DynamicSubTaskUnmanagedAccessor(typeof(DynamicSubTask));

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
            var newAccessor = (IDynamicSubTaskAccessor)managedAccessor;
            var oldAccessor = _accessor as IDynamicSubTaskAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.SubSubTasks.Clear();
                }

                if(!skipDefaults || oldAccessor.Summary != default(string))
                {
                    newAccessor.Summary = oldAccessor.Summary;
                }
                newAccessor.CompletionReport = oldAccessor.CompletionReport;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SubSubTasks, newAccessor.SubSubTasks, update, skipDefaults);
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

        public static explicit operator DynamicSubTask(RealmValue val) => val.AsRealmObject<DynamicSubTask>();

        public static implicit operator RealmValue(DynamicSubTask val) => RealmValue.Object(val);

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
        private class DynamicSubTaskObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new DynamicSubTaskManagedAccessor();

            public IRealmObjectBase CreateInstance() => new DynamicSubTask();

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
    internal interface IDynamicSubTaskAccessor : IRealmAccessor
    {
        string Summary { get; set; }

        CompletionReport CompletionReport { get; set; }

        IList<DynamicSubSubTask> SubSubTasks { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DynamicSubTaskManagedAccessor : ManagedAccessor, IDynamicSubTaskAccessor
    {
        public string Summary
        {
            get => (string)GetValue("Summary");
            set => SetValue("Summary", value);
        }

        public CompletionReport CompletionReport
        {
            get => (CompletionReport)GetValue("CompletionReport");
            set => SetValue("CompletionReport", value);
        }

        private IList<DynamicSubSubTask> _subSubTasks;
        public IList<DynamicSubSubTask> SubSubTasks
        {
            get
            {
                if (_subSubTasks == null)
                {
                    _subSubTasks = GetListValue<DynamicSubSubTask>("SubSubTasks");
                }

                return _subSubTasks;
            }
        }
    }

    internal class DynamicSubTaskUnmanagedAccessor : UnmanagedAccessor, IDynamicSubTaskAccessor
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

        private CompletionReport _completionReport;
        public CompletionReport CompletionReport
        {
            get => _completionReport;
            set
            {
                _completionReport = value;
                RaisePropertyChanged("CompletionReport");
            }
        }

        public IList<DynamicSubSubTask> SubSubTasks { get; } = new List<DynamicSubSubTask>();

        public DynamicSubTaskUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Summary" => _summary,
                "CompletionReport" => _completionReport,
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
                case "CompletionReport":
                    CompletionReport = (CompletionReport)val;
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
            "SubSubTasks" => (IList<T>)SubSubTasks,

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