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
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

public class ModuleWeaver
{
    // Will log an informational message to MSBuild
    public Action<string> LogDebug { get; set; } = m => { };
    public Action<string> LogInfo { get; set; } = m => { };
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
    private TypeReference System_Object;
    private TypeReference System_Boolean;
    private TypeReference System_String;
    private TypeReference System_Type;
    private TypeReference System_IList;

    private MethodReference _propChangedEventArgsConstructor;
    private MethodReference _propChangedEventHandlerInvokeReference;
    private TypeReference _propChangedEventHandlerReference;

    private MethodReference _propChangedDoNotNotifyAttributeConstructorDefinition;

    private readonly Dictionary<string, string> _typeTable = new Dictionary<string, string>
    {
        {"System.String", "String"},
        {"System.Char", "Char"},
        {"System.Byte", "Byte"},
        {"System.Int16", "Int16"},
        {"System.Int32", "Int32"},
        {"System.Int64", "Int64"},
        {"System.Single", "Single"},
        {"System.Double", "Double"},
        {"System.Boolean", "Boolean"},
        {"System.DateTimeOffset", "DateTimeOffset"},
        {"System.Byte[]", "ByteArray"},
        {"System.Nullable`1<System.Char>", "NullableChar"},
        {"System.Nullable`1<System.Byte>", "NullableByte"},
        {"System.Nullable`1<System.Int16>", "NullableInt16"},
        {"System.Nullable`1<System.Int32>", "NullableInt32"},
        {"System.Nullable`1<System.Int64>", "NullableInt64"},
        {"System.Nullable`1<System.Single>", "NullableSingle"},
        {"System.Nullable`1<System.Double>", "NullableDouble"},
        {"System.Nullable`1<System.Boolean>", "NullableBoolean"},
        {"System.Nullable`1<System.DateTimeOffset>", "NullableDateTimeOffset"}
    };

    private readonly List<string> _objectIdTypes = new List<string>
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

        var wovenAttributeClass = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        _wovenAttributeConstructor = ModuleDefinition.ImportReference(wovenAttributeClass.GetConstructors().First());

        var wovenPropertyAttributeClass = _realmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenPropertyAttribute");
        _wovenPropertyAttributeConstructor = ModuleDefinition.ImportReference(wovenPropertyAttributeClass.GetConstructors().First());

        _corLib = AssemblyResolver.Resolve((AssemblyNameReference)ModuleDefinition.TypeSystem.CoreLibrary);
        System_Object = ModuleDefinition.TypeSystem.Object; 
        System_Boolean = ModuleDefinition.TypeSystem.Boolean;
        System_String = ModuleDefinition.TypeSystem.String;
        var typeTypeDefinition = _corLib.MainModule.GetType("System.Type");
        if (typeTypeDefinition == null) // For PCL's System.Type is only accessible as an ExportedType for some reason.
        {
            typeTypeDefinition = _corLib.MainModule.ExportedTypes.First(t => t.FullName == "System.Type").Resolve();
        }
        System_Type = ModuleDefinition.ImportReference(typeTypeDefinition);

