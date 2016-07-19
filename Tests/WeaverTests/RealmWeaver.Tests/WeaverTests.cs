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

extern alias realm;
extern alias propertychanged;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.ComponentModel;

namespace RealmWeaver
{
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.NotUsed)]
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.BeforeRealmWeaver)]
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.AfterRealmWeaver)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.NotUsed)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.BeforeRealmWeaver)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.AfterRealmWeaver)]
    public class Tests
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

        private void WeaveRealm(ModuleDefinition moduleDefinition)
        {
            new realm::ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = moduleDefinition.AssemblyResolver,
                LogError = s => _errors.Add(s),
                LogErrorPoint = (s, point) => _errors.Add(s),
                LogWarningPoint = (s, point) => _warnings.Add(s)
            }.Execute();
        }

        private void WeavePropertyChanged(ModuleDefinition moduleDefinition)
        {
            new propertychanged::ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = moduleDefinition.AssemblyResolver,
                LogWarning = s => _warnings.Add(s)
            }.Execute();
        }

        #endregion

        public enum AssemblyType
        {
            NonPCL,
            PCL
        }

        public enum PropertyChangedWeaver
        {
            NotUsed,
            BeforeRealmWeaver,
            AfterRealmWeaver
        }

        private readonly AssemblyType _assemblyType;
        private readonly PropertyChangedWeaver _propertyChangedWeaver;

        private Assembly _assembly;
        private string _sourceAssemblyPath;
        private string _targetAssemblyPath;

        private readonly List<string> _warnings = new List<string>();
        private readonly List<string> _errors = new List<string>();


        public Tests( AssemblyType assemblyType, PropertyChangedWeaver propertyChangedWeaver)
        {
            _assemblyType = assemblyType;
            _propertyChangedWeaver = propertyChangedWeaver;
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _sourceAssemblyPath = _assemblyType == AssemblyType.NonPCL ?
                typeof(AssemblyToProcess.NonPCLModuleLocator).Assembly.Location :
                typeof(AssemblyToProcess.PCLModuleLocator).Assembly.Location;

            _targetAssemblyPath = _sourceAssemblyPath.Replace(".dll", $".{_assemblyType}_PropertyChangedWeaver{_propertyChangedWeaver}.dll");

            var moduleDefinition = ModuleDefinition.ReadModule(_sourceAssemblyPath);

            var assemblyResolver = (moduleDefinition.AssemblyResolver as DefaultAssemblyResolver);
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(_sourceAssemblyPath));

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // With Mono, we need the actual reference assemblies that we build against, rather than
                // the GAC because typeforwarding might cause the layouts to be different.
                var referenceAssembliesPath = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "Facades");
                assemblyResolver.AddSearchDirectory(referenceAssembliesPath);
            }

            switch (_propertyChangedWeaver)
            {
                case PropertyChangedWeaver.NotUsed:
                    WeaveRealm(moduleDefinition);
                    break;
                
                case PropertyChangedWeaver.BeforeRealmWeaver:
                    WeavePropertyChanged(moduleDefinition);
                    WeaveRealm(moduleDefinition);
                    break;

                case PropertyChangedWeaver.AfterRealmWeaver:
                    WeaveRealm(moduleDefinition);
                    WeavePropertyChanged(moduleDefinition);
                    break;
            }

            // we need to change the assembly name because otherwise Mono loads the original assembly
            moduleDefinition.Assembly.Name.Name += $".{_assemblyType}_PropertyChangedWeaver{_propertyChangedWeaver}";

            moduleDefinition.Write(_targetAssemblyPath);
            _assembly = Assembly.LoadFile(_targetAssemblyPath);
            Console.WriteLine(_assembly.Location);

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

        private static readonly object[][] RandomAndDefaultValues =
        {
            new object[] {"Char", '0', char.MinValue},
            new object[] {"Byte", (byte) 100, (byte) 0},
            new object[] {"Int16", (short) 100, (short) 0},
            new object[] {"Int32", 100, 0},
            new object[] {"Int64", 100L, 0L},
            new object[] {"Single", 123.123f, 0.0f},
            new object[] {"Double", 123.123, 0.0},
            new object[] {"Boolean", true, false},
            new object[] {"String", "str", null},
            new object[] {"NullableChar", '0', null},
            new object[] {"NullableByte", (byte) 100, null},
            new object[] {"NullableInt16", (short) 100, null},
            new object[] {"NullableInt32", 100, null},
            new object[] {"NullableInt64", 100L, null},
            new object[] {"NullableSingle", 123.123f, null},
            new object[] {"NullableDouble", 123.123, null},
            new object[] {"NullableBoolean", true, null}
        };


        private static IEnumerable<object[]> RandomValues()
        {
            return RandomAndDefaultValues.Select(a => new[] {a[0], a[1]});
        }

        [TestCaseSource(nameof(RandomValues))]
        public void GetValueUnmanagedShouldGetBackingField(string typeName, object propertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));
            SetAutoPropertyBackingFieldValue(o, propertyName, propertyValue);

            // Act
            var returnedValue = GetPropertyValue(o, propertyName);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string> { "IsManaged" }));
            Assert.That(returnedValue, Is.EqualTo(propertyValue));
        }

        [TestCaseSource(nameof(RandomValues))]
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

        [TestCaseSource(nameof(RandomValues))]
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

        [TestCaseSource(nameof(RandomAndDefaultValues))]
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

        [TestCaseSource(nameof(RandomAndDefaultValues))]
        public void SetValueManagedShouldRaisePropertyChanged(string typeName, object propertyValue, object defaultPropertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObjectPropertyChanged"));
            o.IsManaged = true;

            var eventRaised = false;
            o.PropertyChanged += new PropertyChangedEventHandler((s, e) =>
            {
                if (e.PropertyName == propertyName)
                    eventRaised = true;
            });

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "RealmObject.Set" + typeName + "Value(propertyName = \"" + propertyName + "\", value = " + propertyValue + ")"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
            Assert.That(eventRaised, Is.True);
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
        public void SetManyRelationship()
        {
            // Arrange
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.Person"));
            var pn1 = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.PhoneNumber"));
            var pn2 = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.PhoneNumber"));
            o.IsManaged = true;

            Assert.Inconclusive("IList property tests still missing");
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

        [Test]
        public void ShouldAddPreserveAttributeToConstructor()
        {
            // Arrange and act
            var personType = _assembly.GetType("AssemblyToProcess.Person");
            var ctor = personType.GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.That(ctor.CustomAttributes.Any(a => a.AttributeType.Name == "PreserveAttribute"));
        }

        [Test]
        public void ShouldAddPreserveAttributeToHelperConstructor()
        {
            // Arrange and act
            var personType = _assembly.GetType("AssemblyToProcess.Person");
            var wovenAttribute = personType.CustomAttributes.Single(a => a.AttributeType.Name == "WovenAttribute");
            var helperType = (Type)wovenAttribute.ConstructorArguments[0].Value;
            var helperConstructor = helperType.GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.That(helperConstructor.CustomAttributes.Any(a => a.AttributeType.Name == "PreserveAttribute"));
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
                "RealmListWithSetter.People has a setter but its type is a IList which only supports getters",
                "IndexedProperties.SingleProperty is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on System.Single",
                "ObjectIdProperties.BooleanProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.Boolean",
                "ObjectIdProperties.DateTimeOffsetProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.DateTimeOffset",
                "ObjectIdProperties.SingleProperty is marked as [ObjectId] which is only allowed on integral and string types, not on System.Single",
                "The type AssemblyToProcess.Employee indirectly inherits from RealmObject which is not supported",
                "class DefaultConstructorMissing must have a public constructor that takes no parameters",
                "class NoPersistedProperties is a RealmObject but has no persisted properties"
            };

            Assert.That(_errors, Is.EquivalentTo(expectedErrors));
            Assert.That(_warnings, Is.EquivalentTo(expectedWarnings));
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(_sourceAssemblyPath,_targetAssemblyPath);
        }
#endif
    }
}
