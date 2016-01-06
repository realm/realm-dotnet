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
        #region helpers

        private static dynamic GetAutoPropertyBackingFieldValue(object o, string propertyName)
        {
            var propertyField = ((Type) o.GetType())
                .GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = propertyField.GetValue(o);
            return fieldValue;
        }

        private static void SetAutoPropertyBackingFieldValue(object o, string propertyName, object propertyValue)
        {
            var propertyField = ((Type) o.GetType())
                .GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            propertyField.SetValue(o, propertyValue);
        }

        public static object GetPropertyValue(object o, string propName)
        {
            return o.GetType().GetProperty(propName).GetValue(o, null);
        }

        public static void SetPropertyValue(object o, string propName, object propertyValue)
        {
            o.GetType().GetProperty(propName).SetValue(o, propertyValue);
        }

        #endregion

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

        [TestCase("FirstName", "Peter")]
        [TestCase("Age", 14)]
        [TestCase("Score", 10.1f)]
        [TestCase("Longitude", 123.123)]
        [TestCase("IsInteresting", true)]
        public void GetValueUnmanagedShouldGetBackindField(string propertyName, object propertyValue)
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            SetAutoPropertyBackingFieldValue(o, propertyName, propertyValue);

            // Act
            var returnedValue = GetPropertyValue(o, propertyName);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
            Assert.That(returnedValue, Is.EqualTo(propertyValue));
        }

        [TestCase("FirstName", "Peter")]
        [TestCase("Age", 14)]
        [TestCase("Score", 10.1f)]
        [TestCase("Longitude", 123.123)]
        [TestCase("IsInteresting", true)]
        public void SetValueUnmanagedShouldSetBackingField(string propertyName, object propertyValue)
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(propertyValue));
        }

        [TestCase("FirstName", "Peter")]
        [TestCase("Age", 14)]
        [TestCase("Score", 10.1f)]
        [TestCase("Longitude", 123.123)]
        [TestCase("IsInteresting", true)]
        public void GetValueManagedShouldGetQueryDatabase(string propertyName, object propertyValue)
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            o.IsManaged = true;

            // Act
            GetPropertyValue(o, propertyName);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Get" + propertyValue.GetType().Name + "Value(propertyName = \"" + propertyName + "\")"
            }));
        }

        [TestCase("FirstName", "Peter")]
        [TestCase("Age", 14, 0)]
        [TestCase("Score", 10.1f, 0)]
        [TestCase("Longitude", 123.123, 0)]
        [TestCase("IsInteresting", true, false)]
        public void SetValueManagedShouldUpdateDatabase(string propertyName, object propertyValue, object defaultPropertyValue = null)
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            o.IsManaged = true;

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Set" + propertyValue.GetType().Name + "Value(propertyName = \"" + propertyName + "\", value = " + propertyValue + ")"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
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
                "RealmObject.SetStringValue(propertyName = \"Email\", value = a@b.com)"
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