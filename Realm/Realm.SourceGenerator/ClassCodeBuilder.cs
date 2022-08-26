////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Realms.SourceGenerator.Utils;

namespace Realms.SourceGenerator
{
    internal class ClassCodeBuilder
    {
        private readonly string[] _defaultNamespaces = new string[]
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Runtime.CompilerServices",
            "System.ComponentModel",
            "Realms",
            "Realms.Weaving",
            "Realms.Generated",
            "Realms.Schema",
        };

        private ClassInfo _classInfo;

        private string _helperClassName;
        private string _accessorInterfaceName;
        private string _managedAccessorClassName;
        private string _unmanagedAccessorClassName;

        public ClassCodeBuilder(ClassInfo classInfo)
        {
            _classInfo = classInfo;

            _helperClassName = $"{_classInfo.Name}ObjectHelper";
            _accessorInterfaceName = $"I{_classInfo.Name}Accessor";
            _managedAccessorClassName = $"{_classInfo.Name}ManagedAccessor";
            _unmanagedAccessorClassName = $"{_classInfo.Name}UnmanagedAccessor";
        }

        public string GenerateSource()
        {
            var usings = GetUsings();

            var partialClassString = GeneratePartialClass().Indent();
            var objectHelperString = GenerateClassObjectHelper().Indent();
            var interfaceString = GenerateInterface().Indent();
            var managedAccessorString = GenerateManagedAccessor().Indent();
            var unmanagedAccessorString = GenerateUnmanagedAccessor().Indent();

            return $@"{usings}

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

        private string GetUsings()
        {
            // TODO We're just sorting the usings alphabetically, we can work on this to put the Systems namespaces in front
            var namespaces = new HashSet<string>() { _classInfo.Namespace };
            namespaces.UnionWith(_defaultNamespaces);

            foreach (var property in _classInfo.Properties)
            {
                namespaces.Add(property.TypeInfo.Namespace);
                namespaces.Add(property.TypeInfo.InternalType?.Namespace);
            }

            return string.Join(Environment.NewLine, namespaces.Where(n => !string.IsNullOrWhiteSpace(n)).Select(s => $"using {s};"));
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

                propertiesBuilder.AppendLine($@"{type} {name} {{ get;{setterString}}}");
                propertiesBuilder.AppendLine();
            }

            return $@"[EditorBrowsable(EditorBrowsableState.Never)]
internal interface {_accessorInterfaceName} : IRealmAccessor
{{
{propertiesBuilder.Indent(trimNewLines: true)}
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

                        schemaProperties.AppendLine(@$"Property.{builderMethodName}(""{property.MapTo ?? property.Name}"", ""{internalTypeString}""),");
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

                        schemaProperties.AppendLine(@$"Property.{builderMethodName}(""{property.MapTo ?? property.Name}"", {internalTypeString}, areElementsNullable: {internalTypeNullable}),");
                    }

                    skipDefaultsContent.AppendLine($"newAccessor.{property.Name}.Clear();");
                    copyToRealm.AppendLine($@"foreach(var val in oldAccessor.{property.Name})
{{
    newAccessor.{property.Name}.Add(val);
}}");
                }
                else if (property.TypeInfo.IsIQueryable)
                {
                    var backlinkProperty = property.Backlink;
                    var backlinkType = property.TypeInfo.InternalType.CompleteTypeString;

                    schemaProperties.AppendLine(@$"Property.Backlinks(""{property.MapTo ?? property.Name}"", ""{backlinkType}"", ""{backlinkProperty}""),");

                    // Nothing to do for the copy to realm part
                }
                else if (property.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    var objectName = property.TypeInfo.CompleteTypeString;
                    schemaProperties.AppendLine(@$"Property.Object(""{property.MapTo ?? property.Name}"", ""{objectName}""),");

                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
                else if (property.TypeInfo.SimpleType == SimpleTypeEnum.RealmValue)
                {
                    schemaProperties.AppendLine(@$"Property.RealmValue(""{property.MapTo ?? property.Name}""),");

                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
                else
                {
                    var realmValueType = GetRealmValueType(property.TypeInfo);
                    var isPrimaryKey = property.IsPrimaryKey.ToCodeString();
                    var isIndexed = property.IsIndexed.ToCodeString();
                    var isNullable = property.TypeInfo.IsNullable.ToCodeString();
                    schemaProperties.AppendLine(@$"Property.Primitive(""{property.MapTo ?? property.Name}"", {realmValueType}, isPrimaryKey: {isPrimaryKey}, isIndexed: {isIndexed}, isNullable: {isNullable}),");

                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
            }

            var skipDefaults = string.Empty;

            if (skipDefaultsContent.Length != 0)
            {
                skipDefaults = $@"if (!skipDefaults)
{{
{skipDefaultsContent.Indent(trimNewLines: true)}
}}
";
            }

            var schema = @$"public static ObjectSchema RealmSchema = new ObjectSchema.Builder(""{_classInfo.Name}"", isEmbedded: {BoolToString(_classInfo.IsEmbedded)})
{{
{schemaProperties.Indent(trimNewLines: true)}
}}.Build();";

            var baseInterface = _classInfo.IsEmbedded ? "IEmbeddedObject" : "IRealmObject";

            // TODO: we may need to generate a parameterless ctor. We should check if there are any user-defined ctors and/or if they are parameterless.
            var contents = $@"{schema}

#region {baseInterface} implementation

private {_accessorInterfaceName} _accessor;

public IRealmAccessor Accessor
{{
    get
    {{
        if (_accessor == null)
        {{
            _accessor = new {_unmanagedAccessorClassName}(typeof({_helperClassName}));
        }}

        return _accessor;
    }}
}}

public bool IsManaged => Accessor.IsManaged;

public bool IsValid => Accessor.IsValid;

public bool IsFrozen => Accessor.IsFrozen;

public Realm Realm => Accessor.Realm;

public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
{{
    var newAccessor = ({_accessorInterfaceName})managedAccessor;

    if (helper != null)
    {{
        var oldAccessor = ({_accessorInterfaceName})Accessor;
{skipDefaults.Indent(2)}{copyToRealm.Indent(2, trimNewLines: true)}
    }}

    _accessor = newAccessor;

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
    Accessor.SubscribeForNotifications(RaisePropertyChanged);
}}

private void UnsubscribeFromNotifications()
{{
    Accessor.UnsubscribeFromNotifications();
}}

public static explicit operator {_classInfo.Name}(RealmValue val) => val.AsRealmObject<{_classInfo.Name}>();

public static implicit operator RealmValue({_classInfo.Name} val) => RealmValue.Object(val);";

            return $@"[Generated]
[Woven(typeof({_helperClassName}))]
public partial class {_classInfo.Name} : {baseInterface}, INotifyPropertyChanged
{{
{contents.Indent()}
}}";
        }

        private string GenerateClassObjectHelper()
        {
            var primaryKeyProperty = _classInfo.PrimaryKey;
            var valueAccessor = primaryKeyProperty == null ? "null" : $"(({_accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name}";

            return $@"[EditorBrowsable(EditorBrowsableState.Never)]
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
        value = {valueAccessor};
        return {BoolToString(primaryKeyProperty != null)};
    }}
}}";
        }

        private string GenerateUnmanagedAccessor()
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
                            getListValueLines.AppendLine($@"""{propertyMapToName}"" => (IList<T>){property.Name},");
                            break;
                        case CollectionTypeEnum.Set:
                            constructorString = $"new HashSet<{parameterString}>(RealmSet<{parameterString}>.Comparer)";
                            getSetValueLines.AppendLine($@"""{propertyMapToName}"" => (ISet<T>){property.Name},");
                            break;
                        case CollectionTypeEnum.Dictionary:
                            constructorString = $"new Dictionary<string, {parameterString}>()";
                            getDictionaryValueLines.AppendLine($@"""{propertyMapToName}"" => (IDictionary<string, TValue>){property.Name},");
                            break;
                        default:
                            throw new NotImplementedException($"Collection {property.TypeInfo.CollectionType} is not supported yet");
                    }

                    var propertyString = $@"public {property.TypeInfo.CompleteTypeString} {property.Name} {{ get; }} = {constructorString};";

                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();
                }
                else if (property.TypeInfo.IsIQueryable)
                {
                    // Properties
                    var propertyString = @$"public {type} {name} => throw new NotSupportedException(""Using backlinks is only possible for managed(persisted) objects."");";

                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();

                    // GetValue
                    getValueLines.AppendLine(@$"""{stringName}"" => throw new NotSupportedException(""Using backlinks is only possible for managed(persisted) objects.""),");
                }
                else
                {
                    // Properties
                    var initializerString = string.IsNullOrEmpty(property.Initializer) ? string.Empty : $" {property.Initializer}";
                    var backingFieldString = $"private {type} {backingFieldName}{initializerString};";

                    var propertyString = @$"public {type} {name}
{{
    get => {backingFieldName};
    set
    {{
        {backingFieldName} = value;
        RaisePropertyChanged(""{stringName}"");
    }}
}}";

                    propertiesString.AppendLine(backingFieldString);
                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();

                    // GetValue
                    getValueLines.AppendLine(@$"""{stringName}"" => {backingFieldName},");

                    // SetValue/SetValueUnique
                    setValueLines.AppendLine($@"case ""{stringName}"":");

                    if (property.IsPrimaryKey)
                    {
                        setValueLines.AppendLine($@"throw new InvalidOperationException(""Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique"");".Indent());

                        setValueUniqueLines.Append($@"if (propertyName != ""{stringName}"")
{{
    throw new InvalidOperationException($""Cannot set the value of an non primary key property ({{propertyName}}) with SetValueUnique"");
}}

{name} = ({type})val;");
                    }
                    else
                    {
                        setValueLines.AppendLine(@$"{name} = ({type})val;
return;".Indent());
                    }
                }
            }

            // GetValue
            string getValueBody;

            if (getValueLines.Length == 0)
            {
                getValueBody = $@"throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}"");";
            }
            else
            {
                getValueBody = $@"return propertyName switch
{{
{getValueLines.Indent(1, trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}""),
}};";
            }

            // SetValue
            string setValueBody;

            if (setValueLines.Length == 0)
            {
                setValueBody = $@"throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");";
            }
            else
            {
                setValueBody = $@"switch (propertyName)
{{
{setValueLines.Indent(1, trimNewLines: true)}
    default:
        throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");
}}";
            }

            // SetValueUnique
            if (setValueUniqueLines.Length == 0)
            {
                setValueUniqueLines.Append(@"throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");");
            }

            // GetListValue
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

            // GetSetValue
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

            // GetDictionaryValue
            string getDictionaryValueBody;

            if (getDictionaryValueLines.Length == 0)
            {
                getDictionaryValueBody = $@"throw new MissingMemberException($""The object does not have a Realm dictionary property with name {{propertyName}}"");";
            }
            else
            {
                getDictionaryValueBody = $@"return propertyName switch
{{
{getDictionaryValueLines.Indent(1, trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a Realm dictionary property with name {{propertyName}}""),
}};";
            }

            return $@"internal class {_unmanagedAccessorClassName} : UnmanagedAccessor, {_accessorInterfaceName}
{{
{propertiesString.Indent(trimNewLines: true)}

    public {_unmanagedAccessorClassName}(Type objectType) : base(objectType)
    {{
    }}

    public override RealmValue GetValue(string propertyName)
    {{
{getValueBody.Indent(2, trimNewLines: true)}
    }}

    public override void SetValue(string propertyName, RealmValue val)
    {{
{setValueBody.Indent(2, trimNewLines: true)}
    }}

    public override void SetValueUnique(string propertyName, RealmValue val)
    {{
{setValueUniqueLines.Indent(2, trimNewLines: true)}
    }}

    public override IList<T> GetListValue<T>(string propertyName)
    {{
{getListValueBody.Indent(2, trimNewLines: true)}
    }}

    public override ISet<T> GetSetValue<T>(string propertyName)
    {{
{getSetValueBody.Indent(2, trimNewLines: true)}
    }}

    public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
    {{
{getDictionaryValueBody.Indent(2, trimNewLines: true)}
    }}
}}";
        }

        private string GenerateManagedAccessor()
        {
            var propertiesBuilder = new StringBuilder();

            foreach (var property in _classInfo.Properties)
            {
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

                    propertiesBuilder.AppendLine(@$"{backingFieldString}
public {type} {name}
{{
    get
    {{
        if ({backingFieldName} == null)
        {{
            {backingFieldName} = {getFieldString}<{internalTypeString}>(""{property.MapTo ?? property.Name}"");
        }}

        return {backingFieldName};
    }}
}}");
                }
                else
                {
                    var getterString = $@"get => ({type})GetValue(""{stringName}"");";

                    var setterMethod = property.IsPrimaryKey ? "SetValueUnique" : "SetValue";
                    var setterString = $@"set => {setterMethod}(""{stringName}"", value);";

                    propertiesBuilder.AppendLine(@$"public {type} {name}
{{
    {getterString}
    {setterString}
}}");
                }

                propertiesBuilder.AppendLine();
            }

            return $@"[EditorBrowsable(EditorBrowsableState.Never)]
internal class {_managedAccessorClassName} : ManagedAccessor, {_accessorInterfaceName}
{{
{propertiesBuilder.Indent(trimNewLines: true)}
}}";
        }

        private static string GetBackingFieldName(string propertyName)
        {
            return "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        }

        private static string GetRealmValueType(PropertyTypeInfo propertyTypeInfo)
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

        private static string BoolToString(bool value) => value ? "true" : "false";
    }
}
