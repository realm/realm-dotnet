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

    MethodReference realmObjectIsManagedGetter;

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
        //System.Diagnostics.Debugger.Launch();  

        typeSystem = ModuleDefinition.TypeSystem;

        var assemblyToReference = ModuleDefinition.AssemblyResolver.Resolve("RealmNet");

        var realmObjectType = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "RealmObject");
        realmObjectIsManagedGetter = ModuleDefinition.ImportReference(realmObjectType.Properties.Single(x => x.Name == "IsManaged").GetMethod);
        var genericGetValueReference = MethodNamed(realmObjectType, "GetValue");
        var genericSetValueReference = MethodNamed(realmObjectType, "SetValue");
        var genericGetListValueReference = MethodNamed(realmObjectType, "GetListValue");
        var genericSetListValueReference = MethodNamed(realmObjectType, "SetListValue");
        var genericGetObjectValueReference = MethodNamed(realmObjectType, "GetObjectValue");
        var genericSetObjectValueReference = MethodNamed(realmObjectType, "SetObjectValue");

        var wovenAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        var wovenAttributeConstructor = ModuleDefinition.Import(wovenAttributeClass.GetConstructors().First());

        var wovenPropertyAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenPropertyAttribute");
        var wovenPropertyAttributeConstructor = ModuleDefinition.ImportReference(wovenPropertyAttributeClass.GetConstructors().First());
        var corlib = ModuleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)ModuleDefinition.TypeSystem.CoreLibrary);
        var stringType = ModuleDefinition.ImportReference(corlib.MainModule.GetType("System.String"));
        var listType = ModuleDefinition.ImportReference(corlib.MainModule.GetType("System.Collections.Generic.List`1"));

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

                var backingField = GetBackingField(prop);

                Debug.Write("  -- Property: " + prop.Name + " (column: " + columnName + ".. ");
                //TODO check if has either setter or getter and adjust accordingly - https://github.com/realm/realm-dotnet/issues/101
                if (prop.PropertyType.Namespace == "System" 
                    && (prop.PropertyType.IsPrimitive || prop.PropertyType.Name == "String" || prop.PropertyType.Name == "DateTimeOffset"))  // most common tested first
                {
                    if (!prop.IsAutomatic())
                        continue;

                    ReplaceGetter(prop, columnName, new GenericInstanceMethod(genericGetValueReference) { GenericArguments = { prop.PropertyType } });
                    ReplaceSetter(prop, columnName, new GenericInstanceMethod(genericSetValueReference) { GenericArguments = { prop.PropertyType } });
                }
                else if (prop.PropertyType.Name == "RealmList`1" && prop.PropertyType.Namespace == "RealmNet")
                {
                    // RealmList allows people to declare lists only of RealmObject due to the class definition
                    if (!prop.IsAutomatic())
                    {
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmList which normally indicates a relationship", sequencePoint);
                        continue;
                    }

                    // we may handle things differently here to handle init with a braced collection
                    var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
                    ReplaceGetter(prop, columnName, new GenericInstanceMethod(genericGetListValueReference) { GenericArguments = { elementType } });
                    ReplaceSetter(prop, columnName, new GenericInstanceMethod(genericSetListValueReference) { GenericArguments = { elementType } });  
                }
                else if (prop.PropertyType.Name == "IList`1" && prop.PropertyType.Namespace == "System.Collections.Generic")
                {
                    // only handle `IList<T> Foo { get; }` properties
                    if (prop.IsAutomatic() && prop.SetMethod == null)
                    {
                        var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
                        var concreteListType = new GenericInstanceType(listType) { GenericArguments = { elementType } };
                        var listConstructor = concreteListType.Resolve().GetConstructors().Single(c => c.IsPublic && c.Parameters.Count == 0);
                        var concreteListConstructor = listConstructor.MakeHostInstanceGeneric(elementType);

                        foreach (var ctor in type.GetConstructors())
                        {
                            PrependListFieldInitializerToConstructor(backingField, ctor, ModuleDefinition.ImportReference(concreteListConstructor));
                        }
                    }
                }
                else if (IsRealmObject(prop.PropertyType))
                {
                    if (!prop.IsAutomatic())
                    {
                        LogWarningPoint($"{type.Name}.{columnName} is not an automatic property but its type is a RealmObject which normally indicates a relationship", sequencePoint);
                        continue;
                    }

                    ReplaceGetter(prop, columnName, new GenericInstanceMethod(genericGetObjectValueReference) { GenericArguments = { prop.PropertyType } });
                    ReplaceSetter(prop, columnName, new GenericInstanceMethod(genericSetObjectValueReference) { GenericArguments = { prop.PropertyType } });  // with casting in the RealmObject methods, should just work
                }
                else if (prop.PropertyType.Name == "DateTime" && prop.PropertyType.Namespace == "System") {
                    LogErrorPoint($"class '{type.Name}' field '{columnName}' is a DateTime which is not supported - use DateTimeOffset instead.", sequencePoint);
                }
                else {
                    LogErrorPoint($"class '{type.Name}' field '{columnName}' is a '{prop.PropertyType}' which is not yet supported", sequencePoint);
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
        il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));
        il.InsertBefore(start, il.Create(OpCodes.Ret));

        Debug.Write("[get] ");
    }

    void ReplaceSetter(PropertyDefinition prop, string columnName, MethodReference setValueReference)
    {
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
        il.InsertBefore(start, il.Create(OpCodes.Call, setValueReference));
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