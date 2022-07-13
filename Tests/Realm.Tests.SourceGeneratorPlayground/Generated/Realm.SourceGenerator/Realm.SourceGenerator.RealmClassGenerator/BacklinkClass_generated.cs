
using System.ComponentModel;
using Realms;
using Realms.Weaving;

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
            set => SetValue("InverseLink", value)
        }

    }

    internal class BacklinkClassUnmanagedAccessor
        : UnmanagedAccessor, IBacklinkClassAccessor
    {
        private UnsupportedBacklink _inverseLink

        public string InverseLink
        {get => _inverseLink;
            set
            {
                stringValue = value;
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
                => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
            }
        
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
            return false
        }
    }
