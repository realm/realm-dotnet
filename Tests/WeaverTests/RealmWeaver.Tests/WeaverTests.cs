/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using Realms;

namespace Tests
{
    [TestFixture]
    public class WeaverTests
    {
        Assembly _assembly;
        string _newAssemblyPath;
        string _assemblyPath;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            _assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
            _assemblyPath = _assemblyPath.Replace("Debug", "Release");
#endif

            _newAssemblyPath = _assemblyPath.Replace(".dll", ".processed.dll");
            File.Copy(_assemblyPath, _newAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(_newAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(_newAssemblyPath);

            _assembly = Assembly.LoadFile(_newAssemblyPath);

            // Try accessing assembly to ensure that the assembly is still valid.
            try
            {
                _assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var item in e.LoaderExceptions)
                    Debug.WriteLine("Loader exception: " + item.Message.ToString());

                Assert.Fail("Load failure");
            }
        }

        [Test]
        public void SetStringValueUnmanagedTest()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));

            // Act
            o.FirstName = "Peter";

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
        }

        [Test]
        public void SetStringValueManagedTest()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            o.IsManaged = true;

            // Act
            o.FirstName = "Peter";

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.SetStringValue(propertyName = \"FirstName\", value = Peter)"
            }));
        }

        [Test]
        public void ShouldNotWeaveIgnoredProperties()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));

            // Act
            o.IsOnline = true;

            // Assert
            Assert.That(o.LogList, Is.Empty);
        }

        [Test]
        public void ShouldFollowMapToAttribute()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            o.IsManaged = true;

            // Act
            o.Email = "a@b.com";

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.SetValue(propertyName = \"Email\", value = a@b.com)"
            }));
        }

        [Test]
        public void ShouldAddWovenAttribute()
        {
            // Arrange and act
            var personType = _assembly.GetType("AssemblyToProcess.Person");

            // Assert
            Assert.That(personType.GetCustomAttributes(typeof (WovenAttribute)).Any());
        }

        [Test, Ignore("Introduce once preserving default constructors is implemented")]
        public void ShouldAddPreserveAttribute()
        {
            // Arrange and act
            var personType = _assembly.GetType("AssemblyToProcess.Person");
            var ctor = personType.GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.That(ctor.GetCustomAttributes(typeof (PreserveAttribute)).Any());
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(_assemblyPath,_newAssemblyPath);
        }
#endif
    }
}