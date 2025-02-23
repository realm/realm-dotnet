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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Realms.SourceGenerator;

internal class RealmObjectCodeBuilder : ClassCodeBuilderBase
{
    private readonly string _helperClassName;
    private readonly string _accessorInterfaceName;
    private readonly string _managedAccessorClassName;
    private readonly string _unmanagedAccessorClassName;
    private readonly string _serializerClassName;

    public RealmObjectCodeBuilder(ClassInfo classInfo, GeneratorConfig generatorConfig)
        : base(classInfo, generatorConfig)
    {
        var className = _classInfo.Name;

        _helperClassName = $"{className}ObjectHelper";
        _accessorInterfaceName = $"I{className}Accessor";
        _managedAccessorClassName = $"{className}ManagedAccessor";
        _unmanagedAccessorClassName = $"{className}UnmanagedAccessor";
        _serializerClassName = $"{className}Serializer";
    }

    private string GenerateInterface()
    {
        var propertiesBuilder = new StringBuilder();

        foreach (var property in _classInfo.Properties)
        {
            var type = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired).CompleteType;
            var name = property.Name;
            var hasSetter = !property.TypeInfo.IsCollection;
            var setterString = hasSetter ? " set;" : string.Empty;

            propertiesBuilder.AppendLine($@"{type} {name} {{ get;{setterString} }}");
            propertiesBuilder.AppendLine();
        }

