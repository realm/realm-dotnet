////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
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

        private Assembly _assembly;
        private string _newAssemblyPath;
        private string _assemblyPath;

        private readonly List<string> _warnings = new List<string>();
        private readonly List<string> _errors = new List<string>();

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
                ModuleDefinition = moduleDefinition,
                LogErrorPoint = (s, point) => _errors.Add(s),
                LogWarningPoint = (s, point) => _warnings.Add(s)
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

        [TestCase("CharProperty", '0')]
        [TestCase("ByteProperty", (byte)100)]
        [TestCase("Int16Property", (short)100)]
        [TestCase("Int32Property", 100)]
        [TestCase("Int64Property", 100L)]
        [TestCase("SingleProperty", 123.123f)]
        [TestCase("DoubleProperty", 123.123)]
        [TestCase("BooleanProperty", true)]
        [TestCase("StringProperty", "str")] 
        [TestCase("NullableCharProperty", '0')]
        [TestCase("NullableByteProperty", (byte)100)]
        [TestCase("NullableInt16Property", (short)100)]
        [TestCase("NullableInt32Property", 100)]
        [TestCase("NullableInt64Property", 100L)]
        [TestCase("NullableSingleProperty", 123.123f)] 
        [TestCase("NullableDoubleProperty", 123.123)] 
        [TestCase("NullableBooleanProperty", true)]
        public void GetValueUnmanagedShouldGetBackingField(string propertyName, object propertyValue)
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));
            SetAutoPropertyBackingFieldValue(o, propertyName, propertyValue);

            // Act
            var returnedValue = GetPropertyValue(o, propertyName);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
            Assert.That(returnedValue, Is.EqualTo(propertyValue));
        }

        [TestCase("Char", '0')]
        [TestCase("Byte", (byte)100)]
        [TestCase("Int16", (short)100)]
        [TestCase("Int32", 100)]
        [TestCase("Int64", 100L)]
        [TestCase("Single", 123.123f)]
        [TestCase("Double", 123.123)]
        [TestCase("Boolean", true)]
        [TestCase("String", "str")] 
        [TestCase("NullableChar", '0')]
        [TestCase("NullableByte", (byte)100)]
        [TestCase("NullableInt16", (short)100)]
        [TestCase("NullableInt32", 100)]
        [TestCase("NullableInt64", 100L)]
        [TestCase("NullableSingle", 123.123f)] 
        [TestCase("NullableDouble", 123.123)] 
        [TestCase("NullableBoolean", true)]
        public void SetValueUnmanagedShouldSetBackingField(string typeName, object propertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(propertyValue));
        }

        [TestCase("Char", '0')]
        [TestCase("Byte", (byte)100)]
        [TestCase("Int16", (short)100)]
        [TestCase("Int32", 100)]
        [TestCase("Int64", 100L)]
        [TestCase("Single", 123.123f)]
        [TestCase("Double", 123,123)]
        [TestCase("Boolean", true)]
        [TestCase("String", "str")] 
        [TestCase("NullableChar", '0')]
        [TestCase("NullableByte", (byte)100)]
        [TestCase("NullableInt16", (short)100)]
        [TestCase("NullableInt32", 100)]
        [TestCase("NullableInt64", 100L)]
        [TestCase("NullableSingle", 123.123f)] 
        [TestCase("NullableDouble", 123.123)] 
        [TestCase("NullableBoolean", true)]
        public void GetValueManagedShouldGetQueryDatabase(string typeName, object propertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));
            o.IsManaged = true;

            // Act
            GetPropertyValue(o, propertyName);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Get" + typeName + "Value(propertyName = \"" + propertyName + "\")"
            }));
        }

        [TestCase("Char", '0', char.MinValue)]
        [TestCase("Byte", (byte)100, (byte)0)]
        [TestCase("Int16", (short)100, (short)0)]
        [TestCase("Int32", 100, 0)]
        [TestCase("Int64", 100L, 0L)]
        [TestCase("Single", 123.123f, 0.0f)]
        [TestCase("Double", 123.123, 0.0)]
        [TestCase("Boolean", true)]
        [TestCase("String", "str", null)] 
        [TestCase("NullableChar", '0', null)]
        [TestCase("NullableByte", (byte)100, null)]
        [TestCase("NullableInt16", (short)100, null)]
        [TestCase("NullableInt32", 100, null)]
        [TestCase("NullableInt64", 100L, null)]
        [TestCase("NullableSingle", 123.123f, null)] 
        [TestCase("NullableDouble", 123.123, null)] 
        [TestCase("NullableBoolean", true, null)]
        public void SetValueManagedShouldUpdateDatabase(string typeName, object propertyValue, object defaultPropertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));
            o.IsManaged = true;

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Set" + typeName + "Value(propertyName = \"" + propertyName + "\", value = " + propertyValue + ")"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
        }


        [TestCase("Char", '0', char.MinValue)]
        [TestCase("Byte", (byte)100, (byte)0)]
        [TestCase("Int16", (short)100, (short)0)]
        [TestCase("Int32", 100, 0)]
        [TestCase("Int64", 100L, 0L)]
        [TestCase("String", "str", null)] 
        public void SettingObjectIdPropertyShouldCallSetUnique(string typeName, object propertyValue, object defaultPropertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.ObjectId" + typeName + "Object"));
            o.IsManaged = true;

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Set" + typeName + "ValueUnique(propertyName = \"" + propertyName + "\", value = " + propertyValue + ")"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
        }

        [Test]
        public void SetRelationship()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            var pn = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.PhoneNumber"));
            o.IsManaged = true;

            // Act
            o.PrimaryNumber = pn;

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.SetObjectValue(propertyName = \"PrimaryNumber\", value = AssemblyToProcess.PhoneNumber)"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, "PrimaryNumber"), Is.Null);
        }

        [Test]
        public void GetRelationship()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            o.IsManaged = true;

            // Act
            GetPropertyValue(o, "PrimaryNumber");

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.GetObjectValue(propertyName = \"PrimaryNumber\")"
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
                "RealmObject.SetStringValue(propertyName = \"Email\", value = a@b.com)"
            }));
        }

        [Test]
        public void ShouldAddWovenAttribute()
        {
            // Arrange and act
            var personType = _assembly.GetType("AssemblyToProcess.Person");

            // Assert
            Assert.That(personType.CustomAttributes.Any(a => a.AttributeType.Name == "WovenAttribute"));
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

        [Test]
        public void MatchErrorsAndWarnings()
        {
            // All warnings and errors are gathered once, so in order to ensure only the correct ones
            // were produced, we make one assertion on all of them here.

            var expectedWarnings = new List<string>()
            {
            };

            var expectedErrors = new List<string>()
            {
                "RealmListWithSetter.People has a setter but its type is a RealmList which only supports getters",
                "IndexedProperties.SingleProperty is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on System.Single",
                "ObjectIdProperties.BooleanProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.Boolean",
                "ObjectIdProperties.DateTimeOffsetProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.DateTimeOffset",
                "ObjectIdProperties.SingleProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.Single"
            };

            Assert.That(_errors, Is.EquivalentTo(expectedErrors));
            Assert.That(_warnings, Is.EquivalentTo(expectedWarnings));
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