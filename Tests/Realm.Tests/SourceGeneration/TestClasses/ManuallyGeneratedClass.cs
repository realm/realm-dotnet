// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        //public int Integer { get; set; }

        //public IList<int> IntegerList { get; }

        //TODO Necessary for now to create the schema
        [WovenProperty]
        public int Integer
        {
            get => _accessor.Integer;
            set => _accessor.Integer = value;
        }

        [WovenProperty]
        public IList<int> IntegerList => _accessor.IntegerList;
    }

    [Woven(typeof(ManuallyGeneratedClassObjectHelper))]
    public partial class ManualllyGeneratedClass : IRealmObject, INotifyPropertyChanged
    {
        //TODO Need to fill out
        public static ObjectSchema RealmSchema; 

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

            // CopyToRealm
            Integer = unmanagedAccessor.Integer;
            if (!skipDefaults)
            {
                IntegerList.Clear();
            }

            foreach (var val in unmanagedAccessor.IntegerList)
            {
                IntegerList.Add(val);
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        protected internal virtual void OnManaged()
        {
        }

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

namespace Realm.Generated
{
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
            value = null;
            return false;
        }
    }

    internal interface IManuallyGeneratedClassAccessor : IRealmAccessor
    {
        int Integer { get; set; }

        IList<int> IntegerList { get; }
    }

    internal class ManuallyGeneratedClassManagedAccessor : ManagedAccessor, IManuallyGeneratedClassAccessor
    {
        public int Integer
        {
            get => (int)base.GetValue(nameof(Integer));
            set => base.SetValue(nameof(Integer), value);
        }

        public IList<int> IntegerList
        {
            get => base.GetListValue<int>(nameof(IntegerList));
        }
    }

    internal class ManuallyGeneratedClassUnmanagedAccessor : UnmanagedAccessor, IManuallyGeneratedClassAccessor
    {
        public int Integer { get; set; }

        public IList<int> IntegerList { get; } = new List<int>();

        public ManuallyGeneratedClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    }
}
