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

        public abstract TypeReference IQueryableOfT { get; }

        public TypeReference System_Type { get; }

        public MethodReference System_Type_GetTypeFromHandle { get; }

        public abstract TypeReference System_Collections_Generic_ListOfT { get; }

        public MethodReference System_Collections_Generic_ListOfT_Constructor { get; private set; }

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

        public TypeReference RealmObject { get; private set; }

        public TypeReference RealmObjectBase { get; private set; }

        public TypeReference EmbeddedObject { get; private set; }

        public TypeReference RealmIntegerOfT { get; private set; }

        public MethodReference RealmIntegerOfT_ConvertToT { get; private set; }

        public MethodReference RealmIntegerOfT_ConvertFromT { get; private set; }

        public MethodReference RealmObject_get_IsManaged { get; private set; }

        public MethodReference RealmObject_get_Realm { get; private set; }

        public MethodReference RealmObject_RaisePropertyChanged { get; private set; }

        public MethodReference RealmObject_GetObjectValue { get; private set; }

        public MethodReference RealmObject_SetObjectValue { get; private set; }

        public MethodReference RealmObject_GetListValue { get; private set; }

        public MethodReference RealmObject_GetBacklinks { get; private set; }

        public MethodReference RealmObject_GetPrimitiveValue { get; private set; }

        public MethodReference RealmObject_SetPrimitiveValue { get; private set; }

        public MethodReference RealmObject_SetPrimitiveValueUnique { get; private set; }

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

        protected ModuleDefinition Module { get; }

        protected Fody.TypeSystem Types { get; }

        protected ImportedReferences(ModuleDefinition module, Fody.TypeSystem types, FrameworkName frameworkName)
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

            System_NullableOfT_Ctor = new MethodReference(".ctor", Types.VoidReference, System_NullableOfT)
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
            ICollectionOfT_Add = new MethodReference("Add", Types.VoidReference, ICollectionOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(ICollectionOfT.GenericParameters.Single()) }
            };

            ICollectionOfT_Clear = new MethodReference("Clear", Types.VoidReference, ICollectionOfT) { HasThis = true };

            ICollectionOfT_get_Count = new MethodReference("get_Count", Types.Int32Reference, ICollectionOfT) { HasThis = true };

            IListOfT_get_Item = new MethodReference("get_Item", IListOfT.GenericParameters.Single(), IListOfT)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.Int32Reference) }
            };

            System_Collections_Generic_ListOfT_Constructor = new MethodReference(".ctor", Types.VoidReference, System_Collections_Generic_ListOfT) { HasThis = true };

            {
                System_Linq_Enumerable_Empty = new MethodReference("Empty", Types.VoidReference, System_Linq_Enumerable);
                var T = new GenericParameter(System_Linq_Enumerable_Empty);
                System_Linq_Enumerable_Empty.ReturnType = new GenericInstanceType(IEnumerableOfT) { GenericArguments = { T } };
                System_Linq_Enumerable_Empty.GenericParameters.Add(T);
            }

            {
                System_Linq_Queryable_AsQueryable = new MethodReference("AsQueryable", Types.VoidReference, System_Linq_Queryable);
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

            {
                RealmIntegerOfT = new TypeReference("Realms", "RealmInteger`1", Module, realmAssembly)
                {
                    IsValueType = true
                };
                var T = GetRealmIntegerGenericParameter(RealmIntegerOfT);
                RealmIntegerOfT.GenericParameters.Add(T);
                var instance = new GenericInstanceType(RealmIntegerOfT) { GenericArguments = { T } };

                RealmIntegerOfT_ConvertToT = new MethodReference("op_Implicit", T, RealmIntegerOfT)
                {
                    Parameters = { new ParameterDefinition(instance) },
                    HasThis = false
                };

                RealmIntegerOfT_ConvertFromT = new MethodReference("op_Implicit", instance, RealmIntegerOfT)
                {
                    Parameters = { new ParameterDefinition(T) },
                    HasThis = false
                };
            }

            {
                Realm_Add = new MethodReference("Add", Types.VoidReference, Realm) { HasThis = true };
                var T = new GenericParameter(Realm_Add) { Constraints = { new GenericParameterConstraint(RealmObject) } };
                Realm_Add.ReturnType = T;
                Realm_Add.GenericParameters.Add(T);
                Realm_Add.Parameters.Add(new ParameterDefinition(T));
                Realm_Add.Parameters.Add(new ParameterDefinition(Types.BooleanReference));
            }

            RealmObject_get_IsManaged = new MethodReference("get_IsManaged", Types.BooleanReference, RealmObjectBase) { HasThis = true };
            RealmObject_get_Realm = new MethodReference("get_Realm", Realm, RealmObjectBase) { HasThis = true };
            RealmObject_RaisePropertyChanged = new MethodReference("RaisePropertyChanged", Types.VoidReference, RealmObjectBase)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.StringReference) }
            };

            {
                RealmObject_GetObjectValue = new MethodReference("GetObjectValue", Types.VoidReference, RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetObjectValue) { Constraints = { new GenericParameterConstraint(RealmObjectBase) } };
                RealmObject_GetObjectValue.ReturnType = T;
                RealmObject_GetObjectValue.GenericParameters.Add(T);
                RealmObject_GetObjectValue.Parameters.Add(new ParameterDefinition(Types.StringReference));
            }

            {
                RealmObject_SetObjectValue = new MethodReference("SetObjectValue", Types.VoidReference, RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_SetObjectValue) { Constraints = { new GenericParameterConstraint(RealmObjectBase) } };
                RealmObject_SetObjectValue.GenericParameters.Add(T);
                RealmObject_SetObjectValue.Parameters.Add(new ParameterDefinition(Types.StringReference));
                RealmObject_SetObjectValue.Parameters.Add(new ParameterDefinition(T));
            }

            {
                RealmObject_GetListValue = new MethodReference("GetListValue", new GenericInstanceType(IListOfT), RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetListValue);
                (RealmObject_GetListValue.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetListValue.GenericParameters.Add(T);
                RealmObject_GetListValue.Parameters.Add(new ParameterDefinition(Types.StringReference));
            }

            {
                RealmObject_GetBacklinks = new MethodReference("GetBacklinks", new GenericInstanceType(IQueryableOfT), RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetBacklinks) { Constraints = { new GenericParameterConstraint(RealmObjectBase) } };
                (RealmObject_GetBacklinks.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetBacklinks.GenericParameters.Add(T);
                RealmObject_GetBacklinks.Parameters.Add(new ParameterDefinition(Types.StringReference));
            }

            {
                RealmObject_GetPrimitiveValue = new MethodReference("GetPrimitiveValue", Types.VoidReference, RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetPrimitiveValue);
                RealmObject_GetPrimitiveValue.ReturnType = T;
                RealmObject_GetPrimitiveValue.GenericParameters.Add(T);
                RealmObject_GetPrimitiveValue.Parameters.Add(new ParameterDefinition(Types.StringReference));
                RealmObject_GetPrimitiveValue.Parameters.Add(new ParameterDefinition(RealmSchema_PropertyType));
            }

            {
                RealmObject_SetPrimitiveValue = new MethodReference("SetPrimitiveValue", Types.VoidReference, RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_SetPrimitiveValue);
                RealmObject_SetPrimitiveValue.GenericParameters.Add(T);
                RealmObject_SetPrimitiveValue.Parameters.Add(new ParameterDefinition(Types.StringReference));
                RealmObject_SetPrimitiveValue.Parameters.Add(new ParameterDefinition(T));
                RealmObject_SetPrimitiveValue.Parameters.Add(new ParameterDefinition(RealmSchema_PropertyType));
            }

            {
                RealmObject_SetPrimitiveValueUnique = new MethodReference("SetPrimitiveValueUnique", Types.VoidReference, RealmObjectBase) { HasThis = true };
                var T = new GenericParameter(RealmObject_SetPrimitiveValueUnique);
                RealmObject_SetPrimitiveValueUnique.GenericParameters.Add(T);
                RealmObject_SetPrimitiveValueUnique.Parameters.Add(new ParameterDefinition(Types.StringReference));
                RealmObject_SetPrimitiveValueUnique.Parameters.Add(new ParameterDefinition(T));
                RealmObject_SetPrimitiveValueUnique.Parameters.Add(new ParameterDefinition(RealmSchema_PropertyType));
            }

            IRealmObjectHelper = new TypeReference("Realms.Weaving", "IRealmObjectHelper", Module, realmAssembly);

            PreserveAttribute = new TypeReference("Realms", "PreserveAttribute", Module, realmAssembly);
            PreserveAttribute_Constructor = new MethodReference(".ctor", Types.VoidReference, PreserveAttribute) { HasThis = true };
            PreserveAttribute_ConstructorWithParams = new MethodReference(".ctor", Types.VoidReference, PreserveAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.BooleanReference), new ParameterDefinition(Types.BooleanReference) }
            };

            WovenAttribute = new TypeReference("Realms", "WovenAttribute", Module, realmAssembly);
            WovenAttribute_Constructor = new MethodReference(".ctor", Types.VoidReference, WovenAttribute)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(System_Type) }
            };

            WovenAssemblyAttribute = new TypeReference("Realms", "WovenAssemblyAttribute", Module, realmAssembly);
            WovenAssemblyAttribute_Constructor = new MethodReference(".ctor", Types.VoidReference, WovenAssemblyAttribute) { HasThis = true };

            WovenPropertyAttribute = new TypeReference("Realms", "WovenPropertyAttribute", Module, realmAssembly);
            WovenPropertyAttribute_Constructor = new MethodReference(".ctor", Types.VoidReference, WovenPropertyAttribute) { HasThis = true };

            ExplicitAttribute = new TypeReference("Realms", "ExplicitAttribute", Module, realmAssembly);

            var realmSchema = new TypeReference("Realms.Schema", "RealmSchema", Module, realmAssembly);
            RealmSchema_AddDefaultTypes = new MethodReference("AddDefaultTypes", Types.VoidReference, realmSchema) { HasThis = false };
            {
                var ienumerableOfType = new GenericInstanceType(IEnumerableOfT)
                {
                    GenericArguments = { System_Type }
                };

                RealmSchema_AddDefaultTypes.Parameters.Add(new ParameterDefinition(ienumerableOfType));
            }
        }

        private void InitializePropertyChanged_Fody(AssemblyNameReference propertyChangedAssembly)
        {
            PropertyChanged_DoNotNotifyAttribute = new TypeReference("PropertyChanged", "DoNotNotifyAttribute", Module, propertyChangedAssembly);
            PropertyChanged_DoNotNotifyAttribute_Constructor = new MethodReference(".ctor", Types.VoidReference, PropertyChanged_DoNotNotifyAttribute) { HasThis = true };
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

        public GenericParameter GetRealmIntegerGenericParameter(IGenericParameterProvider owner)
        {
            var T = new GenericParameter(owner)
            {
                Constraints = { new GenericParameterConstraint(System_ValueType), new GenericParameterConstraint(System_IFormattable) }
            };

            T.Constraints.Add(new GenericParameterConstraint(new GenericInstanceType(System_IComparableOfT)
            {
                GenericParameters = { T }
            }));

            return T;
        }

        internal sealed class NETFramework : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public NETFramework(ModuleDefinition module, Fody.TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                var System_Core = GetOrAddFrameworkReference("System.Core");

                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, System_Core);
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, System_Core);

                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, System_Core);
            }
        }

        private sealed class NETPortable : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public NETPortable(ModuleDefinition module, Fody.TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, GetOrAddFrameworkReference("System.Linq.Expressions"));
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, GetOrAddFrameworkReference("System.Collections"));
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, GetOrAddFrameworkReference("System.Linq"));
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, GetOrAddFrameworkReference("System.Linq.Queryable"));
            }
        }

        private sealed class NetStandard2 : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public NetStandard2(ModuleDefinition module, Fody.TypeSystem types, FrameworkName frameworkName) : base(module, types, frameworkName)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, Module.TypeSystem.CoreLibrary);
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, Module.TypeSystem.CoreLibrary);
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, Module.TypeSystem.CoreLibrary);
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, Module.TypeSystem.CoreLibrary);
            }
        }

        public static ImportedReferences Create(ModuleWeaver weaver, FrameworkName frameworkName)
        {
            var module = weaver.ModuleDefinition;

            ImportedReferences references;
            switch (frameworkName.Identifier)
            {
                case ".NETFramework":
                case "Xamarin.iOS":
                case "MonoAndroid":
                case "Xamarin.Mac":
                    references = new NETFramework(module, weaver.TypeSystem, frameworkName);
                    break;
                case ".NETStandard":
                    if (frameworkName.Version >= new Version(2, 0))
                    {
                        references = new NetStandard2(module, weaver.TypeSystem, frameworkName);
                    }
                    else
                    {
                        references = new NETPortable(module, weaver.TypeSystem, frameworkName);
                    }

                    break;
                case ".NETPortable":
                case ".NETCore":
                case ".NETCoreApp":
                    references = new NETPortable(module, weaver.TypeSystem, frameworkName);
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
}
