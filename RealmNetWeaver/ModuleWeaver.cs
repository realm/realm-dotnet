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

    public Action<string> LogWarning { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    TypeSystem typeSystem;

    MethodReference realmObjectIsManagedGetter;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    public IEnumerable<TypeDefinition> GetMatchingTypes()
    {
         return ModuleDefinition.GetTypes().Where(x => (x.BaseType != null ? x.BaseType.Name == "RealmObject" : false));
        //return ModuleDefinition.GetTypes().Where(x => x.CustomAttributes.Any(a => a.AttributeType.Name == "RealmObjectAttribute"));
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
        realmObjectIsManagedGetter = ModuleDefinition.ImportReference(realmObjectType.Properties.Single(x => x.Name == "IsManaged").GetMethod);
        var genericGetValueReference = MethodNamed(realmObjectType, "GetValue");
        var genericSetValueReference = MethodNamed(realmObjectType, "SetValue");
        //var getListValueReference = MethodNamed(realmObjectType, "GetListValue");
        //var setListValueReference = MethodNamed(realmObjectType, "SetListValue");

        var wovenAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        var wovenAttributeConstructor = ModuleDefinition.Import(wovenAttributeClass.GetConstructors().First());

        var wovenPropertyAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenPropertyAttribute");
        var wovenPropertyAttributeConstructor = ModuleDefinition.ImportReference(wovenPropertyAttributeClass.GetConstructors().First());
        var corlib = ModuleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)ModuleDefinition.TypeSystem.CoreLibrary);
        var stringType = ModuleDefinition.ImportReference(corlib.MainModule.GetType("System.String"));

        foreach (var type in GetMatchingTypes())
        {
            Debug.WriteLine("Weaving " + type.Name);
            foreach (var prop in type.Properties.Where(x => !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
            {
                var columnName = prop.Name;
                var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
                if (mapToAttribute != null)
                    columnName = ((string)mapToAttribute.ConstructorArguments[0].Value);

                var backingField = GetBackingField(prop);

                Debug.Write("  -- Property: " + prop.Name + " (column: " + columnName + ".. ");
                //TODO check if has either setter or getter and adjust accordingly - https://github.com/realm/realm-dotnet/issues/101
                if (prop.PropertyType.Namespace == "RealmNet" && prop.PropertyType.Name == "RealmList`1")
              // TODO maybe support this?  ||prop.PropertyType.Namespace == "System.Collections.Generic" && prop.PropertyType.Name == "IList`1")
                {
                    // we may handle things differently here to handle init with a braced collection
                    AddGetter(prop, columnName, genericGetValueReference);
                    AddSetter(prop, columnName, genericSetValueReference);  // with casting in the RealmObject methods, should just work
                }
                else if (prop.PropertyType.Namespace == "System" 
                    && (prop.PropertyType.IsPrimitive || prop.PropertyType.Name == "String" || prop.PropertyType.Name == "DateTimeOffset"))
                {
                    AddGetter(prop, columnName, genericGetValueReference);
                    AddSetter(prop, columnName, genericSetValueReference);
                }
                else {
                    throw new NotSupportedException($"class '{type.Name}' field '{columnName}' is a {prop.PropertyType.Name} which is not yet supported");
                }

                var wovenPropertyAttribute = new CustomAttribute(wovenPropertyAttributeConstructor);
                wovenPropertyAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringType, backingField.Name));
                prop.CustomAttributes.Add(wovenPropertyAttribute);

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

        /// A synthesized property getter looks like this:
        ///   0: ldarg.0
        ///   1: ldfld <backingField>
        ///   2: ret
        /// We want to change it so it looks like this:
        ///   0: ldarg.0
        ///   1: call RealmNet.RealmObject.get_IsManaged
        ///   2: brfalse.s 7
        ///   3: ldarg.0
        ///   4: ldstr <columnName>
        ///   5: call RealmNet.RealmObject.GetValue<T>
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
        il.InsertBefore(start, il.Create(OpCodes.Call, realmObjectIsManagedGetter));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
        il.InsertBefore(start, il.Create(OpCodes.Call, specializedGetValue));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
    }


    void AddSetter(PropertyDefinition prop, string columnName, MethodReference setValueReference)
    {
        var specializedSetValue = new GenericInstanceMethod(setValueReference);
        specializedSetValue.GenericArguments.Add(prop.PropertyType);

        /// A synthesized property setter looks like this:
        ///   0: ldarg.0
        ///   1: ldarg.1
        ///   2: stfld <backingField>
        ///   3: ret
        /// We want to change it so it looks like this:
        ///   0: ldarg.0
        ///   1: call RealmNet.RealmObject.get_IsManaged
        ///   2: brfalse.s 8
        ///   3: ldarg.0
        ///   4: ldstr <columnName>
        ///   5: ldarg.1
        ///   6: call RealmNet.RealmObject.SetValue<T>
        ///   7: ret
        ///   8: ldarg.0
        ///   9: ldarg.1
        ///   10: stfld <backingField>
        ///   11: ret
        /// This is roughly equivalent to:
        ///   if (!base.IsManaged) this.<backingField> = value;
        ///   else base.SetValue<T>(<columnName>, value);

        var start = prop.SetMethod.Body.Instructions.First();
        var il = prop.SetMethod.Body.GetILProcessor();

        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Call, realmObjectIsManagedGetter));
        il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
        il.InsertBefore(start, il.Create(OpCodes.Ldarg_1));
        il.InsertBefore(start, il.Create(OpCodes.Call, specializedSetValue));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

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

    private static FieldReference GetBackingField(PropertyDefinition property)
    {
        return property.GetMethod.Body.Instructions
            .Where(o => o.OpCode == OpCodes.Ldfld)
            .Select(o => o.Operand)
            .OfType<FieldReference>()
            .SingleOrDefault();
    }
}