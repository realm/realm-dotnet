using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.Diagnostics;
using Tests;

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

        // Try accessing assembly to ensure that the assembly is still valid.
        try
        {
            assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            foreach (var item in e.LoaderExceptions)
                Debug.WriteLine("Loader exception: " + item.Message.ToString());

            Assert.Fail("Load failure");
        }
    }

    [Test]
    public void ShouldCreateTable()
    {
        // Arrange
        var stubConnectionProvider = new StubConnectionProvider();
        var realm = new RealmIO.Realm(stubConnectionProvider);

        // Act
        realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));

        // Assert
        Assert.That(stubConnectionProvider.HasTable("Person"));
        var table = stubConnectionProvider.Tables["Person"];
        Assert.That(table.Columns.Count, Is.EqualTo(5));
        Assert.That(table.Columns["FirstName"], Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ShouldSetPropertyInDatabase()
    {
        // Arrange
        var stubConnectionProvider = new StubConnectionProvider();
        var realm = new RealmIO.Realm(stubConnectionProvider);
        var person = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));

        // Act
        person.FirstName = "John";

        // Assert
        var table = stubConnectionProvider.Tables["Person"];
        Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("John"));
    }

    [Test]
    public void MultipleRows()
    {
        // Arrange
        var stubConnectionProvider = new StubConnectionProvider();
        var realm = new RealmIO.Realm(stubConnectionProvider);
        var person1 = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));
        var person2 = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));
        person1.FirstName = "John";

        // Act
        person2.FirstName = "Peter";
        person1.FirstName = "Joe";

        // Assert
        var table = stubConnectionProvider.Tables["Person"];
        Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("Joe"));
        Assert.That(table.Rows[1]["FirstName"], Is.EqualTo("Peter"));
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyPath,newAssemblyPath);
    }
#endif
}