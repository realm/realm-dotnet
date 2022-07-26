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
using System.Text;

namespace Realms.SourceGenerator
{
    /* What do do:
     * - Connect weaver
     * - Add weaver-tests
     * - Add IQueryable parts
     * - Add collections parts
     * - Add backlinks parts
     * - Check nullability (later)
     * - Fix schema creation
     */
    internal class Generator
    {
        private ClassInfo _classInfo;

        private string _helperClassName;
        private string _accessorInterfaceName;
        private string _managedAccessorClassName;
        private string _unmanagedAccessorClassName;

        public Generator(ClassInfo classInfo)
        {
            _classInfo = classInfo;

            _helperClassName = $"{_classInfo.Name}ObjectHelper";
            _accessorInterfaceName = $"I{_classInfo.Name}Accessor";
            _managedAccessorClassName = $"{_classInfo.Name}ManagedAccessor";
            _unmanagedAccessorClassName = $"{_classInfo.Name}UnmanagedAccessor";
        }

        public string GenerateSource()
        {
            var usings = @$"using System.ComponentModel;
using System.Runtime.CompilerServices;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using {_classInfo.Namespace};";

            var partialClassString = GeneratePartialClass();
            var interfaceString = GenerateInterface();
            var managedAccessorString = GenerateManagedAccessor();
            var unmanagedAccessorString = GeneratedUnmanagedAccessor();
            var objectHelperString = GenerateClassObjectHelper();

            return $@"
// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright {DateTime.Now.Year} Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the ""License"")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an ""AS IS"" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

{usings}

namespace {_classInfo.Namespace}
{{
{partialClassString}
}}

namespace Realms.Generated
{{
{objectHelperString}

{interfaceString}

{managedAccessorString}

{unmanagedAccessorString}
}}
";
        }

        private string GenerateInterface()
        {
            var propertiesBuilder = new StringBuilder();

            for (var i = 0; i < _classInfo.Properties.Count; i++)
            {
                var property = _classInfo.Properties[i];
                var type = property.TypeInfo.TypeString;
                var name = property.Name;
                var hasSetter = !property.TypeInfo.IsCollection && !property.TypeInfo.IsIQueryable;
                var setterString = hasSetter ? " set; " : " ";

                var propertyString = @$"        {type} {name} {{ get;{setterString}}}";
                propertiesBuilder.Append(propertyString);
                if (i != _classInfo.Properties.Count - 1)
                {
                    propertiesBuilder.Append("\n\n");
                }
            }
            
            return $@"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface {_accessorInterfaceName} : IRealmAccessor
    {{
{propertiesBuilder}
    }}";
        }

        private string GetRealmValueType(PropertyTypeInfo propertyTypeInfo)
        {
            return "RealmValueType." + propertyTypeInfo.SimpleType.ToString(); //TODO Need to be more complex than this
        }

