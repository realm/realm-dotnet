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
using System.IO;

namespace Realms
{
    /// <summary>
    /// A Realm instance (also referred to as a realm) represents a Realm database.
    /// </summary>
    /// <remarks>Warning: Realm instances are not thread safe and can not be shared across threads 
    /// You must call GetInstance on each thread in which you want to interact with the realm. 
    /// </remarks>
    public class Realm : IDisposable
    {
        #region static

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

            NativeCommon.SetupExceptionThrower();
        }

        /// <summary>
        /// Configuration that controls the Realm path and other settings.
        /// </summary>
        public RealmConfiguration Config { get; private set; }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="databasePath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <remarks>If you specify a relative path, sandboxing by the OS may cause failure if you specify anything other than a subdirectory. <br />
        /// Instances are cached for a given absolute path and thread, so you may get back the same instance.
        /// </remarks>
        /// <returns>A realm instance, possibly from cache.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the filesystem has an error preventing file creation.</exception>
        public static Realm GetInstance(string databasePath)
        {
            var config = RealmConfiguration.DefaultConfiguration;
            if (!string.IsNullOrEmpty(databasePath))
                config = config.ConfigWithPath(databasePath);
            return GetInstance(config);
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="config">Optional configuration.</param>
        /// <returns>A realm instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the filesystem has an error preventing file creation.</exception>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static Realm GetInstance(RealmConfiguration config=null)
        {
            config = config ??  RealmConfiguration.DefaultConfiguration;
            var schemaInitializer = new SchemaInitializerHandle();

            foreach (var realmObjectClass in RealmObjectClasses)
            {
                var objectSchemaHandle = GenerateObjectSchema(realmObjectClass);
                NativeSchema.initializer_add_object_schema(schemaInitializer, objectSchemaHandle);
            }

            var schemaHandle = new SchemaHandle(schemaInitializer);

            var srHandle = new SharedRealmHandle();

            var readOnly = MarshalHelpers.BoolToIntPtr(false);
            var durability = MarshalHelpers.BoolToIntPtr(false);
            var databasePath = config.DatabasePath;
            IntPtr srPtr = IntPtr.Zero;
            try {
                srPtr = NativeSharedRealm.open(schemaHandle, 
                    databasePath, (IntPtr)databasePath.Length, 
                    readOnly, durability, 
                    config.EncryptionKey,
                    config.SchemaVersion);
            } catch (RealmMigrationNeededException) {
                if (config.ShouldDeleteIfMigrationNeeded)
                {
                    DeleteRealm(config);
                }
                else
                {
                    throw; // rethrow te exception
                    //TODO when have Migration but also consider programmer control over auto migration
                    //MigrateRealm(configuration);
                }
                // create after deleting old reopen after migrating 
                srPtr = NativeSharedRealm.open(schemaHandle, 
                    databasePath, (IntPtr)databasePath.Length, 
                    readOnly, durability, 
                    config.EncryptionKey,
                    config.SchemaVersion);
            }

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                srHandle.SetHandle(srPtr);
            }

            return new Realm(srHandle, config);  // try creating again
        }  // GetInstance


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
                    p.PropertyType.Name == "RealmList`1") ||
                    // IGNORING IList FOR NOW  p.PropertyType.Name == "IList`1") ||
                    Nullable.GetUnderlyingType(p.PropertyType) != null;

                var objectType = "";
                if (!p.PropertyType.IsValueType && p.PropertyType.Name!="String") {
                    if (p.PropertyType.Name == "RealmList`1")  // IGNORING IList FOR NOW   || p.PropertyType.Name == "IList`1")
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

        private Realm(SharedRealmHandle sharedRealmHandle, RealmConfiguration config)
        {
            _sharedRealmHandle = sharedRealmHandle;
            _tableHandles = RealmObjectClasses.ToDictionary(t => t, GetTable);
            Config = config;
            // update OUR config version number in case loaded one from disk
            Config.SchemaVersion = NativeSharedRealm.get_schema_version(sharedRealmHandle);
        }

        /// <summary>
        /// Handler type used by <see cref="RealmChanging"/> 
        /// </summary>
        public delegate void RealmChangingEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Triggered when a realm is about to change (i.e. a transaction is being committed)
        /// </summary>
        public event RealmChangingEventHandler RealmChanging;


        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/> 
        /// </summary>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Triggered when a realm has changed (i.e. a transaction was committed)
        /// </summary>
        public event RealmChangedEventHandler RealmChanged;


        /// <summary>
        /// Checks if database has been closed.
        /// </summary>
        /// <returns>True if closed.</returns>
        public bool IsClosed => _sharedRealmHandle.IsClosed;


        /// <summary>
        ///  Closes the Realm if not already closed. Safe to call repeatedly.
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Close()
        {
            if (IsClosed)
                return;
            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Close handle in a constrained execution region */ }
            finally {
                _sharedRealmHandle.Close();
            }
        }


        /// <summary>
        ///  Dispose automatically closes the Realm if not already closed.
        /// </summary>
        public void Dispose()
        {
            Close();
        }


        /// <summary>
        /// Generic override determines whether the specified <see cref="System.Object"/> is equal to the current Realm.
        /// </summary>
        /// <param name="rhs">The <see cref="System.Object"/> to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public override bool Equals(Object rhs)
        {
            return Equals(rhs as Realm);
        }


        /// <summary>
        /// Determines whether the specified Realm is equal to the current Realm.
        /// </summary>
        /// <param name="rhs">The Realm to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public  bool Equals(Realm rhs)
        {
            if (rhs == null)
                return false;
            if (ReferenceEquals(this, rhs))
                return true;
            return Config.Equals(rhs.Config) && IsClosed == rhs.IsClosed;
        }


        /// <summary>
        /// Determines whether this instance is the same core instance as the specified rhs.
        /// </summary>
        /// <remarks>
        /// You can, and should, have multiple instances open on different threads which have the same path and open the same Realm.
        /// </remarks>
        /// <returns><c>true</c> if this instance is the same core instance the specified rhs; otherwise, <c>false</c>.</returns>
        /// <param name="rhs">The Realm to compare with the current Realm.</param>
        public bool IsSameInstance(Realm rhs)
        {
            return MarshalHelpers.IntPtrToBool(NativeSharedRealm.is_same_instance(_sharedRealmHandle, rhs._sharedRealmHandle));
        }


        /// <summary>
        /// Serves as a hash function for a Realm based on the core instance.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return (int)_sharedRealmHandle.DangerousGetHandle();
        }


        /// <summary>
        ///  Deletes all the files associated with a realm. Hides knowledge of the auxiliary filenames from the programmer.
        /// </summary>
        /// <param name="configuration">A configuration which supplies the realm path.</param>
        static public void DeleteRealm(RealmConfiguration configuration)
        {
            //TODO add cache checking when implemented, https://github.com/realm/realm-dotnet/issues/308
            //when cache checking, uncomment in IntegrationTests.cs RealmInstanceTests.DeleteRealmFailsIfOpenSameThread and add a variant to test open on different thread
            var lockOnWhileDeleting = new object();
            lock (lockOnWhileDeleting)
            {
                var fullpath = configuration.DatabasePath;
                File.Delete(fullpath);
                File.Delete(fullpath + ".log_a");  // eg: name at end of path is EnterTheMagic.realm.log_a   
                File.Delete(fullpath + ".log_b");
                File.Delete(fullpath + ".log");
                File.Delete(fullpath + ".lock");
                File.Delete(fullpath + ".note");
            }
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
        /// <remarks>Using CreateObject is more efficient than creating standalone objects, assigning their values, then using Manage because it avoids copying properties to the realm.</remarks>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>An object which is already managed.</returns>
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
        /// This realm will start managing a RealmObject which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectAlreadyManagedByRealmException">You can't manage the same object twice. This exception is thrown, rather than silently detecting the mistake, to help you debug your code</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm</exception>
        public void Manage<T>(T obj) where T : RealmObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.IsManaged)
            {
                if (obj.Realm._sharedRealmHandle == this._sharedRealmHandle)
                    throw new RealmObjectAlreadyManagedByRealmException("The object is already managed by this realm");

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }


            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot start managing a Realm object outside write transactions");

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
        /// <returns>A transaction in write mode, which is required for any creation or modification of objects persisted in a Realm.</returns>
        public Transaction BeginWrite()
        {
            return new Transaction(_sharedRealmHandle);
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of class T, in this realm.</returns>
        public RealmResults<T> All<T>() where T: RealmObject
        {
            return new RealmResults<T>(this, true);
        }

        /// <summary>
        /// Removes a persistent object from this realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this realm.</param>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="System.ArgumentNullException">If you invoke this with a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove Realm object outside write transactions");

            var tableHandle = _tableHandles[obj.GetType()];
            NativeTable.remove_row(tableHandle, (RowHandle)obj.RowHandle);
        }
    }
}
