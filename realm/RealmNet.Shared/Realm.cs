using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace RealmNet
{
    public class Realm : IDisposable
    {
        private static readonly IEnumerable<Type> RealmObjectClasses;

        static Realm()
        {
            RealmObjectClasses =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                    .Where(t => t != typeof (RealmObject) && typeof (RealmObject).IsAssignableFrom(t))
                select t;

            foreach(var realmType in RealmObjectClasses)
            {
                if (!realmType.GetCustomAttributes(typeof(WovenAttribute), true).Any())
                    Debug.WriteLine("WARNING! The type " + realmType.Name + " is a RealmObject but it has not been woven.");
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static Realm GetInstance(string databasePath)
        {
            var schemaInitializer = new SchemaInitializerHandle();

            foreach (var realmObjectClass in RealmObjectClasses)
            {
                var objectSchemaHandle = GenerateObjectSchema(realmObjectClass);
                NativeSchema.initializer_add_object_schema(schemaInitializer, objectSchemaHandle);
            }

            var schemaHandle = new SchemaHandle(schemaInitializer);

            var srHandle = new SharedRealmHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                var srPtr = NativeSharedRealm.open(schemaHandle, databasePath, (IntPtr)0, (IntPtr)0, "");
                srHandle.SetHandle(srPtr);
            }

            return new Realm(srHandle);
        }

        private static IntPtr GenerateObjectSchema(Type objectClass)
        {
            var objectSchemaPtr = NativeObjectSchema.create(objectClass.Name);

            var propertiesToMap = objectClass.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p =>
                {
                    return p.GetCustomAttributes(false).All(a => a.GetType() != typeof (IgnoreAttribute));
                });

            foreach (var p in propertiesToMap)
            {
                var mapToAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is MapToAttribute) as MapToAttribute;
                var propertyName = mapToAttribute != null ? mapToAttribute.Mapping : p.Name;

                var primaryKeyAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is PrimaryKeyAttribute);
                var isPrimaryKey = primaryKeyAttribute != null;

                var indexedAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is IndexedAttribute);
                var isIndexed = indexedAttribute != null;

                var isNullable = !p.PropertyType.IsValueType || Nullable.GetUnderlyingType(p.PropertyType) != null;

                var columnType = p.PropertyType;
                NativeObjectSchema.add_property(objectSchemaPtr, propertyName, MarshalHelpers.RealmColType(columnType), "", 
                    MarshalHelpers.BoolToIntPtr(isPrimaryKey), MarshalHelpers.BoolToIntPtr(isIndexed), MarshalHelpers.BoolToIntPtr(isNullable));
            }

            return objectSchemaPtr;
        }

        private SharedRealmHandle _sharedRealmHandle;
        internal Dictionary<Type, TableHandle> _tableHandles;
        
        private Realm(SharedRealmHandle sharedRealmHandle)
        {
            _sharedRealmHandle = sharedRealmHandle;
            _tableHandles = RealmObjectClasses.ToDictionary(t => t, GetTable);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private TableHandle GetTable(Type realmType)
        {
            var result = new TableHandle();
            var tableName = "class_" + realmType.Name;

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                var tablePtr = NativeSharedRealm.get_table(_sharedRealmHandle, tableName, (IntPtr)tableName.Length);
                result.SetHandle(tablePtr);
            }
            return result;
        }

        public T CreateObject<T>() where T : RealmObject
        {
            return (T)CreateObject(typeof(T));
        }

        public object CreateObject(Type objectType)
        {
            var result = (RealmObject)Activator.CreateInstance(objectType);

            var tableHandle = _tableHandles[objectType];
            
            var rowPtr = NativeTable.add_empty_row(tableHandle);
            var rowHandle = CreateRowHandle	(rowPtr);

            result._Manage(this, rowHandle);

            return result;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static RowHandle CreateRowHandle(IntPtr rowPtr)
        {
            var rowHandle = new RowHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                rowHandle.SetHandle(rowPtr);
            }

            return rowHandle;
        }

        public Transaction BeginWrite()
        {
            return new Transaction(_sharedRealmHandle);
        }

        public RealmQuery<T> All<T>() where T: RealmObject
        {
            return new RealmQuery<T>(this);
        }

        public void Remove(RealmObject p2)
        {
            var tableHandle = _tableHandles[p2.GetType()];
            NativeTable.remove_row(tableHandle, (RowHandle)p2.RowHandle);
        }

        public void Dispose()
        {
        }
    }
}
