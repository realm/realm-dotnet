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
   
    [Woven(typeof(BacklinkClassObjectHelper))]
    public partial class BacklinkClass : IRealmObject, INotifyPropertyChanged
    {

        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("BacklinkClass", isEmbedded: false)
        {
            Property.Object("InverseLink", "UnsupportedBacklink"),

        }.Build();

        #region IRealmObject implementation

        private IBacklinkClassAccessor _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public BacklinkClass()
        {
            _accessor = new BacklinkClassUnmanagedAccessor(typeof(BacklinkClassObjectHelper));
        }

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var unmanagedAccessor = _accessor;
            _accessor = (BacklinkClassManagedAccessor)managedAccessor;

            if (helper != null)
            {


                InverseLink = unmanagedAccessor.InverseLink;

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
    internal class BacklinkClassObjectHelper : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public ManagedAccessor CreateAccessor() => new BacklinkClassManagedAccessor();

        public IRealmObjectBase CreateInstance()
        {
            return new BacklinkClass();
        }

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            value = null;
            return false;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IBacklinkClassAccessor : IRealmAccessor
    {
        UnsupportedBacklink InverseLink { get; set; }
    }

    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class BacklinkClassManagedAccessor : ManagedAccessor, IBacklinkClassAccessor
    {
        public UnsupportedBacklink InverseLink
        {
            get => (UnsupportedBacklink)GetValue("InverseLink");
            set => SetValue("InverseLink", value);
        }


    }

    
    internal class BacklinkClassUnmanagedAccessor : UnmanagedAccessor, IBacklinkClassAccessor
    {
        private UnsupportedBacklink _inverseLink;
        public UnsupportedBacklink InverseLink
        {
            get => _inverseLink;
            set
            {
                _inverseLink = value;
                RaisePropertyChanged("InverseLink");
            }
        }



        public BacklinkClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "InverseLink" => _inverseLink,

                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "InverseLink":
                    InverseLink = (UnsupportedBacklink)val;
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

        public IQueryable<T> GetBacklinks<T>(string propertyName) where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed(persisted) objects.");

    }
}
