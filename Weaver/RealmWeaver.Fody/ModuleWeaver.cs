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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
public class ModuleWeaver
{
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
    internal const string DateTimeOffsetTypeName = "System.DateTimeOffset";
    internal const string NullableCharTypeName = "System.Nullable`1<System.Char>";
    internal const string NullableByteTypeName = "System.Nullable`1<System.Byte>";
    internal const string NullableInt16TypeName = "System.Nullable`1<System.Int16>";
    internal const string NullableInt32TypeName = "System.Nullable`1<System.Int32>";
    internal const string NullableInt64TypeName = "System.Nullable`1<System.Int64>";
    internal const string NullableSingleTypeName = "System.Nullable`1<System.Single>";
    internal const string NullableDoubleTypeName = "System.Nullable`1<System.Double>";
    internal const string NullableBooleanTypeName = "System.Nullable`1<System.Boolean>";
    internal const string NullableDateTimeOffsetTypeName = "System.Nullable`1<System.DateTimeOffset>";
    
    // Will log an informational message to MSBuild - see https://github.com/Fody/Fody/wiki/ModuleWeaver for details
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public Action<string> LogDebug { get; set; } = m => { };  // MessageImportance.Normal, included in verbosity Detailed

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public Action<string> LogInfo { get; set; } = m => { };  // MessageImportance.High

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public Action<string, SequencePoint> LogWarningPoint { get; set; } = (m, p) => { };

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public Action<string> LogError { get; set; } = m => { };

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public Action<string, SequencePoint> LogErrorPoint { get; set; } = (m, p) => { };

    // An instance of Mono.Cecil.ModuleDefinition for processing
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public ModuleDefinition ModuleDefinition { get; set; }