        private string GeneratePartialClass()
        {
            var schemaProperties = new StringBuilder();
            var copyToRealm = new StringBuilder();
            var skipDefaultsContent = new StringBuilder();

            foreach (var property in _classInfo.Properties)
            {
                if (property.TypeInfo.IsCollection)
                {
                    var internalType = property.TypeInfo.InternalType;

                    var internalTypeIsObject = internalType.SimpleType == SimpleTypeEnum.Object;

                    //TODO Check if I should move it out
                    var collectionString = property.TypeInfo.CollectionType switch
                    {
                        CollectionTypeEnum.List => "List",
                        CollectionTypeEnum.Set => "Set",
                        CollectionTypeEnum.Dictionary => "Dictionary",
                        _ => throw new NotImplementedException(),
                    };

                    var prefix = internalTypeIsObject ? "Object" : "Primitive";
                    var builderMethodName = $"{prefix}{collectionString}";

                    var internalTypeString = internalTypeIsObject ? internalType.TypeString : GetRealmValueType(internalType);

                    schemaProperties.AppendLine(@$"            Property.{builderMethodName}(""{property.MapTo ?? property.Name}"", {internalTypeString}),");


                    skipDefaultsContent.AppendLine($"                    {property.Name}.Clear();");
                    copyToRealm.AppendLine($@"                foreach(var val in unmanagedAccessor.{property.Name})
                {{
                    {property.Name}.Add(val);
                }}");

                }
                else if (property.TypeInfo.IsIQueryable)
                {
                    // Nothing to do for the copy to realm part
                }
                else if (property.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    var objectName = property.TypeInfo.TypeString;
                    schemaProperties.AppendLine(@$"            Property.Object(""{property.MapTo ?? property.Name}"", {objectName}),");
                }
                else
                {
                    var realmValueType = GetRealmValueType(property.TypeInfo); 
                    var primaryKeyString = property.IsPrimaryKey ? ", isPrimaryKey: true" : string.Empty;
                    schemaProperties.AppendLine(@$"            Property.Primitive(""{property.MapTo ?? property.Name}"", {realmValueType}{primaryKeyString}),");

                    copyToRealm.AppendLine(@$"                {property.Name} = unmanagedAccessor.{property.Name};");
                }
            }

            var skipDefaults = string.Empty;

            if (skipDefaultsContent.Length != 0)
            {
                skipDefaults = $@"                if(!skipDefaults)
                {{
{skipDefaultsContent}
                }}";

            }

            var isEmbedded = _classInfo.IsEmbedded ? "true" : "false";
            var schema = @$"        public static ObjectSchema RealmSchema = new ObjectSchema.Builder(""{_classInfo.Name}"", isEmbedded: {isEmbedded})
        {{
{schemaProperties}
        }}.Build();";

            return $@"   
    [Woven(typeof({_helperClassName}))]
    public partial class {_classInfo.Name} : IRealmObject, INotifyPropertyChanged
    {{

{schema}

        #region IRealmObject implementation

        private {_accessorInterfaceName} _accessor;

        public IRealmAccessor Accessor => _accessor;

        public bool IsManaged => _accessor.IsManaged;

        public bool IsValid => _accessor.IsValid;

        public bool IsFrozen => _accessor.IsFrozen;

        public Realm Realm => _accessor.Realm;

        public ObjectSchema ObjectSchema => _accessor.ObjectSchema;

        public {_helperClassName}()
        {{
            _accessor = new {_unmanagedAccessorClassName}(typeof({_helperClassName}));
        }}

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {{
            var unmanagedAccessor = _accessor;
            _accessor = ({_managedAccessorClassName})managedAccessor;

            if (helper != null)
            {{
{skipDefaults}

{copyToRealm}
            }}

            if (_propertyChanged != null)
            {{
                SubscribeForNotifications();
            }}

            OnManaged();
        }}

        #endregion

        private event PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {{
            add
            {{
                if (_propertyChanged == null)
                {{
                    SubscribeForNotifications();
                }}

                _propertyChanged += value;
            }}

            remove
            {{
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {{
                    UnsubscribeFromNotifications();
                }}
            }}
        }}

        partial void OnPropertyChanged(string propertyName);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {{
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }}

        partial void OnManaged();

        private void SubscribeForNotifications()
        {{
            _accessor.SubscribeForNotifications(RaisePropertyChanged);
        }}

        private void UnsubscribeFromNotifications()
        {{
            _accessor.UnsubscribeFromNotifications();
        }}
    }}";
        }

        private string GenerateClassObjectHelper()
        {
            var primaryKeyProperty = _classInfo.PrimaryKey;
            string tryGetPrimaryKeyBody;

            if (primaryKeyProperty != null)
            {
                var valueString = $"value = (({_accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name};";
                tryGetPrimaryKeyBody = $@"{valueString}
            return true;";
            }
            else
            {
                tryGetPrimaryKeyBody = @"value = null;
            return false;";
            }

            return $@"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class {_helperClassName} : IRealmObjectHelper
    {{
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {{
            throw new InvalidOperationException(""This method should not be called for source generated classes."");
        }}

        public ManagedAccessor CreateAccessor() => new {_managedAccessorClassName}();

        public IRealmObjectBase CreateInstance()
        {{
            return new {_classInfo.Name}();
        }}

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {{
            {tryGetPrimaryKeyBody}
        }}
    }}";
        }

        private string GeneratedUnmanagedAccessor()
        {
            var propertiesString = new StringBuilder();
            var getValueLines = new StringBuilder();
            var setValueLines = new StringBuilder();
            var setValueUniqueLines = new StringBuilder();
            var getListValueLines = new StringBuilder();
            var getSetValueLines = new StringBuilder();
            var getDictionaryValueLines = new StringBuilder();

            // Probably we can remove some complexity if we don't care about some extra spaces/lines here and there
            bool isFirstNonCollection = true;

            foreach (var property in _classInfo.Properties)
            {
                var name = property.Name;
                var backingFieldName = "_" + char.ToLowerInvariant(name[0]) + name.Substring(1);
                var type = property.TypeInfo.TypeString;
                var stringName = property.MapTo ?? name;

                if (property.TypeInfo.IsCollection)
                {
                    //GetListValue

                    //GetSetValue

                    //GetDictionaryValue
                }
                else
                {
                    // Properties
                    var backingFieldString = $"        private {type} {backingFieldName};";

                    var propertyString = @$"        public {type} {name}
        {{
            get => {backingFieldName};
            set
            {{
                {backingFieldName} = value;
                RaisePropertyChanged(""{stringName}"");
            }}
        }}";
                    if(isFirstNonCollection)
                    {
                        isFirstNonCollection = false;
                    }
                    else
                    {
                        propertiesString.AppendLine();
                        propertiesString.AppendLine();
                    }

                    propertiesString.AppendLine(backingFieldString);
                    propertiesString.Append(propertyString);

                    //GetValue
                    getValueLines.AppendLine(@$"                ""{stringName}"" => {backingFieldName},");

                    //SetValue/SetValueUnique
                    setValueLines.AppendLine($@"                case ""{stringName}"":");

                    if (property.IsPrimaryKey)
                    {
                        setValueLines.AppendLine($@"throw new InvalidOperationException(""Cannot set the value of a primary key property with SetValue.You need to use SetValueUnique"");");

                        setValueUniqueLines.AppendLine($@"if (propertyName != ""{stringName}"")
                        {{
                            throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");
                        }}

                        {name} = ({type})val;
                        ");
                    }
                    else
                    {
                        setValueLines.AppendLine($@"                    {name} = ({type})val;
                    return;");
                    }
                }
            }

            //Properties

            var propertyBody = propertiesString.ToString();

            //GetValue

            var getValueBody = $@"return propertyName switch
            {{
{getValueLines}
                _ => throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}""),
            }};";

            //SetValue

            var setValueBody = $@"switch (propertyName)
            {{
{setValueLines}
                default:
                        throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");
            }}";

            //SetValueUnique

            if (setValueUniqueLines.Length == 0)
            {
                setValueUniqueLines.Append(@"throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");");
            }

            var setValueUniqueBody = setValueUniqueLines.ToString();

            //GetListValue

            if (getListValueLines.Length == 0)
            {
                getListValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm list property with name { propertyName}"");");
            }

            var getListValueBody = getListValueLines.ToString();


            //GetSetValue

            if (getSetValueLines.Length == 0)
            {
                getSetValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm set property with name { propertyName}"");");
            }

