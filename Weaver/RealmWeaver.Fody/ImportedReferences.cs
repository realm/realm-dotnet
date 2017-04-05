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

#pragma warning disable SA1306

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;

namespace RealmWeaver
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter")]
    internal abstract class ImportedReferences
    {
        protected ModuleDefinition Module { get; }

        protected TypeSystem Types => Module.TypeSystem;

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

        public TypeReference System_DateTimeOffset { get; }

        public abstract TypeReference System_Reflection_IReflectableType { get; }

        public abstract TypeReference System_Reflection_TypeInfo { get; }

        public MethodReference TypeInfoHelper_GetInfo { get; private set; }

        public MethodReference System_DateTimeOffset_op_Inequality { get; private set; }

        public abstract TypeReference System_Collections_Generic_ListOfT { get; }

        public MethodReference System_Collections_Generic_ListOfT_Constructor { get; private set; }

        public abstract TypeReference System_Linq_Enumerable { get; }

        public MethodReference System_Linq_Enumerable_Empty { get; private set; }

        public abstract TypeReference System_Linq_Queryable { get; }

        public MethodReference System_Linq_Queryable_AsQueryable { get; private set; }

        public TypeReference Realm { get; private set; }

        public MethodReference Realm_Add { get; private set; }

        public TypeReference RealmObject { get; private set; }

        public MethodReference RealmObject_get_IsManaged { get; private set; }

        public MethodReference RealmObject_get_Realm { get; private set; }

        public MethodReference RealmObject_RaisePropertyChanged { get; private set; }

        public MethodReference RealmObject_GetObjectValue { get; private set; }

        public MethodReference RealmObject_SetObjectValue { get; private set; }

        public MethodReference RealmObject_GetListValue { get; private set; }

        public MethodReference RealmObject_GetBacklinks { get; private set; }

        public TypeReference IRealmObjectHelper { get; private set; }

        public TypeReference PreserveAttribute { get; private set; }

        public MethodReference PreserveAttribute_Constructor { get; private set; }

        public MethodReference PreserveAttribute_ConstructorWithParams { get; private set; }

        public TypeReference WovenAttribute { get; private set; }

        public MethodReference WovenAttribute_Constructor { get; private set; }

        public TypeReference WovenPropertyAttribute { get; private set; }

        public MethodReference WovenPropertyAttribute_Constructor { get; private set; }

        public TypeReference PropertyChanged_DoNotNotifyAttribute { get; private set; }

        public MethodReference PropertyChanged_DoNotNotifyAttribute_Constructor { get; private set; }

        public MethodReference RealmSchema_AddDefaultTypes { get; private set; }

        protected ImportedReferences(ModuleDefinition module)
        {
            Module = module;

            IEnumerableOfT = new TypeReference("System.Collections.Generic", "IEnumerable`1", Module, Types.CoreLibrary);
            IEnumerableOfT.GenericParameters.Add(new GenericParameter(IEnumerableOfT));

            ICollectionOfT = new TypeReference("System.Collections.Generic", "ICollection`1", Module, Types.CoreLibrary);
            ICollectionOfT.GenericParameters.Add(new GenericParameter(ICollectionOfT));

            IListOfT = new TypeReference("System.Collections.Generic", "IList`1", Module, Types.CoreLibrary);
            IListOfT.GenericParameters.Add(new GenericParameter(IListOfT));

            var runtimeTypeHandle = new TypeReference("System", "RuntimeTypeHandle", Module, Types.CoreLibrary)
            {
                IsValueType = true
            };

            System_Type = new TypeReference("System", "Type", Module, Types.CoreLibrary);
            System_Type_GetTypeFromHandle = new MethodReference("GetTypeFromHandle", System_Type, System_Type)
            { 
                HasThis = false,
                Parameters = { new ParameterDefinition(runtimeTypeHandle) }
            };

            System_DateTimeOffset = new TypeReference("System", "DateTimeOffset", Module, Types.CoreLibrary, valueType: true);

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

            System_DateTimeOffset_op_Inequality = new MethodReference("op_Inequality", Types.Boolean, System_DateTimeOffset)
            {
                Parameters = { new ParameterDefinition(System_DateTimeOffset), new ParameterDefinition(System_DateTimeOffset) }
            };

            System_Collections_Generic_ListOfT_Constructor = new MethodReference(".ctor", Types.Void, System_Collections_Generic_ListOfT) { HasThis = true };

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

        private void InitializeRealm(AssemblyNameReference realmAssembly)
        {
            Realm = new TypeReference("Realms", "Realm", Module, realmAssembly);
            RealmObject = new TypeReference("Realms", "RealmObject", Module, realmAssembly);

            {
                Realm_Add = new MethodReference("Add", Types.Void, Realm) { HasThis = true };
                var T = new GenericParameter(Realm_Add) { Constraints = { RealmObject } };
                Realm_Add.ReturnType = T;
                Realm_Add.GenericParameters.Add(T);
                Realm_Add.Parameters.Add(new ParameterDefinition(T));
                Realm_Add.Parameters.Add(new ParameterDefinition(Types.Boolean));
            }

            RealmObject_get_IsManaged = new MethodReference("get_IsManaged", Types.Boolean, RealmObject) { HasThis = true };
            RealmObject_get_Realm = new MethodReference("get_Realm", Realm, RealmObject) { HasThis = true };
            RealmObject_RaisePropertyChanged = new MethodReference("RaisePropertyChanged", Types.Void, RealmObject)
            {
                HasThis = true,
                Parameters = { new ParameterDefinition(Types.String) }
            };

            {
                RealmObject_GetObjectValue = new MethodReference("GetObjectValue", Types.Void, RealmObject) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetObjectValue) { Constraints = { RealmObject } };
                RealmObject_GetObjectValue.ReturnType = T;
                RealmObject_GetObjectValue.GenericParameters.Add(T);
                RealmObject_GetObjectValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_SetObjectValue = new MethodReference("SetObjectValue", Types.Void, RealmObject) { HasThis = true };
                var T = new GenericParameter(RealmObject_SetObjectValue) { Constraints = { RealmObject } };
                RealmObject_SetObjectValue.GenericParameters.Add(T);
                RealmObject_SetObjectValue.Parameters.Add(new ParameterDefinition(Types.String));
                RealmObject_SetObjectValue.Parameters.Add(new ParameterDefinition(T));
            }

            {
                RealmObject_GetListValue = new MethodReference("GetListValue", new GenericInstanceType(IListOfT), RealmObject) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetListValue) { Constraints = { RealmObject } };
                (RealmObject_GetListValue.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetListValue.GenericParameters.Add(T);
                RealmObject_GetListValue.Parameters.Add(new ParameterDefinition(Types.String));
            }

            {
                RealmObject_GetBacklinks = new MethodReference("GetBacklinks", new GenericInstanceType(IQueryableOfT), RealmObject) { HasThis = true };
                var T = new GenericParameter(RealmObject_GetBacklinks) { Constraints = { RealmObject } };
                (RealmObject_GetBacklinks.ReturnType as GenericInstanceType).GenericArguments.Add(T);
                RealmObject_GetBacklinks.GenericParameters.Add(T);
                RealmObject_GetBacklinks.Parameters.Add(new ParameterDefinition(Types.String));
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

            WovenPropertyAttribute = new TypeReference("Realms", "WovenPropertyAttribute", Module, realmAssembly);
            WovenPropertyAttribute_Constructor = new MethodReference(".ctor", Types.Void, WovenPropertyAttribute) { HasThis = true };

            var realmSchema = new TypeReference("Realms.Schema", "RealmSchema", Module, realmAssembly);
            RealmSchema_AddDefaultTypes = new MethodReference("AddDefaultTypes", Types.Void, realmSchema) { HasThis = false };
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
            PropertyChanged_DoNotNotifyAttribute_Constructor = new MethodReference(".ctor", Types.Void, PropertyChanged_DoNotNotifyAttribute) { HasThis = true };
        }

        private void InitializeDataBinding(AssemblyNameReference dataBindingAssembly)
        {
            {
                var typeInfoHelper = new TypeReference("Realms", "TypeInfoHelper", Module, dataBindingAssembly);
                TypeInfoHelper_GetInfo = new MethodReference("GetInfo", System_Reflection_TypeInfo, typeInfoHelper)
                {
                    HasThis = false
                };

                var T = new GenericParameter(TypeInfoHelper_GetInfo) { Constraints = { RealmObject } };
                TypeInfoHelper_GetInfo.GenericParameters.Add(T);
            }
        }

        protected AssemblyNameReference GetOrAddFrameworkReference(string assemblyName)
        {
            var assembly = Module.AssemblyReferences.SingleOrDefault(a => a.Name == assemblyName);
            if (assembly == null)
            {
                var corlib = (AssemblyNameReference)Types.CoreLibrary;
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

        private sealed class NETFramework : ImportedReferences
        {
            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public override TypeReference System_Reflection_IReflectableType { get; }

            public override TypeReference System_Reflection_TypeInfo { get; }

            public NETFramework(ModuleDefinition module) : base(module)
            {
                var System_Core = GetOrAddFrameworkReference("System.Core");

                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, System_Core);
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, Types.CoreLibrary);
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, System_Core);

                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, System_Core);

                System_Reflection_IReflectableType = new TypeReference("System.Reflection", "IReflectableType", Module, Types.CoreLibrary);
                System_Reflection_TypeInfo = new TypeReference("System.Reflection", "TypeInfo", Module, Types.CoreLibrary);
            }
        }

        private sealed class NETPortable : ImportedReferences
        {
            public override TypeReference System_Reflection_IReflectableType { get; }

            public override TypeReference System_Reflection_TypeInfo { get; }

            public override TypeReference IQueryableOfT { get; }

            public override TypeReference System_Collections_Generic_ListOfT { get; }

            public override TypeReference System_Linq_Enumerable { get; }

            public override TypeReference System_Linq_Queryable { get; }

            public NETPortable(ModuleDefinition module) : base(module)
            {
                IQueryableOfT = new TypeReference("System.Linq", "IQueryable`1", Module, GetOrAddFrameworkReference("System.Linq.Expressions"));
                IQueryableOfT.GenericParameters.Add(new GenericParameter(IQueryableOfT));

                System_Collections_Generic_ListOfT = new TypeReference("System.Collections.Generic", "List`1", Module, GetOrAddFrameworkReference("System.Collections"));
                System_Collections_Generic_ListOfT.GenericParameters.Add(new GenericParameter(System_Collections_Generic_ListOfT));

                System_Linq_Enumerable = new TypeReference("System.Linq", "Enumerable", Module, GetOrAddFrameworkReference("System.Linq"));
                System_Linq_Queryable = new TypeReference("System.Linq", "Queryable", Module, GetOrAddFrameworkReference("System.Linq.Queryable"));

                System_Reflection_IReflectableType = new TypeReference("System.Reflection", "IReflectableType", Module, GetOrAddFrameworkReference("System.Reflection"));
                System_Reflection_TypeInfo = new TypeReference("System.Reflection", "TypeInfo", Module, GetOrAddFrameworkReference("System.Reflection"));
            }
        }

        public static ImportedReferences Create(ModuleDefinition module)
        {
            var targetFramework = module.Assembly.CustomAttributes.Single(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);

            ImportedReferences references;

            var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);

            switch (frameworkName.Identifier)
            {
                case ".NETFramework":
                case "Xamarin.iOS":
                case "MonoAndroid":
                    references = new NETFramework(module);
                    break;
                case ".NETStandard":
                case ".NETPortable":
                case ".NETCore":
                    references = new NETPortable(module);
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

                switch (frameworkName.Identifier)
                {
                    case "Xamarin.iOS":
                    case "MonoAndroid":
                    case ".NETStandard":
                    case ".NETPortable":
                        var dataBindingAssembly = module.AssemblyReferences.SingleOrDefault(r => r.Name == "Realm.DataBinding");
                        if (dataBindingAssembly == null)
                        {
                            dataBindingAssembly = new AssemblyNameReference("Realm.DataBinding", new Version(1, 0, 0, 0));
                            module.AssemblyReferences.Add(dataBindingAssembly);
                        }
                        references.InitializeDataBinding(dataBindingAssembly);
                        break;
                }
            }

            return references;
        }
    }
}
