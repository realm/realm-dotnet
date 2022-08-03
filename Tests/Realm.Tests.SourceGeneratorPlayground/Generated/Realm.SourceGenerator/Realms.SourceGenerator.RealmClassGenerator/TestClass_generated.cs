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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using SourceGeneratorPlayground;

namespace SourceGeneratorPlayground
{
    [Generated]
    [Woven(typeof(TestClassObjectHelper))]
    public partial class TestClass : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("TestClass", isEmbedded: false)
        {Property.ObjectList("ListObjProp", "OtherTestClass"), Property.ObjectSet("SetObjProp", "OtherTestClass"), Property.ObjectDictionary("DictObjProp", "OtherTestClass"), }.Build();
#region IRealmObject implementation
        private ITestClassAccessor _accessor;
        public IRealmAccessor Accessor => _accessor;
        public bool IsManaged => _accessor.IsManaged;
        public bool IsValid => _accessor.IsValid;
        public bool IsFrozen => _accessor.IsFrozen;
        public Realm Realm => _accessor.Realm;
        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;
        public TestClass()
        {
            _accessor = new TestClassUnmanagedAccessor(typeof(TestClassObjectHelper));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (TestClassManagedAccessor)managedAccessor;
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    ListObjProp.Clear();
                    SetObjProp.Clear();
                    DictObjProp.Clear();
                }

                foreach (var val in unmanagedAccessor.ListObjProp)
                {
                    ListObjProp.Add(val);
                }

                foreach (var val in unmanagedAccessor.SetObjProp)
                {
                    SetObjProp.Add(val);
                }

                foreach (var val in unmanagedAccessor.DictObjProp)
                {
                    DictObjProp.Add(val);
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
            _accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            _accessor.UnsubscribeFromNotifications();
        }

        public static explicit operator TestClass(RealmValue val) => val.AsRealmObject<TestClass>();
        public static implicit operator RealmValue(TestClass val) => RealmValue.Object(val);
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TestClassObjectHelper : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public ManagedAccessor CreateAccessor() => new TestClassManagedAccessor();
        public IRealmObjectBase CreateInstance()
        {
            return new TestClass();
        }

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            value = null;
            return false;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ITestClassAccessor : IRealmAccessor
    {
        IList<OtherTestClass> ListObjProp { get; }

        ISet<OtherTestClass> SetObjProp { get; }

        IDictionary<string, OtherTestClass> DictObjProp { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TestClassManagedAccessor : ManagedAccessor, ITestClassAccessor
    {
        private IList<OtherTestClass> _listObjProp;
        public IList<OtherTestClass> ListObjProp
        {
            get
            {
                if (_listObjProp == null)
                {
                    _listObjProp = GetListValue<OtherTestClass>("ListObjProp");
                }

                return _listObjProp;
            }
        }

        private ISet<OtherTestClass> _setObjProp;
        public ISet<OtherTestClass> SetObjProp
        {
            get
            {
                if (_setObjProp == null)
                {
                    _setObjProp = GetSetValue<OtherTestClass>("SetObjProp");
                }

                return _setObjProp;
            }
        }

        private IDictionary<string, OtherTestClass> _dictObjProp;
        public IDictionary<string, OtherTestClass> DictObjProp
        {
            get
            {
                if (_dictObjProp == null)
                {
                    _dictObjProp = GetDictionaryValue<OtherTestClass>("DictObjProp");
                }

                return _dictObjProp;
            }
        }
    }

    internal class TestClassUnmanagedAccessor : UnmanagedAccessor, ITestClassAccessor
    {
        public IList<OtherTestClass> ListObjProp { get; } = new List<OtherTestClass>();
        public ISet<OtherTestClass> SetObjProp { get; } = new HashSet<OtherTestClass>(RealmSet<OtherTestClass>.Comparer);
        public IDictionary<string, OtherTestClass> DictObjProp { get; } = new Dictionary<string, OtherTestClass>();
        public TestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "ListObjProp" => (IList<T>)ListObjProp,
                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
            };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "SetObjProp" => (ISet<T>)SetObjProp,
                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
            };
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "DictObjProp" => (IDictionary<string, TValue>)DictObjProp,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }

        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase => throw new NotSupportedException("Using the GetBacklinks is only possible for managed(persisted) objects.");
    }
}