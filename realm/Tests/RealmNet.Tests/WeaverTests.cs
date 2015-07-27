using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Interop.Providers;
using InteropShared;
using Mono.Cecil;
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;

namespace Tests
{
    [TestFixture]
    public class WeaverTests
    {
        Assembly assembly;
        string newAssemblyPath;
        string assemblyPath;

        [TestFixtureSetUp]
        public void FixtureSetup()
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

        [SetUp]
        public void Setup()
        {
            Realm.ActiveCoreProvider = ProviderFactory.Make();
        }

        [Test]
        public void ShouldCreateTable()
        {
            // Arrange
            var realm = Realm.GetInstance();

            // Act
            realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));

            // Assert
            Assert.That(Realm.ActiveCoreProvider.HasTable("Person"));
           // var table = coreProviderStub_.Tables["Person"];
           // Assert.That(table.Columns.Count, Is.EqualTo(5));
           // Assert.That(table.Columns["FirstName"], Is.EqualTo(typeof(string)));
        }

        [Test]
        public void ShouldSetPropertyInDatabase()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));

            // Act
            person.FirstName = "John";

            // Assert
           // var table = coreProviderStub_.Tables["Person"];
           // Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("John"));
        }

        [Test]
        public void ShouldKeepMultipleRowsSeparate()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person1 = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));
            var person2 = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));
            person1.FirstName = "John";

            // Act
            person2.FirstName = "Peter";
            person1.FirstName = "Joe";

            // Assert
           // var table = coreProviderStub_.Tables["Person"];
           // Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("Joe"));
           // Assert.That(table.Rows[1]["FirstName"], Is.EqualTo("Peter"));
        }

        [Test]
        public void ShouldFollowMapToAttributeOnProperties()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person = (dynamic)realm.CreateObject(assembly.GetType("AssemblyToProcess.Person"));

            // Act
            person.Email = "john@johnson.com";

            // Assert
           // var table = coreProviderStub_.Tables["Person"];
           // Assert.That(table.Rows[0]["Email"], Is.EqualTo("john@johnson.com"));
        }

        [Test]
        public void ShouldFollowMapToAttributeOnClasses()
        {
            // Arrange
            var realm = Realm.GetInstance();

            // Act
            realm.CreateObject(assembly.GetType("AssemblyToProcess.RemappedClass"));

            // Assert
           // Assert.That(coreProviderStub_.HasTable("RemappedTable"), "The table RemappedTable was not found");
           // Assert.That(!coreProviderStub_.HasTable("RemappedClass"), "The table RemappedClass was found though it should not exist");
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(assemblyPath,newAssemblyPath);
        }
#endif
    }
}