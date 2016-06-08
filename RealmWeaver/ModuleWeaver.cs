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
using System.Collections;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

public class ModuleWeaver
{
    public Action<string> LogDebug { get; set; }

    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }

    public Action<string, SequencePoint> LogWarningPoint { get; set; }

    public Action<string, SequencePoint> LogErrorPoint { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    private AssemblyDefinition RealmAssembly;
    private TypeDefinition RealmObject;
    private MethodReference RealmObjectIsManagedGetter;

    private AssemblyDefinition CorLib;
    private TypeDefinition System_Object;
    private TypeDefinition System_Boolean;
    private TypeReference System_String;
    private TypeReference System_Type;

    private MethodReference PropChangedEventArgsConstructor;
    private MethodReference PropChangedEventHandlerInvokeReference;
    private TypeReference PropChangedEventHandlerReference;

    private MethodReference PropChangedDoNotNotifyAttributeConstructorDefinition;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogDebug = m => { };
        LogInfo = m => { };
        LogWarningPoint = (m, p) => { };
        LogErrorPoint = (m, p) => { };
    }

    IEnumerable<TypeDefinition> GetMatchingTypes()
    {
        foreach (var type in ModuleDefinition.GetTypes().Where(t => t.IsDescendedFrom(RealmObject)))
        {
            if (type.BaseType.IsSameAs(RealmObject))
            {
                yield return type;
            }
            else
            {
                LogErrorPoint($"The type {type.FullName} indirectly inherits from RealmObject which is not supported", type.GetConstructors().FirstOrDefault()?.Body?.Instructions?.First()?.SequencePoint);
            }
        }
    }

    bool IsRealmObject(TypeReference prop)
    {
        string leafClassName = prop.Name;
        // TODO make smart enough to cope with subclasses of classes descending from RealmObject
        // for now is good enough to cope with only direct subclasses
        var matches = ModuleDefinition.GetTypes().Where(x => (x.BaseType != null && x.BaseType.Name == "RealmObject" && x.Name == leafClassName));
        return matches.Count() == 1;
    }

    public void Execute()
    {
        // UNCOMMENT THIS DEBUGGER LAUNCH TO BE ABLE TO RUN A SEPARATE VS INSTANCE TO DEBUG WEAVING WHILST BUILDING
        // Debugger.Launch();  

        var submitAnalytics = System.Threading.Tasks.Task.Factory.StartNew (() => {
            var analytics = new RealmWeaver.Analytics(ModuleDefinition);
            try {
                analytics.SubmitAnalytics();
            } catch(Exception e) {
                LogDebug("Error submitting analytics: " + e.Message);
            }
        });

        RealmAssembly = ModuleDefinition.AssemblyResolver.Resolve("Realm");  // Note that the assembly is Realm but the namespace Realms with the s

        RealmObject = RealmAssembly.MainModule.GetTypes().First(x => x.Name == "RealmObject");
        RealmObjectIsManagedGetter = ModuleDefinition.ImportReference(RealmObject.Properties.Single(x => x.Name == "IsManaged").GetMethod);
        
        var typeTable = new Dictionary<string, string>()
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

        // Cache of getter and setter methods for the various types.
        var methodTable = new Dictionary<string, Tuple<MethodReference, MethodReference>>();

        var objectIdTypes = new List<string>
        {
            "System.String",
            "System.Char",
            "System.Byte",
            "System.Int16",
            "System.Int32",
            "System.Int64",
        };

        var indexableTypes = new List<string>(objectIdTypes);
        indexableTypes.Add("System.Boolean");
        indexableTypes.Add("System.DateTimeOffset");

        var genericGetObjectValueReference = LookupMethodAndImport(RealmObject, "GetObjectValue");
        var genericSetObjectValueReference = LookupMethodAndImport(RealmObject, "SetObjectValue");
        var genericGetListValueReference = LookupMethodAndImport(RealmObject, "GetListValue");

        var wovenAttributeClass = RealmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        var wovenAttributeConstructor = ModuleDefinition.ImportReference(wovenAttributeClass.GetConstructors().First());

        var wovenPropertyAttributeClass = RealmAssembly.MainModule.GetTypes().First(x => x.Name == "WovenPropertyAttribute");
        var wovenPropertyAttributeConstructor = ModuleDefinition.ImportReference(wovenPropertyAttributeClass.GetConstructors().First());

        CorLib = ModuleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)ModuleDefinition.TypeSystem.CoreLibrary);
        System_Object = CorLib.MainModule.GetType("System.Object");
        System_Boolean = CorLib.MainModule.GetType("System.Boolean");
        System_String = ModuleDefinition.ImportReference(CorLib.MainModule.GetType("System.String"));
        System_Type = ModuleDefinition.ImportReference(CorLib.MainModule.GetType("System.Type"));
        // WARNING the GetType("System.Collections.Generic.List`1") below RETURNS NULL WHEN COMPILING A PCL
        // UNUSED SO COMMENT OUT var listType = ModuleDefinition.ImportReference(CorLib.MainModule.GetType("System.Collections.Generic.List`1"));

        var systemAssembly = ModuleDefinition.AssemblyResolver.Resolve("System");
        var systemObjectModelAssembly = TryResolveAssembly("System.ObjectModel");

        var propertyChangedEventArgs = LookupType("PropertyChangedEventArgs", systemObjectModelAssembly, systemAssembly);
        PropChangedEventArgsConstructor = ModuleDefinition.ImportReference(propertyChangedEventArgs.GetConstructors().First());

        var propChangedEventHandlerDefinition = LookupType("PropertyChangedEventHandler", systemObjectModelAssembly, systemAssembly);
        PropChangedEventHandlerReference = ModuleDefinition.ImportReference(propChangedEventHandlerDefinition);
        PropChangedEventHandlerInvokeReference = ModuleDefinition.ImportReference(propChangedEventHandlerDefinition.Methods.First(x => x.Name == "Invoke"));

        // If the solution has a reference to PropertyChanged.Fody, let's look up the DoNotNotifyAttribute for use later.
        var usesPropertyChangedFody = ModuleDefinition.AssemblyReferences.Any(X => X.Name == "PropertyChanged");
        if (usesPropertyChangedFody)
        {
            var propChangedAssembly = ModuleDefinition.AssemblyResolver.Resolve("PropertyChanged");
            var doNotNotifyAttributeDefinition = propChangedAssembly.MainModule.GetTypes().First(X => X.Name == "DoNotNotifyAttribute");
            PropChangedDoNotNotifyAttributeConstructorDefinition = ModuleDefinition.ImportReference(doNotNotifyAttributeDefinition.GetConstructors().First());
        }

        Debug.WriteLine("Weaving file: " + ModuleDefinition.FullyQualifiedName);

        foreach (var type in GetMatchingTypes())
        {
            if (type == null) {
                Debug.WriteLine("Weaving skipping null type from GetMatchingTypes");
                continue;
            }
            Debug.WriteLine("Weaving " + type.Name);

            var typeImplementsPropertyChanged = type.Interfaces.Any(t => t.FullName == "System.ComponentModel.INotifyPropertyChanged");

            EventDefinition propChangedEventDefinition = null;
            FieldDefinition propChangedFieldDefinition = null;

            if (typeImplementsPropertyChanged)
            {
                propChangedEventDefinition = type.Events.First(X => X.FullName.EndsWith("::PropertyChanged"));
                propChangedFieldDefinition = type.Fields.First(X => X.FullName.EndsWith("::PropertyChanged"));
            }

            foreach (var prop in type.Properties.Where(x => x.HasThis && !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
            {
                var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;

                var columnName = prop.Name;
                var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
                if (mapToAttribute != null)
                    columnName = ((string)mapToAttribute.ConstructorArguments[0].Value);

                var backingField = GetBackingField(prop);

                Debug.Write("  - " + prop.PropertyType.FullName + " " + prop.Name + " (Column: " + columnName + ").. ");

                var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
                if (isIndexed && (!indexableTypes.Contains(prop.PropertyType.FullName)))
                {
                    LogErrorPoint($"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}", sequencePoint);
                    continue;
                }

                var isObjectId = prop.CustomAttributes.Any(a => a.AttributeType.Name == "ObjectIdAttribute");
                if (isObjectId && (!objectIdTypes.Contains(prop.PropertyType.FullName)))
                {
                    LogErrorPoint($"{type.Name}.{prop.Name} is marked as [ObjectId] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}", sequencePoint);
                    continue;
                }

                if (!prop.IsAutomatic())
                {
                    if (IsRealmObject(prop.PropertyType))
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship", sequencePoint);

                    Debug.WriteLine("Skipped because it's not automatic.");
                    continue;
                }
                if (typeTable.ContainsKey(prop.PropertyType.FullName))
                {
                    var typeId = prop.PropertyType.FullName + (isObjectId ? " unique" : "");
                    if (!methodTable.ContainsKey(typeId))
                    {
                        var getter = LookupMethodAndImport(RealmObject, "Get" + typeTable[prop.PropertyType.FullName] + "Value");
                        var setter = LookupMethodAndImport(RealmObject, "Set" + typeTable[prop.PropertyType.FullName] + "Value" + (isObjectId ? "Unique": ""));
                        methodTable[typeId] = Tuple.Create(getter, setter);
                    }

                    ReplaceGetter(prop, columnName, methodTable[typeId].Item1);
                    ReplaceSetter(prop, backingField, columnName, methodTable[typeId].Item2, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);
                }
//                else if (prop.PropertyType.Name == "IList`1" && prop.PropertyType.Namespace == "System.Collections.Generic")
                else if (prop.PropertyType.Name == "RealmList`1" && prop.PropertyType.Namespace == "Realms")
                {
                    // RealmList allows people to declare lists only of RealmObject due to the class definition
                    if (!prop.IsAutomatic())
                    {
                        LogErrorPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmList which normally indicates a relationship", sequencePoint);
                        continue;
                    }
                    if (prop.SetMethod != null)
                    {
                        LogErrorPoint($"{type.Name}.{columnName} has a setter but its type is a RealmList which only supports getters", sequencePoint);
                        continue;
                    }

                    var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
                    ReplaceGetter(prop, columnName, new GenericInstanceMethod(genericGetListValueReference) { GenericArguments = { elementType } });
                }
                else if (IsRealmObject(prop.PropertyType))
                {
                    if (!prop.IsAutomatic())
                    {
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship", sequencePoint);
                        continue;
                    }

                    ReplaceGetter(prop, columnName, new GenericInstanceMethod(genericGetObjectValueReference) { GenericArguments = { prop.PropertyType } });
                    ReplaceSetter(prop, backingField, columnName, new GenericInstanceMethod(genericSetObjectValueReference) { GenericArguments = { prop.PropertyType } }, typeImplementsPropertyChanged, propChangedEventDefinition, propChangedFieldDefinition);  // with casting in the RealmObject methods, should just work
                }
                else if (prop.PropertyType.FullName == "System.DateTime")
                {
                    LogErrorPoint($"class '{type.Name}' field '{prop.Name}' is a DateTime which is not supported - use DateTimeOffset instead.", sequencePoint);
                }
                else
                {
                    LogErrorPoint($"class '{type.Name}' field '{columnName}' is a '{prop.PropertyType}' which is not yet supported", sequencePoint);
                }

                var wovenPropertyAttribute = new CustomAttribute(wovenPropertyAttributeConstructor);
                wovenPropertyAttribute.ConstructorArguments.Add(new CustomAttributeArgument(System_String, backingField.Name));
                prop.CustomAttributes.Add(wovenPropertyAttribute);

                Debug.WriteLine("");
            }

            var wovenAttribute = new CustomAttribute(wovenAttributeConstructor);
            TypeReference helperType = WeaveRealmObjectHelper(type);
            wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(System_Type, helperType));
            type.CustomAttributes.Add(wovenAttribute);
            Debug.WriteLine("");
        }

        submitAnalytics.Wait();
        return;
    }

    private TypeDefinition LookupType(string typeName, params AssemblyDefinition[] assemblies)
    {
        if (typeName == null)
            throw new ArgumentNullException(nameof(typeName));

        if (assemblies.Length == 0)
            throw new ArgumentException("One or more assemblies must be specified to look up type: " + typeName, nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            if (assembly == null)
                continue;

            var type = assembly.MainModule.Types.FirstOrDefault(X => X.Name == typeName);

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

    private AssemblyDefinition TryResolveAssembly(string assemblyName)
    {
        try
        {
            return ModuleDefinition.AssemblyResolver.Resolve(assemblyName);
        }
        catch
        {
            LogInfo("Failed to resolve assembly: " + assemblyName);
            return null;
        }
    }

    void PrependListFieldInitializerToConstructor(FieldReference field, MethodDefinition constructor, MethodReference listConstructor)
    {
        var start = constructor.Body.Instructions.First();
        var il = constructor.Body.GetILProcessor();
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Newobj, listConstructor));
        il.InsertBefore(start, il.Create(OpCodes.Stfld, field));
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
        ///   else return base.GetValue<T>(<columnName>);

        var start = prop.GetMethod.Body.Instructions.First();
        var il = prop.GetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Call, RealmObjectIsManagedGetter));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
        il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
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
        ///   1. call Realm.RealmObject.get_IsManaged
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
        ///   14. call Realm.RealmObject.SetValue<T>
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
        ///   1: call Realm.RealmObject.get_IsManaged
        ///   2: brfalse.s 8
        ///   3: ldarg.0
        ///   4: ldstr <columnName>
        ///   5: ldarg.1
        ///   6: call Realm.RealmObject.SetValue<T>
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
            il.InsertBefore(start, il.Create(OpCodes.Call, RealmObjectIsManagedGetter));
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

            if (RealmObjectIsManagedGetter == null)
                throw new ArgumentNullException(nameof(RealmObjectIsManagedGetter));

            if (setValueReference == null)
                throw new ArgumentNullException(nameof(setValueReference));

            if (PropChangedEventArgsConstructor == null)
                throw new ArgumentNullException(nameof(PropChangedEventArgsConstructor));

            // Whilst we're only targetting auto-properties here, someone like PropertyChanged.Fody
            // may have already come in and rewritten our IL. Lets clear everything and start from scratch.
            var il = prop.SetMethod.Body.GetILProcessor();
            prop.SetMethod.Body.Instructions.Clear();
            prop.SetMethod.Body.Variables.Clear();

            // While we can tidy up PropertyChanged.Fody IL if we're ran after it, we can't do a heck of a lot
            // if they're the last one in.
            // To combat this, we'll check if the PropertyChanged assembly is available, and if so, attribute
            // the property such that PropertyChanged.Fody won't touch it.
            if (PropChangedDoNotNotifyAttributeConstructorDefinition != null)
                prop.CustomAttributes.Add(new CustomAttribute(PropChangedDoNotNotifyAttributeConstructorDefinition));

            if (System_Boolean == null)
                throw new ApplicationException("System_Boolean is null");

            prop.SetMethod.Body.Variables.Add(new VariableDefinition("handler", PropChangedEventHandlerReference));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(System_Boolean));
            prop.SetMethod.Body.Variables.Add(new VariableDefinition(System_Boolean));

            var ret = il.Create(OpCodes.Ret);

            /*
                ldarg.0
                call instance bool TestingILGeneration.RealmObject::get_IsManaged()
                ldc.i4.0
                ceq
                stloc.1
                ldloc.1
                brfalse.s IL_0017
            */
            il.Append(il.Create(OpCodes.Ldarg_0));                         
            il.Append(il.Create(OpCodes.Call, RealmObjectIsManagedGetter));
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
                call instance void TestingILGeneration.RealmObject::SetValue<int32>(string, !!0)
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
            il.Append(il.Create(OpCodes.Newobj, PropChangedEventArgsConstructor));
            il.Append(il.Create(OpCodes.Callvirt, PropChangedEventHandlerInvokeReference));

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

    private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType)
    {
        var helperType = new TypeDefinition(null, "RealmHelper",
                                            TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                            ModuleDefinition.ImportReference(System_Object));

        helperType.Interfaces.Add(ModuleDefinition.ImportReference(RealmAssembly.MainModule.GetType("Realms.Weaving.IRealmObjectHelper")));

        var createInstance = new MethodDefinition("CreateInstance", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, ModuleDefinition.ImportReference(RealmObject));
        {
            var il = createInstance.Body.GetILProcessor();
            il.Emit(OpCodes.Newobj, realmObjectType.GetConstructors().Single(c => c.Parameters.Count == 0 && !c.IsStatic));
            il.Emit(OpCodes.Ret);
        }
        helperType.Methods.Add(createInstance);

        const MethodAttributes ctorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var ctor = new MethodDefinition(".ctor", ctorAttributes, ModuleDefinition.TypeSystem.Void);
        {
            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ModuleDefinition.ImportReference(System_Object.GetConstructors().Single()));
            il.Emit(OpCodes.Ret);
        }

        helperType.Methods.Add(ctor);

        realmObjectType.NestedTypes.Add(helperType);

        return helperType;
    }
}