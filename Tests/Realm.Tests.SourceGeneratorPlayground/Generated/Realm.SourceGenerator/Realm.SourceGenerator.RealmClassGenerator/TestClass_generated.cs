
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
using Realms;
using Realms.Weaving;

namespace SourceGeneratorPlayground
{
    public partial class TestClass : IRealmObject, INotifyPropertyChanged
    {


    }
}

namespace Realm.Generated
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
        public string IntProp
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
"                IntProp" => _intProp,
"                _stringProp" => _stringProp,

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