            var getSetValueBody = getSetValueLines.ToString();


            //GetDictionaryValue

            if (getDictionaryValueLines.Length == 0)
            {
                getDictionaryValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm dictionary property with name { propertyName}"");");
            }

            var getDictionaryBody = getDictionaryValueLines.ToString();

            return $@"    
    internal class {_unmanagedAccessorClassName} : UnmanagedAccessor, {_accessorInterfaceName}
    {{
{propertyBody}

        public {_unmanagedAccessorClassName}(Type objectType) : base(objectType)
        {{
        }}

        public override RealmValue GetValue(string propertyName)
        {{
            {getValueBody}
        }}

        public override void SetValue(string propertyName, RealmValue val)
        {{
            {setValueBody}
        }}

        public override void SetValueUnique(string propertyName, RealmValue val)
        {{
            {setValueUniqueBody}
        }}

        public override IList<T> GetListValue<T>(string propertyName)
        {{
            {getListValueBody}
        }}

        public override ISet<T> GetSetValue<T>(string propertyName)
        {{
            {getSetValueBody}
        }}

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {{
            {getDictionaryBody}
        }}
    }}";
        }

        private string GenerateManagedAccessor()
        {
            var propertiesBuilder = new StringBuilder();

            for (var i = 0; i < _classInfo.Properties.Count; i++)
            {
                var property = _classInfo.Properties[i];
                var type = property.TypeInfo.TypeString;
                var name = property.Name;
                var stringName = property.MapTo ?? name;

                // TODO Where does IQueryable go?
                if (property.TypeInfo.IsCollection)
                {
                    //TODO
                }
                else
                {
                    var getterString = $@"get => ({type})GetValue(""{stringName}"");";

                    var setterMethod = property.IsPrimaryKey ? "SetValueUnique" : "SetValue";
                    var setterString = $@"set => {setterMethod}(""{stringName}"", value);";

                    var propertyString = @$"        public {type} {name}
        {{
            {getterString}
            {setterString}
        }}";
                    propertiesBuilder.Append(propertyString);

                    if (i != _classInfo.Properties.Count - 1)
                    {
                        propertiesBuilder.AppendLine();
                        propertiesBuilder.AppendLine();
                    }
                }
            }

            return $@"    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class {_managedAccessorClassName} : ManagedAccessor, {_accessorInterfaceName}
    {{
{propertiesBuilder}
    }}";
        }
    }
}
