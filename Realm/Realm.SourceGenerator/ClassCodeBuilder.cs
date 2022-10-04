﻿////////////////////////////////////////////////////////////////////////////
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
            "System.Runtime.Serialization",
            "System.Xml.Serialization",
            "System.Reflection",
            "System.ComponentModel",
            "Realms",
            "Realms.Weaving",
            "Realms.Schema",
        };

        private ClassInfo _classInfo;

        private string _helperClassName;
        private string _accessorInterfaceName;
        private string _managedAccessorClassName;
        private string _unmanagedAccessorClassName;
        private string _generatedNamespaceName;

        public ClassCodeBuilder(ClassInfo classInfo)
        {
            _classInfo = classInfo;

            var className = _classInfo.Name;

            _helperClassName = $"{className}ObjectHelper";
            _accessorInterfaceName = $"I{className}Accessor";
            _managedAccessorClassName = $"{className}ManagedAccessor";
            _unmanagedAccessorClassName = $"{className}UnmanagedAccessor";
            _generatedNamespaceName = $"{_classInfo.Namespace}.Generated";
        }

        public string GenerateSource()
        {
            var usings = GetUsings();

            var partialClassString = GeneratePartialClass().Indent();
            var interfaceString = GenerateInterface().Indent();
            var managedAccessorString = GenerateManagedAccessor().Indent();
            var unmanagedAccessorString = GenerateUnmanagedAccessor().Indent();

            return $@"// <auto-generated />
{usings}

namespace {_classInfo.Namespace}
{{
{partialClassString}
}}

namespace {_generatedNamespaceName}
{{
{interfaceString}

{managedAccessorString}

{unmanagedAccessorString}
}}";
        }

        private string GetUsings()
        {
            var namespaces = new HashSet<string>() { _classInfo.Namespace, _generatedNamespaceName };
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
                var hasSetter = !property.TypeInfo.IsCollection;
                var setterString = hasSetter ? " set;" : string.Empty;

                propertiesBuilder.AppendLine($@"{type} {name} {{ get;{setterString} }}");
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
                    if (property.TypeInfo.IsBacklink)
                    {
                        var backlinkProperty = property.GetMappedOrOriginalBacklink();
                        var backlinkType = property.TypeInfo.InternalType.MapTo ?? property.TypeInfo.InternalType.CompleteTypeString;

                        schemaProperties.AppendLine(@$"Property.Backlinks(""{property.GetMappedOrOriginalName()}"", ""{backlinkType}"", ""{backlinkProperty}"", managedName: ""{property.Name}""),");

                        // Nothing to do for the copy to realm part
                    }
                    else
                    {
                        var internalType = property.TypeInfo.InternalType;

                        var internalTypeIsObject = internalType.ScalarType == ScalarType.Object;
                        var internalTypeIsRealmValue = internalType.ScalarType == ScalarType.RealmValue;

                        string addValLine = string.Empty;

                        if (internalTypeIsObject)
                        {
                            var builderMethodName = property.TypeInfo.CollectionType switch
                            {
                                CollectionType.List => "ObjectList",
                                CollectionType.Set => "ObjectSet",
                                CollectionType.Dictionary => "ObjectDictionary",
                                _ => throw new NotImplementedException(),
                            };

                            var internalTypeString = internalType.MapTo ?? internalType.CompleteTypeString;
                            schemaProperties.AppendLine(@$"Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", ""{internalTypeString}"", managedName: ""{property.Name}""),");

                            if (internalType.ObjectType == ObjectType.RealmObject)
                            {
                                if (property.TypeInfo.IsDictionary)
                                {
                                    addValLine = "newAccessor.Realm.Add(val.Value, update);";
                                }
                                else
                                {
                                    addValLine = "newAccessor.Realm.Add(val, update);";
                                }
                            }
                        }
                        else if (internalTypeIsRealmValue)
                        {
                            var builderMethodName = property.TypeInfo.CollectionType switch
                            {
                                CollectionType.List => "RealmValueList",
                                CollectionType.Set => "RealmValueSet",
                                CollectionType.Dictionary => "RealmValueDictionary",
                                _ => throw new NotImplementedException(),
                            };

                            schemaProperties.AppendLine(@$"Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", managedName: ""{property.Name}""),");
                        }
                        else
                        {
                            var builderMethodName = property.TypeInfo.CollectionType switch
                            {
                                CollectionType.List => "PrimitiveList",
                                CollectionType.Set => "PrimitiveSet",
                                CollectionType.Dictionary => "PrimitiveDictionary",
                                _ => throw new NotImplementedException(),
                            };

                            var internalTypeString = GetRealmValueType(internalType);
                            var internalTypeNullable = property.IsRequired ? "false" : internalType.IsNullable.ToCodeString();

                            schemaProperties.AppendLine(@$"Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", {internalTypeString}, areElementsNullable: {internalTypeNullable}, managedName: ""{property.Name}""),");
                        }

                        skipDefaultsContent.AppendLine($"newAccessor.{property.Name}.Clear();");

                        copyToRealm.AppendLine($@"
CollectionExtensions.PopulateCollection(oldAccessor.{property.Name}, newAccessor.{property.Name}, update, skipDefaults);
");
                    }
                }
                else if (property.TypeInfo.ScalarType == ScalarType.Object)
                {
                    var objectName = property.TypeInfo.MapTo ?? property.TypeInfo.CompleteTypeString;
                    schemaProperties.AppendLine(@$"Property.Object(""{property.GetMappedOrOriginalName()}"", ""{objectName}"", managedName: ""{property.Name}""),");

                    if (property.TypeInfo.ObjectType == ObjectType.RealmObject)
                    {
                        copyToRealm.AppendLine(@$"if(oldAccessor.{property.Name} != null)
{{
    newAccessor.Realm.Add(oldAccessor.{property.Name}, update);
}}");
                    }

                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
                else if (property.TypeInfo.ScalarType == ScalarType.RealmValue)
                {
                    schemaProperties.AppendLine(@$"Property.RealmValue(""{property.GetMappedOrOriginalName()}"", managedName: ""{property.Name}""),");

                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
                else
                {
                    var realmValueType = GetRealmValueType(property.TypeInfo);
                    var isPrimaryKey = property.IsPrimaryKey.ToCodeString();
                    var isIndexed = property.IsIndexed.ToCodeString();
                    var isNullable = property.IsRequired ? "false" : property.TypeInfo.IsNullable.ToCodeString();
                    schemaProperties.AppendLine(@$"Property.Primitive(""{property.GetMappedOrOriginalName()}"", {realmValueType}, isPrimaryKey: {isPrimaryKey}, isIndexed: {isIndexed}, isNullable: {isNullable}, managedName: ""{property.Name}""),");

                    var shouldSetAlways = property.IsRequired ||
                        property.TypeInfo.NullableAnnotation == NullableAnnotation.Annotated ||
                        property.TypeInfo.IsRealmInteger ||
                        property.TypeInfo.ScalarType == ScalarType.Date ||
                        property.TypeInfo.ScalarType == ScalarType.Decimal ||
                        property.TypeInfo.ScalarType == ScalarType.ObjectId ||
                        property.TypeInfo.ScalarType == ScalarType.Guid;

                    if (shouldSetAlways)
                    {
                        copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                    }
                    else
                    {
                        copyToRealm.AppendLine(@$"if(!skipDefaults || oldAccessor.{property.Name} != default({property.TypeInfo.CompleteTypeString}))
{{
    newAccessor.{property.Name} = oldAccessor.{property.Name};
}}");
                    }
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

            var schema = @$"public static ObjectSchema RealmSchema = new ObjectSchema.Builder(""{_classInfo.MapTo ?? _classInfo.Name}"", {_classInfo.ObjectType.ToObjectSchemaObjectType()})
{{
{schemaProperties.Indent(trimNewLines: true)}
}}.Build();";

            var baseInterface = _classInfo.ObjectType.ToInterface();
            var parameterlessConstructorString = _classInfo.HasParameterlessConstructor ? string.Empty : $"private {_classInfo.Name}() {{}}";

            var contents = $@"{schema}

#region {baseInterface} implementation

private {_accessorInterfaceName} _accessor;

IRealmAccessor IRealmObjectBase.Accessor => Accessor;

internal {_accessorInterfaceName} Accessor => _accessor = _accessor ?? new {_unmanagedAccessorClassName}(typeof({_classInfo.Name}));

[IgnoreDataMember, XmlIgnore]
public bool IsManaged => Accessor.IsManaged;

[IgnoreDataMember, XmlIgnore]
public bool IsValid => Accessor.IsValid;

[IgnoreDataMember, XmlIgnore]
public bool IsFrozen => Accessor.IsFrozen;

[IgnoreDataMember, XmlIgnore]
public Realm Realm => Accessor.Realm;

[IgnoreDataMember, XmlIgnore]
public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

[IgnoreDataMember, XmlIgnore]
public DynamicObjectApi DynamicApi => Accessor.DynamicApi;

[IgnoreDataMember, XmlIgnore]
public int BacklinksCount => Accessor.BacklinksCount;

{parameterlessConstructorString}

public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
{{
    var newAccessor = ({_accessorInterfaceName})managedAccessor;
    var oldAccessor = _accessor as {_accessorInterfaceName};
    _accessor = newAccessor;

    if (helper != null)
    {{
{skipDefaults.Indent(2)}
{copyToRealm.Indent(2, trimNewLines: true)}
    }}

    if (_propertyChanged != null)
    {{
        SubscribeForNotifications();
    }}

    OnManaged();
}}

#endregion

partial void OnManaged();

{(_classInfo.HasPropertyChangedEvent ? string.Empty :
$@"private event PropertyChangedEventHandler _propertyChanged;

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

private void SubscribeForNotifications()
{{
    Accessor.SubscribeForNotifications(RaisePropertyChanged);
}}

private void UnsubscribeFromNotifications()
{{
    Accessor.UnsubscribeFromNotifications();
}}")}

public static explicit operator {_classInfo.Name}(RealmValue val) => val.AsRealmObject<{_classInfo.Name}>();

public static implicit operator RealmValue({_classInfo.Name} val) => RealmValue.Object(val);

[EditorBrowsable(EditorBrowsableState.Never)]
public TypeInfo GetTypeInfo()
{{
    return Accessor.GetTypeInfo(this);
}}

{(_classInfo.OverridesEquals ? string.Empty :
$@"public override bool Equals(object obj)
{{
    if (obj is null)
    {{
        return false;
    }}

    if (ReferenceEquals(this, obj))
    {{
        return true;
    }}

    if (obj is InvalidObject)
    {{
        return !IsValid;
    }}

    if (obj is not IRealmObjectBase iro)
    {{
        return false;
    }}

    return Accessor.Equals(iro.Accessor);
}}")}

{(_classInfo.OverridesGetHashCode ? string.Empty :
$@"public override int GetHashCode()
{{
    return IsManaged ? Accessor.GetHashCode() : base.GetHashCode();
}}")}

{(_classInfo.OverridesToString ? string.Empty :
$@"public override string ToString()
{{
    return Accessor.ToString();
}}")}";

            var classString = $@"[Generated]
[Woven(typeof({_helperClassName}))]
{SyntaxFacts.GetText(_classInfo.Accessibility)} partial class {_classInfo.Name} : {baseInterface}, INotifyPropertyChanged, IReflectableType
{{
{contents.Indent()}

{GenerateClassObjectHelper().Indent()}
}}";

            return classString;
        }

        private string GenerateClassObjectHelper()
        {
            var primaryKeyProperty = _classInfo.PrimaryKey;
            var valueAccessor = primaryKeyProperty == null ? "null" : $"(({_accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name}";

            return $@"[EditorBrowsable(EditorBrowsableState.Never)]
private class {_helperClassName} : IRealmObjectHelper
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
                    if (property.TypeInfo.IsBacklink)
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
                        var parameterString = property.TypeInfo.InternalType.CompleteTypeString;
                        var propertyMapToName = property.GetMappedOrOriginalName();

                        string constructorString;

                        switch (property.TypeInfo.CollectionType)
                        {
                            case CollectionType.List:
                                constructorString = $"new List<{parameterString}>()";
                                getListValueLines.AppendLine($@"""{propertyMapToName}"" => (IList<T>){property.Name},");
                                break;
                            case CollectionType.Set:
                                constructorString = $"new HashSet<{parameterString}>(RealmSet<{parameterString}>.Comparer)";
                                getSetValueLines.AppendLine($@"""{propertyMapToName}"" => (ISet<T>){property.Name},");
                                break;
                            case CollectionType.Dictionary:
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
        RaisePropertyChanged(""{name}"");
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
    throw new InvalidOperationException($""Cannot set the value of non primary key property ({{propertyName}}) with SetValueUnique"");
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

                if (property.TypeInfo.IsCollection)
                {
                    var backingFieldName = GetBackingFieldName(property.Name);
                    var backingFieldString = $@"private {type} {backingFieldName};";
                    var internalTypeString = property.TypeInfo.InternalType.CompleteTypeString;

                    string getFieldString;

                    if (property.TypeInfo.IsBacklink)
                    {
                        getFieldString = "GetBacklinks";
                    }
                    else
                    {
                        getFieldString = property.TypeInfo.CollectionType switch
                        {
                            CollectionType.List => "GetListValue",
                            CollectionType.Set => "GetSetValue",
                            CollectionType.Dictionary => "GetDictionaryValue",
                            _ => throw new NotImplementedException(),
                        };
                    }

                    propertiesBuilder.AppendLine(@$"{backingFieldString}
public {type} {name}
{{
    get
    {{
        if ({backingFieldName} == null)
        {{
            {backingFieldName} = {getFieldString}<{internalTypeString}>(""{property.GetMappedOrOriginalName()}"");
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
            var scalarType = propertyTypeInfo.IsRealmInteger ? propertyTypeInfo.InternalType.ScalarType : propertyTypeInfo.ScalarType;

            var endString = scalarType switch
            {
                ScalarType.Int => "Int",
                ScalarType.Bool => "Bool",
                ScalarType.String => "String",
                ScalarType.Data => "Data",
                ScalarType.Date => "Date",
                ScalarType.Float => "Float",
                ScalarType.Double => "Double",
                ScalarType.Object => "Object",
                ScalarType.RealmValue => "RealmValue",
                ScalarType.ObjectId => "ObjectId",
                ScalarType.Decimal => "Decimal128",
                ScalarType.Guid => "Guid",
                _ => throw new NotImplementedException(),
            };

            return "RealmValueType." + endString;
        }

        private static string BoolToString(bool value) => value ? "true" : "false";
    }
}
