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

extern alias propertychanged;
extern alias realm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Mono.Cecil;
using NUnit.Framework;
using Realms;
using Realms.Weaving;

namespace RealmWeaver
{
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.NotUsed)]
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.BeforeRealmWeaver)]
    [TestFixture(AssemblyType.NonPCL, PropertyChangedWeaver.AfterRealmWeaver)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.NotUsed)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.BeforeRealmWeaver)]
    [TestFixture(AssemblyType.PCL, PropertyChangedWeaver.AfterRealmWeaver)]
    public class Tests : WeaverTestBase
    {
        #region helpers

        private static dynamic GetAutoPropertyBackingFieldValue(object o, string propertyName)
        {
            var propertyField = ((Type)o.GetType())
                .GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = propertyField.GetValue(o);
            return fieldValue;
        }

        private static void SetAutoPropertyBackingFieldValue(object o, string propertyName, object propertyValue)
        {
            var propertyField = ((Type)o.GetType())
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

        private void WeavePropertyChanged(ModuleDefinition moduleDefinition)
        {
            // Disable CheckForEquality, because this will rewrite all our properties and some tests will
            // behave differently based on whether PropertyChanged is weaved or not.
            // Those differences will be unlikely to affect real world scenarios, but affect the tests:
            //   WovenCopyToRealm_ShouldAlwaysSetNullableProperties -> does not call native methods
            //   ShouldFollowMapToAttribute -> checks for (value != this.Email_) which adds two extra entries in the LogList
            // Additionally, the tests don't test the exact behavior of Realm + PropertyChanged, because the check for
            // Fody.PropertyChanged will always return 'false' (ModuleWeaver.cs@214).
            var config = new XElement("PropertyChanged");
            config.SetAttributeValue("CheckForEquality", false);
            new propertychanged::ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = moduleDefinition.AssemblyResolver,
                LogWarning = s => _warnings.Add(s),
                Config = config
            }.Execute();
        }

        private static string GetCoreMethodName(string type)
        {
            type = type.Replace("Counter", string.Empty);
            if (_realmIntegerBackedTypes.Contains(type))
            {
                return (type.StartsWith("Nullable") ? "Nullable" : string.Empty) + "RealmInteger";
            }

            return type;
        }

        #endregion

        private static readonly IEnumerable<string> _realmIntegerBackedTypes = new[]
        {
            "Byte",
            "Int16",
            "Int32",
            "Int64",
            "NullableByte",
            "NullableInt16",
            "NullableInt32",
            "NullableInt64"
        };

        public enum PropertyChangedWeaver
        {
            NotUsed,
            BeforeRealmWeaver,
            AfterRealmWeaver
        }

        private readonly PropertyChangedWeaver _propertyChangedWeaver;

        private Assembly _assembly;
        private string _sourceAssemblyPath;
        private string _targetAssemblyPath;

        public Tests(AssemblyType assemblyType, PropertyChangedWeaver propertyChangedWeaver) : base(assemblyType)
        {
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

            var assemblyResolver = moduleDefinition.AssemblyResolver as DefaultAssemblyResolver;
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
                {
                    Debug.WriteLine("Loader exception: " + item.Message.ToString());
                }

                Assert.Fail("Load failure");
            }
        }

        private static readonly object[][] RandomAndDefaultValues =
        {
            new object[] { "Char", '0', char.MinValue },
            new object[] { "Byte", (byte)100, (byte)0 },
            new object[] { "Int16", (short)100, (short)0 },
            new object[] { "Int32", 100, 0 },
            new object[] { "Int64", 100L, 0L },
            new object[] { "Single", 123.123f, 0.0f },
            new object[] { "Double", 123.123, 0.0 },
            new object[] { "Boolean", true, false },
            new object[] { "String", "str", null },
            new object[] { "NullableChar", '0', null },
            new object[] { "NullableByte", (byte)100, null },
            new object[] { "NullableInt16", (short)100, null },
            new object[] { "NullableInt32", 100, null },
            new object[] { "NullableInt64", 100L, null },
            new object[] { "NullableSingle", 123.123f, null },
            new object[] { "NullableDouble", 123.123, null },
            new object[] { "NullableBoolean", true, null },
            new object[] { "ByteCounter", (RealmInteger<byte>)100, (byte)0 },
            new object[] { "Int16Counter", (RealmInteger<short>)100, (short)0 },
            new object[] { "Int32Counter", (RealmInteger<int>)100, 0 },
            new object[] { "Int64Counter", (RealmInteger<long>)100L, 0L },
            new object[] { "NullableByteCounter", (RealmInteger<byte>)100, null },
            new object[] { "NullableInt16Counter", (RealmInteger<short>)100, null },
            new object[] { "NullableInt32Counter", (RealmInteger<int>)100, null },
            new object[] { "NullableInt64Counter", (RealmInteger<long>)100L, null },
        };

        private static IEnumerable<object[]> RandomValues()
        {
            return RandomAndDefaultValues.Select(a => new[] { a[0], a[1] });
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
                $"RealmObject.Get{GetCoreMethodName(typeName)}Value(propertyName = \"{propertyName}\")"
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
                $"RealmObject.Set{GetCoreMethodName(typeName)}Value(propertyName = \"{propertyName}\", value = {propertyValue})"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
        }

        [TestCaseSource(nameof(RandomAndDefaultValues))]
        public void SetValueManagedShouldNotRaisePropertyChanged(string typeName, object propertyValue, object defaultPropertyValue)
        {
            // We no longer manually raise PropertyChanged in the setter for managed objects.
            // Instead, we subscribe for notificaitons from core, and propagate those.

            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));
            o.IsManaged = true;

            var eventRaised = false;
            o.PropertyChanged += new PropertyChangedEventHandler((s, e) =>
            {
                eventRaised |= e.PropertyName == propertyName;
            });

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                $"RealmObject.Set{GetCoreMethodName(typeName)}Value(propertyName = \"{propertyName}\", value = {propertyValue})"
            }));
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(defaultPropertyValue));
            Assert.That(eventRaised, Is.False);
        }

        [TestCaseSource(nameof(RandomValues))]
        public void SetValueUnmanagedShouldRaisePropertyChanged(string typeName, object propertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.AllTypesObject"));

            var eventRaised = false;
            o.PropertyChanged += new PropertyChangedEventHandler((s, e) =>
            {
                eventRaised |= e.PropertyName == propertyName;
            });

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(GetAutoPropertyBackingFieldValue(o, propertyName), Is.EqualTo(propertyValue));
            Assert.That(eventRaised, Is.True);
        }

        [TestCase("Char", '0', char.MinValue)]
        [TestCase("Byte", (byte)100, (byte)0)]
        [TestCase("Int16", (short)100, (short)0)]
        [TestCase("Int32", 100, 0)]
        [TestCase("Int64", 100L, 0L)]
        [TestCase("String", "str", null)]
        public void SettingPrimaryKeyPropertyShouldCallSetUnique(string typeName, object propertyValue, object defaultPropertyValue)
        {
            // Arrange
            var propertyName = typeName + "Property";
            var o = (dynamic)Activator.CreateInstance(_assembly.GetType("AssemblyToProcess.PrimaryKey" + typeName + "Object"));
            o.IsManaged = true;

            // Act
            SetPropertyValue(o, propertyName, propertyValue);

            // Assert
            Assert.That(o.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                $"RealmObject.Set{GetCoreMethodName(typeName)}ValueUnique(propertyName = \"{propertyName}\", value = {propertyValue})"
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
        public void ShouldWeaveBacklinksGetters()
        {
            var objectType = _assembly.GetType("AssemblyToProcess.PhoneNumber");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            Assert.That(instance.Persons, Is.TypeOf(typeof(EnumerableQuery<>).MakeGenericType(_assembly.GetType("AssemblyToProcess.Person"))));
            Assert.That(instance.Persons, Is.SameAs(instance.Persons)); // should cache instances

            instance = (dynamic)Activator.CreateInstance(objectType);
            instance.IsManaged = true;

            var persons = instance.Persons;
            persons = instance.Persons;

            // the getter is invoked only once because the result is cached
            Assert.That(instance.LogList, Is.EqualTo(new[] { "IsManaged", "RealmObject.GetBacklinks(propertyName = \"Persons\")" }));
        }

        [Test]
        public void ShouldNotWeaveIQueryablePropertiesWithoutBacklinkAttribute()
        {
            var objectType = _assembly.GetType("AssemblyToProcess.Person");
            var property = objectType.GetProperty("SomeQueryableProperty");

            Assert.That(property.GetCustomAttribute<Realms.WovenPropertyAttribute>(), Is.Null);
        }

        [Test]
        public void MatchErrorsAndWarnings()
        {
            // All warnings and errors are gathered once, so in order to ensure only the correct ones
            // were produced, we make one assertion on all of them here.

            var expectedWarnings = new string[0];

            var expectedErrors = new[]
            {
                "RealmListWithSetter.People has a setter but its type is a IList which only supports getters.",
                "IndexedProperties.SingleProperty is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on System.Single.",
                "PrimaryKeyProperties.BooleanProperty is marked as [PrimaryKey] which is only allowed on integral and string types, not on System.Boolean.",
                "PrimaryKeyProperties.DateTimeOffsetProperty is marked as [PrimaryKey] which is only allowed on integral and string types, not on System.DateTimeOffset.",
                "PrimaryKeyProperties.SingleProperty is marked as [PrimaryKey] which is only allowed on integral and string types, not on System.Single.",
                "The type AssemblyToProcess.Employee indirectly inherits from RealmObject which is not supported.",
                "Class DefaultConstructorMissing must have a public constructor that takes no parameters.",
                "Class NoPersistedProperties is a RealmObject but has no persisted properties.",
                "NotSupportedProperties.DateTimeProperty is a DateTime which is not supported - use DateTimeOffset instead.",
                "NotSupportedProperties.NullableDateTimeProperty is a DateTime? which is not supported - use DateTimeOffset? instead.",
                "NotSupportedProperties.EnumProperty is a 'AssemblyToProcess.NotSupportedProperties/MyEnum' which is not yet supported.",
                "Class PrimaryKeyProperties has more than one property marked with [PrimaryKey].",
                "IncorrectAttributes.AutomaticId has [PrimaryKey] applied, but it's not persisted, so those attributes will be ignored.",
                "IncorrectAttributes.AutomaticDate has [Indexed] applied, but it's not persisted, so those attributes will be ignored.",
                "IncorrectAttributes.Email_ has [MapTo] applied, but it's not persisted, so those attributes will be ignored.",
                "IncorrectAttributes.Date_ has [Indexed], [MapTo] applied, but it's not persisted, so those attributes will be ignored.",
                "InvalidBacklinkRelationships.ParentRelationship has [Backlink] applied, but is not IQueryable.",
                "InvalidBacklinkRelationships.ChildRelationShips has [Backlink] applied, but is not IQueryable.",
                "InvalidBacklinkRelationships.WritableBacklinksProperty has a setter but also has [Backlink] applied, which only supports getters.",
                "InvalidBacklinkRelationships.BacklinkNotAppliedProperty is IQueryable, but doesn't have [Backlink] applied.",
                "The property 'Person.PhoneNumbers' does not constitute a link to 'InvalidBacklinkRelationships' as described by 'InvalidBacklinkRelationships.NoSuchRelationshipProperty'.",
                "RequiredProperties.CharProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Char.",
                "RequiredProperties.ByteProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Byte.",
                "RequiredProperties.Int16Property is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Int16.",
                "RequiredProperties.Int32Property is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Int32.",
                "RequiredProperties.Int64Property is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Int64.",
                "RequiredProperties.SingleProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Single.",
                "RequiredProperties.DoubleProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Double.",
                "RequiredProperties.BooleanProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Boolean.",
                "RequiredProperties.DateTimeOffsetProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.DateTimeOffset.",
                "RequiredProperties.ObjectProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on AssemblyToProcess.Person.",
                "RequiredProperties.ListProperty is marked as [Required] which is only allowed on strings or nullable scalar types, not on System.Collections.Generic.IList`1<AssemblyToProcess.Person>."
            };

            Assert.That(_errors, Is.EquivalentTo(expectedErrors));
            Assert.That(_warnings, Is.EquivalentTo(expectedWarnings));
        }

        [TestCase("String", "string")]
        [TestCase("Char", 'd')]
        [TestCase("Byte", (byte)3)]
        [TestCase("Int16", (Int16)3)]
        [TestCase("Int32", 3)]
        [TestCase("Int64", (Int64)3)]
        [TestCase("Single", (float)3.3)]
        [TestCase("Double", 3.3)]
        [TestCase("Boolean", true)]
        public void WovenCopyToRealm_ShouldSetNonDefaultProperties(string propertyName, object propertyValue, string typeName = "NonNullableProperties")
        {
            var objectType = _assembly.GetType($"AssemblyToProcess.{typeName}");
            var instance = (dynamic)Activator.CreateInstance(objectType);
            SetPropertyValue(instance, propertyName, propertyValue);

            CopyToRealm(objectType, instance);

            Assert.That(instance.LogList, Is.EqualTo(new List<string>
            {
                "IsManaged",
                "IsManaged",
                $"RealmObject.Set{GetCoreMethodName(propertyName)}Value(propertyName = \"{propertyName}\", value = {propertyValue})"
            }));
        }

        [Test]
        public void WovenCopyToRealm_ShouldAlwaysSetDateTimeOffsetProperties()
        {
            // DateTimeOffset can't be set as a constant
            WovenCopyToRealm_ShouldSetNonDefaultProperties("DateTimeOffset", default(DateTimeOffset), "DateTimeOffsetProperty");
            WovenCopyToRealm_ShouldSetNonDefaultProperties("DateTimeOffset", new DateTimeOffset(1, 1, 1, 1, 1, 1, TimeSpan.Zero), "DateTimeOffsetProperty");
        }

        [Test]
        public void WovenCopyToRealm_ShouldSetNonDefaultByteArrayProperties()
        {
            // ByteArray can't be set as a constant
            WovenCopyToRealm_ShouldSetNonDefaultProperties("ByteArray", new byte[] { 4, 3, 2 });
        }

        [Test]
        public void WovenCopyToRealm_ShouldAlwaysSetNullableProperties()
        {
            var objectType = _assembly.GetType("AssemblyToProcess.NullableProperties");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            CopyToRealm(objectType, instance);

            var properties = ((Type)instance.GetType()).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetList = properties.Where(p => p.Name != "IsManaged" && p.Name != "Realm")
                                       .SelectMany(p =>
                                       {
                                           return new[]
                                           {
                                               "IsManaged",
                                               $"RealmObject.SetNullable{GetCoreMethodName(p.Name)}Value(propertyName = \"{p.Name}\", value = )"
                                           };
                                       })
                                       .ToList();
            Assert.That(instance.LogList, Is.EqualTo(targetList));
        }

        [TestCase("Char")]
        [TestCase("Byte")]
        [TestCase("Int16")]
        [TestCase("Int32")]
        [TestCase("Int64")]
        [TestCase("String")]
        public void WovenCopyToRealm_ShouldAlwaysSetPrimaryKeyProperties(string type)
        {
            var objectType = _assembly.GetType($"AssemblyToProcess.PrimaryKey{type}Object");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            CopyToRealm(objectType, instance);

            var propertyType = objectType.GetProperty(type + "Property").PropertyType;
            var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType).ToString() : string.Empty;
            Assert.That(instance.LogList, Is.EqualTo(new List<string> { "IsManaged", $"RealmObject.Set{GetCoreMethodName(type)}ValueUnique(propertyName = \"{type}Property\", value = {defaultValue})" }));
        }

        [TestCase("RequiredObject", true)]
        [TestCase("NonRequiredObject", false)]
        public void WovenCopyToRealm_ShouldAlwaysSetRequiredProperties(string type, bool required)
        {
            var objectType = _assembly.GetType($"AssemblyToProcess.{type}");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            CopyToRealm(objectType, instance);

            List<string> targetList;
            if (required)
            {
                targetList = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(p => p.Name != "IsManaged" && p.Name != "Realm")
                                       .SelectMany(p =>
                                       {
                                           return new[]
                                           {
                                               "IsManaged",
                                               $"RealmObject.Set{p.Name}Value(propertyName = \"{p.Name}\", value = )"
                                           };
                                       })
                                       .ToList();
            }
            else
            {
                targetList = new List<string>();
            }

            Assert.That(instance.LogList, Is.EqualTo(targetList));
        }

        [TestCase("PKObjectOne")]
        [TestCase("PKObjectTwo")]
        [TestCase("PKObjectThree")]
        public void WovenCopyToRealm_ShouldSetPrimaryKeysFirst(string @class)
        {
            var objectType = _assembly.GetType($"AssemblyToProcess.{@class}");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            CopyToRealm(objectType, instance);

            Assert.That(((IEnumerable<string>)instance.LogList).Take(2), Is.EqualTo(new[] { "IsManaged", "RealmObject.SetRealmIntegerValueUnique(propertyName = \"Id\", value = 0)" }));
        }

        [Test]
        public void WovenCopyToRealm_ShouldResetBacklinks()
        {
            var objectType = _assembly.GetType("AssemblyToProcess.PhoneNumber");
            var instance = (dynamic)Activator.CreateInstance(objectType);

            CopyToRealm(objectType, instance);
            var persons = instance.Persons;

            Assert.That(instance.LogList, Is.EqualTo(new[] { "IsManaged", "RealmObject.GetBacklinks(propertyName = \"Persons\")" }));
        }

        private static void CopyToRealm(Type objectType, dynamic instance)
        {
            var wovenAttribute = objectType.CustomAttributes.Single(a => a.AttributeType.Name == "WovenAttribute");
            var helperType = (Type)wovenAttribute.ConstructorArguments[0].Value;
            var helper = (IRealmObjectHelper)Activator.CreateInstance(helperType);
            instance.IsManaged = true;
            helper.CopyToRealm(instance, update: false, setPrimaryKey: true);
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(_sourceAssemblyPath, _targetAssemblyPath);
        }
#endif
    }
}
