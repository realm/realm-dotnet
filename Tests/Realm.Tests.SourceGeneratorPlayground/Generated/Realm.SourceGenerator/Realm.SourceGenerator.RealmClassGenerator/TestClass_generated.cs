
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ITestClassAccessor : IRealmAccessor
    {
        string StringPropNullable { get; set; }


    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TestClassManagedAccessor : ManagedAccessor, ITestClassAccessor
    {
        
        public string StringPropNullable
        {
            get => (string)GetValue("StringPropNullable");
            set => SetValue("StringPropNullable", value)
        }

    }

    internal class TestClassUnmanagedAccessor
        : UnmanagedAccessor, ITestClassAccessor
    {
        private string _stringPropNullable

        public string StringPropNullable
        {get => _stringPropNullable;
            set
            {
                stringValue = value;
                RaisePropertyChanged("StringPropNullable");
            }
        }


        public TestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            
            return propertyName switch
            {
                "StringPropNullable" => _stringPropNullable,
                => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
            }
        
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            
            switch (propertyName)
            {
                
                    case "StringPropNullable":
                        StringPropNullable = (string)val;
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
            return false
        }
    }