////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RealmWeaver
{
    /// <summary>
    /// This weaver is used both by Fody and the Unity weaver, so make sure to only reference Mono.Cecil and nothing Fody or Unity specific.
    /// </summary>
    internal partial class Weaver
    {
        private const MethodAttributes DefaultMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

        internal const string StringTypeName = "System.String";
        internal const string ByteArrayTypeName = "System.Byte[]";
        internal const string CharTypeName = "System.Char";
        internal const string ByteTypeName = "System.Byte";
        internal const string Int16TypeName = "System.Int16";
        internal const string Int32TypeName = "System.Int32";
        internal const string Int64TypeName = "System.Int64";
        internal const string SingleTypeName = "System.Single";
        internal const string DoubleTypeName = "System.Double";
        internal const string BooleanTypeName = "System.Boolean";
        internal const string DecimalTypeName = "System.Decimal";
        internal const string Decimal128TypeName = "MongoDB.Bson.Decimal128";
        internal const string ObjectIdTypeName = "MongoDB.Bson.ObjectId";
        internal const string DateTimeOffsetTypeName = "System.DateTimeOffset";
        internal const string GuidTypeName = "System.Guid";
        internal const string RealmValueTypeName = "Realms.RealmValue";
        internal const string NullableCharTypeName = "System.Nullable`1<System.Char>";
        internal const string NullableByteTypeName = "System.Nullable`1<System.Byte>";
        internal const string NullableInt16TypeName = "System.Nullable`1<System.Int16>";
        internal const string NullableInt32TypeName = "System.Nullable`1<System.Int32>";
        internal const string NullableInt64TypeName = "System.Nullable`1<System.Int64>";
        internal const string NullableSingleTypeName = "System.Nullable`1<System.Single>";
        internal const string NullableDoubleTypeName = "System.Nullable`1<System.Double>";
        internal const string NullableBooleanTypeName = "System.Nullable`1<System.Boolean>";
        internal const string NullableDecimalTypeName = "System.Nullable`1<System.Decimal>";
        internal const string NullableDecimal128TypeName = "System.Nullable`1<MongoDB.Bson.Decimal128>";
        internal const string NullableDateTimeOffsetTypeName = "System.Nullable`1<System.DateTimeOffset>";
        internal const string NullableObjectIdTypeName = "System.Nullable`1<MongoDB.Bson.ObjectId>";
        internal const string NullableGuidTypeName = "System.Nullable`1<System.Guid>";

        private static readonly HashSet<string> _realmValueTypes = new HashSet<string>
        {
            CharTypeName,
            SingleTypeName,
            DoubleTypeName,
            BooleanTypeName,
            DecimalTypeName,
            Decimal128TypeName,
            ObjectIdTypeName,
            DateTimeOffsetTypeName,
            GuidTypeName,
            NullableCharTypeName,
            NullableSingleTypeName,
            NullableDoubleTypeName,
            NullableBooleanTypeName,
            NullableDateTimeOffsetTypeName,
            NullableDecimalTypeName,
            NullableDecimal128TypeName,
            NullableObjectIdTypeName,
            NullableGuidTypeName,
            ByteTypeName,
            Int16TypeName,
            Int32TypeName,
            Int64TypeName,
            NullableByteTypeName,
            NullableInt16TypeName,
            NullableInt32TypeName,
            NullableInt64TypeName,
            $"Realms.RealmInteger`1<{ByteTypeName}>",
            $"Realms.RealmInteger`1<{Int16TypeName}>",
            $"Realms.RealmInteger`1<{Int32TypeName}>",
            $"Realms.RealmInteger`1<{Int64TypeName}>",
            $"System.Nullable`1<Realms.RealmInteger`1<{ByteTypeName}>>",
            $"System.Nullable`1<Realms.RealmInteger`1<{Int16TypeName}>>",
            $"System.Nullable`1<Realms.RealmInteger`1<{Int32TypeName}>>",
            $"System.Nullable`1<Realms.RealmInteger`1<{Int64TypeName}>>",
            ByteArrayTypeName,
            StringTypeName,
            RealmValueTypeName,
        };

        private static readonly IEnumerable<string> _primaryKeyTypes = new[]
        {
            StringTypeName,
            CharTypeName,
            ByteTypeName,
            Int16TypeName,
            Int32TypeName,
            Int64TypeName,
            ObjectIdTypeName,
            GuidTypeName,
            NullableCharTypeName,
            NullableByteTypeName,
            NullableInt16TypeName,
            NullableInt32TypeName,
            NullableInt64TypeName,
            NullableObjectIdTypeName,
            NullableGuidTypeName
        };

        private static readonly HashSet<string> RealmPropertyAttributes = new HashSet<string>
        {
            "PrimaryKeyAttribute",
            "IndexedAttribute",
            "MapToAttribute",
        };

        private readonly Lazy<MethodReference> _propertyChanged_DoNotNotify_Ctor;

        private readonly ImportedReferences _references;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly ILogger _logger;

        private IEnumerable<TypeDefinition> GetMatchingTypes()
        {
            foreach (var type in _moduleDefinition.GetTypes().Where(t => t.IsRealmObjectDescendant(_references)))
            {
                if (type.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute"))
                {
                    continue;
                }
                else if (type.IsValidRealmObjectBaseInheritor(_references))
                {
                    yield return type;
                }
                else
                {
                    _logger.Error($"The type {type.FullName} indirectly inherits from RealmObject which is not supported.", type.GetConstructors().FirstOrDefault()?.DebugInformation?.SequencePoints?.FirstOrDefault());
                }
            }
        }

        public Weaver(ModuleDefinition module, ILogger logger, string framework)
        {
            //// UNCOMMENT THIS DEBUGGER LAUNCH TO BE ABLE TO RUN A SEPARATE VS INSTANCE TO DEBUG WEAVING WHILST BUILDING
            //// System.Diagnostics.Debugger.Launch();

            _moduleDefinition = module;
            _logger = logger;
            _references = ImportedReferences.Create(_moduleDefinition, framework);
            _propertyChanged_DoNotNotify_Ctor = new Lazy<MethodReference>(GetOrAddPropertyChanged_DoNotNotify);
        }

        public WeaveModuleResult Execute(Analytics.Config analyticsConfig)
        {
            _logger.Debug("Weaving file: " + _moduleDefinition.FileName);

            if (_references.WovenAssemblyAttribute == null)
            {
                return WeaveModuleResult.Skipped($"Not weaving assembly '{_moduleDefinition.Assembly.Name}' because it doesn't reference Realm.");
            }

            var isWoven = _moduleDefinition.Assembly.CustomAttributes.Any(a => a.AttributeType.IsSameAs(_references.WovenAssemblyAttribute));
            if (isWoven)
            {
                return WeaveModuleResult.Skipped($"Not weaving assembly '{_moduleDefinition.Assembly.Name}' because it has already been processed.");
            }

            var submitAnalytics = Task.Run(() =>
            {
                analyticsConfig.ModuleName = _moduleDefinition.Name;
                analyticsConfig.IsUsingSync = IsUsingSync();
                var analytics = new Analytics(analyticsConfig);
                try
                {
                    var payload = analytics.SubmitAnalytics();
#if DEBUG
                    _logger.Info($@"
----------------------------------
Analytics payload
{payload}
----------------------------------");
#endif
                }
                catch (Exception e)
                {
                    _logger.Debug("Error submitting analytics: " + e.Message);
                }
            });

            var matchingTypes = GetMatchingTypes().ToArray();

            var weaveResults = matchingTypes.Select(type =>
            {
                try
                {
                    return WeaveType(type);
                }
                catch (Exception e)
                {
                    _logger.Error($"An unexpected error occurred while weaving '{type.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}");
                    return WeaveTypeResult.Error(type.Name);
                }
            }).ToArray();

            WeaveSchema(matchingTypes);

            var wovenAssemblyAttribute = new CustomAttribute(_references.WovenAssemblyAttribute_Constructor);
            _moduleDefinition.Assembly.CustomAttributes.Add(wovenAssemblyAttribute);

            submitAnalytics.Wait();

            var failedResults = weaveResults.Where(r => !r.IsSuccessful);
            if (failedResults.Any())
            {
                return WeaveModuleResult.Error($"The following types had errors when woven: {string.Join(", ", failedResults.Select(f => f.Type))}");
            }

            return WeaveModuleResult.Success(weaveResults);
        }

        private WeaveTypeResult WeaveType(TypeDefinition type)
        {
            _logger.Debug("Weaving " + type.Name);

            var didSucceed = true;
            var persistedProperties = new List<WeavePropertyResult>();
            foreach (var prop in type.Properties.Where(x => x.HasThis && !x.CustomAttributes.Any(a => a.AttributeType.Name == "IgnoredAttribute")))
            {
                try
                {
                    var weaveResult = WeaveProperty(prop, type);
                    if (weaveResult.Woven)
                    {
                        persistedProperties.Add(weaveResult);
                    }
                    else
                    {
                        var sequencePoint = prop.GetSequencePoint();
                        if (!string.IsNullOrEmpty(weaveResult.ErrorMessage))
                        {
                            // We only want one error point, so even though there may be more problems, we only log the first one.
                            _logger.Error(weaveResult.ErrorMessage, sequencePoint);

                            // We set didSucceed to false, but we want to continue weaving so that if there are multiple problems, they'll all be reported.
                            didSucceed = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(weaveResult.WarningMessage))
                            {
                                _logger.Warning(weaveResult.WarningMessage, sequencePoint);
                            }

                            var realmAttributeNames = prop.CustomAttributes
                                                          .Select(a => a.AttributeType.Name)
                                                          .Intersect(RealmPropertyAttributes)
                                                          .OrderBy(a => a)
                                                          .Select(a => $"[{a.Replace("Attribute", string.Empty)}]");

                            if (realmAttributeNames.Any())
                            {
                                _logger.Warning($"{type.Name}.{prop.Name} has {string.Join(", ", realmAttributeNames)} applied, but it's not persisted, so those attributes will be ignored.", sequencePoint);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var sequencePoint = prop.GetSequencePoint();
                    _logger.Error(
                        $"Unexpected error caught weaving property '{type.Name}.{prop.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}",
                        sequencePoint);

                    return WeaveTypeResult.Error(type.Name);
                }
            }

            if (!persistedProperties.Any())
            {
                _logger.Error($"Class {type.Name} is a RealmObject but has no persisted properties.", type.GetSequencePoint());
                return WeaveTypeResult.Error(type.Name);
            }

            var pkProperty = persistedProperties.FirstOrDefault(p => p.IsPrimaryKey);
            if (type.IsEmbeddedObjectInheritor(_references) && pkProperty != null)
            {
                _logger.Error($"Class {type.Name} is an EmbeddedObject but has a primary key {pkProperty.Property.Name} defined.", type.GetSequencePoint());
                return WeaveTypeResult.Error(type.Name);
            }

            if (persistedProperties.Count(p => p.IsPrimaryKey) > 1)
            {
                _logger.Error($"Class {type.Name} has more than one property marked with [PrimaryKey].", type.GetSequencePoint());
                return WeaveTypeResult.Error(type.Name);
            }

            var objectConstructor = type.GetConstructors()
                .SingleOrDefault(c => c.Parameters.Count == 0 && !c.IsStatic);
            if (objectConstructor == null)
            {
                var nonDefaultConstructor = type.GetConstructors().First();
                var sequencePoint = nonDefaultConstructor.DebugInformation.SequencePoints.FirstOrDefault();
                _logger.Error($"Class {type.Name} must have a public constructor that takes no parameters.", sequencePoint);
                return WeaveTypeResult.Error(type.Name);
            }

            var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
            objectConstructor.CustomAttributes.Add(preserveAttribute);
            preserveAttribute = new CustomAttribute(_references.PreserveAttribute_ConstructorWithParams); // recreate so has new instance
            preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_moduleDefinition.TypeSystem.Boolean, true)); // AllMembers
            preserveAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_moduleDefinition.TypeSystem.Boolean, false)); // Conditional
            type.CustomAttributes.Add(preserveAttribute);
            _logger.Debug($"Added [Preserve] to {type.Name} and its constructor.");

            var wovenAttribute = new CustomAttribute(_references.WovenAttribute_Constructor);
            TypeReference helperType = WeaveRealmObjectHelper(type, objectConstructor, persistedProperties);
            wovenAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_references.System_Type, helperType));
            type.CustomAttributes.Add(wovenAttribute);

            return didSucceed ? WeaveTypeResult.Success(type.Name, persistedProperties) : WeaveTypeResult.Error(type.Name);
        }

        private WeavePropertyResult WeaveProperty(PropertyDefinition prop, TypeDefinition type)
        {
            var columnName = prop.Name;
            var mapToAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "MapToAttribute");
            if (mapToAttribute != null)
            {
                columnName = (string)mapToAttribute.ConstructorArguments[0].Value;
            }

            if (prop.GetMethod == null)
            {
                return WeavePropertyResult.Skipped();
            }

            var backingField = prop.GetBackingField();
            var isIndexed = prop.CustomAttributes.Any(a => a.AttributeType.Name == "IndexedAttribute");
            if (isIndexed && !prop.IsIndexable(_references))
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {prop.PropertyType.FullName}.");
            }

            var isPrimaryKey = prop.IsPrimaryKey(_references);
            if (isPrimaryKey && (!_primaryKeyTypes.Contains(prop.PropertyType.FullName)))
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is marked as [PrimaryKey] which is only allowed on integral and string types, not on {prop.PropertyType.FullName}.");
            }

            var isRequired = prop.IsRequired(_references);
            if (isRequired &&
                !prop.IsCollection(typeof(string)) &&
                !prop.IsCollection(typeof(byte[])) &&
                !prop.IsNullable() &&
                prop.PropertyType.FullName != StringTypeName &&
                prop.PropertyType.FullName != ByteArrayTypeName)
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is marked as [Required] which is only allowed on strings or nullable scalar types, not on {prop.PropertyType.FullName}.");
            }

            if (!prop.IsAutomatic())
            {
                if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
                {
                    return WeavePropertyResult.Warning($"{type.Name}.{prop.Name} is not an automatic property but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.");
                }

                if (prop.ContainsAsymmetricObject(_references))
                {
                    return WeavePropertyResult.Warning($"{type.Name}.{prop.Name} is not an automatic property but its type is a AsymmetricObject. This usually indicates a relationship but AsymmetricObjects are not allowed to be the receiving end of any relationships.");
                }

                return WeavePropertyResult.Skipped();
            }

            var backlinkAttribute = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "BacklinkAttribute");
            if (backlinkAttribute != null && !prop.IsIQueryable())
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} has [Backlink] applied, but is not IQueryable.");
            }

            if (_realmValueTypes.Contains(prop.PropertyType.FullName))
            {
                if (prop.SetMethod == null)
                {
                    return WeavePropertyResult.Skipped();
                }

                var setter = isPrimaryKey ? _references.RealmObject_SetValueUnique : _references.RealmObject_SetValue;

                ReplaceGetter(prop, columnName, _references.RealmObject_GetValue);
                ReplaceSetter(prop, backingField, columnName, setter);
            }
            else if (prop.IsCollection(out var collectionType))
            {
                var genericArguments = ((GenericInstanceType)prop.PropertyType).GenericArguments;
                var elementType = genericArguments.Last();

                if (!elementType.Resolve().IsValidRealmObjectBaseInheritor(_references))
                {
                    if (elementType.IsRealmInteger(out _, out _))
                    {
                        return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is an {collectionType}<RealmInteger> which is not supported.");
                    }

                    if (!_realmValueTypes.Contains(elementType.FullName))
                    {
                        return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is an {collectionType} but its generic type is {elementType.Name} which is not supported by Realm.");
                    }
                }
                else if (elementType.IsAsymmetricObjectDescendant(_references))
                {
                    return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is an {collectionType}<AsymmetricObject>, but AsymmetricObjects aren't allowed to be contained in any RealmObject inheritor.");
                }

                if (prop.SetMethod != null)
                {
                    return WeavePropertyResult.Error($"{type.Name}.{prop.Name} has a setter but its type is a {collectionType} which only supports getters.");
                }

                switch (collectionType)
                {
                    case RealmCollectionType.IList:
                        ReplaceCollectionGetter(prop, backingField, columnName,
                                            new GenericInstanceMethod(_references.RealmObject_GetListValue) { GenericArguments = { elementType } });
                        break;
                    case RealmCollectionType.ISet:
                        if (elementType.Resolve().IsEmbeddedObjectInheritor(_references))
                        {
                            return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.");
                        }

                        ReplaceCollectionGetter(prop, backingField, columnName,
                                            new GenericInstanceMethod(_references.RealmObject_GetSetValue) { GenericArguments = { elementType } });
                        break;
                    case RealmCollectionType.IDictionary:
                        var keyType = genericArguments.First();
                        if (keyType != _references.Types.String)
                        {
                            return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is a Dictionary<{keyType.Name}, {elementType.Name}> but only string keys are currently supported by Realm.");
                        }

                        ReplaceCollectionGetter(prop, backingField, columnName,
                                            new GenericInstanceMethod(_references.RealmObject_GetDictionaryValue) { GenericArguments = { elementType } });
                        break;
                }
            }
            else if (prop.ContainsAsymmetricObject(_references))
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is of type AsymmetricObject, but AsymmetricObjects aren't allowed to be the receiving end of any relationship.");
            }
            else if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
            {
                if (prop.SetMethod == null)
                {
                    return WeavePropertyResult.Warning($"{type.Name}.{prop.Name} does not have a setter but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.");
                }

                // with casting in the _realmObject methods, should just work
                ReplaceGetter(prop, columnName, _references.RealmObject_GetValue);
                ReplaceSetter(prop, backingField, columnName, _references.RealmObject_SetValue);
            }
            else if (prop.IsIQueryable())
            {
                if (backlinkAttribute == null)
                {
                    return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is IQueryable, but doesn't have [Backlink] applied.");
                }

                if (prop.SetMethod != null)
                {
                    return WeavePropertyResult.Error($"{type.Name}.{prop.Name} has a setter but also has [Backlink] applied, which only supports getters.");
                }

                if (type.IsAsymmetricObjectDescendant(_references))
                {
                    return WeavePropertyResult.Error($"{type.Name}.{prop.Name} has [Backlink] applied which is not allowed on AsymmetricObject.");
                }

                var elementType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single();
                var inversePropertyName = (string)backlinkAttribute.ConstructorArguments[0].Value;
                var inverseProperty = elementType.Resolve().Properties.SingleOrDefault(p => p.Name == inversePropertyName);

                if (inverseProperty == null || (!inverseProperty.PropertyType.IsSameAs(type) && !inverseProperty.IsCollection(type)))
                {
                    return WeavePropertyResult.Error($"The property '{elementType.Name}.{inversePropertyName}' does not constitute a link to '{type.Name}' as described by '{type.Name}.{prop.Name}'.");
                }

                if (backingField is FieldDefinition backingDef)
                {
                    // without a set; auto property has this flag we must clear
                    backingDef.Attributes &= ~FieldAttributes.InitOnly;
                }

                ReplaceBacklinksGetter(prop, backingField, columnName, elementType);
            }
            else if (prop.PropertyType.GetElementType().FullName == "System.Collections.Generic.List`1")
            {
                var genericType = ((GenericInstanceType)prop.PropertyType).GenericArguments.Single().Name;
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is declared as List<{genericType}> which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList<{genericType}>, otherwise annotate the property with the [Ignored] attribute.");
            }
            else if (prop.SetMethod == null)
            {
                return WeavePropertyResult.Skipped();
            }
            else if (prop.PropertyType.FullName == "System.DateTime")
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is a DateTime which is not supported - use DateTimeOffset instead.");
            }
            else if (prop.PropertyType.FullName == "System.Nullable`1<System.DateTime>")
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is a DateTime? which is not supported - use DateTimeOffset? instead.");
            }
            else
            {
                return WeavePropertyResult.Error($"{type.Name}.{prop.Name} is a '{prop.PropertyType}' which is not yet supported.");
            }

            var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
            prop.CustomAttributes.Add(preserveAttribute);

            var wovenPropertyAttribute = new CustomAttribute(_references.WovenPropertyAttribute_Constructor);
            prop.CustomAttributes.Add(wovenPropertyAttribute);

            var primaryKeyMsg = isPrimaryKey ? "[PrimaryKey]" : string.Empty;
            var indexedMsg = isIndexed ? "[Indexed]" : string.Empty;
            _logger.Debug($"Woven {type.Name}.{prop.Name} as a {prop.PropertyType.FullName} {primaryKeyMsg} {indexedMsg}.");
            return WeavePropertyResult.Success(prop, backingField, isPrimaryKey, isIndexed);
        }

        private void ReplaceGetter(PropertyDefinition prop, string columnName, MethodReference getValueReference)
        {
            //// A synthesized property getter looks like this:
            ////   0: ldarg.0
            ////   1: ldfld <backingField>
            ////   2: ret
            //// We want to change it so it looks like this:
            ////   0: ldarg.0
            ////   1: call Realms.RealmObject.get_IsManaged
            ////   2: brfalse.s 7
            ////   3: ldarg.0
            ////   4: ldstr <columnName>
            ////   5: call Realms.RealmObject.GetValue
            ////   6: call op_explicit prop.PropertyType
            ////   7: ret
            ////   8: ldarg.0
            ////   9: ldfld <backingField>
            ////  10: ret
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged) return this.<backingField>;
            ////   return base.GetValue(<columnName>);
            ////
            //// For RealmObject targets, there's no implicit conversion from RealmValue to
            //// prop.PropertyType, so we convert implicitly to RealmObjectBase, then cast.
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged) return this.<backingField>;
            ////   return (TargetType)*(RealmObjectBase)*base.GetValue(<columnName>);

            var start = prop.GetMethod.Body.Instructions.First();
            var il = prop.GetMethod.Body.GetILProcessor();

            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
            il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
            il.InsertBefore(start, il.Create(OpCodes.Brfalse_S, start));
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
            il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName)); // [stack = this | name ]

            il.InsertBefore(start, il.Create(OpCodes.Call, getValueReference));

            var convertType = prop.PropertyType;
            if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
            {
                convertType = _references.RealmObjectBase;
            }

            if (!prop.IsRealmValue())
            {
                var convertMethod = new MethodReference("op_Explicit", convertType, _references.RealmValue)
                {
                    Parameters = { new ParameterDefinition(_references.RealmValue) },
                    HasThis = false
                };

                il.InsertBefore(start, il.Create(OpCodes.Call, convertMethod));
            }

            // This only happens when we have a relationship - explicitly cast.
            if (convertType != prop.PropertyType)
            {
                il.InsertBefore(start, il.Create(OpCodes.Castclass, prop.PropertyType));
            }

            il.InsertBefore(start, il.Create(OpCodes.Ret));
        }

        private static void ReplaceCollectionGetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference getCollectionValueReference)
        {
            if (backingField is FieldDefinition backingFieldDef)
            {
                backingFieldDef.Attributes &= ~FieldAttributes.InitOnly;  // without a set; auto property has this flag we must clear
            }

            //// A synthesized property getter looks like this:
            ////   0: ldarg.0  // load the this pointer
            ////   1: ldfld <backingField>
            ////   2: ret
            //// We want to change it so it looks somewhat like this, in C#
            ////
            ////  if (<backingField> == null)
            ////  {
            ////     <backingField> = GetCollectionValue<T>(<columnName>);
            ////  }
            ////  // original auto-generated getter starts here
            ////  return <backingField>; // supplied by the generated getter

            var start = prop.GetMethod.Body.Instructions.First();  // this is a label for return <backingField>;
            var il = prop.GetMethod.Body.GetILProcessor();

            // if (<backingField>) goto start
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Ldfld, backingField));
            il.InsertBefore(start, il.Create(OpCodes.Brtrue_S, start));

            // <backingField> is null -> <backingField> = GetCollectionValue<T>(<columnName>)
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(start, il.Create(OpCodes.Ldstr, columnName));
            il.InsertBefore(start, il.Create(OpCodes.Call, getCollectionValueReference));
            il.InsertBefore(start, il.Create(OpCodes.Stfld, backingField));

            // note that we do NOT insert a ret, unlike other weavers, as usual path branches and
            // FALL THROUGH to return the backing field.
        }

        // WARNING
        // This code setting the backing field only works if the field is settable after init
        // if you don't have an automatic set; on the property, it shows in the debugger with
        //         Attributes    Private | InitOnly    Mono.Cecil.FieldAttributes
        private void ReplaceBacklinksGetter(PropertyDefinition prop, FieldReference backingField, string columnName, TypeReference elementType)
        {
            //// A synthesized property getter looks like this:
            ////   0: ldarg.0  // load the this pointer
            ////   1: ldfld <backingField>
            ////   2: ret
            //// We want to change it so it looks somewhat like this, in C#
            ////
            ////  if (<backingField> == null)
            ////  {
            ////     if (IsManaged)
            ////           <backingField> = GetBacklinks<T>(<columnName>);
            ////     else
            ////           <backingField> = new Enumerable.Empty<T>.AsQueryable();
            ////  }
            ////  // original auto-generated getter starts here
            ////  return <backingField>; // supplied by the generated getter OR RealmObject._CopyDataFromBackingFields

            var start = prop.GetMethod.Body.Instructions.First();  // this is a label for return <backingField>;
            var il = prop.GetMethod.Body.GetILProcessor();

            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for field ref [ -> this]
            il.InsertBefore(start, il.Create(OpCodes.Ldfld, backingField)); // [ this -> field]
            il.InsertBefore(start, il.Create(OpCodes.Brtrue_S, start));  // []

            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for stfld in both branches [ -> this ]
            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0));  // this for call [ this -> this, this]
            il.InsertBefore(start, il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));  // [ this, this -> this,  isManaged ]

            // push in the label then go relative to that - so we can forward-ref the lable insert if/else blocks backwards
            var labelElse = il.Create(OpCodes.Nop);  // [this]
            il.InsertBefore(start, labelElse); // else
            il.InsertBefore(start, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.System_Linq_Enumerable_Empty) { GenericArguments = { elementType } })); // [this, enumerable]
            il.InsertBefore(start, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.System_Linq_Queryable_AsQueryable) { GenericArguments = { elementType } })); // [this, queryable]
            il.InsertBefore(start, il.Create(OpCodes.Stfld, backingField));  // [this, queryable -> ]

            // fall through to start to read it back from backing field and return
            // if block before else now gets inserted
            il.InsertBefore(labelElse, il.Create(OpCodes.Brfalse_S, labelElse));  // [this,  isManaged -> this]
            il.InsertBefore(labelElse, il.Create(OpCodes.Ldarg_0)); // this for call [ this -> this, this ]
            il.InsertBefore(labelElse, il.Create(OpCodes.Ldstr, columnName));  // [this, this -> this, this, name ]
            il.InsertBefore(labelElse, il.Create(OpCodes.Call, new GenericInstanceMethod(_references.RealmObject_GetBacklinks) { GenericArguments = { elementType } })); // [this, this, name -> this, queryable ]
            il.InsertBefore(labelElse, il.Create(OpCodes.Stfld, backingField)); // [this, queryable -> ]
            il.InsertBefore(labelElse, il.Create(OpCodes.Br_S, start));

            // note that we do NOT insert a ret, unlike other weavers, as usual path branches and
            // FALL THROUGH to return the backing field.
        }

        private void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference)
        {
            //// A synthesized property setter looks like this:
            ////   0: ldarg.0
            ////   1: ldarg.1
            ////   2: stfld <backingField>
            ////   3: ret
            ////
            //// We want to change it so it looks like this:
            ////   0: ldarg.0
            ////   1: call Realms.RealmObject.get_IsManaged
            ////   2: brfalse.s 8
            ////   3: ldarg.0
            ////   4: ldstr <columnName>
            ////   5: ldarg.1
            ////   6: call Realms.RealmObject.SetValue<T>
            ////   7: ret
            ////   8: ldarg.0
            ////   9: ldarg.1
            ////   10: stfld <backingField>
            ////   11: ret
            ////
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged) this.<backingField> = value;
            ////   else base.SetValue<T>(<columnName>, value);

            if (setValueReference == null)
            {
                throw new ArgumentNullException(nameof(setValueReference));
            }

            // Whilst we're only targetting auto-properties here, someone like PropertyChanged.Fody
            // may have already come in and rewritten our IL. Lets clear everything and start from scratch.
            var il = prop.SetMethod.Body.GetILProcessor();
            prop.SetMethod.Body.Instructions.Clear();
            prop.SetMethod.Body.Variables.Clear();

            // While we can tidy up PropertyChanged.Fody IL if we're ran after it, we can't do a heck of a lot
            // if they're the last one in. To combat this, we'll add our own version of [DoNotNotify] which
            // PropertyChanged.Fody will respect.
            prop.CustomAttributes.Add(new CustomAttribute(_propertyChanged_DoNotNotify_Ctor.Value));

            var managedSetStart = il.Create(OpCodes.Ldarg_0);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
            il.Append(il.Create(OpCodes.Brtrue_S, managedSetStart));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Stfld, backingField));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldstr, prop.Name));
            il.Append(il.Create(OpCodes.Call, _references.RealmObject_RaisePropertyChanged));
            il.Append(il.Create(OpCodes.Ret));

            il.Append(managedSetStart);
            il.Append(il.Create(OpCodes.Ldstr, columnName));
            il.Append(il.Create(OpCodes.Ldarg_1));

            if (!prop.IsRealmValue())
            {
                var convertType = prop.PropertyType;
                if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
                {
                    convertType = _references.RealmObjectBase;
                }

                var convertMethod = new MethodReference("op_Implicit", _references.RealmValue, _references.RealmValue)
                {
                    Parameters = { new ParameterDefinition(convertType) },
                    HasThis = false
                };

                il.Append(il.Create(OpCodes.Call, convertMethod));
            }

            il.Append(il.Create(OpCodes.Call, setValueReference));
            il.Append(il.Create(OpCodes.Ret));
        }

        private TypeDefinition WeaveRealmObjectHelper(TypeDefinition realmObjectType, MethodDefinition objectConstructor, List<WeavePropertyResult> properties)
        {
            var helperType = new TypeDefinition(null, "RealmHelper",
                                                TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit,
                                                _moduleDefinition.TypeSystem.Object);

            helperType.Interfaces.Add(new InterfaceImplementation(_references.IRealmObjectHelper));

            var createInstance = new MethodDefinition("CreateInstance", DefaultMethodAttributes, _references.IRealmObjectBase);
            {
                var il = createInstance.Body.GetILProcessor();
                il.Emit(OpCodes.Newobj, objectConstructor);
                il.Emit(OpCodes.Ret);
            }

            helperType.Methods.Add(createInstance);

            var copyToRealm = new MethodDefinition("CopyToRealm", DefaultMethodAttributes, _moduleDefinition.TypeSystem.Void);
            {
                // This roughly translates to
                /*
                    var castInstance = (ObjectType)instance;

                    *foreach* non-list woven property in castInstance's schema
                    *if* castInstace.field is a RealmObject descendant
                        castInstance.Realm.Add(castInstance.field, update);
                        castInstance.Property = castInstance.Field;
                    *else if* property is PK
                        *do nothing*
                    *else if* property is [Required] or nullable
                        castInstance.Property = castInstance.Field;
                    *else*
                        if (!skipDefaults || castInstance.field != default(fieldType))
                        {
                            castInstance.Property = castInstance.Field;
                        }

                    *foreach* list woven property in castInstance's schema
                    var list = castInstance.field;
                    castInstance.field = null;
                    if (!skipDefaults)
                    {
                        castInstance.Property.Clear();
                    }
                    if (list != null)
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            castInstance.Realm.Add(list[i], update);
                            castInstance.Property.Add(list[i]);
                        }
                    }

                    *foreach* set woven property in castInstance's schema
                    var set = castInstance.field;
                    castInstance.field = null;
                    if (!skipDefaults)
                    {
                        castInstance.Property.Clear();
                    }
                    if (set != null)
                    {
                        castInstance.Realm.Add(set);
                        castInstance.Property.Union(set);
                    }
                */

                var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, _references.IRealmObjectBase);
                copyToRealm.Parameters.Add(instanceParameter);

                var updateParameter = new ParameterDefinition("update", ParameterAttributes.None, _moduleDefinition.TypeSystem.Boolean);
                copyToRealm.Parameters.Add(updateParameter);

                var skipDefaultsParameter = new ParameterDefinition("skipDefaults", ParameterAttributes.None, _moduleDefinition.TypeSystem.Boolean);
                copyToRealm.Parameters.Add(skipDefaultsParameter);

                copyToRealm.Body.Variables.Add(new VariableDefinition(realmObjectType));

                copyToRealm.Body.InitLocals = true;
                var il = copyToRealm.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ldarg_1));
                il.Append(il.Create(OpCodes.Castclass, _moduleDefinition.ImportReference(realmObjectType)));
                il.Append(il.Create(OpCodes.Stloc_0));

                // We'll process collections separately as those require variable access
                foreach (var prop in properties.Where(p => !p.IsPrimaryKey && !p.Property.IsCollection(out _)))
                {
                    var property = prop.Property;
                    var field = prop.Field;

                    if (property.SetMethod != null)
                    {
                        // If the property is RealmObject, we want the following code to execute:
                        // if (castInstance.field != null)
                        // {
                        //     castInstance.Realm.Add(castInstance.field, update)
                        // }
                        // castInstance.Property = castInstance.field;
                        //
                        // *addPlaceholder* will be the Brfalse instruction that will skip the call to Add if the field is null.
                        Instruction addPlaceholder = null;

                        // We can skip setting properties that have their default values unless:
                        var shouldSetAlways = property.IsNullable() || // The property is nullable - those should be set explicitly to null
                                              property.IsRequired(_references) || // Needed for validating that the property is not null (string)
                                              property.IsDateTimeOffset() || // Core's DateTimeOffset property defaults to 1970-1-1, so we should override
                                              property.PropertyType.IsRealmInteger(out _, out _) || // structs are not implicitly falsy/truthy so the IL is significantly different; we can optimize this case in the future
                                              property.IsDecimal() ||
                                              property.IsDecimal128() ||
                                              property.IsRealmValue() ||
                                              property.IsObjectId() ||
                                              property.IsGuid();

                        // If the property is non-nullable, we want the following code to execute:
                        // if (!skipDefaults || castInstance.field != default(fieldType))
                        // {
                        //     castInstance.Property = castInstance.field
                        // }
                        //
                        // *updatePlaceholder* will be the Brtrue instruction that will skip the default check and move to the
                        // property setting logic. The default check branching instruction is inserted above the *setStartPoint*
                        // instruction later on.
                        Instruction skipDefaultsPlaceholder = null;
                        if (property.ContainsRealmObject(_references))
                        {
                            il.Append(il.Create(OpCodes.Ldloc_0));
                            il.Append(il.Create(OpCodes.Ldfld, field));

                            addPlaceholder = il.Create(OpCodes.Nop);
                            il.Append(addPlaceholder);

                            il.Append(il.Create(OpCodes.Ldloc_0));
                            il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_Realm));
                            il.Append(il.Create(OpCodes.Ldloc_0));
                            il.Append(il.Create(OpCodes.Ldfld, field));
                            il.Append(il.Create(OpCodes.Ldarg_2));
                            il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.Realm_Add) { GenericArguments = { field.FieldType } }));
                            il.Append(il.Create(OpCodes.Pop));
                        }
                        else if (!shouldSetAlways)
                        {
                            il.Append(il.Create(OpCodes.Ldarg_3));
                            skipDefaultsPlaceholder = il.Create(OpCodes.Nop);
                            il.Append(skipDefaultsPlaceholder);

                            il.Append(il.Create(OpCodes.Ldloc_0));
                            il.Append(il.Create(OpCodes.Ldfld, field));

                            if (property.IsSingle())
                            {
                                il.Append(il.Create(OpCodes.Ldc_R4, 0f));
                            }
                            else if (property.IsDouble())
                            {
                                il.Append(il.Create(OpCodes.Ldc_R8, 0.0));
                            }
                        }

                        var setStartPoint = il.Create(OpCodes.Ldloc_0);
                        il.Append(setStartPoint);
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldfld, field));
                        il.Append(il.Create(OpCodes.Call, property.SetMethod));

                        var setEndPoint = il.Create(OpCodes.Nop);
                        il.Append(setEndPoint);

                        if (property.ContainsRealmObject(_references))
                        {
                            if (addPlaceholder != null)
                            {
                                il.Replace(addPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                            }
                        }
                        else if (!shouldSetAlways)
                        {
                            // Branching instruction to check if we're trying to set the default value of a property.
                            if (property.IsSingle() || property.IsDouble())
                            {
                                il.InsertBefore(setStartPoint, il.Create(OpCodes.Beq_S, setEndPoint));
                            }
                            else
                            {
                                il.InsertBefore(setStartPoint, il.Create(OpCodes.Brfalse_S, setEndPoint));
                            }

                            if (skipDefaultsPlaceholder != null)
                            {
                                il.Replace(skipDefaultsPlaceholder, il.Create(OpCodes.Brfalse_S, setStartPoint));
                            }
                        }
                    }
                    else if (property.IsIQueryable())
                    {
                        il.Append(il.Create(OpCodes.Ldloc_0));
                        il.Append(il.Create(OpCodes.Ldnull));
                        il.Append(il.Create(OpCodes.Stfld, field));
                    }
                    else
                    {
                        var sequencePoint = property.GetMethod.DebugInformation.SequencePoints.FirstOrDefault();
                        _logger.Error($"{realmObjectType.Name}.{property.Name} does not have a setter and is not an IList. This is an error in Realm, so please file a bug report.", sequencePoint);
                    }
                }

                // Process collection properties
                foreach (var prop in properties)
                {
                    if (!prop.Property.IsCollection(out var collectionType))
                    {
                        continue;
                    }

                    var property = prop.Property;
                    var field = prop.Field;

                    var elementType = ((GenericInstanceType)property.PropertyType).GenericArguments.Last();
                    var propertyGetterMethodReference = _moduleDefinition.ImportReference(property.GetMethod);

                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldfld, field));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Ldnull));

                    il.Append(il.Create(OpCodes.Stfld, field));
                    il.Append(il.Create(OpCodes.Ldloc_0));
                    il.Append(il.Create(OpCodes.Call, propertyGetterMethodReference));

                    il.Append(il.Create(OpCodes.Ldarg_2));
                    il.Append(il.Create(OpCodes.Ldarg_3));

                    if (collectionType == RealmCollectionType.IDictionary)
                    {
                        il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.CollectionExtensions_PopulateDictionary) { GenericArguments = { elementType } }));
                    }
                    else
                    {
                        il.Append(il.Create(OpCodes.Call, new GenericInstanceMethod(_references.CollectionExtensions_PopulateCollection) { GenericArguments = { elementType } }));
                    }
                }

                il.Emit(OpCodes.Ret);
            }

            copyToRealm.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));
            helperType.Methods.Add(copyToRealm);

            var getPrimaryKeyValue = new MethodDefinition("TryGetPrimaryKeyValue", DefaultMethodAttributes, _moduleDefinition.TypeSystem.Boolean);
            {
                var instanceParameter = new ParameterDefinition("instance", ParameterAttributes.None, _references.IRealmObjectBase);
                getPrimaryKeyValue.Parameters.Add(instanceParameter);

                var valueParameter = new ParameterDefinition("value", ParameterAttributes.Out, new ByReferenceType(_moduleDefinition.TypeSystem.Object))
                {
                    IsOut = true
                };
                getPrimaryKeyValue.Parameters.Add(valueParameter);

                getPrimaryKeyValue.Body.Variables.Add(new VariableDefinition(_moduleDefinition.ImportReference(realmObjectType)));

                var il = getPrimaryKeyValue.Body.GetILProcessor();
                var pkProperty = properties.FirstOrDefault(p => p.IsPrimaryKey);

                if (pkProperty != null)
                {
                    getPrimaryKeyValue.Body.InitLocals = true;

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, _moduleDefinition.ImportReference(realmObjectType));
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Callvirt, _moduleDefinition.ImportReference(pkProperty.Property.GetMethod));
                    if (!pkProperty.Property.IsString())
                    {
                        il.Emit(OpCodes.Box, pkProperty.Property.PropertyType);
                    }

                    il.Emit(OpCodes.Stind_Ref);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stind_Ref);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ret);
                }
            }

            getPrimaryKeyValue.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));
            helperType.Methods.Add(getPrimaryKeyValue);

            const MethodAttributes CtorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var ctor = new MethodDefinition(".ctor", CtorAttributes, _moduleDefinition.TypeSystem.Void);
            {
                var il = ctor.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, new MethodReference(".ctor", _moduleDefinition.TypeSystem.Void, _moduleDefinition.TypeSystem.Object) { HasThis = true });
                il.Emit(OpCodes.Ret);
            }

            var preserveAttribute = new CustomAttribute(_references.PreserveAttribute_Constructor);
            ctor.CustomAttributes.Add(preserveAttribute);

            helperType.Methods.Add(ctor);

            realmObjectType.NestedTypes.Add(helperType);

            return helperType;
        }

        private bool IsUsingSync()
        {
            try
            {
                return IsMethodUsed(_references.SyncConfiguration);
            }
            catch
            {
                return false;
            }
        }

        private bool IsMethodUsed(TypeReference type)
        {
            return _moduleDefinition.GetTypes()
                       .SelectMany(t => t.Methods)
                       .Where(m => m.HasBody)
                       .SelectMany(m => m.Body.Instructions)
                       .Any(i => i.OpCode == OpCodes.Newobj &&
                                 i.Operand is MethodReference mRef &&
                                 mRef.ConstructsType(type));
        }

        private MethodReference GetOrAddPropertyChanged_DoNotNotify()
        {
            // If the assembly has a reference to PropertyChanged.Fody, let's look up the DoNotNotifyAttribute for use later.
            // It is possible that a project references PropertyChanged.Fody but it doesn't show up in the Module references
            // because it is not being used in code. There's not much we can do about it in this case as there's no obvious
            // way to determine whether PropertyChanged is in use or not. That's why we construct our own PropertyChanged.DoNotNotify
            // attribute and apply it to Realm-processed properties.
            var propertyChanged_Fody = _moduleDefinition.FindReference("PropertyChanged");
            if (propertyChanged_Fody != null)
            {
                var propertyChanged_DoNotNotify = new TypeReference("PropertyChanged", "DoNotNotifyAttribute", _moduleDefinition, propertyChanged_Fody);
                return new MethodReference(".ctor", _moduleDefinition.TypeSystem.Void, propertyChanged_DoNotNotify) { HasThis = true };
            }

            var system_Attribute = new TypeReference("System", "Attribute", _moduleDefinition, _moduleDefinition.TypeSystem.CoreLibrary);

            var userAssembly_DoNotNotify = new TypeDefinition("PropertyChanged", "DoNotNotifyAttribute",
                                TypeAttributes.Class | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed,
                                system_Attribute);

            const MethodAttributes CtorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var userAssembly_DoNotNotify_Ctor = new MethodDefinition(".ctor", CtorAttributes, _moduleDefinition.TypeSystem.Void);
            {
                var il = userAssembly_DoNotNotify_Ctor.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, new MethodReference(".ctor", _moduleDefinition.TypeSystem.Void, system_Attribute) { HasThis = true });
                il.Emit(OpCodes.Ret);
            }

            userAssembly_DoNotNotify.Methods.Add(userAssembly_DoNotNotify_Ctor);
            _moduleDefinition.Types.Add(userAssembly_DoNotNotify);

            return userAssembly_DoNotNotify_Ctor;
        }
    }
}
