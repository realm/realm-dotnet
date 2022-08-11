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
using System.Data.SqlTypes;
using System.Text;

namespace Realms.SourceGenerator
{
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
using MongoDB.Bson;
using {_classInfo.Namespace};";

            var partialClassString = GeneratePartialClass();
            var interfaceString = GenerateInterface();
            var managedAccessorString = GenerateManagedAccessor();
            var unmanagedAccessorString = GeneratedUnmanagedAccessor();
            var objectHelperString = GenerateClassObjectHelper();

            return $@"// ////////////////////////////////////////////////////////////////////////////
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

            foreach (var property in _classInfo.Properties)
            {
                var type = property.TypeInfo.CompleteTypeString;
                var name = property.Name;
                var hasSetter = !property.TypeInfo.IsCollection && !property.TypeInfo.IsIQueryable;
                var setterString = hasSetter ? " set; " : " ";

                var propertyString = @$"        {type} {name} {{ get;{setterString}}}";
                propertiesBuilder.Append(propertyString);
                propertiesBuilder.AppendLine().AppendLine();
            }
            
            return $@"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface {_accessorInterfaceName} : IRealmAccessor
    {{
{propertiesBuilder}
    }}";
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

                    if (internalTypeIsObject)
                    {
                        var builderMethodName = property.TypeInfo.CollectionType switch
                        {
                            CollectionTypeEnum.List => "ObjectList",
                            CollectionTypeEnum.Set => "ObjectSet",
                            CollectionTypeEnum.Dictionary => "ObjectDictionary",
                            _ => throw new NotImplementedException(),
                        };

                        var internalTypeString = internalType.CompleteTypeString;

                        schemaProperties.AppendLine(@$"            Property.{builderMethodName}(""{property.MapTo ?? property.Name}"", ""{internalTypeString}""),");
                    }
                    else
                    {
                        var builderMethodName = property.TypeInfo.CollectionType switch
                        {
                            CollectionTypeEnum.List => "PrimitiveList",
                            CollectionTypeEnum.Set => "PrimitiveSet",
                            CollectionTypeEnum.Dictionary => "PrimitiveDictionary",
                            _ => throw new NotImplementedException(),
                        };

                        var internalTypeString = GetRealmValueType(internalType);

                        var internalTypeNullable = internalType.IsNullable.ToCodeString();

                        schemaProperties.AppendLine(@$"            Property.{builderMethodName}(""{property.MapTo ?? property.Name}"", {internalTypeString}, areElementsNullable: {internalTypeNullable}),");
                    }

