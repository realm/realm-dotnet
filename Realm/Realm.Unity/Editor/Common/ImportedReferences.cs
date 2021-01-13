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
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;

namespace RealmWeaver
{
#pragma warning disable CS0618 // Type or member is obsolete - Mono.Cecil.TypeSystem is obsolete, but we need it in Unity

    internal abstract class ImportedReferences
    {
        public FrameworkName Framework { get; }

        public TypeReference IEnumerableOfT { get; }

        public TypeReference ICollectionOfT { get; }

        public MethodReference ICollectionOfT_Add { get; private set; }

        public MethodReference ICollectionOfT_Clear { get; private set; }

        public MethodReference ICollectionOfT_get_Count { get; private set; }

        public TypeReference IListOfT { get; }

        public MethodReference IListOfT_get_Item { get; private set; }

        public abstract TypeReference ISetOfT { get; }

        public abstract TypeReference IDictionaryOfTKeyTValue { get; }

        public MethodReference ISetOfT_UnionWith { get; private set; }

        public abstract TypeReference IQueryableOfT { get; }

        public TypeReference System_Type { get; }

        public MethodReference System_Type_GetTypeFromHandle { get; }

        public abstract TypeReference System_Collections_Generic_ListOfT { get; }

        public MethodReference System_Collections_Generic_ListOfT_Constructor { get; private set; }

        public abstract TypeReference System_Collections_Generic_HashSetOfT { get; }

        public MethodReference System_Collections_Generic_HashSetOfT_Constructor { get; private set; }

        public abstract TypeReference System_Collections_Generic_DictionaryOfTKeyTValue { get; }

        public MethodReference System_Collections_Generic_DictionaryOfTKeyTValue_Constructor { get; private set; }

        public abstract TypeReference System_Linq_Enumerable { get; }

        public MethodReference System_Linq_Enumerable_Empty { get; private set; }

        public abstract TypeReference System_Linq_Queryable { get; }

        public MethodReference System_Linq_Queryable_AsQueryable { get; private set; }

        public TypeReference System_ValueType { get; }

        public TypeReference System_IFormattable { get; }

        public TypeReference System_IComparableOfT { get; }

        public TypeReference System_NullableOfT { get; }

        public MethodReference System_NullableOfT_GetValueOrDefault { get; }

        public MethodReference System_NullableOfT_Ctor { get; }

        public TypeReference Realm { get; private set; }

        public MethodReference Realm_Add { get; private set; }

        public MethodReference Realm_AddCollection { get; private set; }

        public TypeReference RealmObject { get; private set; }

        public TypeReference RealmObjectBase { get; private set; }

        public TypeReference EmbeddedObject { get; private set; }

        public MethodReference RealmObject_get_IsManaged { get; private set; }

        public MethodReference RealmObject_get_Realm { get; private set; }

        public MethodReference RealmObject_RaisePropertyChanged { get; private set; }

        public MethodReference RealmObject_GetListValue { get; private set; }

        public MethodReference RealmObject_GetSetValue { get; private set; }

        public MethodReference RealmObject_GetDictionaryValue { get; private set; }

        public MethodReference RealmObject_GetBacklinks { get; private set; }

        public TypeReference RealmValue { get; private set; }

        public MethodReference RealmObject_GetValue { get; private set; }

        public MethodReference RealmObject_SetValue { get; private set; }

        public MethodReference RealmObject_SetValueUnique { get; private set; }

        public TypeReference IRealmObjectHelper { get; private set; }

        public TypeReference PreserveAttribute { get; private set; }

        public MethodReference PreserveAttribute_Constructor { get; private set; }

        public MethodReference PreserveAttribute_ConstructorWithParams { get; private set; }

        public TypeReference WovenAttribute { get; private set; }

        public MethodReference WovenAttribute_Constructor { get; private set; }

        public TypeReference WovenAssemblyAttribute { get; private set; }

        public MethodReference WovenAssemblyAttribute_Constructor { get; private set; }

        public TypeReference ExplicitAttribute { get; private set; }

        public TypeReference WovenPropertyAttribute { get; private set; }

        public MethodReference WovenPropertyAttribute_Constructor { get; private set; }

