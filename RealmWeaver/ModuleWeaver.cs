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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public class ModuleWeaver
{
    // Will log an informational message to MSBuild - see https://github.com/Fody/Fody/wiki/ModuleWeaver for details
    public Action<string> LogDebug { get; set; } = m => { };  // MessageImportance.Normal, included in verbosity Detailed
    public Action<string> LogInfo { get; set; } = m => { };  // MessageImportance.High
    public Action<string, SequencePoint> LogWarningPoint { get; set; } = (m, p) => { };
    public Action<string> LogError { get; set; } = m => { };
    public Action<string, SequencePoint> LogErrorPoint { get; set; } = (m, p) => { };

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    public IAssemblyResolver AssemblyResolver { get; set; }

    private AssemblyDefinition _realmAssembly;
    private TypeDefinition _realmObject;
    private MethodReference _realmObjectIsManagedGetter;

    private AssemblyDefinition _corLib;
    private TypeReference _system_Object;
    private TypeReference _system_Boolean;
    private TypeReference _system_Type;
    private TypeReference _system_IList;
    private TypeReference _system_DateTimeOffset;
    private TypeReference _system_Int32;
    private MethodReference _system_DatetimeOffset_Op_Inequality;

    private MethodReference _propChangedEventArgsConstructor;
    private MethodReference _propChangedEventHandlerInvokeReference;
    private TypeReference _propChangedEventHandlerReference;

    private MethodReference _propChangedDoNotNotifyAttributeConstructorDefinition;

    private readonly Dictionary<string, string> _typeTable = new Dictionary<string, string>
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

    private readonly List<string> _primaryKeyTypes = new List<string>
    {
        "System.String",
        "System.Char",
        "System.Byte",
        "System.Int16",
        "System.Int32",
        "System.Int64",
    };

    private readonly List<string> _indexableTypes = new List<string>
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
                LogErrorPoint($"The type {type.FullName} indirectly inherits from RealmObject which is not supported", type.GetConstructors().FirstOrDefault()?.Body?.Instructions?.First()?.SequencePoint);
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

        // Cache of getter and setter methods for the various types.
        var methodTable = new Dictionary<string, Tuple<MethodReference, MethodReference>>();

        _genericGetObjectValueReference = LookupMethodAndImport(_realmObject, "GetObjectValue");
        _genericSetObjectValueReference = LookupMethodAndImport(_realmObject, "SetObjectValue");
        _genericGetListValueReference = LookupMethodAndImport(_realmObject, "GetListValue");

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

        var systemAssembly = AssemblyResolver.Resolve("System");
        var systemObjectModelAssembly = AssemblyResolver.Resolve("System.ObjectModel");

        var propertyChangedEventArgs = LookupType("PropertyChangedEventArgs", systemObjectModelAssembly, systemAssembly);
        _propChangedEventArgsConstructor = ModuleDefinition.ImportReference(propertyChangedEventArgs.GetConstructors().First());

        var propChangedEventHandlerDefinition = LookupType("PropertyChangedEventHandler", systemObjectModelAssembly, systemAssembly);
        _propChangedEventHandlerReference = ModuleDefinition.ImportReference(propChangedEventHandlerDefinition);
        _propChangedEventHandlerInvokeReference = ModuleDefinition.ImportReference(propChangedEventHandlerDefinition.Methods.First(x => x.Name == "Invoke"));

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

        var typeImplementsPropertyChanged =
            type.Interfaces.Any(t => t.FullName == "System.ComponentModel.INotifyPropertyChanged");

        EventDefinition propChangedEventDefinition = null;
        FieldDefinition propChangedFieldDefinition = null;

        if (typeImplementsPropertyChanged)
        {
            propChangedEventDefinition = type.Events.First(X => X.FullName.EndsWith("::PropertyChanged"));
            propChangedFieldDefinition = type.Fields.First(X => X.FullName.EndsWith("::PropertyChanged"));
        }

        var persistedProperties = new Dictionary<PropertyDefinition, FieldReference>();
        foreach (var prop in type.Properties.Where(x => x.HasThis && !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
        {
            try
            {
                FieldReference field;
                var wasWoven = WeaveProperty(prop, type, methodTable, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition, out field);
                if (wasWoven)
                {
                    persistedProperties[prop] = field;
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
            LogError($"class {type.Name} is a RealmObject but has no persisted properties");
            return;
        }

        var objectConstructor = type.GetConstructors()
            .SingleOrDefault(c => c.Parameters.Count == 0 && c.IsPublic && !c.IsStatic);
        if (objectConstructor == null)
        {
            var nonDefaultConstructor = type.GetConstructors().First();
            var sequencePoint = nonDefaultConstructor.Body.Instructions.First().SequencePoint;
            LogErrorPoint($"class {type.Name} must have a public constructor that takes no parameters", sequencePoint);
            return;
        }

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
        objectConstructor.CustomAttributes.Add(preserveAttribute);
        preserveAttribute = new CustomAttribute(_preserveAttributeConstructorWithParams);  // recreate so has new instance
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Boolean, true));  // AllMembers
        preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Boolean, false));  // Conditional
        type.CustomAttributes.Add(preserveAttribute);
        LogDebug($"Added [Preserve] to {type.Name} and its constructor");

        var wovenAttribute = new CustomAttribute(_wovenAttributeConstructor);
        TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor, persistedProperties);
        wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_system_Type, helperType));
        type.CustomAttributes.Add(wovenAttribute);
        Debug.WriteLine(string.Empty);
    }


    private bool WeaveProperty(PropertyDefinition prop, TypeDefinition type, Dictionary<string, Tuple<MethodReference, MethodReference>> methodTable,
                               bool typeImplementsPropertyChanged, EventDefinition propChangedEventDefinition,
                               FieldDefinition propChangedFieldDefinition, out FieldReference backingField)
    {
        var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;

        var columnName = prop.Name;
        var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
        if (mapToAttribute != null)
            columnName = ((string)mapToAttribute.ConstructorArguments[0].Value);

        backingField = GetBackingField(prop);
        var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
        if (isIndexed && (!_indexableTypes.Contains(prop.PropertyType.FullName)))
        {
            LogErrorPoint(
                $"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}",
                sequencePoint);
            return false;
        }

        var isPrimaryKey = prop.CustomAttributes.Any(a => a.AttributeType.Name == "PrimaryKeyAttribute");
        if (isPrimaryKey && (!_primaryKeyTypes.Contains(prop.PropertyType.FullName)))
        {
            LogErrorPoint(
                $"{type.Name}.{prop.Name} is marked as [PrimaryKey] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}",
                sequencePoint);
            return false;
        }

        if (!prop.IsAutomatic())
        {
            if (prop.PropertyType.Resolve().BaseType.IsSameAs(_realmObject))
                LogWarningPoint(
                    $"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship",
                    sequencePoint);
            return false;
        }
        if (_typeTable.ContainsKey(prop.PropertyType.FullName))
        {
            // If the property is automatic but doesn't have a setter, we should still ignore it.
            if (prop.SetMethod == null)
                return false;

            var typeId = prop.PropertyType.FullName + (isPrimaryKey ? " unique" : string.Empty);
            if (!methodTable.ContainsKey(typeId))
            {
                var getter = LookupMethodAndImport(_realmObject, "Get" + _typeTable[prop.PropertyType.FullName] + "Value");
                var setter = LookupMethodAndImport(_realmObject,
                    "Set" + _typeTable[prop.PropertyType.FullName] + "Value" + (isPrimaryKey ? "Unique" : string.Empty));
                methodTable[typeId] = Tuple.Create(getter, setter);
            }

            ReplaceGetter(prop, columnName, methodTable[typeId].Item1);
            ReplaceSetter(prop, backingField, columnName, methodTable[typeId].Item2, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
        }

        // treat IList and RealmList similarly but IList gets a default so is useable as standalone
        // IList or RealmList allows people to declare lists only of _realmObject due to the class definition
        else if (IsIList(prop))
        {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (!elementType.Resolve().BaseType.IsSameAs(_realmObject))
            {
                LogWarningPoint(
                    $"SKIPPING {type.Name}.{columnName} because it is an IList but its generic type is not a RealmObject subclass, so will not persist",
                    sequencePoint);
                return false;
            }

            if (prop.SetMethod != null)
            {
                LogErrorPoint(
                    $"{type.Name}.{columnName} has a setter but its type is a IList which only supports getters",
                    sequencePoint);
                return false;
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
        else if (IsRealmList(prop))
        {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (prop.SetMethod != null)
            {
                LogErrorPoint(
                    $"{type.Name}.{columnName} has a setter but its type is a RealmList which only supports getters",
                    sequencePoint);
                return false;
            }

            ReplaceGetter(prop, columnName,
                new GenericInstanceMethod(_genericGetListValueReference) { GenericArguments = { elementType } });
        }
        else if (prop.PropertyType.Resolve().BaseType.IsSameAs(_realmObject))
        {
            if (!prop.IsAutomatic())
            {
                LogWarningPoint(
                    $"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship",
                    sequencePoint);
                return false;
            }

            ReplaceGetter(prop, columnName,
                new GenericInstanceMethod(_genericGetObjectValueReference) { GenericArguments = { prop.PropertyType } });
            ReplaceSetter(prop, backingField, columnName,
                new GenericInstanceMethod(_genericSetObjectValueReference) { GenericArguments = { prop.PropertyType } },
                typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
            // with casting in the _realmObject methods, should just work
        }
        else if (prop.PropertyType.FullName == "System.DateTime")
        {
            LogErrorPoint(
                $"class '{type.Name}' field '{prop.Name}' is a DateTime which is not supported - use DateTimeOffset instead.",
                sequencePoint);
        }
        else if (prop.PropertyType.FullName == "System.Nullable`1<System.DateTime>")
        {
            LogErrorPoint(
                $"class '{type.Name}' field '{prop.Name}' is a DateTime? which is not supported - use DateTimeOffset? instead.",
                sequencePoint);
        }
        else
        {
            LogErrorPoint(
                $"class '{type.Name}' field '{columnName}' is a '{prop.PropertyType}' which is not yet supported",
                sequencePoint);
        }

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
        prop.CustomAttributes.Add(preserveAttribute);

        var wovenPropertyAttribute = new CustomAttribute(_wovenPropertyAttributeConstructor);
        prop.CustomAttributes.Add(wovenPropertyAttribute);

        Debug.WriteLine(string.Empty);

        var primaryKeyMsg = isPrimaryKey ? "[PrimaryKey]" : string.Empty;
        var indexedMsg = isIndexed ? "[Indexed]" : string.Empty;
        LogDebug($"Woven {type.Name}.{prop.Name} as a {prop.PropertyType.FullName} {primaryKeyMsg} {indexedMsg}");
        return true;
    }

    private static TypeDefinition LookupType(string typeName, params AssemblyDefinition[] assemblies)
    {
        if (typeName == null)
            throw new ArgumentNullException(nameof(typeName));

        if (assemblies.Length == 0)
            throw new ArgumentException("One or more assemblies must be specified to look up type: " + typeName, nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            var type = assembly?.MainModule.Types.FirstOrDefault(x => x.Name == typeName);

            if (type != null)
                return type;
        }

        throw new ApplicationException("Unable to find type: " + typeName);
    }

    private static MethodDefinition LookupMethod(TypeDefinition typeDefinition, string methodName)
    {
        var method = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

        if (method == null)
            throw new ApplicationException("Unable to find method: " + methodName);

        return method;
    }

    private MethodReference LookupMethodAndImport(TypeDefinition typeDefinition, string methodName)
    {
        return ModuleDefinition.ImportReference(LookupMethod(typeDefinition, methodName));
    }

    private void ReplaceGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference)
    {
        /// A synthesized property getter looks like this:
        ///   0: ldarg.0
        ///   1: ldfld <backingField>
        ///   2: ret
        /// We want to change it so it looks like this:
        ///   0: ldarg.0
        ///   1: call Realms.RealmObject.get_IsManaged
        ///   2: brfalse.s 7
        ///   3: ldarg.0
        ///   4: ldstr <columnName>
        ///   5: call Realms.RealmObject.GetValue<T>
        ///   6: ret
        ///   7: ldarg.0
        ///   8: ldfld <backingField>
        ///   9: ret
        /// This is roughly equivalent to:
        ///   if (!base.IsManaged) return this.<backingField>;
        ///   return base.GetValue<T>(<columnName>);

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
            return <backingField>;  // supplied by the generated getter OR RealmObject._CopyDataFromBackingFieldsToRow
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

    private void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference, bool weavePropertyChanged, EventDefinition propChangedEventDefinition, FieldDefinition propChangedFieldDefinition)
    {
        //// A synthesized property setter looks like this:
        ////   0: ldarg.0
        ////   1: ldarg.1
        ////   2: stfld <backingField>
        ////   3: ret
        ////   
        //// If we want to weave support for INotifyPropertyChanged as well, we want to change it so it looks like this:
        ////   0. ldarg.0
        ////   1. call Realms.RealmObject.get_IsManaged
        ////   2. ldc.i4.0
        ////   3. ceq
        ////   4. stloc.1
        ////   5. ldloc.1
        ////   6. brfalse.s 11
        ////   7. ldarg.0
        ////   8. ldarg.1
        ////   9. stfld <backingField>
        ////   10. br.s 15
        ////   11. ldarg.0
        ////   12. ldstr <columnName>
        ////   13. ldarg.1
        ////   14. call Realms.RealmObject.SetValue<T>
        ////   15. ldarg.0
        ////   16. ldfld PropertyChanged
        ////   17. stloc.0
        ////   18. ldloc.0
        ////   19. ldnull
        ////   20. cgt.un
        ////   21. stloc.2
        ////   22. ldloc.2
        ////   23. brfalse.s 30
        ////   24. ldarg.0
        ////   25. ldfld PropertyChanged
        ////   26. ldarg.0
        ////   27. ldstr <columnName>
        ////   28. newobj PropertyChangedEventArgs
        ////   29. callvirt PropertyChangedEventHandler.Invoke
        ////   30. ret
        ////   
        //// This is roughly equivalent to:
        ////   if (!base.IsManaged) this.<backingField> = value;
        ////   else base.SetValue<T>(<columnName>, value);
        ////     
        ////   if (PropertyChanged != null)
        ////     PropertyChanged(this, new PropertyChangedEventArgs(<columnName>);
        ////  
        //// If we want to only weave support for Realm (without INotifyPropertyChanged), we want to change it so it looks like this:
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
            throw new ArgumentNullException(nameof(setValueReference));

        if (!weavePropertyChanged)
        {
            var start = prop.SetMethod.Body.Instructions.First();
            var il = prop.SetMethod.Body.GetILProcessor();

            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Call, _realmObjectIsManagedGetter));
            il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(start, il.Create(OpCodes.Call, setValueReference));
            il.InsertBefore(start, il.Create(OpCodes.Ret));
        }
        else
        {
            if (propChangedEventDefinition == null)
                throw new ArgumentNullException(nameof(propChangedEventDefinition));

            if (propChangedFieldDefinition == null)
                throw new ArgumentNullException(nameof(propChangedFieldDefinition));

            if (_realmObjectIsManagedGetter == null)
                throw new ArgumentNullException(nameof(_realmObjectIsManagedGetter));

            if (setValueReference == null)
                throw new ArgumentNullException(nameof(setValueReference));

            if (_propChangedEventArgsConstructor == null)
                throw new ArgumentNullException(nameof(_propChangedEventArgsConstructor));

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
                prop.CustomAttributes.Add(new CustomAttribute(_propChangedDoNotNotifyAttributeConstructorDefinition));

            if (_system_Boolean == null)
                throw new ApplicationException("System_Boolean is null");

            prop.SetMethod.Body.Variables.Add(new VariableDefinition("handler", _propChangedEventHandlerReference));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(_system_Boolean));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(_system_Boolean));

            var ret = il.Create(OpCodes.Ret);

            /*
                ldarg.0
                call instance bool TestingILGeneration._realmObject::get_IsManaged()
                ldc.i4.0
                ceq
                stloc.1
                ldloc.1
                brfalse.s IL_0017
            */
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, _realmObjectIsManagedGetter));
            il.Append(il.Create(OpCodes.Ldc_I4_0));
            il.Append(il.Create(OpCodes.Ceq));
            il.Append(il.Create(OpCodes.Stloc_1));
            il.Append(il.Create(OpCodes.Ldloc_1));
            var jumpToLabelA = il.Create(OpCodes.Nop);
            il.Append(jumpToLabelA); // Jump to A

            /*
                ldarg.0
                ldarg.1
                stfld int32 TestingILGeneration.TestClass1::_myProperty
                br.s IL_0024
            */
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Stfld, backingField));
            var jumpToLabelB = il.Create(OpCodes.Nop);
            il.Append(jumpToLabelB);

            /*
                ldarg.0
                ldstr "MyProperty"
                ldarg.1
                call instance void TestingILGeneration._realmObject::SetValue<int32>(string, !!0)
            */
            var labelA = il.Create(OpCodes.Ldarg_0);
            il.Append(labelA); /* A */
            il.Append(il.Create(OpCodes.Ldstr, columnName));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call, setValueReference));

            /*
                ldarg.0
                ldfld class [mscorlib]System.EventHandler`1<class [System]System.ComponentModel.PropertyChangedEventArgs> TestingILGeneration.TestClass1::PropertyChanged
                stloc.0
                ldloc.0
                ldnull
                cgt.un
                stloc.2
                ldloc.2
                brfalse.s IL_004a
            */
            var labelB = il.Create(OpCodes.Ldarg_0);
            il.Append(labelB); /* B */
            il.Append(il.Create(OpCodes.Ldfld, propChangedFieldDefinition));
            il.Append(il.Create(OpCodes.Stloc_0));
            il.Append(il.Create(OpCodes.Ldloc_0));
            il.Append(il.Create(OpCodes.Ldnull));
            il.Append(il.Create(OpCodes.Cgt_Un));
            il.Append(il.Create(OpCodes.Stloc_2));
            il.Append(il.Create(OpCodes.Ldloc_2));
            il.Append(il.Create(OpCodes.Brfalse, ret)); /* JUMP TO RET */

            /*
                ldarg.0
                ldfld class [mscorlib]System.EventHandler`1<class [System]System.ComponentModel.PropertyChangedEventArgs> TestingILGeneration.TestClass1::PropertyChanged
                ldarg.0
                ldstr "MyProperty"
                newobj instance void [System]System.ComponentModel.PropertyChangedEventArgs::.ctor(string)
                callvirt instance void class [mscorlib]System.EventHandler`1<class [System]System.ComponentModel.PropertyChangedEventArgs>::Invoke(object, !0)
            */
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldfld, propChangedFieldDefinition));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldstr, columnName));
            il.Append(il.Create(OpCodes.Newobj, _propChangedEventArgsConstructor));
            il.Append(il.Create(OpCodes.Callvirt, _propChangedEventHandlerInvokeReference));

            // Replace jumps above now that we've injected everything.
            il.Replace(jumpToLabelA, il.Create(OpCodes.Brfalse, labelA));
            il.Replace(jumpToLabelB, il.Create(OpCodes.Br, labelB));

            // Finish with a return.
            il.Append(ret);

            // Let Cecil optimize things for us. 
            prop.SetMethod.Body.OptimizeMacros();
        }

        Debug.Write("[set] ");
    }

    private static FieldReference GetBackingField(PropertyDefinition property)
    {
        return property.GetMethod.Body.Instructions
            .Where(o => o.OpCode == OpCodes.Ldfld)
            .Select(o => o.Operand)
            .OfType<FieldReference>()
            .SingleOrDefault();
    }

    private static bool IsIList(PropertyDefinition property)
    {
        return property.PropertyType.Name == "IList`1" && property.PropertyType.Namespace == "System.Collections.Generic";
    }

    private static bool IsRealmList(PropertyDefinition property)
    {
        return property.PropertyType.Name == "RealmList`1" && property.PropertyType.Namespace == "Realms";
    }

    private static bool IsDateTimeOffset(PropertyDefinition property)
    {
        return property.PropertyType.Name == "DateTimeOffset" && property.PropertyType.Namespace == "System";
    }

    private static bool IsNullable(PropertyDefinition property)
    {
        return property.PropertyType.Name.Contains("Nullable`1") && property.PropertyType.Namespace == "System";
    }

    private static bool IsSingle(PropertyDefinition property)
    {
        return property.PropertyType.Name == "Single" && property.PropertyType.Namespace == "System";
    }

    private static bool IsDouble(PropertyDefinition property)
    {
        return property.PropertyType.Name == "Double" && property.PropertyType.Namespace == "System";
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

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor, IDictionary<PropertyDefinition, FieldReference> properties)
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
                if (castInstance.field != default(fieldType))
                {
                    castInstance.Property = castInstance.Field;
                }

                *foreach* list woven property in castInstance's schema
                var list = castInstance.field;
                castInstance.field = null;
                for (var i = 0; i < list.Count; i++)
                {
                    castInstance.Property.Add(list[i]);
                }
            */

            var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, ModuleDefinition.ImportReference(_realmObject));
            copyToRealm.Parameters.Add(instanceParameter);

            copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(realmObjectType)));

            byte currentStloc = 1;
            if (properties.Any(p => IsDateTimeOffset(p.Key)))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(_system_DateTimeOffset));
                currentStloc++;
            }

            foreach (var kvp in properties.Where(kvp => IsIList(kvp.Key) || IsRealmList(kvp.Key)))
            {
                copyToRealm.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(kvp.Value.FieldType)));
                copyToRealm.Body.Variables.Add(new VariableDefinition(_system_Int32));
            }

            var il = copyToRealm.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Castclass, ModuleDefinition.ImportReference(realmObjectType)));
            il.Append(il.Create(OpCodes.Stloc_0));

            foreach (var kvp in properties)
            {
                var property = kvp.Key;
                var field = kvp.Value;

                if (property.SetMethod != null)
                {
                    if (!IsNullable(property))
                    {
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));

                        if (IsDateTimeOffset(property))
                        {
                            il.Append(il.Create(OpCodes.Ldloca_S, (byte)1));
                            il.Append(il.Create(OpCodes.Initobj, field.FieldType));
                            il.Append(il.Create(OpCodes.Ldloc_1));
                            il.Append(il.Create(OpCodes.Call, _system_DatetimeOffset_Op_Inequality));
                        }
                        else if (IsSingle(property))
                        {
                            il.Append(il.Create(OpCodes.Ldc_R4, 0f));
                        }
                        else if (IsDouble(property))
                        {
                            il.Append(il.Create(OpCodes.Ldc_R8, 0.0));
                        }
                    }

                    var jumpLabel = il.Create(OpCodes.Nop);
                    il.Append(jumpLabel);
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Call, ModuleDefinition.ImportReference(property.SetMethod)));
                    var label = il.Create(OpCodes.Nop);
                    il.Append(label);

                    if (!IsNullable(property))
                    {
                        if (IsSingle(property) || IsDouble(property))
                        {
                            il.Replace(jumpLabel, il.Create(OpCodes.Beq_S, label));
                        }
                        else
                        {
                            il.Replace(jumpLabel, il.Create(OpCodes.Brfalse_S, label));
                        }
                    }
                }
                else if (IsIList(property) || IsRealmList(property))
                {
                    var propertyTypeDefinition = property.PropertyType.Resolve();
                    var genericType = (GenericInstanceType)property.PropertyType;
                    var iList_Get_ItemMethodReference = GetIListMethodReference(propertyTypeDefinition, "get_Item", genericType);
                    var iList_AddMethodReference = GetIListMethodReference(propertyTypeDefinition, "Add", genericType);
                    var iList_Get_CountMethodReference = GetIListMethodReference(propertyTypeDefinition, "get_Count", genericType);

                    var iteratorStLoc = (byte)(currentStloc + 1);
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldfld, field));

                    var jumpPlaceholder = il.Create(OpCodes.Nop);
                    il.Append(jumpPlaceholder);

                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Stloc_S, currentStloc));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldnull));
                    il.Append(il.Create(OpCodes.Stfld, field));
                    il.Append(il.Create(OpCodes.Ldc_I4_0));
                    il.Append(il.Create(OpCodes.Stloc_S, iteratorStLoc));

                    var cyclePlaceholder = il.Create(OpCodes.Nop);
                    il.Append(cyclePlaceholder);

                    var cycleStart = il.Create(OpCodes.Nop);
                    il.Append(cycleStart);

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

                    var jumpLabel = il.Create(OpCodes.Nop);
                    il.Append(jumpLabel);
                    il.Replace(jumpPlaceholder, il.Create(OpCodes.Brfalse_S, jumpLabel));

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
}