                    skipDefaultsContent.AppendLine($"                    {property.Name}.Clear();");
                    copyToRealm.AppendLine($@"                foreach(var val in unmanagedAccessor.{property.Name})
                {{
                    {property.Name}.Add(val);
                }}");

                }
                else if (property.TypeInfo.IsIQueryable)
                {
                    var backlinkProperty = property.Backlink;
                    var backlinkType = property.TypeInfo.InternalType.CompleteTypeString;

                    schemaProperties.AppendLine(@$"            Property.Backlinks(""{property.MapTo ?? property.Name}"", ""{backlinkType}"", ""{backlinkProperty}""),");

                    // Nothing to do for the copy to realm part
                }
                else if (property.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    var objectName = property.TypeInfo.CompleteTypeString;
                    schemaProperties.AppendLine(@$"            Property.Object(""{property.MapTo ?? property.Name}"", ""{objectName}""),");

                    copyToRealm.AppendLine(@$"                {property.Name} = unmanagedAccessor.{property.Name};");
                }
                else if (property.TypeInfo.SimpleType == SimpleTypeEnum.RealmValue)
                {
                    schemaProperties.AppendLine(@$"            Property.RealmValue(""{property.MapTo ?? property.Name}""),");

                    copyToRealm.AppendLine(@$"                {property.Name} = unmanagedAccessor.{property.Name};");
                }
                else
                {
                    var realmValueType = GetRealmValueType(property.TypeInfo);
                    var isPrimaryKey = property.IsPrimaryKey.ToCodeString();
                    var isIndexed = property.IsIndexed.ToCodeString();
                    var isNullable = property.TypeInfo.IsNullable.ToCodeString();
                    schemaProperties.AppendLine(@$"            Property.Primitive(""{property.MapTo ?? property.Name}"", {realmValueType}, isPrimaryKey: {isPrimaryKey}, isIndexed: {isIndexed}, isNullable: {isNullable}),");

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
    [Generated]
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

        public {_classInfo.Name}()
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

        public static explicit operator {_classInfo.Name}(RealmValue val) => val.AsRealmObject<{_classInfo.Name}>();

        public static implicit operator RealmValue({_classInfo.Name} val) => RealmValue.Object(val);
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

            foreach (var property in _classInfo.Properties)
            {
                var name = property.Name;
                var backingFieldName = GetBackingFieldName(name);
                var type = property.TypeInfo.CompleteTypeString;
                var stringName = property.MapTo ?? name;

                if (property.TypeInfo.IsCollection)
                {
                    var propertyMapToName = property.MapTo ?? property.Name;
                    var parameterString = property.TypeInfo.InternalType.CompleteTypeString;

                    string constructorString;

                    switch (property.TypeInfo.CollectionType)
                    {
                        case CollectionTypeEnum.List:
                            constructorString = $"new List<{parameterString}>()";
                            getListValueLines.AppendLine($@"                ""{propertyMapToName}"" => (IList<T>){property.Name},");
                            break;
                        case CollectionTypeEnum.Set:
                            constructorString = $"new HashSet<{parameterString}>(RealmSet<{parameterString}>.Comparer)";
                            getSetValueLines.AppendLine($@"                ""{propertyMapToName}"" => (ISet<T>){property.Name},");
                            break;
                        case CollectionTypeEnum.Dictionary:
                            constructorString = $"new Dictionary<string, {parameterString}>()";
                            getDictionaryValueLines.AppendLine($@"                ""{propertyMapToName}"" => (IDictionary<string, TValue>){property.Name},");
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    var propertyString = $@"        public {property.TypeInfo.CompleteTypeString} {property.Name} {{ get; }} = {constructorString};";

                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();
                }
                else if (property.TypeInfo.IsIQueryable)
                {
                    // Properties

                    var propertyString = @$"        public {type} {name} => throw new NotSupportedException(""Using backlinks is only possible for managed(persisted) objects."");";

                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();

                    //GetValue
                    getValueLines.AppendLine(@$"                ""{stringName}"" =>  throw new NotSupportedException(""Using backlinks is only possible for managed(persisted) objects.""),");
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

                    propertiesString.AppendLine(backingFieldString);
                    propertiesString.Append(propertyString);
                    propertiesString.AppendLine();

                    //GetValue
                    getValueLines.AppendLine(@$"                ""{stringName}"" => {backingFieldName},");

                    //SetValue/SetValueUnique
                    setValueLines.AppendLine($@"                case ""{stringName}"":");

                    if (property.IsPrimaryKey)
                    {
                        setValueLines.AppendLine($@"                    throw new InvalidOperationException(""Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique"");");

                setValueUniqueLines.Append($@"if (propertyName != ""{stringName}"")
            {{
                throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");
            }}

            {name} = ({type})val;");
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

            string getValueBody;

            if (getValueLines.Length == 0)
            {
                getValueBody = $@"throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}"");";
            }
            else
            {
                getValueBody = $@"return propertyName switch
            {{
{getValueLines}
                _ => throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}""),
            }};";
            }

            //SetValue

            string setValueBody;

            if (setValueLines.Length == 0)
            {
                setValueBody = $@"throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");";
            }
            else
            {
                setValueBody = $@"switch (propertyName)
            {{
{setValueLines}
                default:
                        throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");
            }}";
            }

            //SetValueUnique

            if (setValueUniqueLines.Length == 0)
            {
                setValueUniqueLines.Append(@"throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");");
            }

            var setValueUniqueBody = setValueUniqueLines.ToString();

            //GetListValue

            string getListValueBody;

            if (getListValueLines.Length == 0)
            {
                getListValueBody = $@"throw new MissingMemberException($""The object does not have a Realm list property with name {{propertyName}}"");";
            }
            else
            {
                getListValueBody = $@"return propertyName switch
            {{
{getListValueLines}
                _ => throw new MissingMemberException($""The object does not have a Realm list property with name {{propertyName}}""),
            }};";
            }

            //GetSetValue

            string getSetValueBody;

            if (getSetValueLines.Length == 0)
            {
                getSetValueBody = $@"throw new MissingMemberException($""The object does not have a Realm set property with name {{propertyName}}"");";
            }
            else
            {
                getSetValueBody = $@"return propertyName switch
            {{
{getSetValueLines}
                _ => throw new MissingMemberException($""The object does not have a Realm set property with name {{propertyName}}""),
            }};";
            }

            //GetDictionaryValue

            string getDictionaryValueBody;

            if (getDictionaryValueLines.Length == 0)
            {
                getDictionaryValueBody = $@"throw new MissingMemberException($""The object does not have a Realm dictionary property with name {{propertyName}}"");";
            }
            else
            {
                getDictionaryValueBody = $@"return propertyName switch
            {{
{getDictionaryValueLines}
                _ => throw new MissingMemberException($""The object does not have a Realm dictionary property with name {{propertyName}}""),
            }};";
            }

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
            {getDictionaryValueBody}
        }}

        public IQueryable<T> GetBacklinks<T>(string propertyName) where T : IRealmObjectBase
            => throw new NotSupportedException(""Using the GetBacklinks is only possible for managed(persisted) objects."");

    }}";
        }

        private string GenerateManagedAccessor()
        {
            var propertiesBuilder = new StringBuilder();

            for (var i = 0; i < _classInfo.Properties.Count; i++)
            {
                var property = _classInfo.Properties[i];
                var type = property.TypeInfo.CompleteTypeString;
                var name = property.Name;
                var stringName = property.MapTo ?? name;

                if (property.TypeInfo.IsCollection || property.TypeInfo.IsIQueryable)
                {
                    var backingFieldName = GetBackingFieldName(property.Name);
                    var backingFieldString = $@"private {type} {backingFieldName};";
                    var internalTypeString = property.TypeInfo.InternalType.CompleteTypeString;

                    string getFieldString;

                    if (property.TypeInfo.IsCollection)
                    {
                        getFieldString = property.TypeInfo.CollectionType switch
                        {
                            CollectionTypeEnum.List => "GetListValue",
                            CollectionTypeEnum.Set => "GetSetValue",
                            CollectionTypeEnum.Dictionary => "GetDictionaryValue",
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        getFieldString = "GetBacklinks";
                    }

                    var propertyString = @$"        {backingFieldString}
        public {type} {name}
        {{
            get
            {{
                if({backingFieldName} == null)
                {{
                    {backingFieldName} = {getFieldString}<{internalTypeString}>(""{property.MapTo ?? property.Name}"");
                }}

                return {backingFieldName};
            }}
        }}";

                    propertiesBuilder.AppendLine(propertyString);

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
                }

                propertiesBuilder.AppendLine();
            }

            return $@"    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class {_managedAccessorClassName} : ManagedAccessor, {_accessorInterfaceName}
    {{
{propertiesBuilder}
    }}";
        }

        private string GetBackingFieldName(string propertyName)
        {
            return "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        }

        private string GetRealmValueType(PropertyTypeInfo propertyTypeInfo)
        {
            var simpleType = propertyTypeInfo.IsRealmInteger ? propertyTypeInfo.InternalType.SimpleType : propertyTypeInfo.SimpleType;

            var endString = simpleType switch
            {
                SimpleTypeEnum.Int => "Int",
                SimpleTypeEnum.Bool => "Bool",
                SimpleTypeEnum.String => "String",
                SimpleTypeEnum.Data => "Data",
                SimpleTypeEnum.Date => "Date",
                SimpleTypeEnum.Float => "Float",
                SimpleTypeEnum.Double => "Double",
                SimpleTypeEnum.Object => "Object",
                SimpleTypeEnum.RealmValue => "RealmValue",
                SimpleTypeEnum.ObjectId => "ObjectId",
                SimpleTypeEnum.Decimal => "Decimal128",
                SimpleTypeEnum.Guid => "Guid",
                _ => throw new NotImplementedException(),
            };

            return "RealmValueType." + endString;
        }
    }
}
