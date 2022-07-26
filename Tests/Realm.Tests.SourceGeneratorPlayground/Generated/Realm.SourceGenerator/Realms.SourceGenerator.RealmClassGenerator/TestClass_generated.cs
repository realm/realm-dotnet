
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
            Property.Primitive("IntProp", RealmValueType.Int),
            Property.Primitive("_stringProp", RealmValueType.String),
            Property.PrimitiveList("ListIntProp", RealmValueType.Int),

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

                }

                IntProp = unmanagedAccessor.IntProp;
                StringProp = unmanagedAccessor.StringProp;
                foreach(var val in unmanagedAccessor.ListIntProp)
                {
                    ListIntProp.Add(val);
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
            value = null;
            return false;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ITestClassAccessor : IRealmAccessor
    {
        int IntProp { get; set; }

        string StringProp { get; set; }

        IList<int> ListIntProp { get; }
    }

    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TestClassManagedAccessor : ManagedAccessor, ITestClassAccessor
    {
        public int IntProp
        {
            get => (int)GetValue("IntProp");
            set => SetValue("IntProp", value);
        }

        public string StringProp
        {
            get => (string)GetValue("_stringProp");
            set => SetValue("_stringProp", value);
        }


    }

    
    internal class TestClassUnmanagedAccessor : UnmanagedAccessor, ITestClassAccessor
    {
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

        private string _stringProp;
        public string StringProp
        {
            get => _stringProp;
            set
            {
                _stringProp = value;
                RaisePropertyChanged("_stringProp");
            }
        }

        public TestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "IntProp" => _intProp,
                "_stringProp" => _stringProp,

                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "IntProp":
                    IntProp = (int)val;
                    return;
                case "_stringProp":
                    StringProp = (string)val;
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
            throw new MissingMemberException($"The object does not have a Realm list property with name { propertyName}");
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm set property with name { propertyName}");
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm dictionary property with name { propertyName}");
        }
    }
}
