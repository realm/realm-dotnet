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

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public IAssemblyResolver AssemblyResolver { get; set; }

    private AssemblyDefinition _realmAssembly;
    private TypeDefinition _realmObject;
    private MethodReference _realmObjectIsManagedGetter;
    private MethodReference _realmObjectRealmGetter;
    private MethodReference _realmAddGenericReference;
    private MethodReference _realmObjectRaisePropertyChanged;

    private AssemblyDefinition _corLib;
    private TypeReference _system_Object;
    private TypeReference _system_Boolean;
    private TypeReference _system_Type;
    private TypeReference _system_IList;
    private TypeReference _system_DateTimeOffset;
    private TypeReference _system_Int32;
    private MethodReference _system_DatetimeOffset_Op_Inequality;

    private MethodReference _propChangedDoNotNotifyAttributeConstructorDefinition;

    private static readonly Dictionary<string, string> _typeTable = new Dictionary<string, string>
    {
        { "System.String", "String" },
        { "System.Char", "Char" },
        { "System.Byte", "Byte" },
        { "System.Int16", "Int16" },
        { "System.Int32", "Int32" },
        { "System.Int64", "Int64" },
        { "System.Single", "Single" },
        { "System.Double", "Double" },
        { "System.Boolean", "Boolean" },
        { "System.DateTimeOffset", "DateTimeOffset" },
        { "System.Byte[]", "ByteArray" },
        { "System.Nullable`1<System.Char>", "NullableChar" },
        { "System.Nullable`1<System.Byte>", "NullableByte" },
        { "System.Nullable`1<System.Int16>", "NullableInt16" },
        { "System.Nullable`1<System.Int32>", "NullableInt32" },
        { "System.Nullable`1<System.Int64>", "NullableInt64" },
        { "System.Nullable`1<System.Single>", "NullableSingle" },
        { "System.Nullable`1<System.Double>", "NullableDouble" },
        { "System.Nullable`1<System.Boolean>", "NullableBoolean" },
        { "System.Nullable`1<System.DateTimeOffset>", "NullableDateTimeOffset" }
    };

    private static readonly List<string> _primaryKeyTypes = new List<string>
    {
        "System.String",
        "System.Char",
        "System.Byte",
        "System.Int16",
        "System.Int32",
        "System.Int64",
        "System.Nullable`1<System.Char>",
        "System.Nullable`1<System.Byte>",
        "System.Nullable`1<System.Int16>",
        "System.Nullable`1<System.Int32>",
        "System.Nullable`1<System.Int64>",
    };

    private static readonly List<string> _indexableTypes = new List<string>
    {
        "System.String",
        "System.Char",
        "System.Byte",
        "System.Int16",
        "System.Int32",
        "System.Int64",
        "System.Boolean",
        "System.DateTimeOffset"
    };

    private static readonly HashSet<string> RealmPropertyAttributes = new HashSet<string>
    {
        "PrimaryKeyAttribute",
        "IndexedAttribute",
        "MapToAttribute",
    };

    private MethodReference _genericGetObjectValueReference;
    private MethodReference _genericSetObjectValueReference;
    private MethodReference _genericGetListValueReference;
    private MethodReference _preserveAttributeConstructor;
    private MethodReference _preserveAttributeConstructorWithParams;
    private MethodReference _wovenAttributeConstructor;
    private MethodReference _wovenPropertyAttributeConstructor;

    private IEnumerable<TypeDefinition> GetMatchingTypes()
    {
        foreach (var type in ModuleDefinition.GetTypes().Where(t => t.IsDescendedFrom(_realmObject)))
        {
            if (type.BaseType.IsSameAs(_realmObject))
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

        _realmAssembly = AssemblyResolver.Resolve("Realm");  // Note that the assembly is Realm but the namespace Realms with the s

        _realmObject = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "RealmObject");
        _realmObjectIsManagedGetter = ModuleDefinition.ImportReference(_realmObject.Properties.Single(x => x.Name == "IsManaged").GetMethod);
        _realmObjectRealmGetter = ModuleDefinition.ImportReference(_realmObject.Properties.Single(p => p.Name == "Realm").GetMethod);
        _realmObjectRaisePropertyChanged = ModuleDefinition.ImportReference(_realmObject.Methods.Single(m => m.Name == "RaisePropertyChanged"));

        var realm = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "Realm");
        _realmAddGenericReference = ModuleDefinition.ImportReference(realm.Methods.First(x => x.Name == "Add" && x.HasGenericParameters));

        // Cache of getter and setter methods for the various types.
        var methodTable = new Dictionary<string, Tuple<MethodReference, MethodReference>>();

        _genericGetObjectValueReference = _realmObject.LookupMethodReference("GetObjectValue", ModuleDefinition);
        _genericSetObjectValueReference = _realmObject.LookupMethodReference("SetObjectValue", ModuleDefinition);
        _genericGetListValueReference = _realmObject.LookupMethodReference("GetListValue", ModuleDefinition);

        var preserveAttributeClass = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "PreserveAttribute");
        _preserveAttributeConstructor = ModuleDefinition.ImportReference(preserveAttributeClass.GetConstructors().Single(c => c.Parameters.Count == 0));
        _preserveAttributeConstructorWithParams = ModuleDefinition.ImportReference(preserveAttributeClass.GetConstructors().Single(c => c.Parameters.Count == 2));

        var wovenAttributeClass = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        _wovenAttributeConstructor = ModuleDefinition.ImportReference(wovenAttributeClass.GetConstructors().First());

        var wovenPropertyAttributeClass = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenPropertyAttribute");
        _wovenPropertyAttributeConstructor = ModuleDefinition.ImportReference(wovenPropertyAttributeClass.GetConstructors().First());

        _corLib = AssemblyResolver.Resolve((AssemblyNameReference)ModuleDefinition.TypeSystem.CoreLibrary);
        _system_Object = ModuleDefinition.TypeSystem.Object;
        _system_Boolean = ModuleDefinition.TypeSystem.Boolean;
        _system_Int32 = ModuleDefinition.TypeSystem.Int32;

        var dateTimeOffsetType = GetTypeFromSystemAssembly("System.DateTimeOffset");
        _system_DateTimeOffset = ModuleDefinition.ImportReference(dateTimeOffsetType);
        _system_DatetimeOffset_Op_Inequality = ModuleDefinition.ImportReference(dateTimeOffsetType.GetMethods().Single(m => m.Name == "op_Inequality"));

        _system_Type = ModuleDefinition.ImportReference(GetTypeFromSystemAssembly("System.Type"));

        var listTypeDefinition = _corLib.MainModule.GetType("System.Collections.Generic.List`1");

        if (listTypeDefinition == null)
        {
            _system_IList = ModuleDefinition.ImportReference(typeof(System.Collections.Generic.List<>));
        }
        else
        {
            _system_IList = ModuleDefinition.ImportReference(listTypeDefinition);
        }

        // If the solution has a reference to PropertyChanged.Fody, let's look up the DoNotNotifyAttribute for use later.
        var usesPropertyChangedFody = ModuleDefinition.AssemblyReferences.Any(X => X.Name == "PropertyChanged");
        if (usesPropertyChangedFody)
        {
            var propChangedAssembly = AssemblyResolver.Resolve("PropertyChanged");
            var doNotNotifyAttributeDefinition = propChangedAssembly.MainModule.GetTypes().First(X => X.Name == "DoNotNotifyAttribute");
            _propChangedDoNotNotifyAttributeConstructorDefinition = ModuleDefinition.ImportReference(doNotNotifyAttributeDefinition.GetConstructors().First());
        }

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
            .SingleOrDefault(c => c.Parameters.Count == 0 && c.IsPublic && !c.IsStatic);
        if (objectConstructor == null)
        {
            var nonDefaultConstructor = type.GetConstructors().First();
            var sequencePoint = nonDefaultConstructor.Body.Instructions.First().SequencePoint;
            LogErrorPoint($"Class {type.Name} must have a public constructor that takes no parameters.", sequencePoint);
            return;
        }

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
        objectConstructor.CustomAttributes.Add(preserveAttribute);
        preserveAttribute = new CustomAttribute(_preserveAttributeConstructorWithParams); // recreate so has new instance
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Boolean, true)); // AllMembers
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Boolean, false)); // Conditional
        type.CustomAttributes.Add(preserveAttribute);
        LogDebug($"Added [Preserve] to {type.Name} and its constructor.");

        var wovenAttribute = new CustomAttribute(_wovenAttributeConstructor);
        TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor, persistedProperties);
        wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Type, helperType));
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

        if (!prop.IsAutomatic())
        {
            if (prop.PropertyType.Resolve().BaseType.IsSameAs(_realmObject))
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
            if (!methodTable.ContainsKey(typeId))
            {
                var getter = _realmObject.LookupMethodReference("Get" + _typeTable[prop.PropertyType.FullName] + "Value", ModuleDefinition);
                var setter = _realmObject.LookupMethodReference("Set" + _typeTable[prop.PropertyType.FullName] + "Value" + (isPrimaryKey ? "Unique" : string.Empty), ModuleDefinition);
                methodTable[typeId] = Tuple.Create(getter, setter);
            }

            ReplaceGetter(prop, columnName, methodTable[typeId].Item1);
            ReplaceSetter(prop, backingField, columnName, methodTable[typeId].Item2);
        }

        // treat IList and RealmList similarly but IList gets a default so is useable as standalone
        // IList or RealmList allows people to declare lists only of _realmObject due to the class definition
        else if (prop.IsIList())
        {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (!elementType.Resolve().BaseType.IsSameAs(_realmObject))
            {
                return WeaveResult.Warning($"SKIPPING {type.Name}.{columnName} because it is an IList but its generic type is not a RealmObject subclass, so will not persist.");
            }

            if (prop.SetMethod != null)
            {
                return WeaveResult.Error($"{type.Name}.{columnName} has a setter but its type is a IList which only supports getters.");
            }

            var concreteListType = new GenericInstanceType(_system_IList) { GenericArguments = { elementType } };
            var listConstructor = concreteListType.Resolve().GetConstructors().Single(c => c.IsPublic && c.Parameters.Count == 0);
            var concreteListConstructor = listConstructor.MakeHostInstanceGeneric(elementType);

            // weaves list getter which also sets backing to List<T>, forcing it to accept us setting it post-init
            var backingDef = backingField as FieldDefinition;
            if (backingDef != null)
            {
                backingDef.Attributes &= ~FieldAttributes.InitOnly;  // without a set; auto property has this flag we must clear
            }

            ReplaceListGetter(prop, backingField, columnName,
                new GenericInstanceMethod(_genericGetListValueReference) { GenericArguments = { elementType } },
                             ModuleDefinition.ImportReference(concreteListConstructor));
        }
        else if (prop.PropertyType.Resolve().BaseType.IsSameAs(_realmObject))
        {
            if (!prop.IsAutomatic())
            {
                return WeaveResult.Warning($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship.");
            }

            // with casting in the _realmObject methods, should just work
            ReplaceGetter(prop, columnName,
                new GenericInstanceMethod(_genericGetObjectValueReference) { GenericArguments = { prop.PropertyType } });
            ReplaceSetter(prop, backingField, columnName,
                new GenericInstanceMethod(_genericSetObjectValueReference) { GenericArguments = { prop.PropertyType } });
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

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
        prop.CustomAttributes.Add(preserveAttribute);

        var wovenPropertyAttribute = new CustomAttribute(_wovenPropertyAttributeConstructor);
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
        il.InsertBefore(start, il.Create(OpCodes.Call, _realmObjectIsManagedGetter));
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
        il.InsertBefore(start, il.Create(OpCodes.Call, _realmObjectIsManagedGetter));  // [ this, this -> this,  isManaged ]

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
        if (_propChangedDoNotNotifyAttributeConstructorDefinition != null)
        {
            prop.CustomAttributes.Add(new CustomAttribute(_propChangedDoNotNotifyAttributeConstructorDefinition));
        }

        var managedSetStart = il.Create(OpCodes.Ldarg_0);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Call, _realmObjectIsManagedGetter));
        il.Append(il.Create(OpCodes.Brtrue_S, managedSetStart));

        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Stfld, backingField));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Ldstr, prop.Name));
        il.Append(il.Create(OpCodes.Call, _realmObjectRaisePropertyChanged));
        il.Append(il.Create(OpCodes.Ret));

        il.Append(managedSetStart);
        il.Append(il.Create(OpCodes.Ldstr, columnName));
        il.Append(il.Create(OpCodes.Ldarg_1));
        il.Append(il.Create(OpCodes.Call, setValueReference));
        il.Append(il.Create(OpCodes.Ret));

        Debug.Write("[set] ");
    }

    private MethodReference GetIListMethodReference(TypeDefinition interfaceType, string methodName, GenericInstanceType genericInstance)
    {
        MethodReference reference = null;
        var definition = interfaceType.GetMethods().FirstOrDefault(m => m.FullName.Contains($"::{methodName}"));
        if (definition == null)
        {
            foreach (var parent in interfaceType.Interfaces)
            {
                reference = GetIListMethodReference(parent.Resolve(), methodName, genericInstance);
                if (reference != null)
                {
                    break;
                }
            }
        }
        else
        {
            var generic = definition.MakeHostInstanceGeneric(genericInstance.GenericArguments.ToArray());
            reference = ModuleDefinition.ImportReference(generic);
        }

        return reference;
    }

    private TypeDefinition GetTypeFromSystemAssembly(string typeName)
    {
        var objectTypeDefinition = _corLib.MainModule.GetType(typeName);
        if (objectTypeDefinition == null) // For PCL's System.XXX is only accessible as an ExportedType for some reason.
        {
            objectTypeDefinition = _corLib.MainModule.ExportedTypes.First(t => t.FullName == typeName).Resolve();
        }

        return objectTypeDefinition;
    }

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor, List<WeaveResult> properties)
    {
        var helperType = new TypeDefinition(null, "RealmHelper",
                                            TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                            _system_Object);

        helperType.Interfaces.Add(ModuleDefinition.ImportReference(_realmAssembly.MainModule.GetType("Realms.Weaving.IRealmObjectHelper")));

        var createInstance = new MethodDefinition("CreateInstance", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.ImportReference(_realmObject));
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
                *else*
                    if (castInstance.field != default(fieldType))
                    {
                        castInstance.Property = castInstance.Field;
                    }

                *foreach* list woven property in castInstance's schema
                var list = castInstance.field;
                castInstance.field = null;
                for (var i = 0; i < list.Count; i++)
                {
                    castInstance.Realm.Add(list[i], update);
                    castInstance.Property.Add(list[i]);
                }
            */

            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, ModuleDefinition.ImportReference(_realmObject));
            copyToRealm.Parameters.Add(instanceParameter);

            var updateParameter = new ParameterDefinition("update", ParameterAttributes.None, ModuleDefinition.TypeSystem.Boolean);
            copyToRealm.Parameters.Add(updateParameter);

            copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(realmObjectType)));

            byte currentStloc = 1;
            if (properties.Any(p => p.Property.IsDateTimeOffset()))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(_system_DateTimeOffset));
                currentStloc++;
            }

            foreach (var prop in properties.Where(p => p.Property.IsIList()))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(prop.Field.FieldType)));
                copyToRealm.Body.Variables.Add(new VariableDefinition(_system_Int32));
            }

            var il = copyToRealm.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Castclass, ModuleDefinition.ImportReference(realmObjectType)));
            il.Append(il.Create(OpCodes.Stloc_0));

            foreach (var prop in properties)
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
                    // if (update || castInstance.field != default(fieldType))
                    // {
                    //     castInstance.Property = castInstance.field
                    // }
                    //
                    // This ensures that if we're updating, we'll be copy each value to realm, even if it's the default value for the property,
                    // because we have no idea what the previous value was. If it's an add, we're certain that the row will contain the default value, so no need to set it.
                    // *updatePlaceholder* will be the Brtrue instruction that will skip the default check and move to the property setting logic.
                    // The default check branching instruction is inserted above the *setStartPoint* instruction later on.
                    Instruction updatePlaceholder = null;
                    if (property.IsDescendantOf(_realmObject))
                    {
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        addPlaceholder = il.Create(OpCodes.Nop);
                        il.Append(addPlaceholder);

                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Call, _realmObjectRealmGetter));
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));
                        il.Append(il.Create(OpCodes.Ldarg_2));
                        il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_realmAddGenericReference) { GenericArguments = { field.FieldType } }));
                        il.Append(il.Create(OpCodes.Pop));
                    }
                    else if (!property.IsNullable() && !property.IsPrimaryKey())
                    {
                        il.Append(il.Create(OpCodes.Ldarg_2));
                        updatePlaceholder = il.Create(OpCodes.Nop);
                        il.Append(updatePlaceholder);

                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        if (property.IsDateTimeOffset())
                        {
                            // DateTimeOffset's default value is not falsy, so we need to create a new instance and compare to that.
                            il.Append(il.Create(OpCodes.Ldloca_S, (byte)1));
                            il.Append(il.Create(OpCodes.Initobj, field.FieldType));
                            il.Append(il.Create(OpCodes.Ldloc_1));
                            il.Append(il.Create(OpCodes.Call, _system_DatetimeOffset_Op_Inequality));
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
                    il.Append(il.Create(OpCodes.Call, ModuleDefinition.ImportReference(property.SetMethod)));

                    var setEndPoint = il.Create(OpCodes.Nop);
                    il.Append(setEndPoint);

                    if (property.IsDescendantOf(_realmObject))
                    {
                        if (addPlaceholder != null)
                        {
                            il.Replace(addPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                        }
                    }
                    else if (!property.IsNullable() && !property.IsPrimaryKey())
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

                        if (updatePlaceholder != null)
                        {
                            il.Replace(updatePlaceholder, il.Create(OpCodes.Brtrue_S, setStartPoint));
                        }
                    }
                }
                else if (property.IsIList())
                {
                    var propertyTypeDefinition = property.PropertyType.Resolve();
                    var genericType = (GenericInstanceType)property.PropertyType;
                    var iList_Get_ItemMethodReference = GetIListMethodReference(propertyTypeDefinition, "get_Item", genericType);
                    var iList_AddMethodReference = GetIListMethodReference(propertyTypeDefinition, "Add", genericType);
                    var iList_Get_CountMethodReference = GetIListMethodReference(propertyTypeDefinition, "get_Count", genericType);

                    var iteratorStLoc = (byte)(currentStloc + 1);

                    // if (update ||
                    var isUpdateCheck = il.Create(OpCodes.Ldarg_2);
                    il.Append(isUpdateCheck);

                    // castInstance.field != null)
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    var nullCheck = il.Create(OpCodes.Ldfld, field);
                    il.Append(nullCheck);

                    // var list = castInstance.field;
                    // castInstance.field = null;
                    var setterStart = il.Create(OpCodes.Ldloc_0);
                    il.Append(setterStart);
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Stloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldnull));
                    il.Append(il.Create(OpCodes.Stfld, field));
                    il.Append(il.Create(OpCodes.Ldc_I4_0));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cyclePlaceholder = il.Create(OpCodes.Nop);
                    il.Append(cyclePlaceholder);

                    // this.Realm.Add(list[i], update)
                    var cycleStart = il.Create(OpCodes.Ldloc_0);
                    il.Append(cycleStart);
                    il.Append(il.Create(OpCodes.Call, _realmObjectRealmGetter));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Callvirt, iList_Get_ItemMethodReference));
                    il.Append(il.Create(OpCodes.Ldarg_2));
                    il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_realmAddGenericReference) { GenericArguments = { genericType.GenericArguments.Single() } }));
                    il.Append(il.Create(OpCodes.Pop));

                    // Property.Add(list[i]);
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Callvirt, ModuleDefinition.ImportReference(property.GetMethod)));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Callvirt, iList_Get_ItemMethodReference));
                    il.Append(il.Create(OpCodes.Callvirt, iList_AddMethodReference));
                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Ldc_I4_1));
                    il.Append(il.Create(OpCodes.Add));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cycleLabel = il.Create(OpCodes.Nop);
                    il.Append(cycleLabel);
                    il.Replace(cyclePlaceholder, il.Create(OpCodes.Br_S, cycleLabel));

                    il.Append(il.Create(OpCodes.Ldloc_S, iteratorStLoc));
                    il.Append(il.Create(OpCodes.Ldloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Callvirt, iList_Get_CountMethodReference));
                    il.Append(il.Create(OpCodes.Blt_S, cycleStart));

                    var setterEnd = il.Create(OpCodes.Nop);
                    il.Append(setterEnd);
                    il.InsertAfter(nullCheck, il.Create(OpCodes.Brfalse_S, setterEnd));

                    il.InsertAfter(isUpdateCheck, il.Create(OpCodes.Brtrue_S, setterStart));

                    currentStloc += 2;
                }
                else
                {
                    var sequencePoint = property.GetMethod.Body.Instructions.First().SequencePoint;
                    LogErrorPoint($"{realmObjectType.Name}.{property.Name} does not have a setter and is not an IList. This is an error in Realm, so please file a bug report.", sequencePoint);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        copyToRealm.CustomAttributes.Add(new CustomAttribute(_preserveAttributeConstructor));
        helperType.Methods.Add(copyToRealm);

        var getPrimaryKeyValue = new MethodDefinition("TryGetPrimaryKeyValue", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.TypeSystem.Boolean);
        {
            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, ModuleDefinition.ImportReference(_realmObject));
            getPrimaryKeyValue.Parameters.Add(instanceParameter);

            var valueParameter = new ParameterDefinition("value", ParameterAttributes.Out, new ByReferenceType(_system_Object))
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

        getPrimaryKeyValue.CustomAttributes.Add(new CustomAttribute(_preserveAttributeConstructor));
        helperType.Methods.Add(getPrimaryKeyValue);

        const MethodAttributes CtorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var ctor = new MethodDefinition(".ctor", CtorAttributes, ModuleDefinition.TypeSystem.Void);
        {
            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ModuleDefinition.ImportReference(_system_Object.Resolve().GetConstructors().Single()));
            il.Emit(OpCodes.Ret);
        }

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
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