        return $@"[EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
internal interface {_accessorInterfaceName} : Realms.IRealmAccessor
{{
{propertiesBuilder.Indent(trimNewLines: true)}
}}";
    }

    protected override string GeneratePartialClass()
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
                    var backlinkType = property.TypeInfo.InternalType.MapTo ?? property.TypeInfo.InternalType.TypeString;

                    schemaProperties.AppendLine(@$"Realms.Schema.Property.Backlinks(""{property.GetMappedOrOriginalName()}"", ""{backlinkType}"", ""{backlinkProperty}"", managedName: ""{property.Name}""),");

                    // Nothing to do for the copy to realm part
                }
                else
                {
                    var internalType = property.TypeInfo.InternalType;

                    var internalTypeIsObject = internalType.ScalarType == ScalarType.Object;
                    var internalTypeIsRealmValue = internalType.ScalarType == ScalarType.RealmValue;

                    if (internalTypeIsObject)
                    {
                        var builderMethodName = $"Object{property.TypeInfo.CollectionType}";

                        var internalTypeString = internalType.MapTo ?? internalType.TypeString;
                        schemaProperties.AppendLine(@$"Realms.Schema.Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", ""{internalTypeString}"", managedName: ""{property.Name}""),");
                    }
                    else if (internalTypeIsRealmValue)
                    {
                        var builderMethodName = $"RealmValue{property.TypeInfo.CollectionType}";

                        schemaProperties.AppendLine(@$"Realms.Schema.Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", managedName: ""{property.Name}""),");
                    }
                    else
                    {
                        var builderMethodName = $"Primitive{property.TypeInfo.CollectionType}";

                        var internalTypeString = GetRealmValueType(internalType);
                        var internalTypeNullable = property.IsRequired ? "false" : internalType.IsNullable.ToCodeString();

                        schemaProperties.AppendLine(@$"Realms.Schema.Property.{builderMethodName}(""{property.GetMappedOrOriginalName()}"", {internalTypeString}, areElementsNullable: {internalTypeNullable}, managedName: ""{property.Name}""),");
                    }

                    skipDefaultsContent.AppendLine($"newAccessor.{property.Name}.Clear();");

                    // The namespace is necessary, otherwise there is a conflict if the class is in the global namespace
                    copyToRealm.AppendLine($@"Realms.CollectionExtensions.PopulateCollection(oldAccessor.{property.Name}, newAccessor.{property.Name}, update, skipDefaults);");
                }
            }
            else if (property.TypeInfo.ScalarType == ScalarType.Object)
            {
                var objectName = property.TypeInfo.MapTo ?? property.TypeInfo.TypeString;
                schemaProperties.AppendLine(@$"Realms.Schema.Property.Object(""{property.GetMappedOrOriginalName()}"", ""{objectName}"", managedName: ""{property.Name}""),");

                if (property.TypeInfo.ObjectType == ObjectType.RealmObject)
                {
                    copyToRealm.AppendLine(@$"if (oldAccessor.{property.Name} != null && newAccessor.Realm != null)
{{
    newAccessor.Realm.Add(oldAccessor.{property.Name}, update);
}}");
                }

                copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
            }
            else if (property.TypeInfo.ScalarType == ScalarType.RealmValue)
            {
                schemaProperties.AppendLine(@$"Realms.Schema.Property.RealmValue(""{property.GetMappedOrOriginalName()}"", managedName: ""{property.Name}""),");

                copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
            }
            else
            {
                var realmValueType = GetRealmValueType(property.TypeInfo);
                var isPrimaryKey = property.IsPrimaryKey.ToCodeString();
                var indexType = property.Index.ToCodeString();
                var isNullable = property.IsRequired ? "false" : property.TypeInfo.IsNullable.ToCodeString();
                schemaProperties.AppendLine(@$"Realms.Schema.Property.Primitive(""{property.GetMappedOrOriginalName()}"", {realmValueType}, isPrimaryKey: {isPrimaryKey}, indexType: {indexType}, isNullable: {isNullable}, managedName: ""{property.Name}""),");

                // The rules for determining whether to always set the property value are:
                // 1. If the property has [Required], always set it - this is only the case for string and byte[] properties.
                // 2. If the property is a string or byte[], and it's not nullable, always set it. This is because Core's default
                //    for these properties is "" and byte[0], which is different from the C# default (null).
                // 3. If the property is a DateTimeOffset, always set it. This is because Core's default for this property is
                //    1970-01-01T00:00:00Z, which is different from the C# default (0000-00-00T00:00:00Z).
                var shouldSetAlways = property.IsRequired ||
                    (property.TypeInfo.ScalarType == ScalarType.String && property.TypeInfo.NullableAnnotation != NullableAnnotation.Annotated) ||
                    (property.TypeInfo.ScalarType == ScalarType.Data && property.TypeInfo.NullableAnnotation != NullableAnnotation.Annotated) ||
                    property.TypeInfo.ScalarType == ScalarType.Date;

                if (shouldSetAlways)
                {
                    copyToRealm.AppendLine(@$"newAccessor.{property.Name} = oldAccessor.{property.Name};");
                }
                else
                {
                    copyToRealm.AppendLine(@$"if (!skipDefaults || oldAccessor.{property.Name} != default({property.TypeInfo.CompleteFullyQualifiedString}))
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

        var objectTypeString = $"ObjectSchema.ObjectType.{_classInfo.ObjectType}";

        var schema = @$"/// <summary>
/// Defines the schema for the <see cref=""{_classInfo.Name}""/> class.
/// </summary>
[System.Reflection.Obfuscation]
public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder(""{_classInfo.MapTo ?? _classInfo.Name}"", {objectTypeString})
{{
{schemaProperties.Indent(trimNewLines: true)}
}}.Build();";

        var parameterlessConstructorString = _classInfo.HasParameterlessConstructor
            ? string.Empty
            : @$"#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
private {_classInfo.Name}() {{}}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.";

        var helperString = string.Empty;

        if (!string.IsNullOrEmpty(skipDefaults) || copyToRealm.Length > 0)
        {
            var helperContent = new StringBuilder();

            if (!string.IsNullOrEmpty(skipDefaults))
            {
                helperContent.AppendLine(skipDefaults);
            }

            if (copyToRealm.Length > 0)
            {
                helperContent.Append(copyToRealm);
            }

            helperString = @$"if (helper != null && oldAccessor != null)
{{
{helperContent.Indent()}}}";
        }

        var contents = $@"{schema}

{parameterlessConstructorString}

#region {_baseInterface} implementation

private {_accessorInterfaceName}? _accessor;

Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

private {_accessorInterfaceName} Accessor => _accessor ??= new {_unmanagedAccessorClassName}(typeof({_classInfo.Name}));

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public bool IsManaged => Accessor.IsManaged;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public bool IsValid => Accessor.IsValid;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public bool IsFrozen => Accessor.IsFrozen;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public Realms.Realm? Realm => Accessor.Realm;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema!;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public int BacklinksCount => Accessor.BacklinksCount;

{(_classInfo.ObjectType != ObjectType.EmbeddedObject ? string.Empty :
    $@"/// <inheritdoc />
{_ignoreFieldAttribute.Value}
public Realms.IRealmObjectBase? Parent => Accessor.GetParent();")}

void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
{{
    var newAccessor = ({_accessorInterfaceName})managedAccessor;
    var oldAccessor = _accessor;
    _accessor = newAccessor;

{helperString.Indent()}

    if (_propertyChanged != null)
    {{
        SubscribeForNotifications();
    }}

    OnManaged();
}}

#endregion

/// <summary>
/// Called when the object has been managed by a Realm.
/// </summary>
/// <remarks>
/// This method will be called either when a managed object is materialized or when an unmanaged object has been
/// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
/// it is not yet clear whether the object is managed or not.
/// </remarks>
partial void OnManaged();

{(_classInfo.HasPropertyChangedEvent ? string.Empty :
    @"private event PropertyChangedEventHandler? _propertyChanged;

/// <inheritdoc />
public event PropertyChangedEventHandler? PropertyChanged
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

/// <summary>
/// Called when a property has changed on this class.
/// </summary>
/// <param name=""propertyName"">The name of the property.</param>
/// <remarks>
/// For this method to be called, you need to have first subscribed to <see cref=""PropertyChanged""/>.
/// This can be used to react to changes to the current object, e.g. raising <see cref=""PropertyChanged""/> for computed properties.
/// </remarks>
/// <example>
/// <code>
/// class MyClass : IRealmObject
/// {
///     public int StatusCodeRaw { get; set; }
///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
///     partial void OnPropertyChanged(string propertyName)
///     {
///         if (propertyName == nameof(StatusCodeRaw))
///         {
///             RaisePropertyChanged(nameof(StatusCode));
///         }
///     }
/// }
/// </code>
/// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref=""PropertyChanged""/>
/// subscribers that <c>StatusCode</c> has changed, we implement <see cref=""OnPropertyChanged""/> and
/// raise <see cref=""PropertyChanged""/> manually by calling <see cref=""RaisePropertyChanged""/>.
/// </example>
partial void OnPropertyChanged(string? propertyName);

private void RaisePropertyChanged([CallerMemberName] string propertyName = """")
{
    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    OnPropertyChanged(propertyName);
}

private void SubscribeForNotifications()
{
    Accessor.SubscribeForNotifications(RaisePropertyChanged);
}

private void UnsubscribeFromNotifications()
{
    Accessor.UnsubscribeFromNotifications();
}")}

/// <summary>
/// Converts a <see cref=""Realms.RealmValue""/> to <see cref=""{_classInfo.Name}""/>. Equivalent to <see cref=""Realms.RealmValue.AsNullableRealmObject{{T}}""/>.
/// </summary>
/// <param name=""val"">The <see cref=""Realms.RealmValue""/> to convert.</param>
/// <returns>The <see cref=""{_classInfo.Name}""/> stored in the <see cref=""Realms.RealmValue""/>.</returns>
public static explicit operator {_classInfo.Name}?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<{_classInfo.Name}>();

/// <summary>
/// Implicitly constructs a <see cref=""Realms.RealmValue""/> from <see cref=""{_classInfo.Name}""/>.
/// </summary>
/// <param name=""val"">The value to store in the <see cref=""Realms.RealmValue""/>.</param>
/// <returns>A <see cref=""Realms.RealmValue""/> containing the supplied <paramref name=""val""/>.</returns>
public static implicit operator Realms.RealmValue({_classInfo.Name}? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

/// <summary>
/// Implicitly constructs a <see cref=""Realms.QueryArgument""/> from <see cref=""{_classInfo.Name}""/>.
/// </summary>
/// <param name=""val"">The value to store in the <see cref=""Realms.QueryArgument""/>.</param>
/// <returns>A <see cref=""Realms.QueryArgument""/> containing the supplied <paramref name=""val""/>.</returns>
public static implicit operator Realms.QueryArgument({_classInfo.Name}? val) => (Realms.RealmValue)val;

/// <inheritdoc />
[EditorBrowsable(EditorBrowsableState.Never)]
public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

{(_classInfo.OverridesEquals ? string.Empty :
    @"/// <inheritdoc />
public override bool Equals(object? obj)
{
    if (obj is null)
    {
        return false;
    }

    if (ReferenceEquals(this, obj))
    {
        return true;
    }

    if (obj is InvalidObject)
    {
        return !IsValid;
    }

    if (!(obj is Realms.IRealmObjectBase iro))
    {
        return false;
    }

    return Accessor.Equals(iro.Accessor);
}")}

{(_classInfo.OverridesGetHashCode ? string.Empty :
    @"/// <inheritdoc />
public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();")}

{(_classInfo.OverridesToString ? string.Empty :
    @"/// <inheritdoc />
public override string? ToString() => Accessor.ToString();")}";

        var classString = $@"[Generated]
[Woven(typeof({_helperClassName})), Realms.Preserve(AllMembers = true)]
{SyntaxFacts.GetText(_classInfo.Accessibility)} partial class {_classInfo.Name} : {_baseInterface}, INotifyPropertyChanged, IReflectableType
{{

    [Realms.Preserve]
    static {_classInfo.Name}()
    {{
        Realms.Serialization.RealmObjectSerializer.Register(new {_serializerClassName}());
    }}

{contents.Indent()}

{GenerateClassObjectHelper().Indent()}

{GenerateInterface().Indent()}

{GenerateManagedAccessor().Indent()}

{GenerateUnmanagedAccessor().Indent()}

{GenerateSerializer().Indent()}
}}";

        return classString;
    }

    private string GenerateClassObjectHelper()
    {
        var primaryKeyProperty = _classInfo.PrimaryKey;
        var valueAccessor = primaryKeyProperty == null ? "RealmValue.Null" : $"(({_accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name}";

        return $@"[EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
private class {_helperClassName} : Realms.Weaving.IRealmObjectHelper
{{
    public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
    {{
        throw new InvalidOperationException(""This method should not be called for source generated classes."");
    }}

    public Realms.ManagedAccessor CreateAccessor() => new {_managedAccessorClassName}();

    public Realms.IRealmObjectBase CreateInstance() => new {_classInfo.Name}();

    public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
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
            var (type, internalType, needsNullForgiving) = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired);
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
                    string constructorString;

                    switch (property.TypeInfo.CollectionType)
                    {
                        case CollectionType.List:
                            constructorString = $"new List<{internalType}>()";
                            getListValueLines.AppendLine($@"""{stringName}"" => (IList<T>){property.Name},");
                            break;
                        case CollectionType.Set:
                            constructorString = $"new HashSet<{internalType}>(RealmSet<{internalType}>.Comparer)";
                            getSetValueLines.AppendLine($@"""{stringName}"" => (ISet<T>){property.Name},");
                            break;
                        case CollectionType.Dictionary:
                            constructorString = $"new Dictionary<string, {internalType}>()";
                            getDictionaryValueLines.AppendLine($@"""{stringName}"" => (IDictionary<string, TValue>){property.Name},");
                            break;
                        default:
                            throw new NotImplementedException($"Collection {property.TypeInfo.CollectionType} is not supported yet");
                    }

                    var propertyString = $@"public {property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired).CompleteType} {property.Name} {{ get; }} = {constructorString};";

                    propertiesString.AppendLine(propertyString);
                    propertiesString.AppendLine();
                }
            }
            else
            {
                // Properties
                var initializerString = string.Empty;

                if (!string.IsNullOrEmpty(property.Initializer))
                {
                    initializerString = $" {property.Initializer}";
                }
                else if (needsNullForgiving)
                {
                    initializerString = " = null!";
                }

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

                var forceNotNullable = type == "string" || type == "byte[]" ? "!" : string.Empty;

                if (property.IsPrimaryKey)
                {
                    setValueLines.AppendLine(@"throw new InvalidOperationException(""Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique"");".Indent());

                    setValueUniqueLines.Append($@"if (propertyName != ""{stringName}"")
{{
    throw new InvalidOperationException($""Cannot set the value of non primary key property ({{propertyName}}) with SetValueUnique"");
}}

{name} = ({type})val{forceNotNullable};");
                }
                else
                {
                    setValueLines.AppendLine(@$"{name} = ({type})val{forceNotNullable};
return;".Indent());
                }
            }
        }

        // GetValue
        string getValueBody;

        if (getValueLines.Length == 0)
        {
            getValueBody = @"throw new MissingMemberException($""The object does not have a gettable Realm property with name {propertyName}"");";
        }
        else
        {
            getValueBody = $@"return propertyName switch
{{
{getValueLines.Indent(trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}""),
}};";
        }

        // SetValue
        string setValueBody;

        if (setValueLines.Length == 0)
        {
            setValueBody = @"throw new MissingMemberException($""The object does not have a settable Realm property with name {propertyName}"");";
        }
        else
        {
            setValueBody = $@"switch (propertyName)
{{
{setValueLines.Indent(trimNewLines: true)}
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
            getListValueBody = @"throw new MissingMemberException($""The object does not have a Realm list property with name {propertyName}"");";
        }
        else
        {
            getListValueBody = $@"return propertyName switch
{{
{getListValueLines.Indent(trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a Realm list property with name {{propertyName}}""),
}};";
        }

        // GetSetValue
        string getSetValueBody;

        if (getSetValueLines.Length == 0)
        {
            getSetValueBody = @"throw new MissingMemberException($""The object does not have a Realm set property with name {propertyName}"");";
        }
        else
        {
            getSetValueBody = $@"return propertyName switch
{{
{getSetValueLines.Indent(trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a Realm set property with name {{propertyName}}""),
}};";
        }

        // GetDictionaryValue
        string getDictionaryValueBody;

        if (getDictionaryValueLines.Length == 0)
        {
            getDictionaryValueBody = @"throw new MissingMemberException($""The object does not have a Realm dictionary property with name {propertyName}"");";
        }
        else
        {
            getDictionaryValueBody = $@"return propertyName switch
{{
{getDictionaryValueLines.Indent(trimNewLines: true)}
    _ => throw new MissingMemberException($""The object does not have a Realm dictionary property with name {{propertyName}}""),
}};";
        }

        return $@"[EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
private class {_unmanagedAccessorClassName} : Realms.UnmanagedAccessor, {_accessorInterfaceName}
{{
    public override ObjectSchema ObjectSchema => {_classInfo.Name}.RealmSchema;

{propertiesString.Indent(trimNewLines: true)}

    public {_unmanagedAccessorClassName}(Type objectType) : base(objectType)
    {{
    }}

    public override Realms.RealmValue GetValue(string propertyName)
    {{
{getValueBody.Indent(2, trimNewLines: true)}
    }}

    public override void SetValue(string propertyName, Realms.RealmValue val)
    {{
{setValueBody.Indent(2, trimNewLines: true)}
    }}

    public override void SetValueUnique(string propertyName, Realms.RealmValue val)
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
            var (type, internalType, needsNullForgiving) = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired);
            var name = property.Name;
            var stringName = property.MapTo ?? name;

            if (property.TypeInfo.IsCollection)
            {
                var backingFieldName = GetBackingFieldName(property.Name);
                var nullableForgivingString = needsNullForgiving ? " = null!" : string.Empty;
                var backingFieldString = $@"private {type} {backingFieldName}{nullableForgivingString};";

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
            {backingFieldName} = {getFieldString}<{internalType}>(""{property.GetMappedOrOriginalName()}"");
        }}

        return {backingFieldName};
    }}
}}");
            }
            else
            {
                var forceNotNullable = type is "string" or "byte[]" ? "!" : string.Empty;

                var getterString = $@"get => ({type})GetValue(""{stringName}""){forceNotNullable};";

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

        return $@"[EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
private class {_managedAccessorClassName} : Realms.ManagedAccessor, {_accessorInterfaceName}
{{
{propertiesBuilder.Indent(trimNewLines: true)}
}}";
    }

    private string GenerateSerializer()
    {
        var serializeValueLines = new StringBuilder();
        var readValueLines = new StringBuilder();
        var readArrayElementLines = new StringBuilder();
        var readDocumentFieldLines = new StringBuilder();
        var readArrayLines = new StringBuilder();
        var readDictionaryLines = new StringBuilder();

        foreach (var property in _classInfo.Properties)
        {
            var name = property.Name;
            var stringName = property.GetMappedOrOriginalName();

            if (property.TypeInfo.IsBacklink)
            {
                continue; // Backlinks are not de/serialized
            }
            else if (property.TypeInfo.IsCollection)
            {
                serializeValueLines.AppendLine($"Write{property.TypeInfo.CollectionType}(context, args, \"{stringName}\", value.{name});");
                if (property.TypeInfo.IsDictionary)
                {
                    var type = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired).InternalType;

                    var deserialize = property.TypeInfo.InternalType!.ObjectType is ObjectType.None or ObjectType.EmbeddedObject
                        ? $"BsonSerializer.LookupSerializer<{type}>().Deserialize(context)"
                        : $"Realms.Serialization.RealmObjectSerializer.LookupSerializer<{type}>()!.DeserializeById(context)!";

                    readDocumentFieldLines.AppendLine($@"case ""{stringName}"":
    instance.{name}[fieldName] = {deserialize};
    break;");

                    readDictionaryLines.AppendLine($@"case ""{stringName}"":");
                }
                else
                {
                    var type = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired).InternalType;

                    var deserialize = property.TypeInfo.InternalType!.ObjectType is ObjectType.None or ObjectType.EmbeddedObject
                        ? $"BsonSerializer.LookupSerializer<{type}>().Deserialize(context)"
                        : $"Realms.Serialization.RealmObjectSerializer.LookupSerializer<{type}>()!.DeserializeById(context)!";

                    readArrayElementLines.AppendLine($@"case ""{stringName}"":
    instance.{name}.Add({deserialize});
    break;");
                    readArrayLines.AppendLine($@"case ""{stringName}"":");
                }
            }
            else
            {
                var type = property.TypeInfo.GetCorrectlyAnnotatedTypeName(property.IsRequired).CompleteType;

                serializeValueLines.AppendLine($"WriteValue(context, args, \"{stringName}\", value.{name});");
                var deserialize = property.TypeInfo.ObjectType is ObjectType.None or ObjectType.EmbeddedObject
                    ? $"BsonSerializer.LookupSerializer<{type}>().Deserialize(context)"
                    : $"Realms.Serialization.RealmObjectSerializer.LookupSerializer<{type}>()!.DeserializeById(context)";
                readValueLines.AppendLine($@"case ""{stringName}"":
    instance.{name} = {deserialize};
    break;");
            }
        }

        if (readArrayLines.Length > 0)
        {
            readValueLines.Append(readArrayLines);
            readValueLines.AppendLine(@"    ReadArray(instance, name, context);
    break;");
        }

        if (readDictionaryLines.Length > 0)
        {
            readValueLines.Append(readDictionaryLines);
            readValueLines.AppendLine(@"    ReadDictionary(instance, name, context);
    break;");
        }

        return $@"[EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
private class {_serializerClassName} : Realms.Serialization.RealmObjectSerializerBase<{_classInfo.Name}>
{{
    public override string SchemaName => ""{_classInfo.MapTo ?? _classInfo.Name}"";

    protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, {_classInfo.Name} value)
    {{
        context.Writer.WriteStartDocument();

{serializeValueLines.Indent(2, trimNewLines: true)}

        context.Writer.WriteEndDocument();
    }}

    protected override {_classInfo.Name} CreateInstance() => new {_classInfo.Name}();

    protected override void ReadValue({_classInfo.Name} instance, string name, BsonDeserializationContext context)
    {{
{(readValueLines.Length == 0
    ? "// No Realm properties to deserialize"
    : $@"switch (name)
{{
{readValueLines.Indent(trimNewLines: true)}
    default:
        context.Reader.SkipValue();
        break;
}}").Indent(2)}
    }}

    protected override void ReadArrayElement({_classInfo.Name} instance, string name, BsonDeserializationContext context)
    {{
{(readArrayElementLines.Length == 0
    ? "// No persisted list/set properties to deserialize"
    : $@"switch (name)
{{
{readArrayElementLines.Indent(trimNewLines: true)}
}}").Indent(2)}
    }}

    protected override void ReadDocumentField({_classInfo.Name} instance, string name, string fieldName, BsonDeserializationContext context)
    {{
{(readDocumentFieldLines.Length == 0
    ? "// No persisted dictionary properties to deserialize"
    : $@"switch (name)
{{
{readDocumentFieldLines.Indent(trimNewLines: true)}
}}").Indent(2)}
    }}
}}";
    }

    private static string GetBackingFieldName(string propertyName)
    {
        return "_" + char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
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

        return "Realms.RealmValueType." + endString;
    }

    private static string BoolToString(bool value) => value ? "true" : "false";
}
