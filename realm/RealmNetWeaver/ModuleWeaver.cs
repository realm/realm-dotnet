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
        var genericGetValueReference = MethodNamed(realmObjectType, "GetValue");
        var genericSetValueReference = MethodNamed(realmObjectType, "SetValue");
        var getListValueReference = MethodNamed(realmObjectType, "GetListValue");
        var setListValueReference = MethodNamed(realmObjectType, "SetListValue");

        var wovenAttributeClass = assemblyToReference.MainModule.GetTypes().First(x => x.Name == "WovenAttribute");
        var wovenAttributeConstructor = ModuleDefinition.Import(wovenAttributeClass.GetConstructors().First());

        foreach (var type in GetMatchingTypes())
        {
            Debug.WriteLine("Weaving " + type.Name);
            foreach (var prop in type.Properties.Where(x => !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoreAttribute")))
            {
                var columnName = prop.Name;
                var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
                if (mapToAttribute != null)
                    columnName = ((string)mapToAttribute.ConstructorArguments[0].Value);

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
                    && (prop.PropertyType.IsPrimitive || prop.PropertyType.Name == "String"))
                {
                    AddGetter(prop, columnName, genericGetValueReference);
                    AddSetter(prop, columnName, genericSetValueReference);
                }
                else {
                    throw new NotSupportedException($"class '{type.Name}' field '{columnName}' is a {prop.PropertyType.Name} which is not yet supported");
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