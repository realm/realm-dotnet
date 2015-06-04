using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.Diagnostics;

[TestFixture]
public class WeaverTests
{
    Assembly assembly;
    string newAssemblyPath;
    string assemblyPath;

    [TestFixtureSetUp]
    public void Setup()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
        assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

        newAssemblyPath = assemblyPath.Replace(".dll", ".processed.dll");
        File.Copy(assemblyPath, newAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssemblyPath);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition
        };

        weavingTask.Execute();
        moduleDefinition.Write(newAssemblyPath);

        assembly = Assembly.LoadFile(newAssemblyPath);
    }

    [Test]
    public void ValidateHelloWorldIsInjected()
    {
        try
        {
            assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var item in e.LoaderExceptions)
                Console.WriteLine("Loader exception: " + item.Message.ToString());

            Assert.Fail("Load failure");
        }

        foreach (var t in assembly.GetTypes())
        {
            Console.WriteLine("Type: " + t.Name + " implements: " + string.Join<System.Type>(", ", t.GetInterfaces()));
        }

        var personType = assembly.GetType("AssemblyToProcess.PersonTest");
        var person = (dynamic)Activator.CreateInstance(personType);

        Assert.AreEqual("John", person.Name);
    }

    [Test]
    public void ValidateClassGeneration()
    {
        try
        {
            assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var item in e.LoaderExceptions)
                Console.WriteLine("Loader exception: " + item.Message.ToString());

            Assert.Fail("Load failure");
        }

        //var person = new AssemblyToProcess.Person();
        var personType = assembly.GetType("AssemblyToProcess.Person");
        var person = (dynamic)Activator.CreateInstance(personType);

        string fullName = person.FullName;

        Debug.WriteLine("person.FullName: " + fullName);
        //Debug.WriteLine("person.Address: " + person.Address);
        //person.FullName = "John Smith";
        //person.Address = "10 Downing Street";
    }
    
#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyPath,newAssemblyPath);
    }
#endif
}