        public TypeReference PropertyChanged_DoNotNotifyAttribute { get; private set; }

        public MethodReference PropertyChanged_DoNotNotifyAttribute_Constructor { get; private set; }

        public MethodReference RealmSchema_AddDefaultTypes { get; private set; }

        public TypeReference RealmSchema_PropertyType { get; private set; }

        public TypeReference SyncConfiguration { get; private set; }

        public MethodReference CollectionExtensions_PopulateCollection { get; private set; }

        public MethodReference CollectionExtensions_PopulateDictionary { get; private set; }

        protected ModuleDefinition Module { get; }

        public TypeSystem Types { get; }

        protected ImportedReferences(ModuleDefinition module, TypeSystem types, FrameworkName frameworkName)
        {
            Module = module;
            Types = types;
            Framework = frameworkName;

            IEnumerableOfT = new TypeReference("System.Collections.Generic", "IEnumerable`1", Module, Module.TypeSystem.CoreLibrary);
            IEnumerableOfT.GenericParameters.Add(new GenericParameter(IEnumerableOfT));

            ICollectionOfT = new TypeReference("System.Collections.Generic", "ICollection`1", Module, Module.TypeSystem.CoreLibrary);
            ICollectionOfT.GenericParameters.Add(new GenericParameter(ICollectionOfT));

            IListOfT = new TypeReference("System.Collections.Generic", "IList`1", Module, Module.TypeSystem.CoreLibrary);
            IListOfT.GenericParameters.Add(new GenericParameter(IListOfT));

            System_ValueType = new TypeReference("System", "ValueType", Module, Module.TypeSystem.CoreLibrary);

            System_IFormattable = new TypeReference("System", "IFormattable", Module, Module.TypeSystem.CoreLibrary);

            System_IComparableOfT = new TypeReference("System", "IComparable`1", Module, Module.TypeSystem.CoreLibrary);
            System_IComparableOfT.GenericParameters.Add(new GenericParameter(System_IComparableOfT));

            System_NullableOfT = new TypeReference("System", "Nullable`1", Module, Module.TypeSystem.CoreLibrary) { IsValueType = true };
            System_NullableOfT.GenericParameters.Add(new GenericParameter(System_NullableOfT)
            {
                Constraints = { new GenericParameterConstraint(System_ValueType) }
            });

            System_NullableOfT_GetValueOrDefault = new MethodReference("GetValueOrDefault", System_NullableOfT.GenericParameters[0], System_NullableOfT)
            {
                HasThis = true
            };

            System_NullableOfT_Ctor = new MethodReference(".ctor", Types.Void, System_NullableOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(System_NullableOfT.GenericParameters[0]) }
            };

            var runtimeTypeHandle = new TypeReference("System", "RuntimeTypeHandle", Module, Module.TypeSystem.CoreLibrary)
            {
                IsValueType = true
            };

            System_Type = new TypeReference("System", "Type", Module, Module.TypeSystem.CoreLibrary);
            System_Type_GetTypeFromHandle = new MethodReference("GetTypeFromHandle", System_Type, System_Type)
            {
                HasThis = false,
                Parameters = { new ParameterDefinition(runtimeTypeHandle) }
            };

            // If the assembly has a reference to PropertyChanged.Fody, let's look up the DoNotNotifyAttribute for use later.
            var PropertyChanged_Fody = Module.AssemblyReferences.SingleOrDefault(a => a.Name == "PropertyChanged");
            if (PropertyChanged_Fody != null)
            {
                InitializePropertyChanged_Fody(PropertyChanged_Fody);
            }
        }

        private void InitializeFrameworkMethods()
        {
            ICollectionOfT_Add = new MethodReference("Add", Types.Void, ICollectionOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(ICollectionOfT.GenericParameters.Single()) }
            };

            ICollectionOfT_Clear = new MethodReference("Clear", Types.Void, ICollectionOfT) { HasThis = true };

            ICollectionOfT_get_Count = new MethodReference("get_Count", Types.Int32, ICollectionOfT) { HasThis = true };

