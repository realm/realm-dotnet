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
   
    [Woven(typeof(TestClassObjectHelper))]
    public partial class TestClass : IRealmObject, INotifyPropertyChanged
    {

        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("TestClass", isEmbedded: false)
        {
            Property.Primitive("RealmValueInt", RealmValueType., isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("RealmValueIntNullable", RealmValueType., isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("IntProp", RealmValueType.Int, isPrimaryKey: false, isIndexed: true, isNullable: false),
            Property.Primitive("GuidPrimaryKey", RealmValueType.Guid, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.PrimitiveList("ListIntProp", RealmValueType.Int, areElementsNullable: false),
            Property.PrimitiveSet("SetIntProp", RealmValueType.Int, areElementsNullable: false),
            Property.PrimitiveDictionary("DictIntProp", RealmValueType.Int, areElementsNullable: false),

        }.Build();

        #region IRealmObject implementation

        private ITestClassAccessor _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public TestClassObjectHelper()
        {
            _accessor = new TestClassUnmanagedAccessor(typeof(TestClassObjectHelper));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (TestClassManagedAccessor)managedAccessor;

            if (helper != null)
            {
                if(!skipDefaults)
                {
                    ListIntProp.Clear();
                    SetIntProp.Clear();
                    DictIntProp.Clear();

                }

                RealmValueInt = unmanagedAccessor.RealmValueInt;
                RealmValueIntNullable = unmanagedAccessor.RealmValueIntNullable;
                IntProp = unmanagedAccessor.IntProp;
                GuidPrimaryKey = unmanagedAccessor.GuidPrimaryKey;
                foreach(var val in unmanagedAccessor.ListIntProp)
                {
                    ListIntProp.Add(val);
                }
                foreach(var val in unmanagedAccessor.SetIntProp)
                {
                    SetIntProp.Add(val);
                }
                foreach(var val in unmanagedAccessor.DictIntProp)
                {
                    DictIntProp.Add(val);
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
            value = ((ITestClassAccessor)instance.Accessor).GuidPrimaryKey;
            return true;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ITestClassAccessor : IRealmAccessor
    {
        RealmInteger<int> RealmValueInt { get; set; }

        RealmInteger<int>? RealmValueIntNullable { get; set; }

        int IntProp { get; set; }

        Guid GuidPrimaryKey { get; set; }

        IList<int> ListIntProp { get; }

        ISet<int> SetIntProp { get; }

        IDictionary<string, int> DictIntProp { get; }
    }

    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TestClassManagedAccessor : ManagedAccessor, ITestClassAccessor
    {
        public RealmInteger<int> RealmValueInt
        {
            get => (RealmInteger<int>)GetValue("RealmValueInt");
            set => SetValue("RealmValueInt", value);
        }

        public RealmInteger<int>? RealmValueIntNullable
        {
            get => (RealmInteger<int>?)GetValue("RealmValueIntNullable");
            set => SetValue("RealmValueIntNullable", value);
        }

        public int IntProp
        {
            get => (int)GetValue("IntProp");
            set => SetValue("IntProp", value);
        }

        public Guid GuidPrimaryKey
        {
            get => (Guid)GetValue("GuidPrimaryKey");
            set => SetValueUnique("GuidPrimaryKey", value);
        }

        private IList<int> _listIntProp;
        public IList<int> ListIntProp
        {
            get
            {
                if(_listIntProp == null)
                {
                    _listIntProp = GetListValue<int>("ListIntProp");
                }

                return _listIntProp;
            }
        }


        private ISet<int> _setIntProp;
        public ISet<int> SetIntProp
        {
            get
            {
                if(_setIntProp == null)
                {
                    _setIntProp = GetSetValue<int>("SetIntProp");
                }

                return _setIntProp;
            }
        }


        private IDictionary<string, int> _dictIntProp;
        public IDictionary<string, int> DictIntProp
        {
            get
            {
                if(_dictIntProp == null)
                {
                    _dictIntProp = GetDictionaryValue<int>("DictIntProp");
                }

                return _dictIntProp;
            }
        }



    }

    
    internal class TestClassUnmanagedAccessor : UnmanagedAccessor, ITestClassAccessor
    {
        private RealmInteger<int> _realmValueInt;
        public RealmInteger<int> RealmValueInt
        {
            get => _realmValueInt;
            set
            {
                _realmValueInt = value;
                RaisePropertyChanged("RealmValueInt");
            }
        }

        private RealmInteger<int>? _realmValueIntNullable;
        public RealmInteger<int>? RealmValueIntNullable
        {
            get => _realmValueIntNullable;
            set
            {
                _realmValueIntNullable = value;
                RaisePropertyChanged("RealmValueIntNullable");
            }
        }

        private int _intProp;
        public int IntProp
        {
            get => _intProp;
            set
            {
                _intProp = value;
                RaisePropertyChanged("IntProp");
            }
        }

        private Guid _guidPrimaryKey;
        public Guid GuidPrimaryKey
        {
            get => _guidPrimaryKey;
            set
            {
                _guidPrimaryKey = value;
                RaisePropertyChanged("GuidPrimaryKey");
            }
        }

        public IList<int> ListIntProp { get; } = new List<int>();


        public ISet<int> SetIntProp { get; } = new HashSet<int>(RealmSet<int>.Comparer);


        public IDictionary<string, int> DictIntProp { get; } = new Dictionary<string, int>();




        public TestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "RealmValueInt" => _realmValueInt,
                "RealmValueIntNullable" => _realmValueIntNullable,
                "IntProp" => _intProp,
                "GuidPrimaryKey" => _guidPrimaryKey,

                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "RealmValueInt":
                    RealmValueInt = (RealmInteger<int>)val;
                    return;
                case "RealmValueIntNullable":
                    RealmValueIntNullable = (RealmInteger<int>?)val;
                    return;
                case "IntProp":
                    IntProp = (int)val;
                    return;
                case "GuidPrimaryKey":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");

                default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "GuidPrimaryKey")
            {
                throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
            }

            GuidPrimaryKey = (Guid)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "ListIntProp" => (IList<T>)ListIntProp,

                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}");
            }
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "SetIntProp" => (ISet<T>)SetIntProp,

                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
            }
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "DictIntProp" => (IDictionary<string, TValue>)DictIntProp,

                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}");
            }
        }

        public IQueryable<T> GetBacklinks<T>(string propertyName) where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed(persisted) objects.");

    }
}
