/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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
    public Action<string> LogInfo { get; set; }

    public Action<string, SequencePoint> LogWarningPoint { get; set; }

    public Action<string, SequencePoint> LogErrorPoint { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    TypeSystem typeSystem;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m => { };
        LogWarningPoint = (m, p) => { };
        LogErrorPoint = (m, p) => { };
    }

    IEnumerable<TypeDefinition> GetMatchingTypes()
    {
        return ModuleDefinition.GetTypes().Where(x => (x.BaseType != null && x.BaseType.Name == "RealmObject"));
    }

    bool IsRealmObject(TypeReference prop)
    {
        string leafClassName = prop.Name;
        // TODO make smart enough to cope with subclasses of classes descending from RealmObject
        // for now is good enough to cope with only direct subclasses
        var matches = ModuleDefinition.GetTypes().Where(x => (x.BaseType != null && x.BaseType.Name == "RealmObject" && x.Name == leafClassName));
        return matches.Count() == 1;
    }


    internal MethodReference MethodNamed(TypeDefinition assemblyType, string name)
    {
        return ModuleDefinition.Import(assemblyType.Methods.First(x => x.Name == name));
    }


    public void Execute()
    {
        // UNCOMMENT THIS DEBUGGER LAUNCH TO BE ABLE TO RUN A SEPARATE VS INSTANCE TO DEBUG WEAVING WHILST BUILDING
        // note that it may work better with a different VS version, eg: use VS2012 to debug a VS2015 build
        // System.Diagnostics.Debugger.Launch();  

        typeSystem = ModuleDefinition.TypeSystem;

        var assemblyToReference = ModuleDefinition.AssemblyResolver.Resolve("RealmNet");

        var realmObjectType = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "RealmObject");
        var genericGetValueReference = MethodNamed(realmObjectType, "GetValue");
        var genericSetValueReference = MethodNamed(realmObjectType, "SetValue");
        var genericGetListValueReference = MethodNamed(realmObjectType, "GetListValue");
        var genericSetListValueReference = MethodNamed(realmObjectType, "SetListValue");
        var genericGetObjectValueReference = MethodNamed(realmObjectType, "GetObjectValue");
        var genericSetObjectValueReference = MethodNamed(realmObjectType, "SetObjectValue");

        var wovenAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        var wovenAttributeConstructor = ModuleDefinition.Import(wovenAttributeClass.GetConstructors().First());

        foreach (var type in GetMatchingTypes())
        {
            Debug.WriteLine("Weaving " + type.Name);
            foreach (var prop in type.Properties.Where(x => !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
            {
                var sequencePoint = prop.GetMethod.Body.Instructions.First().SequencePoint;

                var columnName = prop.Name;
                var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
                if (mapToAttribute != null)
                    columnName = ((string)mapToAttribute.ConstructorArguments[0].Value);

                Debug.Write("  -- Property: " + prop.Name + " (column: " + columnName + ".. ");
                //TODO check if has either setter or getter and adjust accordingly - https://github.com/realm/realm-dotnet/issues/101
                if (prop.PropertyType.Namespace == "System" 
                    && (prop.PropertyType.IsPrimitive || prop.PropertyType.Name == "String" || prop.PropertyType.Name == "DateTimeOffset"))  // most common tested first
                {
                    AddGetter(prop, columnName, genericGetValueReference);
                    AddSetter(prop, columnName, genericSetValueReference);
                }
                else if (prop.PropertyType.Namespace == "RealmNet" && prop.PropertyType.Name == "RealmList`1")
                {
                    if (!prop.IsAutomatic())
                    {
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmList which normally indicates a relationship", sequencePoint);
                    }

                    // we may handle things differently here to handle init with a braced collection
                    AddGetter(prop, columnName, genericGetListValueReference);
                    AddSetter(prop, columnName, genericSetListValueReference);  
                }
                else if (IsRealmObject(prop.PropertyType))
                {
                    if (!prop.IsAutomatic())
                    {
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship", sequencePoint);
                    }

                    AddGetter(prop, columnName, genericGetObjectValueReference);
                    AddSetter(prop, columnName, genericSetObjectValueReference);  // with casting in the RealmObject methods, should just work
                }
                else {
                    LogErrorPoint($"class '{type.Name}' field '{columnName}' is a {prop.PropertyType} which is not yet supported", sequencePoint);
                }

                Debug.WriteLine("");
            }

            type.CustomAttributes.Add(new CustomAttribute(wovenAttributeConstructor));
            Debug.WriteLine("");
        }

        return;
    }


    void AddGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference)
    {
        var specializedGetValue = new GenericInstanceMethod(getValueReference);
        specializedGetValue.GenericArguments.Add(prop.PropertyType);

        prop.GetMethod.Body.Instructions.Clear();
        var getProcessor = prop.GetMethod.Body.GetILProcessor();
        getProcessor.Emit(OpCodes.Ldarg_0);
        getProcessor.Emit(OpCodes.Ldstr, columnName);
        getProcessor.Emit(OpCodes.Call, specializedGetValue);
        getProcessor.Emit(OpCodes.Ret);
        Debug.Write("[get] ");
    }


    void AddSetter(PropertyDefinition prop, string columnName, MethodReference setValueReference)
    {
        var specializedSetValue = new GenericInstanceMethod(setValueReference);
        specializedSetValue.GenericArguments.Add(prop.PropertyType);

        prop.SetMethod.Body.Instructions.Clear();
        var setProcessor = prop.SetMethod.Body.GetILProcessor();
        setProcessor.Emit(OpCodes.Ldarg_0);
        setProcessor.Emit(OpCodes.Ldstr, columnName);
        setProcessor.Emit(OpCodes.Ldarg_1);
        setProcessor.Emit(OpCodes.Call, specializedSetValue);
        setProcessor.Emit(OpCodes.Ret);

        Debug.Write("[set] ");
    }


    void AddConstructor(TypeDefinition newType)
    {
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeSystem.Void);
        var objectConstructor = ModuleDefinition.Import(typeSystem.Object.Resolve().GetConstructors().First());
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    void AddHelloWorld( TypeDefinition newType)
    {
        var method = new MethodDefinition("World", MethodAttributes.Public, typeSystem.String);
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldstr, "Hello World!!");
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }
}