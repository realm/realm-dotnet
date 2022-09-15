////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Realms.Generated;
using Realms.Schema;
using Realms.Weaving;

namespace Realms.Tests.SourceGeneration.TestClasses
{
#pragma warning disable CA1507 // Use nameof to express symbol names

    [Ignored]
    public partial class ManualllyGeneratedClass : IRealmObject
    {
        #region Original class

        // public int Integer { get; set; }

        // public IList<int> IntegerList { get; }

        // [MapTo("_name")]
        // public string Name { get; set; }

        // [PrimaryKey]
        // public int PKey { get; set; }
        #endregion

        #region Weaved class

        public int IntValue
        {
            get => Accessor.IntValue;
            set => Accessor.IntValue = value;
        }

        public IList<int> ListValue => Accessor.ListValue;

        [MapTo("_string")]
        public string StringValue
        {
            get => Accessor.StringValue;
            set => Accessor.StringValue = value;
        }

        [PrimaryKey]
        public int PrimaryKeyValue
        {
            get => Accessor.PrimaryKeyValue;
            set => Accessor.PrimaryKeyValue = value;
        }

        #endregion
    }

    [Woven(typeof(ManuallyGeneratedClassObjectHelper))]
    public partial class ManualllyGeneratedClass : IRealmObject, INotifyPropertyChanged
    {
        // The schema property could be part of an interface, but for that the user needs to have at least .NET 5.0 and C# 8.0
        // So we need to use reflection to check if this exists.
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ManualllyGeneratedClass", isEmbedded: false)
        {
            Property.Primitive("IntValue", RealmValueType.Int),
            Property.PrimitiveList("ListValue", RealmValueType.Int),
            Property.Primitive("_string", RealmValueType.String),
            Property.Primitive("PrimaryKeyValue", RealmValueType.Int, isPrimaryKey: true),
        }.Build();

        #region IRealmObject implementation

        private IManuallyGeneratedClassAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IManuallyGeneratedClassAccessor Accessor => _accessor ??= new ManuallyGeneratedClassUnmanagedAccessor(typeof(ManualllyGeneratedClass));

        public bool IsManaged => Accessor.IsManaged;

        public bool IsValid => Accessor.IsValid;

        public bool IsFrozen => Accessor.IsFrozen;

        public Realm Realm => Accessor.Realm;

        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;

        public int BacklinksCount => Accessor.BacklinksCount;

        public ManualllyGeneratedClass()
        {
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IManuallyGeneratedClassAccessor)managedAccessor;

            if (helper != null)
            {
                var oldAccessor = Accessor;

                newAccessor.IntValue = oldAccessor.IntValue;
                newAccessor.StringValue = oldAccessor.StringValue;

                if (!skipDefaults)
                {
                    newAccessor.ListValue.Clear();
                }

                foreach (var val in oldAccessor.ListValue)
                {
                    newAccessor.ListValue.Add(val);
                }
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class ManuallyGeneratedClassObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new ManuallyGeneratedClassManagedAccessor();

            public IRealmObjectBase CreateInstance()
            {
                return new ManualllyGeneratedClass();
            }

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IManuallyGeneratedClassAccessor)instance.Accessor).PrimaryKeyValue;
                return true;
            }
        }
    }
}

// Having a separate namespace allows to hide the implementation details better.
namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IManuallyGeneratedClassAccessor : IRealmAccessor
    {
        int IntValue { get; set; }

        IList<int> ListValue { get; }

        string StringValue { get; set; }

        int PrimaryKeyValue { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ManuallyGeneratedClassManagedAccessor
        : ManagedAccessor, IManuallyGeneratedClassAccessor
    {
        private IList<int> _listValue;

        /** If we want to make this more efficient, we can use property indexes here.
         * We should have all the info necessary to compute them during source generation (as we do in Realm.CreateRealmObjectMetadata).
         */
        public int IntValue
        {
            get => (int)GetValue("IntValue");
            set => SetValue("IntValue", value);
        }

        public IList<int> ListValue
        {
            get
            {
                if (_listValue == null)
                {
                    _listValue = GetListValue<int>("ListValue");
                }

                return _listValue;
            }
        }

        public string StringValue
        {
            get => (string)GetValue("_string");
            set => SetValue("_string", value);
        }

        public int PrimaryKeyValue
        {
            get => (int)GetValue("PrimaryKeyValue");
            set => SetValueUnique("PrimaryKeyValue", value);
        }
    }

    internal class ManuallyGeneratedClassUnmanagedAccessor
        : UnmanagedAccessor, IManuallyGeneratedClassAccessor
    {
        private int intValue;
        private string stringValue;
        private int primaryKeyValue;

        public int IntValue
        {
            get => intValue;
            set
            {
                intValue = value;
                RaisePropertyChanged("IntValue");
            }
        }

        public IList<int> ListValue { get; } = new List<int>();

        public string StringValue
        {
            get => stringValue;
            set
            {
                stringValue = value;
                RaisePropertyChanged("_string");
            }
        }

        public int PrimaryKeyValue
        {
            get => primaryKeyValue;
            set
            {
                primaryKeyValue = value;
                RaisePropertyChanged("PrimaryKeyValue");
            }
        }

        public ManuallyGeneratedClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_string" => stringValue,
                "IntValue" => intValue,
                "PrimaryKeyValue" => primaryKeyValue,
                _ => throw new ArgumentException($"The object does not have a Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_string":
                    StringValue = (string)val;
                    return;
                case "IntValue":
                    IntValue = (int)val;
                    return;
                case "PrimaryKeyValue":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "ListValue" => (IList<T>)ListValue,
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

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "PrimaryKeyValue")
            {
                throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
            }

            PrimaryKeyValue = (int)val;
        }
    }
#pragma warning restore CA1507 // Use nameof to express symbol names
}
