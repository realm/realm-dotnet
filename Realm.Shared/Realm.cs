/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Realms
{
    /// <summary>
    /// A Realm instance (also referred to as a realm) represents a Realm database.
    /// </summary>
    /// <remarks>Warning: Realm instances are not thread safe and can not be shared across threads. 
    /// You must call GenerateInstance on each thread you want to interact with the realm on. 
    /// </remarks>
    public class Realm : IDisposable
    {
        #region static

        private static readonly IEnumerable<Type> RealmObjectClasses;

        /// <summary>
        /// Standard filename to be combined with a platform-specific document directory in the per-platform InteropConfig.GetStandardDatabasePath
        /// </summary>
        /// <returns>A string representing a filename only, no path.</returns>
        static string _DefaultDatabaseName = "default.realm";

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

            NativeCommon.SetupExceptionThrower();
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="databasePath">Optional path to the realm, must be a valid full path for the current platform</param>
        /// <returns>A realm instance</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static Realm GetInstance(string databasePath = null)
        {
            if (databasePath == null)
                databasePath = System.IO.Path.Combine(InteropConfig.GetDefaultDatabasePath(), _DefaultDatabaseName);
            else if (!System.IO.Path.IsPathRooted(databasePath))
                databasePath = System.IO.Path.Combine(InteropConfig.GetDefaultDatabasePath(), databasePath);

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
                var readOnly = MarshalHelpers.BoolToIntPtr(false);
                var durability = MarshalHelpers.BoolToIntPtr(false);
                var srPtr = NativeSharedRealm.open(schemaHandle, databasePath, (IntPtr)databasePath.Length, readOnly, durability, "", (IntPtr)0);
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
                    return p.GetCustomAttributes(false).OfType<WovenPropertyAttribute>().Any();
                });

            foreach (var p in propertiesToMap)
            {
                var mapToAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is MapToAttribute) as MapToAttribute;
                var propertyName = mapToAttribute != null ? mapToAttribute.Mapping : p.Name;

                var objectIdAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is ObjectIdAttribute);
                var isObjectId = objectIdAttribute != null;

                var indexedAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is IndexedAttribute);
                var isIndexed = indexedAttribute != null;

                var isNullable = !(p.PropertyType.IsValueType || 
                    p.PropertyType.Name == "RealmList`1" ||
                    p.PropertyType.Name == "IList`1") ||
                    Nullable.GetUnderlyingType(p.PropertyType) != null;

                var objectType = "";
                if (!p.PropertyType.IsValueType && p.PropertyType.Name!="String") {
                    if (p.PropertyType.Name == "RealmList`1" || p.PropertyType.Name == "IList`1")
                        objectType = p.PropertyType.GetGenericArguments()[0].Name;
                    else {
                        if (p.PropertyType.BaseType.Name == "RealmObject")
                            objectType = p.PropertyType.Name;
                    }
                }
                var columnType = p.PropertyType;
                NativeObjectSchema.add_property(objectSchemaPtr, propertyName, MarshalHelpers.RealmColType(columnType), objectType, 
                    MarshalHelpers.BoolToIntPtr(isObjectId), MarshalHelpers.BoolToIntPtr(isIndexed), MarshalHelpers.BoolToIntPtr(isNullable));
            }

            return objectSchemaPtr;
        }

        #endregion

        private SharedRealmHandle _sharedRealmHandle;
        internal Dictionary<Type, TableHandle> _tableHandles;

        internal bool IsInTransaction => MarshalHelpers.IntPtrToBool(NativeSharedRealm.is_in_transaction(_sharedRealmHandle));

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

        /// <summary>
        /// Factory for a managed object in a realm. Only valid within a Write transaction.
        /// </summary>
        /// <remarks>Using CreateObject is more efficient than creating standalone objects, assigning their values, then using Attach because it avoids copying properties to the realm.</remarks>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>An object which is already managed</returns>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        public T CreateObject<T>() where T : RealmObject
        {
            return (T)CreateObject(typeof(T));
        }

        private object CreateObject(Type objectType)
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot create Realm object outside write transactions");

            var result = (RealmObject)Activator.CreateInstance(objectType);

            var tableHandle = _tableHandles[objectType];
            
            var rowPtr = NativeTable.add_empty_row(tableHandle);
            var rowHandle = CreateRowHandle	(rowPtr);

            result._Manage(this, rowHandle);

            return result;
        }

        /// <summary>
        /// Attaches a RealmObject which has been created as a standalone object, to this realm.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectAlreadyOwnedByRealmException">You can't attach the same object twice. This exception is thrown, rather than silently detecting the mistake, to help you debug your code</exception>
        /// <exception cref="RealmObjectOwnedByAnotherRealmException">You can't attach an object to more than one realm</exception>
        public void Attach<T>(T obj) where T : RealmObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.IsManaged)
            {
                if (obj.Realm._sharedRealmHandle == this._sharedRealmHandle)
                    throw new RealmObjectAlreadyOwnedByRealmException("The object is already owned by this realm");

                throw new RealmObjectOwnedByAnotherRealmException("Cannot attach an object to a realm when it's already owned by another realm");
            }


            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot attach a Realm object outside write transactions");

            var tableHandle = _tableHandles[typeof(T)];

            var rowPtr = NativeTable.add_empty_row(tableHandle);
            var rowHandle = CreateRowHandle(rowPtr);

            obj._Manage(this, rowHandle);
            obj._CopyDataFromBackingFieldsToRow();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static RowHandle CreateRowHandle(IntPtr rowPtr)
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

        /// <summary>
        /// Factory for a write Transaction. Essential object to create scope for updates.
        /// </summary>
        /// <example><c>
        /// using (var trans = myrealm.BeginWrite()) { 
        ///     var rex = myrealm.CreateObject<Dog>();
        ///     rex.Name = "Rex";
        ///     trans.Commit();
        /// }</c>
        /// </example>
        /// <returns>A transaction in write mode, which is required for any creation or modification of objects persisted in a Realm</returns>
        public Transaction BeginWrite()
        {
            return new Transaction(_sharedRealmHandle);
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>A RealmQuery that without further filtering, allows iterating all objects of class T, in this realm</returns>
        public RealmQuery<T> All<T>() where T: RealmObject
        {
            return new RealmQuery<T>(this);
        }

        /// <summary>
        /// Removes a persistent object from this realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this realm</param>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="System.ArgumentNullException">If you invoke this with a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove Realm object outside write transactions");

            var tableHandle = _tableHandles[obj.GetType()];
            NativeTable.remove_row(tableHandle, (RowHandle)obj.RowHandle);
        }

        /// <summary>
        /// Standard Dispose which has no action or side-effects for a Realm.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
