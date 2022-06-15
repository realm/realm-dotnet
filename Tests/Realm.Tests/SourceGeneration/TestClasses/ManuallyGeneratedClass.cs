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
using Realm.Generated;
using Realms;
using Realms.Schema;
using Realms.Tests.SourceGeneration.TestClasses;
using Realms.Weaving;

namespace Realms.Tests.SourceGeneration.TestClasses
{
    public partial class ManualllyGeneratedClass : IRealmObject
    {
        #region Original class

        //public int Integer { get; set; }

        //public IList<int> IntegerList { get; }

        //[MapTo("_name")]
        //public string Name { get; set; }

        //[PrimaryKey]
        //public int PKey { get; set; }

        #endregion

        #region Weaved class

        public int IntValue
        {
            get => _accessor.IntValue;
            set => _accessor.IntValue = value;
        }

        public IList<int> ListValue => _accessor.ListValue;

        [MapTo("_string")]
        public string StringValue
        {
            get => _accessor.StringValue;
            set => _accessor.StringValue = value;
        }

        [PrimaryKey]
        public int PrimaryKeyValue
        {
            get => _accessor.PrimaryKeyValue;
            set => _accessor.PrimaryKeyValue = value;
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

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public ManualllyGeneratedClass()
        {
            _accessor = new ManuallyGeneratedClassUnmanagedAccessor(typeof(ManualllyGeneratedClass));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (IManuallyGeneratedClassAccessor)managedAccessor;

            if (helper != null)
            {
                IntValue = unmanagedAccessor.IntValue;
                StringValue = unmanagedAccessor.StringValue;

                if (!skipDefaults)
                {
                    ListValue.Clear();
                }

                foreach (var val in unmanagedAccessor.ListValue)
                {
                    ListValue.Add(val);
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
                if (IsManaged && _propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (IsManaged &&
                    _propertyChanged == null)
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
    }
}

// Having a separate namespace allows to hide the implementation details better.
namespace Realm.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ManuallyGeneratedClassObjectHelper : IRealmObjectHelper
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
            get => GetListValue<int>("ListValue");
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
        public int IntValue { get; set; }

        public IList<int> ListValue { get; } = new List<int>();

        public string StringValue { get; set; }

        public int PrimaryKeyValue { get; set; }

        public ManuallyGeneratedClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    }
}