    private static readonly Dictionary<string, string> _typeTable = new Dictionary<string, string>
    {
        { StringTypeName, "String" },
        { CharTypeName, "Char" },
        { ByteTypeName, "Byte" },
        { Int16TypeName, "Int16" },
        { Int32TypeName, "Int32" },
        { Int64TypeName, "Int64" },
        { SingleTypeName, "Single" },
        { DoubleTypeName, "Double" },
        { BooleanTypeName, "Boolean" },
        { DateTimeOffsetTypeName, "DateTimeOffset" },
        { ByteArrayTypeName, "ByteArray" },
        { NullableCharTypeName, "NullableChar" },
        { NullableByteTypeName, "NullableByte" },
        { NullableInt16TypeName, "NullableInt16" },
        { NullableInt32TypeName, "NullableInt32" },
        { NullableInt64TypeName, "NullableInt64" },
        { NullableSingleTypeName, "NullableSingle" },
        { NullableDoubleTypeName, "NullableDouble" },
        { NullableBooleanTypeName, "NullableBoolean" },
        { NullableDateTimeOffsetTypeName, "NullableDateTimeOffset" }
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

    private static readonly IEnumerable<string> _indexableTypes = new[]
    {
        StringTypeName,
        CharTypeName,
        ByteTypeName,
        Int16TypeName,
        Int32TypeName,
        Int64TypeName,
        BooleanTypeName,
        DateTimeOffsetTypeName
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
        foreach (var type in ModuleDefinition.GetTypes().Where(t => t.IsDescendedFrom(_references.RealmObject)))
        {
            if (type.BaseType.IsSameAs(_references.RealmObject))
            {
                yield return type;
            }
            else
            {
                LogErrorPoint($"The type {type.FullName} indirectly inherits from RealmObject which is not supported.", type.GetConstructors().FirstOrDefault()?.Body?.Instructions?.First()?.SequencePoint);
            }
        }
    }

    public void Execute()
    {
        // UNCOMMENT THIS DEBUGGER LAUNCH TO BE ABLE TO RUN A SEPARATE VS INSTANCE TO DEBUG WEAVING WHILST BUILDING
        // Debugger.Launch();  

        Debug.WriteLine("Weaving file: " + ModuleDefinition.FullyQualifiedName);

        var submitAnalytics = System.Threading.Tasks.Task.Factory.StartNew(() =>
        {
            var analytics = new RealmWeaver.Analytics(ModuleDefinition);
            try
            {
                analytics.SubmitAnalytics();
            }
            catch (Exception e)
            {
                LogDebug("Error submitting analytics: " + e.Message);
            }
        });

        _references = RealmWeaver.ImportedReferences.Create(ModuleDefinition);

        // Cache of getter and setter methods for the various types.
        var methodTable = new Dictionary<string, Tuple<MethodReference, MethodReference>>();

        foreach (var type in GetMatchingTypes())
        {
            try
            {
                WeaveType(type, methodTable);
            }
            catch (Exception e)
            {
                LogError($"Unexpected error caught weaving type '{type.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}");
            }
        }

        submitAnalytics.Wait();
    }

    private void WeaveType(TypeDefinition type, Dictionary<string, Tuple<MethodReference, MethodReference>> methodTable)
    {
        Debug.WriteLine("Weaving " + type.Name);

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
                    var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;
                    if (!string.IsNullOrEmpty(weaveResult.ErrorMessage))
                    {
                        // We only want one error point, so even though there may be more problems, we only log the first one.
                        LogErrorPoint(weaveResult.ErrorMessage, sequencePoint);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(weaveResult.WarningMessage))
                        {
                            LogWarningPoint(weaveResult.WarningMessage, sequencePoint);
                        }

                        var realmAttributeNames = prop.CustomAttributes
                                                      .Select(a => a.AttributeType.Name)
                                                      .Intersect(RealmPropertyAttributes)
                                                      .Select(a => $"[{a.Replace("Attribute", string.Empty)}]");
                        
                        if (realmAttributeNames.Any())
                        {
                            LogErrorPoint($"{type.Name}.{prop.Name} has {string.Join(", ", realmAttributeNames)} applied, but it's not persisted, so those attributes will be ignored.", sequencePoint);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;
                LogErrorPoint(
                    $"Unexpected error caught weaving property '{type.Name}.{prop.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}",
                    sequencePoint);
            }
        }

        if (!persistedProperties.Any())
        {
            LogError($"Class {type.Name} is a RealmObject but has no persisted properties.");
            return;
        }

        if (persistedProperties.Count(p => p.IsPrimaryKey) > 1)
        {
            LogError($"Class {type.Name} has more than one property marked with [PrimaryKey].");
            return;
        }

        var objectConstructor = type.GetConstructors()
            .SingleOrDefault(c => c.Parameters.Count == 0 && !c.IsStatic);
        if (objectConstructor == null)
        {
            var nonDefaultConstructor = type.GetConstructors().First();
            var sequencePoint = nonDefaultConstructor.Body.Instructions.First().SequencePoint;
            LogErrorPoint($"Class {type.Name} must have a public constructor that takes no parameters.", sequencePoint);
            return;
        }

        var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
        objectConstructor.CustomAttributes.Add(preserveAttribute);
        preserveAttribute = new CustomAttribute(_references.PreserveAttribute_ConstructorWithParams); // recreate so has new instance
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, true)); // AllMembers
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.Boolean, false)); // Conditional
        type.CustomAttributes.Add(preserveAttribute);
        LogDebug($"Added [Preserve] to {type.Name} and its constructor.");

        var wovenAttribute = new CustomAttribute(_references.WovenAttribute_Constructor);
        TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor, persistedProperties);
        wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_references.System_Type, helperType));
        type.CustomAttributes.Add(wovenAttribute);
        Debug.WriteLine(string.Empty);
    }

    private WeaveResult WeaveProperty(PropertyDefinition prop, TypeDefinition type, Dictionary<string, Tuple<MethodReference, MethodReference>> methodTable)
    {
        var columnName = prop.Name;
        var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
        if (mapToAttribute != null)
        {
            columnName = (string)mapToAttribute.ConstructorArguments[0].Value;
        }

        var backingField = prop.GetBackingField();
        var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
        if (isIndexed && (!_indexableTypes.Contains(prop.PropertyType.FullName)))
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}.");
        }

        var isPrimaryKey = prop.IsPrimaryKey();
        if (isPrimaryKey && (!_primaryKeyTypes.Contains(prop.PropertyType.FullName)))
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [PrimaryKey] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}.");
        }

        var isRequired = prop.IsRequired();
        if (isRequired && 
            !prop.IsNullable() &&
            prop.PropertyType.FullName != StringTypeName && 
            prop.PropertyType.FullName != ByteArrayTypeName)
        {
            return WeaveResult.Error($"{type.Name}.{prop.Name} is marked as [Required] which is only allowed on strings or nullable scalar types, not on {prop.PropertyType.FullName}.");
        }

        if (!prop.IsAutomatic())
        {
            if (prop.PropertyType.Resolve().BaseType.IsSameAs(_references.RealmObject))
            {
                return WeaveResult.Warning($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship.");
            }

            return WeaveResult.Skipped();
        }

        if (_typeTable.ContainsKey(prop.PropertyType.FullName))
        {
            // If the property is automatic but doesn't have a setter, we should still ignore it.
            if (prop.SetMethod == null)
            {
                return WeaveResult.Skipped();
            }

            var typeId = prop.PropertyType.FullName + (isPrimaryKey ? " unique" : string.Empty);
            Tuple<MethodReference, MethodReference> realmAccessors;
            if (!methodTable.TryGetValue(typeId, out realmAccessors))
            {
                var getter = new MethodReference("Get" + _typeTable[prop.PropertyType.FullName] + "Value", prop.PropertyType, _references.RealmObject)
                {
                    HasThis = true,
                    Parameters = { new ParameterDefinition(ModuleDefinition.TypeSystem.String) }
                };
                var setter = new MethodReference("Set" + _typeTable[prop.PropertyType.FullName] + "Value" + (isPrimaryKey ? "Unique" : string.Empty), ModuleDefinition.TypeSystem.Void, _references.RealmObject)
                {
                    HasThis = true,
                    Parameters =
                    {
                        new ParameterDefinition(ModuleDefinition.TypeSystem.String),
                        new ParameterDefinition(prop.PropertyType)
                    }
                };

                methodTable[typeId] = realmAccessors = Tuple.Create(getter, setter);
            }

            ReplaceGetter(prop, columnName, realmAccessors.Item1);
            ReplaceSetter(prop, backingField, columnName, realmAccessors.Item2);
        }
        else if (prop.IsIList())
        {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (!elementType.Resolve().BaseType.IsSameAs(_references.RealmObject))
            {
                return WeaveResult.Warning($"SKIPPING {type.Name}.{columnName} because it is an IList but its generic type is not a RealmObject subclass, so will not persist.");
            }

            if (prop.SetMethod != null)
            {
                return WeaveResult.Error($"{type.Name}.{columnName} has a setter but its type is a IList which only supports getters.");
            }

            var concreteListConstructor = _references.System_Collections_Generic_ListOfT_Constructor.MakeHostInstanceGeneric(elementType);

            // weaves list getter which also sets backing to List<T>, forcing it to accept us setting it post-init
            var backingDef = backingField as FieldDefinition;
            if (backingDef != null)
            {
                backingDef.Attributes &= ~FieldAttributes.InitOnly;  // without a set; auto property has this flag we must clear
            }

            ReplaceListGetter(prop, backingField, columnName,
                              new GenericInstanceMethod(_references.RealmObject_GetListValue) { GenericArguments = { elementType } },
                              concreteListConstructor);
        }
        else if (prop.PropertyType.Resolve().BaseType.IsSameAs(_references.RealmObject))
        {
            // with casting in the _realmObject methods, should just work
            ReplaceGetter(prop, columnName,
                          new GenericInstanceMethod(_references.RealmObject_GetObjectValue) { GenericArguments = { prop.PropertyType } });
            ReplaceSetter(prop, backingField, columnName,
                          new GenericInstanceMethod(_references.RealmObject_SetObjectValue) { GenericArguments = { prop.PropertyType } });
        }
        else if (prop.IsIQueryable())
        {
            var backlinkAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "BacklinkAttribute");
            if (backlinkAttribute == null)
            {
                return WeaveResult.Skipped();
            }

            if (prop.SetMethod != null)
            {
                return WeaveResult.Error("Backlink properties must be read-only.");
            }

            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            var inversePropertyName = (string)backlinkAttribute.ConstructorArguments[0].Value;
            var inverseProperty = elementType.Resolve().Properties.SingleOrDefault(p => p.Name == inversePropertyName);

            if (inverseProperty == null || (!inverseProperty.PropertyType.IsSameAs(type) && !inverseProperty.IsIList(type)))
            {
                return WeaveResult.Error($"The property '{elementType.Name}.{inversePropertyName}' does not constitute a link to '{type.Name}' as described by '{type.Name}.{prop.Name}'.");
            }

            var backingDef = backingField as FieldDefinition;
            if (backingDef != null)
            {
                backingDef.Attributes &= ~FieldAttributes.InitOnly; // without a set; auto property has this flag we must clear
            }

            ReplaceBacklinksGetter(prop, backingField, columnName, elementType);
        }
        else if (prop.SetMethod == null)
        {
            return WeaveResult.Skipped();
        }
        else if (prop.PropertyType.FullName == "System.DateTime")
        {
            return WeaveResult.Error($"Class '{type.Name}' field '{prop.Name}' is a DateTime which is not supported - use DateTimeOffset instead.");
        }
        else if (prop.PropertyType.FullName == "System.Nullable`1<System.DateTime>")
        {
            return WeaveResult.Error($"Class '{type.Name}' field '{prop.Name}' is a DateTime? which is not supported - use DateTimeOffset? instead.");
        }
        else
        {
            return WeaveResult.Error($"Class '{type.Name}' field '{columnName}' is a '{prop.PropertyType}' which is not yet supported.");
        }

        var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
        prop.CustomAttributes.Add(preserveAttribute);

        var wovenPropertyAttribute = new CustomAttribute(_references.WovenPropertyAttribute_Constructor);
        prop.CustomAttributes.Add(wovenPropertyAttribute);

        Debug.WriteLine(string.Empty);

        var primaryKeyMsg = isPrimaryKey ? "[PrimaryKey]" : string.Empty;
        var indexedMsg = isIndexed ? "[Indexed]" : string.Empty;
        LogDebug($"Woven {type.Name}.{prop.Name} as a {prop.PropertyType.FullName} {primaryKeyMsg} {indexedMsg}.");
        return WeaveResult.Success(prop, backingField, isPrimaryKey);
    }

    private void ReplaceGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference)
    {
        // A synthesized property getter looks like this:
        //   0: ldarg.0
        //   1: ldfld <backingField>
        //   2: ret
        // We want to change it so it looks like this:
        //   0: ldarg.0
        //   1: call Realms.RealmObject.get_IsManaged
        //   2: brfalse.s 7
        //   3: ldarg.0
        //   4: ldstr <columnName>
        //   5: call Realms.RealmObject.GetValue<T>
        //   6: ret
        //   7: ldarg.0
        //   8: ldfld <backingField>
        //   9: ret
        // This is roughly equivalent to:
        //   if (!base.IsManaged) return this.<backingField>;
        //   return base.GetValue<T>(<columnName>);

        var start = prop.GetMethod.Body.Instructions.First();
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
        il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName)); // [stack = this | name ]
        il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
    }

    // WARNING
    // This code setting the backing field only works if the field is settable after init
    // if you don't have an automatic set; on the property, it shows in the debugger with
    //         Attributes    Private | InitOnly    Mono.Cecil.FieldAttributes
    private void ReplaceListGetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference getListValueReference, MethodReference listConstructor)
    {
        // A synthesized property getter looks like this:
        //   0: ldarg.0  // load the this pointer
        //   1: ldfld <backingField>
        //   2: ret
        // We want to change it so it looks somewhat like this, in C#
        /*
            if (<backingField> == null)
            {
               if (IsManaged)
                     <backingField> = GetListObject<T>(<columnName>);
               else
                     <backingField> = new List<T>();
            }
            // original auto-generated getter starts here
            return <backingField>; // supplied by the generated getter OR RealmObject._CopyDataFromBackingFields
        */

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
        // A synthesized property getter looks like this:
        //   0: ldarg.0  // load the this pointer
        //   1: ldfld <backingField>
        //   2: ret
        // We want to change it so it looks somewhat like this, in C#
        /*
            if (<backingField> == null)
            {
               if (IsManaged)
                     <backingField> = GetBacklinks<T>(<columnName>);
               else
                     <backingField> = new Enumerable.Empty<T>.AsQueryable();
            }
            // original auto-generated getter starts here
            return <backingField>; // supplied by the generated getter OR RealmObject._CopyDataFromBackingFields
        */

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

    private void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference)
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
        il.Append(il.Create(OpCodes.Call, setValueReference));
        il.Append(il.Create(OpCodes.Ret));

        Debug.Write("[set] ");
    }

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor, List<WeaveResult> properties)
    {
        var helperType = new TypeDefinition(null, "RealmHelper",
                                            TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                            ModuleDefinition.TypeSystem.Object);

        helperType.Interfaces.Add(_references.IRealmObjectHelper);

        var createInstance = new MethodDefinition("CreateInstance", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, _references.RealmObject);
        {
            var il = createInstance.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, objectConstructor);
            il.Emit(OpCodes.Ret);
        }

        helperType.Methods.Add(createInstance);

        var copyToRealm = new MethodDefinition("CopyToRealm", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.TypeSystem.Void);
        {
            // This roughly translates to
            /*
                var castInstance = (ObjectType)instance;
                
                *foreach* non-list woven property in castInstance's schema
                *if* castInstace.field is a RealmObject descendant
                    castInstance.Realm.Add(castInstance.field, update);
                    castInstance.Field = castInstance.field;
                *else if* property is PK
                    if (setPrimaryKey)
                    {
                        castInstance.Property = castInstance.Field;
                    }
                *else if* property is [Required] or nullable
                    castInstance.Property = castInstance.Field;
                *else* 
                    if (!setPrimaryKey || castInstance.field != default(fieldType))
                    {
                        castInstance.Property = castInstance.Field;
                    }

                *foreach* list woven property in castInstance's schema
                var list = castInstance.field;
                castInstance.field = null;
                if (!setPrimaryKey)
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

            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, _references.RealmObject);
            copyToRealm.Parameters.Add(instanceParameter);

            var updateParameter = new ParameterDefinition("update", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean);
            copyToRealm.Parameters.Add(updateParameter);

            var setPrimaryKeyParameter = new ParameterDefinition("setPrimaryKey", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean);
            copyToRealm.Parameters.Add(setPrimaryKeyParameter);

            copyToRealm.Body.Variables.Add(new VariableDefinition(realmObjectType));

            byte currentStloc = 1;
            if (properties.Any(p => p.Property.IsDateTimeOffset()))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(_references.System_DateTimeOffset));
                currentStloc++;
            }

            foreach (var prop in properties.Where(p => p.Property.IsIList()))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(prop.Field.FieldType)));
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.TypeSystem.Int32));
            }

            var il = copyToRealm.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Castclass, ModuleDefinition.ImportReference(realmObjectType)));
            il.Append(il.Create(OpCodes.Stloc_0));

            foreach (var prop in properties.OrderByDescending(p => p.Property.IsPrimaryKey()))
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

                    // If the property is non-nullable, we want the following code to execute:
                    // if (!setPrimaryKey || castInstance.field != default(fieldType))
                    // {
                    //     castInstance.Property = castInstance.field
                    // }
                    //
                    // This ensures that if we're updating, we'll be copy each value to realm, even if it's the default value for the property,
                    // because we have no idea what the previous value was. If it's an add, we're certain that the row will contain the default value, so no need to set it.
                    // *updatePlaceholder* will be the Brtrue instruction that will skip the default check and move to the property setting logic.
                    // The default check branching instruction is inserted above the *setStartPoint* instruction later on.
                    Instruction setPrimaryKeyPlaceholder = null;
                    if (property.IsDescendantOf(_references.RealmObject))
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
                    else if (!property.IsNullable() && !property.IsPrimaryKey() && !property.IsRequired())
                    {
                        il.Append(il.Create(OpCodes.Ldarg_3));
                        setPrimaryKeyPlaceholder = il.Create(OpCodes.Nop);
                        il.Append(setPrimaryKeyPlaceholder);

                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        if (property.IsDateTimeOffset())
                        {
                            // DateTimeOffset's default value is not falsy, so we need to create a new instance and compare to that.
                            il.Append(il.Create(OpCodes.Ldloca_S, (byte)1));
                            il.Append(il.Create(OpCodes.Initobj, field.FieldType));
                            il.Append(il.Create(OpCodes.Ldloc_1));
                            il.Append(il.Create(OpCodes.Call, _references.System_DateTimeOffset_op_Inequality));
                        }
                        else if (property.IsSingle())
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

                    if (property.IsDescendantOf(_references.RealmObject))
                    {
                        if (addPlaceholder != null)
                        {
                            il.Replace(addPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                        }
                    }
                    else if (!property.IsNullable() && !property.IsPrimaryKey() && !property.IsRequired())
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

                        if (setPrimaryKeyPlaceholder != null)
                        {
                            il.Replace(setPrimaryKeyPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                        }
                    }
                    else if (property.IsPrimaryKey())
                    {
                        // If setPrimaryKey is false, we skip setting the property
                        il.InsertBefore(setStartPoint, il.Create(OpCodes.Ldarg_3));
                        il.InsertBefore(setStartPoint, il.Create(OpCodes.Brfalse_S, setEndPoint));
                    }
                }
                else if (property.IsIList())
                {
                    var elementType = ((GenericInstanceType)property.PropertyType).GenericArguments.Single();
                    var propertyGetterMethodReference = ModuleDefinition.ImportReference(property.GetMethod);

                    var iteratorStLoc = (byte)(currentStloc + 1);

                    // if (!setPrimaryKey ||
                    var isPrimaryKeyCheck = il.Create(OpCodes.Ldarg_3);
                    il.Append(isPrimaryKeyCheck);

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

                    // if (!setPrimaryKey)
                    var clearSkipPlaceholder = il.Create(OpCodes.Ldarg_3);
                    il.Append(clearSkipPlaceholder);

                    // castInstance.Property.Clear();
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Call, propertyGetterMethodReference));
                    il.Append(il.Create(OpCodes.Callvirt, _references.ICollectionOfT_Clear.MakeHostInstanceGeneric(elementType)));

                    // if (list == null)
                    var localListVarNullCheck = il.Create(OpCodes.Ldloc_S, currentStloc);
                    il.Append(localListVarNullCheck);

                    // if (setPrimaryKey), skip List.Clear and jump to checking the list == null
                    il.InsertAfter(clearSkipPlaceholder, il.Create(OpCodes.Brtrue_S, localListVarNullCheck));

                    il.Append(il.Create(OpCodes.Ldc_I4_0));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cyclePlaceholder = il.Create(OpCodes.Nop);
                    il.Append(cyclePlaceholder);

                    // castInstance.Realm.Add(list[i], update)
                    var cycleStart = il.Create(OpCodes.Ldloc_0);
                    il.Append(cycleStart);
                    il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_Realm));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Callvirt, _references.IListOfT_get_Item.MakeHostInstanceGeneric(elementType)));
                    il.Append(il.Create(OpCodes.Ldarg_2));
                    il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.Realm_Add) { GenericArguments = { elementType } }));
                    il.Append(il.Create(OpCodes.Pop));

                    // castInstance.Property.Add(list[i]);
                    il.Append(il.Create(OpCodes.Ldloc_0));
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
                    il.InsertAfter(isPrimaryKeyCheck, il.Create(OpCodes.Brfalse_S, setterStart));

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
                    var sequencePoint = property.GetMethod.Body.Instructions.First().SequencePoint;
                    LogErrorPoint($"{realmObjectType.Name}.{property.Name} does not have a setter and is not an IList. This is an error in Realm, so please file a bug report.", sequencePoint);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        copyToRealm.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));
        helperType.Methods.Add(copyToRealm);

        var getPrimaryKeyValue = new MethodDefinition("TryGetPrimaryKeyValue", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.TypeSystem.Boolean);
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

        public readonly string ErrorMessage;

        public readonly string WarningMessage;

        public bool Woven;

        public readonly PropertyDefinition Property;

        public readonly FieldReference Field;

        public readonly bool IsPrimaryKey;

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
}