            IListOfT_get_Item = new MethodReference("get_Item", IListOfT.GenericParameters.Single(), IListOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.Int32) }
            };

            ISetOfT_UnionWith = new MethodReference("UnionWith", Types.Void, ISetOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(new GenericInstanceType(IEnumerableOfT) { GenericArguments = { ISetOfT.GenericParameters.Single() } }) }
            };

            System_Collections_Generic_ListOfT_Constructor = new MethodReference(".ctor", Types.Void, System_Collections_Generic_ListOfT) { HasThis = true };

            System_Collections_Generic_HashSetOfT_Constructor = new MethodReference(".ctor", Types.Void, System_Collections_Generic_HashSetOfT) { HasThis = true };

            System_Collections_Generic_DictionaryOfTKeyTValue_Constructor = new MethodReference(".ctor", Types.Void, System_Collections_Generic_DictionaryOfTKeyTValue) { HasThis = true };

            {
                System_Linq_Enumerable_Empty = new MethodReference("Empty", Types.Void, System_Linq_Enumerable);
                var T = new GenericParameter(System_Linq_Enumerable_Empty);
                System_Linq_Enumerable_Empty.ReturnType = new GenericInstanceType(IEnumerableOfT) { GenericArguments = { T } };
                System_Linq_Enumerable_Empty.GenericParameters.Add(T);
            }

            {
                System_Linq_Queryable_AsQueryable = new MethodReference("AsQueryable", Types.Void, System_Linq_Queryable);
                var T = new GenericParameter(System_Linq_Queryable_AsQueryable);
                System_Linq_Queryable_AsQueryable.Parameters.Add(new ParameterDefinition(new GenericInstanceType(IEnumerableOfT) { GenericArguments = { T } }));
                System_Linq_Queryable_AsQueryable.ReturnType = new GenericInstanceType(IQueryableOfT) { GenericArguments = { T } };
                System_Linq_Queryable_AsQueryable.GenericParameters.Add(T);
            }
        }

        private void InitializeRealm(IMetadataScope realmAssembly)
        {
            Realm = new TypeReference("Realms", "Realm", Module, realmAssembly);
            RealmObjectBase = new TypeReference("Realms", "RealmObjectBase", Module, realmAssembly);
            RealmObject = new TypeReference("Realms", "RealmObject", Module, realmAssembly);
            EmbeddedObject = new TypeReference("Realms", "EmbeddedObject", Module, realmAssembly);
            RealmSchema_PropertyType = new TypeReference("Realms.Schema", "PropertyType", Module, realmAssembly, valueType: true);
            RealmValue = new TypeReference("Realms", "RealmValue", Module, realmAssembly, valueType: true);

            {
                Realm_Add = new MethodReference("Add", Types.Void, Realm) { HasThis = true };
                var T = new GenericParameter(Realm_Add) { Constraints = { new GenericParameterConstraint(RealmObject) } };
                Realm_Add.ReturnType = T;
                Realm_Add.GenericParameters.Add(T);
                Realm_Add.Parameters.Add(new ParameterDefinition(T));
                Realm_Add.Parameters.Add(new ParameterDefinition(Types.Boolean));
            }

            {
                Realm_AddCollection = new MethodReference("Add", Types.Void, Realm) { HasThis = true };
                var T = new GenericParameter(Realm_AddCollection) { Constraints = { new GenericParameterConstraint(RealmObject) } };
                Realm_AddCollection.GenericParameters.Add(T);
                Realm_AddCollection.Parameters.Add(new ParameterDefinition(new GenericInstanceType(IEnumerableOfT) { GenericArguments = { T } }));
                Realm_AddCollection.Parameters.Add(new ParameterDefinition(Types.Boolean));
            }

            RealmObject_get_IsManaged = new MethodReference("get_IsManaged", Types.Boolean, RealmObjectBase) { HasThis = true };
            RealmObject_get_Realm = new MethodReference("get_Realm", Realm, RealmObjectBase) { HasThis = true };
            RealmObject_RaisePropertyChanged = new MethodReference("RaisePropertyChanged", Types.Void, RealmObjectBase)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.String) }
            };

            {
                RealmObject_GetListValue = new MethodReference("GetListValue", new GenericInstanceType(IListOfT), RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetListValue);
                (RealmObject_GetListValue.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetListValue.GenericParameters.Add(T);
                RealmObject_GetListValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_GetSetValue = new MethodReference("GetSetValue", new GenericInstanceType(ISetOfT), RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetSetValue);
                (RealmObject_GetSetValue.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetSetValue.GenericParameters.Add(T);
                RealmObject_GetSetValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_GetDictionaryValue = new MethodReference("GetDictionaryValue", new GenericInstanceType(IDictionaryOfTKeyTValue), RealmObjectBase) { HasThis = true };
                var TValue = new GenericParameter(RealmObject_GetSetValue);
                (RealmObject_GetDictionaryValue.ReturnType as GenericInstanceType).GenericArguments.Add(Types.String);
                (RealmObject_GetDictionaryValue.ReturnType as GenericInstanceType).GenericArguments.Add(TValue);
                RealmObject_GetDictionaryValue.GenericParameters.Add(TValue);
                RealmObject_GetDictionaryValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_GetBacklinks = new MethodReference("GetBacklinks", new GenericInstanceType(IQueryableOfT), RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetBacklinks) { Constraints = { new GenericParameterConstraint(RealmObjectBase) } };
                (RealmObject_GetBacklinks.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetBacklinks.GenericParameters.Add(T);
                RealmObject_GetBacklinks.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_GetValue = new MethodReference("GetValue", Types.Void, RealmObjectBase) { HasThis = true };
                RealmObject_GetValue.ReturnType = RealmValue;
                RealmObject_GetValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_SetValue = new MethodReference("SetValue", Types.Void, RealmObjectBase) { HasThis = true };
                RealmObject_SetValue.Parameters.Add(new ParameterDefinition(Types.String));
                RealmObject_SetValue.Parameters.Add(new ParameterDefinition(RealmValue));
            }

            {
                RealmObject_SetValueUnique = new MethodReference("SetValueUnique", Types.Void, RealmObjectBase) { HasThis = true };
                RealmObject_SetValueUnique.Parameters.Add(new ParameterDefinition(Types.String));
                RealmObject_SetValueUnique.Parameters.Add(new ParameterDefinition(RealmValue));
            }

            IRealmObjectHelper = new TypeReference("Realms.Weaving", "IRealmObjectHelper", Module, realmAssembly);

            PreserveAttribute = new TypeReference("Realms", "PreserveAttribute", Module, realmAssembly);
            PreserveAttribute_Constructor = new MethodReference(".ctor", Types.Void, PreserveAttribute) { HasThis = true };
            PreserveAttribute_ConstructorWithParams = new MethodReference(".ctor", Types.Void, PreserveAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.Boolean), new ParameterDefinition(Types.Boolean) }
            };

            WovenAttribute = new TypeReference("Realms", "WovenAttribute", Module, realmAssembly);
            WovenAttribute_Constructor = new MethodReference(".ctor", Types.Void, WovenAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(System_Type) }
            };

            WovenAssemblyAttribute = new TypeReference("Realms", "WovenAssemblyAttribute", Module, realmAssembly);
            WovenAssemblyAttribute_Constructor = new MethodReference(".ctor", Types.Void, WovenAssemblyAttribute) { HasThis = true };

            WovenPropertyAttribute = new TypeReference("Realms", "WovenPropertyAttribute", Module, realmAssembly);
            WovenPropertyAttribute_Constructor = new MethodReference(".ctor", Types.Void, WovenPropertyAttribute) { HasThis = true };

            ExplicitAttribute = new TypeReference("Realms", "ExplicitAttribute", Module, realmAssembly);

            var realmSchema = new TypeReference("Realms.Schema", "RealmSchema", Module, realmAssembly);
            RealmSchema_AddDefaultTypes = new MethodReference("AddDefaultTypes", Types.Void, realmSchema) { HasThis = false };
            {
                var ienumerableOfType = new GenericInstanceType(IEnumerableOfT)
                {
                    GenericArguments = { System_Type }
                };

                RealmSchema_AddDefaultTypes.Parameters.Add(new ParameterDefinition(ienumerableOfType));
            }

            SyncConfiguration = new TypeReference("Realms.Sync", "SyncConfiguration", Module, realmAssembly);

            var collectionExtensions = new TypeReference("Realms", "CollectionExtensions", Module, realmAssembly);
            CollectionExtensions_PopulateCollection = new MethodReference("PopulateCollection", Types.Void, collectionExtensions) { HasThis = false };
            {
                var T = new GenericParameter(CollectionExtensions_PopulateCollection);
                CollectionExtensions_PopulateCollection.GenericParameters.Add(T);
                CollectionExtensions_PopulateCollection.Parameters.Add(new ParameterDefinition(new GenericInstanceType(ICollectionOfT) { GenericArguments = { T } }));
                CollectionExtensions_PopulateCollection.Parameters.Add(new ParameterDefinition(new GenericInstanceType(ICollectionOfT) { GenericArguments = { T } }));
                CollectionExtensions_PopulateCollection.Parameters.Add(new ParameterDefinition(Types.Boolean));
                CollectionExtensions_PopulateCollection.Parameters.Add(new ParameterDefinition(Types.Boolean));
            }

            CollectionExtensions_PopulateDictionary = new MethodReference("PopulateCollection", Types.Void, collectionExtensions) { HasThis = false };
            {
                var T = new GenericParameter(CollectionExtensions_PopulateDictionary);
                CollectionExtensions_PopulateDictionary.GenericParameters.Add(T);
                CollectionExtensions_PopulateDictionary.Parameters.Add(new ParameterDefinition(new GenericInstanceType(IDictionaryOfTKeyTValue) { GenericArguments = { Types.String, T } }));
                CollectionExtensions_PopulateDictionary.Parameters.Add(new ParameterDefinition(new GenericInstanceType(IDictionaryOfTKeyTValue) { GenericArguments = { Types.String, T } }));
                CollectionExtensions_PopulateDictionary.Parameters.Add(new ParameterDefinition(Types.Boolean));
                CollectionExtensions_PopulateDictionary.Parameters.Add(new ParameterDefinition(Types.Boolean));
            }
        }

        private void InitializePropertyChanged_Fody(AssemblyNameReference propertyChangedAssembly)
        {
            PropertyChanged_DoNotNotifyAttribute = new TypeReference("PropertyChanged", "DoNotNotifyAttribute", Module, propertyChangedAssembly);
            PropertyChanged_DoNotNotifyAttribute_Constructor = new MethodReference(".ctor", Types.Void, PropertyChanged_DoNotNotifyAttribute) { HasThis = true };
        }

        protected AssemblyNameReference GetOrAddFrameworkReference(string assemblyName)
        {
            var assembly = Module.AssemblyReferences.SingleOrDefault(a => a.Name == assemblyName);
            if (assembly == null)
            {
                var corlib = (AssemblyNameReference)Module.TypeSystem.CoreLibrary;
                assembly = new AssemblyNameReference(assemblyName, corlib.Version)
                {
                    Attributes = corlib.Attributes,
                    Culture = corlib.Culture,
                    PublicKeyToken = corlib.PublicKeyToken
                };

                Module.AssemblyReferences.Add(assembly);
            }

            return assembly;
        }

        public FieldReference GetPropertyTypeField(string name)
        {
            return new FieldReference(name, RealmSchema_PropertyType, RealmSchema_PropertyType);
        }

        internal sealed class NETFramework : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Collections_Generic_HashSetOfT { get; }

            public override TypeReference System_Collections_Generic_DictionaryOfTKeyTValue { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public override TypeReference ISetOfT { get; }

            public override TypeReference IDictionaryOfTKeyTValue { get; }

            public NETFramework(ModuleDefinition module, TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, GetOrAddFrameworkReference("System.Core"));
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                ISetOfT = new TypeReference("System.Collections.Generic", "ISet`1", Module, GetOrAddFrameworkReference("System"));
                ISetOfT.GenericParameters.Add(new GenericParameter(ISetOfT));

                IDictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "IDictionary`2", Module, Module.TypeSystem.CoreLibrary);
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Collections_Generic_HashSetOfT = new TypeReference("System.Collections.Generic", "HashSet`1", Module, GetOrAddFrameworkReference("System.Core"));
                System_Collections_Generic_HashSetOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_HashSetOfT));

                System_Collections_Generic_DictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "Dictionary`2", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, GetOrAddFrameworkReference("System.Core"));
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, GetOrAddFrameworkReference("System.Core"));
            }
        }

        private sealed class NETPortable : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Collections_Generic_HashSetOfT { get; }

            public override TypeReference System_Collections_Generic_DictionaryOfTKeyTValue { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public override TypeReference ISetOfT { get; }

            public override TypeReference IDictionaryOfTKeyTValue { get; }

            public NETPortable(ModuleDefinition module, TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, GetOrAddFrameworkReference("System.Linq.Expressions"));
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                ISetOfT = new TypeReference("System.Collections.Generic", "ISet`1", Module, Module.TypeSystem.CoreLibrary);
                ISetOfT.GenericParameters.Add(new GenericParameter(ISetOfT));

                IDictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "IDictionary`2", Module, Module.TypeSystem.CoreLibrary);
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, GetOrAddFrameworkReference("System.Collections"));
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Collections_Generic_HashSetOfT = new TypeReference("System.Collections.Generic", "HashSet`1", Module, GetOrAddFrameworkReference("System.Collections"));
                System_Collections_Generic_HashSetOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_HashSetOfT));

                System_Collections_Generic_DictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "Dictionary`2", Module, GetOrAddFrameworkReference("System.Collections"));
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, GetOrAddFrameworkReference("System.Linq"));
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, GetOrAddFrameworkReference("System.Linq.Queryable"));
            }
        }

        private sealed class NetStandard2 : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Collections_Generic_HashSetOfT { get; }

            public override TypeReference System_Collections_Generic_DictionaryOfTKeyTValue { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public override TypeReference ISetOfT { get; }

            public override TypeReference IDictionaryOfTKeyTValue { get; }

            public NetStandard2(ModuleDefinition module, TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, Module.TypeSystem.CoreLibrary);
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                ISetOfT = new TypeReference("System.Collections.Generic", "ISet`1", Module, Module.TypeSystem.CoreLibrary);
                ISetOfT.GenericParameters.Add(new GenericParameter(ISetOfT));

                IDictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "IDictionary`2", Module, Module.TypeSystem.CoreLibrary);
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));
                IDictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(IDictionaryOfTKeyTValue));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Collections_Generic_HashSetOfT = new TypeReference("System.Collections.Generic", "HashSet`1", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_HashSetOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_HashSetOfT));

                System_Collections_Generic_DictionaryOfTKeyTValue = new TypeReference("System.Collections.Generic", "Dictionary`2", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));
                System_Collections_Generic_DictionaryOfTKeyTValue.GenericParameters.Add(new GenericParameter(System_Collections_Generic_DictionaryOfTKeyTValue));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, Module.TypeSystem.CoreLibrary);
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, Module.TypeSystem.CoreLibrary);
            }
        }

        public static ImportedReferences Create(ModuleDefinition module, FrameworkName frameworkName)
        {
            ImportedReferences references;
            switch (frameworkName.Identifier)
            {
                case ".NETFramework":
                case "Xamarin.iOS":
                case "MonoAndroid":
                case "Xamarin.Mac":
                    references = new NETFramework(module, module.TypeSystem, frameworkName);
                    break;
                case ".NETStandard":
                    if (frameworkName.Version >= new Version(2, 0))
                    {
                        references = new NetStandard2(module, module.TypeSystem, frameworkName);
                    }
                    else
                    {
                        references = new NETPortable(module, module.TypeSystem, frameworkName);
                    }

                    break;
                case ".NETPortable":
                case ".NETCore":
                case ".NETCoreApp":
                    references = new NETPortable(module, module.TypeSystem, frameworkName);
                    break;
                default:
                    throw new Exception($"Unsupported target framework: {frameworkName}");
            }

            references.InitializeFrameworkMethods();

            // Weaver may be run on an assembly which is not **yet** using Realm, if someone just adds nuget and builds.
            var realmAssembly = module.AssemblyReferences.SingleOrDefault(r => r.Name == "Realm");
            if (realmAssembly != null)
            {
                references.InitializeRealm(realmAssembly);
            }
            else if (module.Name == "Realm.dll")
            {
                references.InitializeRealm(module);
            }

            return references;
        }
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
