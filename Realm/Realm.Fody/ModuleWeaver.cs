////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public partial class ModuleWeaver : Fody.BaseModuleWeaver
{
    private const MethodAttributes DefaultMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

    internal const string StringTypeName = "System.String";
    internal const string ByteArrayTypeName = "System.Byte[]";
    internal const string CharTypeName = "System.Char";
    internal const string ByteTypeName = "System.Byte";
    internal const string Int16TypeName = "System.Int16";
    internal const string Int32TypeName = "System.Int32";
    internal const string Int64TypeName = "System.Int64";
    internal const string SingleTypeName = "System.Single";
    internal const string DoubleTypeName = "System.Double";
    internal const string BooleanTypeName = "System.Boolean";
    internal const string DecimalTypeName = "System.Decimal";
    internal const string Decimal128TypeName = "MongoDB.Bson.Decimal128";
    internal const string DateTimeOffsetTypeName = "System.DateTimeOffset";
    internal const string NullableCharTypeName = "System.Nullable`1<System.Char>";
    internal const string NullableByteTypeName = "System.Nullable`1<System.Byte>";
    internal const string NullableInt16TypeName = "System.Nullable`1<System.Int16>";
    internal const string NullableInt32TypeName = "System.Nullable`1<System.Int32>";
    internal const string NullableInt64TypeName = "System.Nullable`1<System.Int64>";
    internal const string NullableSingleTypeName = "System.Nullable`1<System.Single>";
    internal const string NullableDoubleTypeName = "System.Nullable`1<System.Double>";
    internal const string NullableBooleanTypeName = "System.Nullable`1<System.Boolean>";
    internal const string NullableDecimalTypeName = "System.Nullable`1<System.Decimal>";
    internal const string NullableDecimal128TypeName = "System.Nullable`1<MongoDB.Bson.Decimal128>";
    internal const string NullableDateTimeOffsetTypeName = "System.Nullable`1<System.DateTimeOffset>";

    private static readonly Dictionary<string, string> _typeTable = new Dictionary<string, string>
    {
        { StringTypeName, "String" },
        { ByteArrayTypeName, "ByteArray" },
    };

    private static readonly Dictionary<string, string> _primitiveValueTypes = new Dictionary<string, string>
    {
        { CharTypeName, "Int" },
        { SingleTypeName, "Float" },
        { DoubleTypeName, "Double" },
        { BooleanTypeName, "Bool" },
        { DecimalTypeName, "Decimal" },
        { Decimal128TypeName, "Decimal" },
        { DateTimeOffsetTypeName, "Date" },
        { NullableCharTypeName, "NullableInt" },
        { NullableSingleTypeName, "NullableFloat" },
        { NullableDoubleTypeName, "NullableDouble" },
        { NullableBooleanTypeName, "NullableBool" },
        { NullableDateTimeOffsetTypeName, "NullableDate" },
        { NullableDecimalTypeName, "NullableDecimal" },
        { NullableDecimal128TypeName, "NullableDecimal" },
    };

    private static readonly IEnumerable<string> _realmIntegerBackedTypes = new[]
    {
        ByteTypeName,
        Int16TypeName,
        Int32TypeName,
        Int64TypeName,
        NullableByteTypeName,
        NullableInt16TypeName,
        NullableInt32TypeName,
        NullableInt64TypeName,
        $"Realms.RealmInteger`1<{ByteTypeName}>",
        $"Realms.RealmInteger`1<{Int16TypeName}>",
        $"Realms.RealmInteger`1<{Int32TypeName}>",
        $"Realms.RealmInteger`1<{Int64TypeName}>",
        $"System.Nullable`1<Realms.RealmInteger`1<{ByteTypeName}>>",
        $"System.Nullable`1<Realms.RealmInteger`1<{Int16TypeName}>>",
        $"System.Nullable`1<Realms.RealmInteger`1<{Int32TypeName}>>",
        $"System.Nullable`1<Realms.RealmInteger`1<{Int64TypeName}>>"
    };

    private static readonly IEnumerable<string> _primaryKeyTypes = new[]
    {
        StringTypeName,
        CharTypeName,
        ByteTypeName,
        Int16TypeName,
        Int32TypeName,
        Int64TypeName,
        NullableCharTypeName,
        NullableByteTypeName,
        NullableInt16TypeName,
        NullableInt32TypeName,
        NullableInt64TypeName,
    };

    private static readonly HashSet<string> RealmPropertyAttributes = new HashSet<string>
    {
        "PrimaryKeyAttribute",
        "IndexedAttribute",
        "MapToAttribute",
    };

    private RealmWeaver.ImportedReferences _references;

    private IEnumerable<TypeDefinition> GetMatchingTypes()
    {
        foreach (var type in ModuleDefinition.GetTypes().Where(t => t.IsDescendedFrom(_references.RealmObject) || t.IsDescendedFrom(_references.EmbeddedObject)))
        {
            if (type.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute"))
            {
                continue;
            }
            else if (type.IsValidRealmObjectBaseInheritor(_references))
            {
                yield return type;
            }
            else
            {
                WriteError($"The type {type.FullName} indirectly inherits from RealmObject which is not supported.", type.GetConstructors().FirstOrDefault()?.DebugInformation?.SequencePoints?.FirstOrDefault());
            }
        }
    }

    public override void Execute()
    {
        //// UNCOMMENT THIS DEBUGGER LAUNCH TO BE ABLE TO RUN A SEPARATE VS INSTANCE TO DEBUG WEAVING WHILST BUILDING
        //// Debugger.Launch();

        Debug.WriteLine("Weaving file: " + ModuleDefinition.FileName);

        var targetFramework = ModuleDefinition.Assembly.CustomAttributes.Single(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
        var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);

        _references = RealmWeaver.ImportedReferences.Create(this, frameworkName);

        if (_references.WovenAssemblyAttribute == null)
        {
            WriteInfo($"Not weaving assembly '{ModuleDefinition.Assembly.Name}' because it doesn't reference Realm.");
            return;
        }

        var isWoven = ModuleDefinition.Assembly.CustomAttributes.Any(a => a.AttributeType.IsSameAs(_references.WovenAssemblyAttribute));
        if (isWoven)
        {
            WriteInfo($"Not weaving assembly '{ModuleDefinition.Assembly.Name}' because it has already been processed.");
            return;
        }

        var submitAnalytics = Task.Run(() =>
        {
            var analytics = new RealmWeaver.Analytics(frameworkName, ModuleDefinition.Name);
            try
            {
                var payload = analytics.SubmitAnalytics();
#if DEBUG
                WriteDebug($@"
----------------------------------
Analytics payload
{payload}
----------------------------------");
#endif
            }
            catch (Exception e)
            {
                WriteDebug("Error submitting analytics: " + e.Message);
            }
        });

        // Cache of getter and setter methods for the various types.
        var methodTable = new Dictionary<string, Accessors>();

        var matchingTypes = GetMatchingTypes().ToArray();
        foreach (var type in matchingTypes)
        {
            try
            {
                WeaveType(type, methodTable);
            }
            catch (Exception e)
            {
                WriteError($"Unexpected error caught weaving type '{type.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}");
            }
        }

        WeaveSchema(matchingTypes);

        var wovenAssemblyAttribute = new CustomAttribute(_references.WovenAssemblyAttribute_Constructor);
        ModuleDefinition.Assembly.CustomAttributes.Add(wovenAssemblyAttribute);

        submitAnalytics.Wait();
    }

    private void WeaveType(TypeDefinition type, Dictionary<string, Accessors> methodTable)
    {
        WriteDebug("Weaving " + type.Name);

        var persistedProperties = new List<WeaveResult>();
        foreach (var prop in type.Properties.Where(x => x.HasThis && !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
        {
            try
            {
                var weaveResult = WeaveProperty(prop, type, methodTable);
                if (weaveResult.Woven)
                {
                    persistedProperties.Add(weaveResult);
                }
                else
                {
                    var sequencePoint = prop.GetMethod.DebugInformation.SequencePoints.FirstOrDefault();
                    if (!string.IsNullOrEmpty(weaveResult.ErrorMessage))
                    {
                        // We only want one error point, so even though there may be more problems, we only log the first one.
                        WriteError(weaveResult.ErrorMessage, sequencePoint);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(weaveResult.WarningMessage))
                        {
                            WriteWarning(weaveResult.WarningMessage, sequencePoint);
                        }

                        var realmAttributeNames = prop.CustomAttributes
                                                      .Select(a => a.AttributeType.Name)
                                                      .Intersect(RealmPropertyAttributes)
                                                      .OrderBy(a => a)
                                                      .Select(a => $"[{a.Replace("Attribute", string.Empty)}]");

                        if (realmAttributeNames.Any())
                        {
                            WriteError($"{type.Name}.{prop.Name} has {string.Join(", ", realmAttributeNames)} applied, but it's not persisted, so those attributes will be ignored.", sequencePoint);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var sequencePoint = prop.GetMethod.DebugInformation.SequencePoints.FirstOrDefault();
                WriteError(
                    $"Unexpected error caught weaving property '{type.Name}.{prop.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}",
                    sequencePoint);
            }
        }

        if (!persistedProperties.Any())
        {
            WriteError($"Class {type.Name} is a RealmObject but has no persisted properties.");
            return;
        }

        var pkProperty = persistedProperties.FirstOrDefault(p => p.IsPrimaryKey);
        if (type.IsEmbeddedObjectInheritor(_references) && pkProperty != null)
        {
            WriteError($"Class {type.Name} is an EmbeddedObject but has a primary key {pkProperty.Property.Name} defined.");
            return;
        }

        if (persistedProperties.Count(p => p.IsPrimaryKey) > 1)
        {
            WriteError($"Class {type.Name} has more than one property marked with [PrimaryKey].");
            return;
        }

        var objectConstructor = type.GetConstructors()
            .SingleOrDefault(c => c.Parameters.Count == 0 && !c.IsStatic);
        if (objectConstructor == null)
        {
            var nonDefaultConstructor = type.GetConstructors().First();
            var sequencePoint = nonDefaultConstructor.DebugInformation.SequencePoints.FirstOrDefault();
            WriteError($"Class {type.Name} must have a public constructor that takes no parameters.", sequencePoint);
            return;
        }

        var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
        objectConstructor.CustomAttributes.Add(preserveAttribute);
        preserveAttribute = new CustomAttribute(_references.PreserveAttribute_ConstructorWithParams); // recreate so has new instance
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, true)); // AllMembers
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, false)); // Conditional
        type.CustomAttributes.Add(preserveAttribute);
        WriteDebug($"Added [Preserve] to {type.Name} and its constructor.");

        var wovenAttribute = new CustomAttribute(_references.WovenAttribute_Constructor);
        TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor, persistedProperties);
        wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_references.System_Type, helperType));
        type.CustomAttributes.Add(wovenAttribute);
    }

    private WeaveResult WeaveProperty(PropertyDefinition prop, TypeDefinition type, Dictionary<string, Accessors> methodTable)
    {
        var columnName = prop.Name;
        var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
        if (mapToAttribute != null)
        {
            columnName = (string)mapToAttribute.ConstructorArguments[0].Value;
        }

        var backingField = prop.GetBackingField();
        var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
        if (isIndexed && !prop.IsIndexable(_references))
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}.");
        }

        var isPrimaryKey = prop.IsPrimaryKey(_references);
        if (isPrimaryKey && (!_primaryKeyTypes.Contains(prop.PropertyType.FullName)))
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [PrimaryKey] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}.");
        }

        var isRequired = prop.IsRequired(_references);
        if (isRequired &&
            !prop.IsIList(typeof(string)) &&
            !prop.IsNullable() &&
            prop.PropertyType.FullName != StringTypeName &&
            prop.PropertyType.FullName != ByteArrayTypeName)
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [Required] which is only allowed on strings or nullable scalar types, not on {prop.PropertyType.FullName}.");
        }

        if (!prop.IsAutomatic())
        {
            if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
            {
                return WeaveResult.Warning($"{type.Name}.{prop.Name} is not an automatic property but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.");
            }

            return WeaveResult.Skipped();
        }

        var backlinkAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "BacklinkAttribute");
        if (backlinkAttribute != null && !prop.IsIQueryable())
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} has [Backlink] applied, but is not IQueryable.");
        }

        if (_typeTable.ContainsKey(prop.PropertyType.FullName))
        {
            // If the property is automatic but doesn't have a setter, we should still ignore it.
            if (prop.SetMethod == null)
            {
                return WeaveResult.Skipped();
            }

            var realmAccessors = GetAccessors(prop.PropertyType, isPrimaryKey, methodTable);

            ReplaceGetter(prop, columnName, realmAccessors.Getter);
            ReplaceSetter(prop, backingField, columnName, realmAccessors.Setter);
        }
        else if (_primitiveValueTypes.TryGetValue(prop.PropertyType.FullName, out var propertyType))
        {
            if (prop.SetMethod == null)
            {
                return WeaveResult.Skipped();
            }

            var suffix = isPrimaryKey ? "Unique" : string.Empty;
            var typeId = $"{prop.PropertyType.FullName}{suffix}";

            if (!methodTable.TryGetValue(typeId, out var accessors))
            {
                var getter = new GenericInstanceMethod(_references.RealmObject_GetPrimitiveValue) { GenericArguments = { prop.PropertyType } };
                var setter = new GenericInstanceMethod(isPrimaryKey ? _references.RealmObject_SetPrimitiveValueUnique : _references.RealmObject_SetPrimitiveValue)
                {
                    GenericArguments = { prop.PropertyType }
                };
                methodTable[typeId] = accessors = new Accessors { Getter = getter, Setter = setter };
            }

            var propertyTypeRef = _references.GetPropertyTypeField(propertyType);
            ReplaceGetter(prop, columnName, accessors.Getter, propertyTypeRef);
            ReplaceSetter(prop, backingField, columnName, accessors.Setter, propertyTypeRef);
        }
        else if (_realmIntegerBackedTypes.Contains(prop.PropertyType.FullName))
        {
            // If the property is automatic but doesn't have a setter, we should still ignore it.
            if (prop.SetMethod == null)
            {
                return WeaveResult.Skipped();
            }

            if (!prop.PropertyType.IsRealmInteger(out var isNullable, out var integerType))
            {
                isNullable = prop.PropertyType.IsNullable();
                if (isNullable)
                {
                    var genericType = (GenericInstanceType)prop.PropertyType;
                    integerType = genericType.GenericArguments.Single();
                }
                else
                {
                    integerType = prop.PropertyType;
                }
            }

            var prefix = isNullable ? "Nullable" : string.Empty;
            var suffix = isPrimaryKey ? "Unique" : string.Empty;

            var typeId = $"{prefix}{integerType.FullName}{suffix}";
            if (!methodTable.TryGetValue(typeId, out var accessors))
            {
                var genericGetter = new MethodReference($"Get{prefix}RealmIntegerValue", ModuleDefinition.TypeSystem.Void, _references.RealmObjectBase)
                {
                    HasThis = true,
                    Parameters = { new ParameterDefinition(ModuleDefinition.TypeSystem.String) }
                };

                var getterGenericParameter = _references.GetRealmIntegerGenericParameter(genericGetter);
                genericGetter.GenericParameters.Add(getterGenericParameter);

                var returnType = _references.RealmIntegerOfT.MakeGenericInstanceType(getterGenericParameter);
                if (isNullable)
                {
                    returnType = _references.System_NullableOfT.MakeGenericInstanceType(returnType);
                }

                genericGetter.ReturnType = returnType;

                var genericSetter = new MethodReference($"Set{prefix}RealmIntegerValue{suffix}", ModuleDefinition.TypeSystem.Void, _references.RealmObjectBase)
                {
                    HasThis = true,
                    Parameters =
                    {
                        new ParameterDefinition(ModuleDefinition.TypeSystem.String),
                    }
                };

                var setterGenericParameter = _references.GetRealmIntegerGenericParameter(genericSetter);
                genericSetter.GenericParameters.Add(setterGenericParameter);

                var parameterType = _references.RealmIntegerOfT.MakeGenericInstanceType(setterGenericParameter);
                if (isNullable)
                {
                    parameterType = _references.System_NullableOfT.MakeGenericInstanceType(parameterType);
                }

                genericSetter.Parameters.Add(new ParameterDefinition(parameterType));

                var getter = new GenericInstanceMethod(genericGetter) { GenericArguments = { integerType } };
                var setter = new GenericInstanceMethod(genericSetter) { GenericArguments = { integerType } };
                methodTable[typeId] = accessors = new Accessors { Getter = getter, Setter = setter };
            }

            ReplaceRealmIntegerGetter(prop, columnName, accessors.Getter, isNullable, integerType);
            ReplaceRealmIntegerSetter(prop, backingField, columnName, accessors.Setter, isNullable, integerType);
        }
        else if (prop.IsIList())
        {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (!elementType.Resolve().IsValidRealmObjectBaseInheritor(_references) &&
                !_realmIntegerBackedTypes.Contains(elementType.FullName) &&
                !_typeTable.ContainsKey(elementType.FullName) &&
                !_primitiveValueTypes.ContainsKey(elementType.FullName))
            {
                return WeaveResult.Error($"{type.Name}.{prop.Name} is an IList but its generic type is {elementType.Name} which is not supported by Realm.");
            }

            if (prop.SetMethod != null)
            {
                return WeaveResult.Error($"{type.Name}.{prop.Name} has a setter but its type is a IList which only supports getters.");
            }

            var concreteListConstructor = _references.System_Collections_Generic_ListOfT_Constructor.MakeHostInstanceGeneric(elementType);

            // weaves list getter which also sets backing to List<T>, forcing it to accept us setting it post-init
            if (backingField is FieldDefinition backingDef)
            {
                backingDef.Attributes &= ~FieldAttributes.InitOnly;  // without a set; auto property has this flag we must clear
            }

            ReplaceListGetter(prop, backingField, columnName,
                              new GenericInstanceMethod(_references.RealmObject_GetListValue) { GenericArguments = { elementType } },
                              concreteListConstructor);
        }
        else if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
        {
            // with casting in the _realmObject methods, should just work
            ReplaceGetter(prop, columnName,
                          new GenericInstanceMethod(_references.RealmObject_GetObjectValue) { GenericArguments = { prop.PropertyType } });
            ReplaceSetter(prop, backingField, columnName,
                          new GenericInstanceMethod(_references.RealmObject_SetObjectValue) { GenericArguments = { prop.PropertyType } });
        }
        else if (prop.IsIQueryable())
        {
            if (backlinkAttribute == null)
            {
                return WeaveResult.Error($"{type.Name}.{prop.Name} is IQueryable, but doesn't have [Backlink] applied.");
            }

            if (prop.SetMethod != null)
            {
                return WeaveResult.Error($"{type.Name}.{prop.Name} has a setter but also has [Backlink] applied, which only supports getters.");
            }

            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            var inversePropertyName = (string)backlinkAttribute.ConstructorArguments[0].Value;
            var inverseProperty = elementType.Resolve().Properties.SingleOrDefault(p => p.Name == inversePropertyName);

            if (inverseProperty == null || (!inverseProperty.PropertyType.IsSameAs(type) && !inverseProperty.IsIList(type)))
            {
                return WeaveResult.Error($"The property '{elementType.Name}.{inversePropertyName}' does not constitute a link to '{type.Name}' as described by '{type.Name}.{prop.Name}'.");
            }

            if (backingField is FieldDefinition backingDef)
            {
                // without a set; auto property has this flag we must clear
                backingDef.Attributes &= ~FieldAttributes.InitOnly;
            }

            ReplaceBacklinksGetter(prop, backingField, columnName, elementType);
        }
        else if (prop.SetMethod == null)
        {
            return WeaveResult.Skipped();
        }
        else if (prop.PropertyType.FullName == "System.DateTime")
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is a DateTime which is not supported - use DateTimeOffset instead.");
        }
        else if (prop.PropertyType.FullName == "System.Nullable`1<System.DateTime>")
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is a DateTime? which is not supported - use DateTimeOffset? instead.");
        }
        else
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is a '{prop.PropertyType}' which is not yet supported.");
        }

        var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
        prop.CustomAttributes.Add(preserveAttribute);

        var wovenPropertyAttribute = new CustomAttribute(_references.WovenPropertyAttribute_Constructor);
        prop.CustomAttributes.Add(wovenPropertyAttribute);

        Debug.WriteLine(string.Empty);

        var primaryKeyMsg = isPrimaryKey ? "[PrimaryKey]" : string.Empty;
        var indexedMsg = isIndexed ? "[Indexed]" : string.Empty;
        WriteDebug($"Woven {type.Name}.{prop.Name} as a {prop.PropertyType.FullName} {primaryKeyMsg} {indexedMsg}.");
        return WeaveResult.Success(prop, backingField, isPrimaryKey);
    }

    private Accessors GetAccessors(TypeReference backingType, bool isPrimaryKey, IDictionary<string, Accessors> methodTable)
    {
        var typeId = backingType.FullName + (isPrimaryKey ? " unique" : string.Empty);

        if (!_typeTable.TryGetValue(backingType.FullName, out var typeName))
        {
            throw new NotSupportedException($"Unable to find {backingType.FullName} in _typeTable. Please report that to help@realm.io");
        }

        if (!methodTable.TryGetValue(typeId, out var realmAccessors))
        {
            var getter = new MethodReference($"Get{typeName}Value", backingType, _references.RealmObjectBase)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(ModuleDefinition.TypeSystem.String) },
            };

            var setter = new MethodReference($"Set{typeName}Value" + (isPrimaryKey ? "Unique" : string.Empty), ModuleDefinition.TypeSystem.Void, _references.RealmObjectBase)
            {
                HasThis = true,
                Parameters =
                {
                    new ParameterDefinition(ModuleDefinition.TypeSystem.String),
                    new ParameterDefinition(backingType)
                }
            };

            methodTable[typeId] = realmAccessors = new Accessors { Getter = getter, Setter = setter };
        }

        return realmAccessors;
    }

    private void ReplaceGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference, FieldReference propertyTypeRef = null)
    {
        //// A synthesized property getter looks like this:
        ////   0: ldarg.0
        ////   1: ldfld <backingField>
        ////   2: ret
        //// We want to change it so it looks like this:
        ////   0: ldarg.0
        ////   1: call Realms.RealmObject.get_IsManaged
        ////   2: brfalse.s 7
        ////   3: ldarg.0
        ////   4: ldstr <columnName>
        ////   5: call Realms.RealmObject.GetValue<T>
        ////   6: ret
        ////   7: ldarg.0
        ////   8: ldfld <backingField>
        ////   9: ret
        //// This is roughly equivalent to:
        ////   if (!base.IsManaged) return this.<backingField>;
        ////   return base.GetValue<T>(<columnName>);

        var start = prop.GetMethod.Body.Instructions.First();
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
        il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName)); // [stack = this | name ]
        if (propertyTypeRef != null)
        {
            il.InsertBefore(start, il.Create(OpCodes.Ldc_I4, (int)(byte)propertyTypeRef.Resolve().Constant));
        }

        il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
    }

    private void ReplaceRealmIntegerGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference, bool isNullable, TypeReference integerType)
    {
        var start = prop.GetMethod.Body.Instructions.First();
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
        il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));

        if (!prop.PropertyType.IsRealmInteger(out _, out _))
        {
            var convert = _references.RealmIntegerOfT_ConvertToT.MakeHostInstanceGeneric(integerType);
            if (isNullable)
            {
                var nullableRealmIntegerType = new GenericInstanceType(_references.System_NullableOfT)
                {
                    GenericArguments =
                    {
                        new GenericInstanceType(_references.RealmIntegerOfT)
                        {
                            GenericArguments = { integerType }
                        }
                    }
                };

                var localRealmIntegerVariable = new VariableDefinition(nullableRealmIntegerType);
                prop.GetMethod.Body.Variables.Add(localRealmIntegerVariable);

                var localIntegerVariable = new VariableDefinition(prop.PropertyType);
                prop.GetMethod.Body.Variables.Add(localIntegerVariable);

                il.InsertBefore(start, il.Create(OpCodes.Stloc_0));
                il.InsertBefore(start, il.Create(OpCodes.Ldloca_S, localRealmIntegerVariable));

                var hasValue = new MethodReference("get_HasValue", ModuleDefinition.TypeSystem.Boolean, nullableRealmIntegerType) { HasThis = true };
                il.InsertBefore(start, il.Create(OpCodes.Call, hasValue));

                var hasValueBranch = il.Create(OpCodes.Ldloca_S, localRealmIntegerVariable);

                il.InsertBefore(start, il.Create(OpCodes.Brtrue_S, hasValueBranch));
                il.InsertBefore(start, il.Create(OpCodes.Ldloca_S, localIntegerVariable));
                il.InsertBefore(start, il.Create(OpCodes.Initobj, prop.PropertyType));
                il.InsertBefore(start, il.Create(OpCodes.Ldloc_1));
                il.InsertBefore(start, il.Create(OpCodes.Ret));

                il.InsertBefore(start, hasValueBranch);

                var concreteRealmIntegerType = _references.RealmIntegerOfT.MakeGenericInstanceType(integerType);
                var getValueOrDefault = _references.System_NullableOfT_GetValueOrDefault.MakeHostInstanceGeneric(concreteRealmIntegerType);
                il.InsertBefore(start, il.Create(OpCodes.Call, getValueOrDefault));
                il.InsertBefore(start, il.Create(OpCodes.Call, convert));

                var ctor = _references.System_NullableOfT_Ctor.MakeHostInstanceGeneric(integerType);
                il.InsertBefore(start, il.Create(OpCodes.Newobj, ctor));
            }
            else
            {
                il.InsertBefore(start, il.Create(OpCodes.Call, convert));
            }
        }

        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
    }

    // WARNING
    // This code setting the backing field only works if the field is settable after init
    // if you don't have an automatic set; on the property, it shows in the debugger with
    //         Attributes    Private | InitOnly    Mono.Cecil.FieldAttributes
    private void ReplaceListGetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference getListValueReference, MethodReference listConstructor)
    {
        //// A synthesized property getter looks like this:
        ////   0: ldarg.0  // load the this pointer
        ////   1: ldfld <backingField>
        ////   2: ret
        //// We want to change it so it looks somewhat like this, in C#
        ////
        ////  if (<backingField> == null)
        ////  {
        ////     if (IsManaged)
        ////           <backingField> = GetListObject<T>(<columnName>);
        ////     else
        ////           <backingField> = new List<T>();
        ////  }
        ////  // original auto-generated getter starts here
        ////  return <backingField>; // supplied by the generated getter OR RealmObject._CopyDataFromBackingFields

        var start = prop.GetMethod.Body.Instructions.First();  // this is a label for return <backingField>;
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for field ref [ -> this]
        il.InsertBefore(start, il.Create(OpCodes.Ldfld, backingField)); // [ this -> field]
        il.InsertBefore(start, il.Create(OpCodes.Ldnull)); // [field -> field, null]
        il.InsertBefore(start, il.Create(OpCodes.Ceq));  // [field, null -> bool result]
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));  // []

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for stfld in both branches [ -> this ]
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for call [ this -> this, this]
        il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));  // [ this, this -> this,  isManaged ]

        // push in the label then go relative to that - so we can forward-ref the lable insert if/else blocks backwards
        var labelElse = il.Create(OpCodes.Nop);  // [this]
        il.InsertBefore(start, labelElse); // else
        il.InsertBefore(start, il.Create(OpCodes.Newobj, listConstructor)); // [this ->  this, listRef ]
        il.InsertBefore(start, il.Create(OpCodes.Stfld, backingField));  // [this, listRef -> ]

        // fall through to start to read it back from backing field and return
        // if block before else now gets inserted
        il.InsertBefore(labelElse, il.Create(OpCodes.Brfalse_S, labelElse));  // [this,  isManaged -> this]
        il.InsertBefore(labelElse, il.Create(OpCodes.Ldarg_0)); // this for call [ this -> this, this ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Ldstr, columnName));  // [this, this -> this, this, name ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Call, getListValueReference)); // [this, this, name -> this, listRef ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Stfld, backingField)); // [this, listRef -> ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Br_S, start));

        // note that we do NOT insert a ret, unlike other weavers, as usual path branches and
        // FALL THROUGH to return the backing field.

        // Let Cecil optimize things for us.
        // TODO prop.SetMethod.Body.OptimizeMacros();
        Debug.Write("[get list] ");
    }

    // WARNING
    // This code setting the backing field only works if the field is settable after init
    // if you don't have an automatic set; on the property, it shows in the debugger with
    //         Attributes    Private | InitOnly    Mono.Cecil.FieldAttributes
    private void ReplaceBacklinksGetter(PropertyDefinition prop, FieldReference backingField, string columnName, TypeReference elementType)
    {
        //// A synthesized property getter looks like this:
        ////   0: ldarg.0  // load the this pointer
        ////   1: ldfld <backingField>
        ////   2: ret
        //// We want to change it so it looks somewhat like this, in C#
        ////
        ////  if (<backingField> == null)
        ////  {
        ////     if (IsManaged)
        ////           <backingField> = GetBacklinks<T>(<columnName>);
        ////     else
        ////           <backingField> = new Enumerable.Empty<T>.AsQueryable();
        ////  }
        ////  // original auto-generated getter starts here
        ////  return <backingField>; // supplied by the generated getter OR RealmObject._CopyDataFromBackingFields

        var start = prop.GetMethod.Body.Instructions.First();  // this is a label for return <backingField>;
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for field ref [ -> this]
        il.InsertBefore(start, il.Create(OpCodes.Ldfld, backingField)); // [ this -> field]
        il.InsertBefore(start, il.Create(OpCodes.Brtrue_S, start));  // []

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for stfld in both branches [ -> this ]
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for call [ this -> this, this]
        il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));  // [ this, this -> this,  isManaged ]

        // push in the label then go relative to that - so we can forward-ref the lable insert if/else blocks backwards
        var labelElse = il.Create(OpCodes.Nop);  // [this]
        il.InsertBefore(start, labelElse); // else
        il.InsertBefore(start, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.System_Linq_Enumerable_Empty) { GenericArguments = { elementType } })); // [this, enumerable]
        il.InsertBefore(start, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.System_Linq_Queryable_AsQueryable) { GenericArguments = { elementType } })); // [this, queryable]
        il.InsertBefore(start, il.Create(OpCodes.Stfld, backingField));  // [this, queryable -> ]

        // fall through to start to read it back from backing field and return
        // if block before else now gets inserted
        il.InsertBefore(labelElse, il.Create(OpCodes.Brfalse_S, labelElse));  // [this,  isManaged -> this]
        il.InsertBefore(labelElse, il.Create(OpCodes.Ldarg_0)); // this for call [ this -> this, this ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Ldstr, columnName));  // [this, this -> this, this, name ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.RealmObject_GetBacklinks) { GenericArguments = { elementType } })); // [this, this, name -> this, queryable ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Stfld, backingField)); // [this, queryable -> ]
        il.InsertBefore(labelElse, il.Create(OpCodes.Br_S, start));

        // note that we do NOT insert a ret, unlike other weavers, as usual path branches and
        // FALL THROUGH to return the backing field.

        // Let Cecil optimize things for us.
        // TODO prop.SetMethod.Body.OptimizeMacros();
        Debug.Write("[get list] ");
    }

    private void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference, FieldReference propertyTypeRef = null)
    {
        //// A synthesized property setter looks like this:
        ////   0: ldarg.0
        ////   1: ldarg.1
        ////   2: stfld <backingField>
        ////   3: ret
        ////
        //// We want to change it so it looks like this:
        ////   0: ldarg.0
        ////   1: call Realms.RealmObject.get_IsManaged
        ////   2: brfalse.s 8
        ////   3: ldarg.0
        ////   4: ldstr <columnName>
        ////   5: ldarg.1
        ////   6: call Realms.RealmObject.SetValue<T>
        ////   7: ret
        ////   8: ldarg.0
        ////   9: ldarg.1
        ////   10: stfld <backingField>
        ////   11: ret
        ////
        //// This is roughly equivalent to:
        ////   if (!base.IsManaged) this.<backingField> = value;
        ////   else base.SetValue<T>(<columnName>, value);

        if (setValueReference == null)
        {
            throw new ArgumentNullException(nameof(setValueReference));
        }

        // Whilst we're only targetting auto-properties here, someone like PropertyChanged.Fody
        // may have already come in and rewritten our IL. Lets clear everything and start from scratch.
        var il = prop.SetMethod.Body.GetILProcessor();
        prop.SetMethod.Body.Instructions.Clear();
        prop.SetMethod.Body.Variables.Clear();

        // While we can tidy up PropertyChanged.Fody IL if we're ran after it, we can't do a heck of a lot
        // if they're the last one in.
        // To combat this, we'll check if the PropertyChanged assembly is available, and if so, attribute
        // the property such that PropertyChanged.Fody won't touch it.
        if (_references.PropertyChanged_DoNotNotifyAttribute_Constructor != null)
        {
            prop.CustomAttributes.Add(new CustomAttribute(_references.PropertyChanged_DoNotNotifyAttribute_Constructor));
        }

        var managedSetStart = il.Create(OpCodes.Ldarg_0);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
        il.Append(il.Create(OpCodes.Brtrue_S, managedSetStart));

        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Stfld, backingField));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldstr, prop.Name));
        il.Append(il.Create(OpCodes.Call, _references.RealmObject_RaisePropertyChanged));
        il.Append(il.Create(OpCodes.Ret));

        il.Append(managedSetStart);
        il.Append(il.Create(OpCodes.Ldstr, columnName));
        il.Append(il.Create(OpCodes.Ldarg_1));
        if (propertyTypeRef != null)
        {
            il.Append(il.Create(OpCodes.Ldc_I4, (int)(byte)propertyTypeRef.Resolve().Constant));
        }

        il.Append(il.Create(OpCodes.Call, setValueReference));
        il.Append(il.Create(OpCodes.Ret));

        Debug.Write("[set] ");
    }

    private void ReplaceRealmIntegerSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference, bool isNullable, TypeReference integerType)
    {
        // Whilst we're only targetting auto-properties here, someone like PropertyChanged.Fody
        // may have already come in and rewritten our IL. Lets clear everything and start from scratch.
        var il = prop.SetMethod.Body.GetILProcessor();
        prop.SetMethod.Body.Instructions.Clear();
        prop.SetMethod.Body.Variables.Clear();

        // While we can tidy up PropertyChanged.Fody IL if we're ran after it, we can't do a heck of a lot
        // if they're the last one in.
        // To combat this, we'll check if the PropertyChanged assembly is available, and if so, attribute
        // the property such that PropertyChanged.Fody won't touch it.
        if (_references.PropertyChanged_DoNotNotifyAttribute_Constructor != null)
        {
            prop.CustomAttributes.Add(new CustomAttribute(_references.PropertyChanged_DoNotNotifyAttribute_Constructor));
        }

        var managedSetStart = il.Create(OpCodes.Ldarg_0);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
        il.Append(il.Create(OpCodes.Brtrue_S, managedSetStart));

        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Stfld, backingField));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldstr, prop.Name));
        il.Append(il.Create(OpCodes.Call, _references.RealmObject_RaisePropertyChanged));
        il.Append(il.Create(OpCodes.Ret));

        il.Append(managedSetStart);
        il.Append(il.Create(OpCodes.Ldstr, columnName));
        il.Append(il.Create(OpCodes.Ldarg_1));

        var callSetter = il.Create(OpCodes.Call, setValueReference);
        il.Append(callSetter);

        if (!prop.PropertyType.IsRealmInteger(out _, out _))
        {
            var convert = _references.RealmIntegerOfT_ConvertFromT.MakeHostInstanceGeneric(integerType);
            if (isNullable)
            {
                var realmIntegerType = _references.RealmIntegerOfT.MakeGenericInstanceType(integerType);
                var nullableRealmIntegerType = _references.System_NullableOfT.MakeGenericInstanceType(realmIntegerType);

                var localIntegerVariable = new VariableDefinition(prop.PropertyType);
                prop.SetMethod.Body.Variables.Add(localIntegerVariable);

                var localRealmIntegerVariable = new VariableDefinition(nullableRealmIntegerType);
                prop.SetMethod.Body.Variables.Add(localRealmIntegerVariable);

                il.InsertBefore(callSetter, il.Create(OpCodes.Stloc_0));
                il.InsertBefore(callSetter, il.Create(OpCodes.Ldloca_S, localIntegerVariable));

                var hasValue = new MethodReference("get_HasValue", ModuleDefinition.TypeSystem.Boolean, prop.PropertyType) { HasThis = true };
                il.InsertBefore(callSetter, il.Create(OpCodes.Call, hasValue));

                var hasValueBranch = il.Create(OpCodes.Ldloca_S, localIntegerVariable);
                il.InsertBefore(callSetter, il.Create(OpCodes.Brtrue_S, hasValueBranch));
                il.InsertBefore(callSetter, il.Create(OpCodes.Ldloca_S, localRealmIntegerVariable));
                il.InsertBefore(callSetter, il.Create(OpCodes.Initobj, nullableRealmIntegerType));
                il.InsertBefore(callSetter, il.Create(OpCodes.Ldloc_1));
                il.InsertBefore(callSetter, il.Create(OpCodes.Br_S, callSetter));

                il.InsertBefore(callSetter, hasValueBranch);
                var getValueOrDefault = _references.System_NullableOfT_GetValueOrDefault.MakeHostInstanceGeneric(integerType);
                il.InsertBefore(callSetter, il.Create(OpCodes.Call, getValueOrDefault));
                il.InsertBefore(callSetter, il.Create(OpCodes.Call, convert));

                var nullableRealmIntegerCtor = _references.System_NullableOfT_Ctor.MakeHostInstanceGeneric(realmIntegerType);
                il.InsertBefore(callSetter, il.Create(OpCodes.Newobj, nullableRealmIntegerCtor));
            }
            else
            {
                il.InsertBefore(callSetter, il.Create(OpCodes.Call, convert));
            }
        }

        il.Append(il.Create(OpCodes.Ret));

        Debug.Write("[set] ");
    }

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor, List<WeaveResult> properties)
    {
        var helperType = new TypeDefinition(null, "RealmHelper",
                                            TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                            ModuleDefinition.TypeSystem.Object);

        helperType.Interfaces.Add(new InterfaceImplementation(_references.IRealmObjectHelper));

        var createInstance = new MethodDefinition("CreateInstance", DefaultMethodAttributes, _references.RealmObjectBase);
        {
            var il = createInstance.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, objectConstructor);
            il.Emit(OpCodes.Ret);
        }

        helperType.Methods.Add(createInstance);

        var copyToRealm = new MethodDefinition("CopyToRealm", DefaultMethodAttributes, ModuleDefinition.TypeSystem.Void);
        {
            // This roughly translates to
            /*
                var castInstance = (ObjectType)instance;

                *foreach* non-list woven property in castInstance's schema
                *if* castInstace.field is a RealmObject descendant
                    castInstance.Realm.Add(castInstance.field, update);
                    castInstance.Field = castInstance.field;
                *else if* property is PK
                    *do nothing*
                *else if* property is [Required] or nullable
                    castInstance.Property = castInstance.Field;
                *else*
                    if (!skipDefaults || castInstance.field != default(fieldType))
                    {
                        castInstance.Property = castInstance.Field;
                    }

                *foreach* list woven property in castInstance's schema
                var list = castInstance.field;
                castInstance.field = null;
                if (!skipDefaults)
                {
                    castInstance.Property.Clear();
                }
                if (list != null)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        castInstance.Realm.Add(list[i], update);
                        castInstance.Property.Add(list[i]);
                    }
                }
            */

            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, _references.RealmObjectBase);
            copyToRealm.Parameters.Add(instanceParameter);

            var updateParameter = new ParameterDefinition("update", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean);
            copyToRealm.Parameters.Add(updateParameter);

            var skipDefaultsParameter = new ParameterDefinition("skipDefaults", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean);
            copyToRealm.Parameters.Add(skipDefaultsParameter);

            copyToRealm.Body.Variables.Add(new VariableDefinition(realmObjectType));

            byte currentStloc = 1;

            foreach (var prop in properties.Where(p => p.Property.IsIList()))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(prop.Field.FieldType)));
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.TypeSystem.Int32));
            }

            var il = copyToRealm.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Castclass, ModuleDefinition.ImportReference(realmObjectType)));
            il.Append(il.Create(OpCodes.Stloc_0));

            foreach (var prop in properties.Where(p => !p.Property.IsPrimaryKey(_references)))
            {
                var property = prop.Property;
                var field = prop.Field;

                if (property.SetMethod != null)
                {
                    // If the property is RealmObject, we want the following code to execute:
                    // if (castInstance.field != null)
                    // {
                    //     castInstance.Realm.Add(castInstance.field, update)
                    // }
                    // castInstance.Property = castInstance.field;
                    //
                    // *addPlaceholder* will be the Brfalse instruction that will skip the call to Add if the field is null.
                    Instruction addPlaceholder = null;

                    // We can skip setting properties that have their default values unless:
                    var shouldSetAlways = property.IsNullable() || // The property is nullable - those should be set explicitly to null
                                          property.IsRequired(_references) || // Needed for validating that the property is not null (string)
                                          property.IsDateTimeOffset() || // Core's DateTimeOffset property defaults to 1970-1-1, so we should override
                                          property.PropertyType.IsRealmInteger(out _, out _) || // structs are not implicitly falsy/truthy so the IL is significantly different; we can optimize this case in the future
                                          property.IsDecimal() ||
                                          property.IsDecimal128();

                    // If the property is non-nullable, we want the following code to execute:
                    // if (!skipDefaults || castInstance.field != default(fieldType))
                    // {
                    //     castInstance.Property = castInstance.field
                    // }
                    //
                    // *updatePlaceholder* will be the Brtrue instruction that will skip the default check and move to the
                    // property setting logic. The default check branching instruction is inserted above the *setStartPoint*
                    // instruction later on.
                    Instruction skipDefaultsPlaceholder = null;
                    if (property.ContainsRealmObject(_references))
                    {
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        addPlaceholder = il.Create(OpCodes.Nop);
                        il.Append(addPlaceholder);

                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_Realm));
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));
                        il.Append(il.Create(OpCodes.Ldarg_2));
                        il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.Realm_Add) { GenericArguments = { field.FieldType } }));
                        il.Append(il.Create(OpCodes.Pop));
                    }
                    else if (!shouldSetAlways)
                    {
                        il.Append(il.Create(OpCodes.Ldarg_3));
                        skipDefaultsPlaceholder = il.Create(OpCodes.Nop);
                        il.Append(skipDefaultsPlaceholder);

                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        if (property.IsSingle())
                        {
                            il.Append(il.Create(OpCodes.Ldc_R4, 0f));
                        }
                        else if (property.IsDouble())
                        {
                            il.Append(il.Create(OpCodes.Ldc_R8, 0.0));
                        }
                    }

                    var setStartPoint = il.Create(OpCodes.Ldloc_0);
                    il.Append(setStartPoint);
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Call, property.SetMethod));

                    var setEndPoint = il.Create(OpCodes.Nop);
                    il.Append(setEndPoint);

                    if (property.ContainsRealmObject(_references))
                    {
                        if (addPlaceholder != null)
                        {
                            il.Replace(addPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                        }
                    }
                    else if (!shouldSetAlways)
                    {
                        // Branching instruction to check if we're trying to set the default value of a property.
                        if (property.IsSingle() || property.IsDouble())
                        {
                            il.InsertBefore(setStartPoint, il.Create(OpCodes.Beq_S, setEndPoint));
                        }
                        else
                        {
                            il.InsertBefore(setStartPoint, il.Create(OpCodes.Brfalse_S, setEndPoint));
                        }

                        if (skipDefaultsPlaceholder != null)
                        {
                            il.Replace(skipDefaultsPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                        }
                    }
                }
                else if (property.IsIList())
                {
                    var elementType = ((GenericInstanceType)property.PropertyType).GenericArguments.Single();
                    var propertyGetterMethodReference = ModuleDefinition.ImportReference(property.GetMethod);

                    var iteratorStLoc = (byte)(currentStloc + 1);

                    // if (!skipDefaults ||
                    var skipDefaultsCheck = il.Create(OpCodes.Ldarg_3);
                    il.Append(skipDefaultsCheck);

                    // castInstance.field != null)
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    var fieldNullCheck = il.Create(OpCodes.Ldfld, field);
                    il.Append(fieldNullCheck);

                    // var list = castInstance.field;
                    // castInstance.field = null;
                    var setterStart = il.Create(OpCodes.Ldloc_0);
                    il.Append(setterStart);
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Stloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldnull));
                    il.Append(il.Create(OpCodes.Stfld, field));

                    // if (!skipDefaults)
                    var clearSkipPlaceholder = il.Create(OpCodes.Ldarg_3);
                    il.Append(clearSkipPlaceholder);

                    // castInstance.Property.Clear();
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Call, propertyGetterMethodReference));
                    il.Append(il.Create(OpCodes.Callvirt, _references.ICollectionOfT_Clear.MakeHostInstanceGeneric(elementType)));

                    // if (list == null)
                    var localListVarNullCheck = il.Create(OpCodes.Ldloc_S, currentStloc);
                    il.Append(localListVarNullCheck);

                    // if (skipDefaults), skip List.Clear and jump to checking the list == null
                    il.InsertAfter(clearSkipPlaceholder, il.Create(OpCodes.Brtrue_S, localListVarNullCheck));

                    il.Append(il.Create(OpCodes.Ldc_I4_0));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cyclePlaceholder = il.Create(OpCodes.Nop);
                    il.Append(cyclePlaceholder);

                    var cycleStart = il.Create(OpCodes.Ldloc_0);
                    il.Append(cycleStart);

                    if (elementType.Resolve().IsRealmObjectInheritor(_references))
                    {
                        // castInstance.Realm.Add(list[i], update)
                        il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_Realm));
                        il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                        il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                        il.Append(il.Create(OpCodes.Callvirt, _references.IListOfT_get_Item.MakeHostInstanceGeneric(elementType)));
                        il.Append(il.Create(OpCodes.Ldarg_2));
                        il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.Realm_Add) { GenericArguments = { elementType } }));
                        il.Append(il.Create(OpCodes.Pop));
                        il.Append(il.Create(OpCodes.Ldloc_0));
                    }

                    // castInstance.Property.Add(list[i]);
                    il.Append(il.Create(OpCodes.Call, propertyGetterMethodReference));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Callvirt, _references.IListOfT_get_Item.MakeHostInstanceGeneric(elementType)));
                    il.Append(il.Create(OpCodes.Callvirt, _references.ICollectionOfT_Add.MakeHostInstanceGeneric(elementType)));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Ldc_I4_1));
                    il.Append(il.Create(OpCodes.Add));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cycleLabel = il.Create(OpCodes.Nop);
                    il.Append(cycleLabel);
                    il.Replace(cyclePlaceholder, il.Create(OpCodes.Br_S, cycleLabel));

                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Callvirt, _references.ICollectionOfT_get_Count.MakeHostInstanceGeneric(elementType)));
                    il.Append(il.Create(OpCodes.Blt_S, cycleStart));

                    var setterEnd = il.Create(OpCodes.Nop);
                    il.Append(setterEnd);
                    il.InsertAfter(fieldNullCheck, il.Create(OpCodes.Brfalse_S, setterEnd));
                    il.InsertAfter(localListVarNullCheck, il.Create(OpCodes.Brfalse_S, setterEnd));
                    il.InsertAfter(skipDefaultsCheck, il.Create(OpCodes.Brfalse_S, setterStart));

                    currentStloc += 2;
                }
                else if (property.IsIQueryable())
                {
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldnull));
                    il.Append(il.Create(OpCodes.Stfld, field));
                }
                else
                {
                    var sequencePoint = property.GetMethod.DebugInformation.SequencePoints.FirstOrDefault();
                    WriteError($"{realmObjectType.Name}.{property.Name} does not have a setter and is not an IList. This is an error in Realm, so please file a bug report.", sequencePoint);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        copyToRealm.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));
        helperType.Methods.Add(copyToRealm);

        var getPrimaryKeyValue = new MethodDefinition("TryGetPrimaryKeyValue", DefaultMethodAttributes, ModuleDefinition.TypeSystem.Boolean);
        {
            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, _references.RealmObject);
            getPrimaryKeyValue.Parameters.Add(instanceParameter);

            var valueParameter = new ParameterDefinition("value", ParameterAttributes.Out, new ByReferenceType(ModuleDefinition.TypeSystem.Object))
            {
                IsOut = true
            };
            getPrimaryKeyValue.Parameters.Add(valueParameter);

            getPrimaryKeyValue.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(realmObjectType)));

            var il = getPrimaryKeyValue.Body.GetILProcessor();
            var pkProperty = properties.FirstOrDefault(p => p.IsPrimaryKey);

            if (pkProperty != null)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, ModuleDefinition.ImportReference(realmObjectType));
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(pkProperty.Property.GetMethod));
                if (!pkProperty.Property.IsString())
                {
                    il.Emit(OpCodes.Box, pkProperty.Property.PropertyType);
                }

                il.Emit(OpCodes.Stind_Ref);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stind_Ref);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);
            }
        }

        getPrimaryKeyValue.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));
        helperType.Methods.Add(getPrimaryKeyValue);

        const MethodAttributes CtorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var ctor = new MethodDefinition(".ctor", CtorAttributes, ModuleDefinition.TypeSystem.Void);
        {
            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, new MethodReference(".ctor", ModuleDefinition.TypeSystem.Void, ModuleDefinition.TypeSystem.Object) { HasThis = true });
            il.Emit(OpCodes.Ret);
        }

        var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
        ctor.CustomAttributes.Add(preserveAttribute);

        helperType.Methods.Add(ctor);

        realmObjectType.NestedTypes.Add(helperType);

        return helperType;
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System";
        yield return "System.Runtime";
        yield return "System.Core";
        yield return "netstandard";
        yield return "System.Collections";
        yield return "System.ObjectModel";
        yield return "System.Threading";
    }

    private class WeaveResult
    {
        public static WeaveResult Success(PropertyDefinition property, FieldReference field, bool isPrimaryKey)
        {
            return new WeaveResult(property, field, isPrimaryKey);
        }

        public static WeaveResult Warning(string warning)
        {
            return new WeaveResult(warning: warning);
        }

        public static WeaveResult Error(string error)
        {
            return new WeaveResult(error: error);
        }

        public static WeaveResult Skipped()
        {
            return new WeaveResult();
        }

        public string ErrorMessage { get; }

        public string WarningMessage { get; }

        public bool Woven { get; }

        public PropertyDefinition Property { get; }

        public FieldReference Field { get; }

        public bool IsPrimaryKey { get; }

        private WeaveResult(PropertyDefinition property, FieldReference field, bool isPrimaryKey)
        {
            Property = property;
            Field = field;
            IsPrimaryKey = isPrimaryKey;
            Woven = true;
        }

        private WeaveResult(string error = null, string warning = null)
        {
            ErrorMessage = error;
            WarningMessage = warning;
        }
    }

    private struct Accessors
    {
        public MethodReference Getter;

        public MethodReference Setter;
    }
}
