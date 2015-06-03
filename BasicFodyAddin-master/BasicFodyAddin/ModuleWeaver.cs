using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Collections.Generic;

public class ModuleWeaver
{
    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }

    public Action<string> LogWarning { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    TypeSystem typeSystem;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    public IEnumerable<TypeDefinition> GetMachingTypes()
    {
        return ModuleDefinition.GetTypes().Where(x => x.IsInterface);
    }

    public void Execute()
    {
        typeSystem = ModuleDefinition.TypeSystem;

        foreach (var type in GetMachingTypes())
        {
            var className = type.Name.Substring(1);
            var newType = new TypeDefinition(null, className, TypeAttributes.Public, typeSystem.Object);
            newType.Interfaces.Add(type);
            newType.Namespace = type.Namespace;

            AddConstructor(newType);

            foreach (var prop in type.Properties)
            {
                var getterName = "get_" + prop.Name;
                var getter = new MethodDefinition(getterName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, typeSystem.String);
                var getterProcessor = getter.Body.GetILProcessor();
                getterProcessor.Emit(OpCodes.Ldstr, "John");
                getterProcessor.Emit(OpCodes.Ret);
                getter.Overrides.Add(prop.GetMethod);
                newType.Methods.Add(getter);

                var instanceProp = new PropertyDefinition(prop.Name, PropertyAttributes.None, prop.PropertyType);
                instanceProp.GetMethod = getter;
                newType.Properties.Add(instanceProp);
            }

            ModuleDefinition.Types.Add(newType);
        }

        return;

        //var newType = new TypeDefinition(null, "Person", TypeAttributes.Public, typeSystem.Object);

        //AddConstructor(newType);


        //LogInfo("Added type 'Hello' with method 'World'.");
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