        var listTypeDefinition = _corLib.MainModule.GetType("System.Collections.Generic.List`1");
        if (listTypeDefinition == null)
        {
            System_IList = ModuleDefinition.ImportReference(typeof(System.Collections.Generic.List<>));
        }
        else
        {
            System_IList = ModuleDefinition.ImportReference(listTypeDefinition);
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
                LogError( $"Unexpected error caught weaving type '{type.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}");
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

        var persistedPropertyCount = 0;
        foreach (var prop in type.Properties.Where( x => x.HasThis && !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
        {
            try
            {
                var wasWoven = WeaveProperty(prop, type, methodTable, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
                if (wasWoven) persistedPropertyCount++;
            }
            catch (Exception e)
            {
                var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;
                LogErrorPoint(
                    $"Unexpected error caught weaving property '{type.Name}.{prop.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}",
                    sequencePoint);
            }
        }

        if (persistedPropertyCount == 0)
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

        var wovenAttribute = new CustomAttribute(_wovenAttributeConstructor);
        TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor);
        wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(System_Type, helperType));
        type.CustomAttributes.Add(wovenAttribute);
        Debug.WriteLine("");
    }


    private bool WeaveProperty(PropertyDefinition prop, TypeDefinition type, Dictionary<string, Tuple<MethodReference, MethodReference>> methodTable,
        bool typeImplementsPropertyChanged, EventDefinition propChangedEventDefinition,
        FieldDefinition propChangedFieldDefinition)
    {
        var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;

        var columnName = prop.Name;
        var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
        if (mapToAttribute != null)
            columnName = ((string) mapToAttribute.ConstructorArguments[0].Value);

        var backingField = GetBackingField(prop);
        Debug.Write("  - " + prop.PropertyType.FullName + " " + prop.Name + " (Column: " + columnName + ").. ");

        var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
        if (isIndexed && (!_indexableTypes.Contains(prop.PropertyType.FullName)))
        {
            LogErrorPoint(
                $"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}",
                sequencePoint);
            return false;
        }

        var isObjectId = prop.CustomAttributes.Any(a => a.AttributeType.Name == "ObjectIdAttribute");
        if (isObjectId && (!_objectIdTypes.Contains(prop.PropertyType.FullName)))
        {
            LogErrorPoint(
                $"{type.Name}.{prop.Name} is marked as [ObjectId] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}",
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
        if (_typeTable.ContainsKey(prop.PropertyType.FullName)) {
            var typeId = prop.PropertyType.FullName + (isObjectId ? " unique" : "");
            if (!methodTable.ContainsKey(typeId)) {
                var getter = LookupMethodAndImport(_realmObject, "Get" + _typeTable[prop.PropertyType.FullName] + "Value");
                var setter = LookupMethodAndImport(_realmObject,
                    "Set" + _typeTable[prop.PropertyType.FullName] + "Value" + (isObjectId ? "Unique" : ""));
                methodTable[typeId] = Tuple.Create(getter, setter);
            }

            ReplaceGetter(prop, columnName, methodTable[typeId].Item1);
            ReplaceSetter(prop, backingField, columnName, methodTable[typeId].Item2, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
        }

        // treat IList and RealmList similarly but IList gets a default so is useable as standalone
        // IList or RealmList allows people to declare lists only of _realmObject due to the class definition
        else if (prop.PropertyType.Name == "IList`1" && prop.PropertyType.Namespace == "System.Collections.Generic") {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (!elementType.Resolve().BaseType.IsSameAs(_realmObject)) {
                LogWarningPoint(
  $"SKIPPING {type.Name}.{columnName} because it is an IList but its generic type is not a RealmObject subclass, so will not persist",
  sequencePoint);
                return false;
            }

            if (prop.SetMethod != null) {
                LogErrorPoint(
                    $"{type.Name}.{columnName} has a setter but its type is a IList which only supports getters",
                    sequencePoint);
                return false;
            }
            var concreteListType = new GenericInstanceType(System_IList) { GenericArguments = { elementType } };
            var listConstructor = concreteListType.Resolve().GetConstructors().Single(c => c.IsPublic && c.Parameters.Count == 0);
            var concreteListConstructor = listConstructor.MakeHostInstanceGeneric(elementType);

            // weaves list getter which also sets backing to List<T>, forcing it to accept us setting it post-init
            var backingDef = backingField as FieldDefinition;
            if (backingDef != null)
            {
                backingDef.Attributes &= ~FieldAttributes.InitOnly;  // without a set; auto property has this flag we must clear
            }
            ReplaceListGetter(prop, backingField, columnName,
                new GenericInstanceMethod(_genericGetListValueReference) { GenericArguments = { elementType } }, elementType,
                             ModuleDefinition.ImportReference(concreteListConstructor) );
        } else if (prop.PropertyType.Name == "RealmList`1" && prop.PropertyType.Namespace == "Realms") {
            var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
            if (prop.SetMethod != null) {
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
                new GenericInstanceMethod(_genericGetObjectValueReference) {GenericArguments = {prop.PropertyType}});
            ReplaceSetter(prop, backingField, columnName,
                new GenericInstanceMethod(_genericSetObjectValueReference) {GenericArguments = {prop.PropertyType}},
                typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
                // with casting in the _realmObject methods, should just work
        }
        else if (prop.PropertyType.FullName == "System.DateTime")
        {
            LogErrorPoint(
                $"class '{type.Name}' field '{prop.Name}' is a DateTime which is not supported - use DateTimeOffset instead.",
                sequencePoint);
        }
        else
        {
            LogErrorPoint(
                $"class '{type.Name}' field '{columnName}' is a '{prop.PropertyType}' which is not yet supported",
                sequencePoint);
        }

        var wovenPropertyAttribute = new CustomAttribute(_wovenPropertyAttributeConstructor);
        wovenPropertyAttribute.ConstructorArguments.Add(new CustomAttributeArgument(System_String, backingField.Name));
        prop.CustomAttributes.Add(wovenPropertyAttribute);

        Debug.WriteLine("");
        return true;
    }

    private TypeDefinition LookupType(string typeName, params AssemblyDefinition[] assemblies)
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

    private MethodDefinition LookupMethod(TypeDefinition typeDefinition, string methodName)
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

    void ReplaceGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference)
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
    void ReplaceListGetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference getListValueReference, TypeReference elementType, MethodReference listConstructor)
    {
        /// A synthesized property getter looks like this:
        ///   0: ldarg.0  // load the this pointer
        ///   1: ldfld <backingField>
        ///   2: ret
        /// We want to change it so it looks somewhat like this, in C#
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
        il.InsertBefore(start, il.Create(OpCodes.Ldfld, backingField)); //  [ this -> field]
        il.InsertBefore(start, il.Create(OpCodes.Ldnull)); // [field -> field, null]
        il.InsertBefore(start, il.Create(OpCodes.Ceq));  // [field, null -> bool result]
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));  // []

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for stfld in both branches [ -> this ]
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for call [ this -> this, this]
        il.InsertBefore(start, il.Create(OpCodes.Call, _realmObjectIsManagedGetter));  // [ this, this -> this,  isManaged ]

        // push in the label then go relative to that - so we can forward-ref the lable insert if/else blocks backwards

        var labelElse = il.Create(OpCodes.Nop);  //  [this]
        il.InsertBefore(start, labelElse); // else 
        il.InsertBefore(start, il.Create(OpCodes.Newobj, listConstructor)); // [this ->  this, listRef ]
        il.InsertBefore(start, il.Create(OpCodes.Stfld, backingField));  //  [this, listRef -> ]
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
        //TODO prop.SetMethod.Body.OptimizeMacros();

        Debug.Write("[get list] ");
    }

    void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference, bool weavePropertyChanged, EventDefinition propChangedEventDefinition, FieldDefinition propChangedFieldDefinition)
    {
        /// A synthesized property setter looks like this:
        ///   0: ldarg.0
        ///   1: ldarg.1
        ///   2: stfld <backingField>
        ///   3: ret
        ///   
        /// If we want to weave support for INotifyPropertyChanged as well, we want to change it so it looks like this:
        ///   0. ldarg.0
        ///   1. call Realms.RealmObject.get_IsManaged
        ///   2. ldc.i4.0
        ///   3. ceq
        ///   4. stloc.1
        ///   5. ldloc.1
        ///   6. brfalse.s 11
        ///   7. ldarg.0
        ///   8. ldarg.1
        ///   9. stfld <backingField>
        ///   10. br.s 15
        ///   11. ldarg.0
        ///   12. ldstr <columnName>
        ///   13. ldarg.1
        ///   14. call Realms.RealmObject.SetValue<T>
        ///   15. ldarg.0
        ///   16. ldfld PropertyChanged
        ///   17. stloc.0
        ///   18. ldloc.0
        ///   19. ldnull
        ///   20. cgt.un
        ///   21. stloc.2
        ///   22. ldloc.2
        ///   23. brfalse.s 30
        ///   24. ldarg.0
        ///   25. ldfld PropertyChanged
        ///   26. ldarg.0
        ///   27. ldstr <columnName>
        ///   28. newobj PropertyChangedEventArgs
        ///   29. callvirt PropertyChangedEventHandler.Invoke
        ///   30. ret
        ///   
        /// This is roughly equivalent to:
        ///   if (!base.IsManaged) this.<backingField> = value;
        ///   else base.SetValue<T>(<columnName>, value);
        ///     
        ///   if (PropertyChanged != null)
        ///     PropertyChanged(this, new PropertyChangedEventArgs(<columnName>);
        ///  
        /// If we want to only weave support for Realm (without INotifyPropertyChanged), we want to change it so it looks like this:
        ///   0: ldarg.0
        ///   1: call Realms.RealmObject.get_IsManaged
        ///   2: brfalse.s 8
        ///   3: ldarg.0
        ///   4: ldstr <columnName>
        ///   5: ldarg.1
        ///   6: call Realms.RealmObject.SetValue<T>
        ///   7: ret
        ///   8: ldarg.0
        ///   9: ldarg.1
        ///   10: stfld <backingField>
        ///   11: ret
        ///   
        /// This is roughly equivalent to:
        ///   if (!base.IsManaged) this.<backingField> = value;
        ///   else base.SetValue<T>(<columnName>, value);

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

            if (System_Boolean == null)
                throw new ApplicationException("System_Boolean is null");

            prop.SetMethod.Body.Variables.Add(new VariableDefinition("handler", _propChangedEventHandlerReference));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(System_Boolean));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(System_Boolean));

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

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor)
    {
        var helperType = new TypeDefinition(null, "RealmHelper",
                                            TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                            System_Object);

        helperType.Interfaces.Add(ModuleDefinition.ImportReference(_realmAssembly.MainModule.GetType("Realms.Weaving.IRealmObjectHelper")));

        var createInstance = new MethodDefinition("CreateInstance", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.ImportReference(_realmObject));
        {
            var il = createInstance.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, objectConstructor);
            il.Emit(OpCodes.Ret);
        }
        helperType.Methods.Add(createInstance);

        const MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var ctor = new MethodDefinition(".ctor", ctorAttributes, ModuleDefinition.TypeSystem.Void);
        {
            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ModuleDefinition.ImportReference(System_Object.Resolve().GetConstructors().Single()));
            il.Emit(OpCodes.Ret);
        }

        var preserveAttribute = new CustomAttribute(_preserveAttributeConstructor);
        ctor.CustomAttributes.Add(preserveAttribute);

        helperType.Methods.Add(ctor);

        realmObjectType.NestedTypes.Add(helperType);

        return helperType;
    }
}