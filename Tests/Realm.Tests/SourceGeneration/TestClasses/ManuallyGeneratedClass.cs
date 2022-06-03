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
using Realms.Schema;
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
            get
            {
                return (int)_accessor.GetValue("Integer");
            }

            set
            {
                _accessor.SetValue("Integer", value);
            }
        }

        [WovenProperty]
        public IList<int> IntegerList => _accessor.GetListValue<int>("IntegerList");
    }

    [Woven(typeof(ManuallyGeneratedClassObjectHelper))]
    public partial class ManualllyGeneratedClass : IRealmObject, INotifyPropertyChanged
    {
        //TODO Need to fill out
        public static ObjectSchema RealmSchema; 

        #region IRealmObject implementation
        private IRealmAccessor _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public ManualllyGeneratedClass()
        {
            _accessor = new ManuallyGeneratedClassAccessor(GetType());
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = managedAccessor;

            // We don't call helper.CopyToRealm for simplicity, so we don't need to pass the unmanaged accessor
            if (helper != null)
            {
                //List property

                var unmnagedList = unmanagedAccessor.GetListValue<int>("IntegerList");
                var list = managedAccessor.GetListValue<int>("IntegerList");

                if (!skipDefaults)
                {
                    list.Clear();
                }

                if (true)
                {
                    foreach (var val in unmnagedList)
                    {
                        list.Add(val);
                    }
                }

                //Scalar property

                var unmanagedVal = unmanagedAccessor.GetValue("Integer");
                managedAccessor.SetValue("Integer", unmanagedVal);
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

        public interface IManuallyGeneratedClass : IRealmAccessor
        {
            public int Integer { get; set; }

            public IList<int> IntegerList { get; }

        }

        internal class ManuallyGeneratedClassObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

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

        internal class ManuallyGeneratedClassAccessor : IRealmAccessor
        {
            //Backing fields
            private int _integer;

            private List<int> _integerList = new List<int>();
            //End backing fields

            private Type _objectType;

            public ManuallyGeneratedClassAccessor(Type objectType)
            {
                _objectType = objectType;
            }

            public bool IsManaged => false;

            public bool IsValid => true;

            public bool IsFrozen => false;

            public Realm Realm => null;

            public ObjectSchema ObjectSchema => null;

            public int BacklinksCount => 0;

            public RealmObjectBase.Dynamic DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

            public IQueryable<T> GetBacklinks<T>(string propertyName)
                where T : IRealmObjectBase
            {
                throw new InvalidOperationException("Object is not managed, but managed access was attempted");
            }

            public static ThreadSafeReference GetSafeReference()
            {
                throw new InvalidOperationException("Object is not managed, but managed access was attempted");
            }

            public IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                throw new Exception($"There is no set dictionary with name `{propertyName}`");
            }

            public IList<T> GetListValue<T>(string propertyName)
            {
                switch (propertyName)
                {
                    case "IntegerList":
                        return (IList<T>)_integerList;
                    default:
                        throw new Exception($"There is no list property with name `{propertyName}`");
                }
            }

            public ISet<T> GetSetValue<T>(string propertyName)
            {
                throw new Exception($"There is no set property with name `{propertyName}`");
            }

            public RealmValue GetValue(string propertyName)
            {
                switch (propertyName)
                {
                    case "Integer":
                        return _integer;
                    default:
                        throw new Exception($"There is no property with name `{propertyName}`");
                }
            }

            public void SetValue(string propertyName, RealmValue val)
            {
                switch (propertyName)
                {
                    case "Integer":
                        _integer = (int)val;
                        break;
                    default:
                        throw new Exception($"There is no property with name `{propertyName}`");
                }
            }

            public void SetValueUnique(string propertyName, RealmValue val)
            {
                SetValue(propertyName, val);
            }

            public void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
            {
            }

            public void UnsubscribeFromNotifications()
            {
            }

            public override string ToString()
            {
                return $"{_objectType.Name} (unmanaged)";
            }

            public override bool Equals(object obj)
            {
                return false;
            }
        }
    }
}
