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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.IO;
using System.Runtime.InteropServices;

#if __IOS__
using ObjCRuntime;
#endif

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

        private static readonly Type[] RealmObjectClasses;
        private static readonly Dictionary<Type, IntPtr> ObjectSchemaCache;

        // shared string buffer for getter because can only be getting on this one thread per Realm
        internal IntPtr stringGetBuffer = IntPtr.Zero;
        internal int stringGetBufferLen;



        static Realm()
        {
            RealmObjectClasses = AppDomain.CurrentDomain.GetAssemblies()
                                          .SelectMany(a => a.GetTypes())
                                          .Where(t => t.IsSubclassOf(typeof(RealmObject)))
                                          .ToArray();

            foreach(var realmType in RealmObjectClasses)
            {
                if (!realmType.GetCustomAttributes(typeof(WovenAttribute), true).Any())
                    Debug.WriteLine("WARNING! The type " + realmType.Name + " is a RealmObject but it has not been woven.");
            }
            ObjectSchemaCache = new Dictionary<Type, IntPtr>();
            NativeCommon.Initialize();
            NativeCommon.register_notify_realm_changed(NotifyRealmChanged);
        }

        #if __IOS__
        [MonoPInvokeCallback (typeof (NativeCommon.NotifyRealmCallback))]
        #endif
        private static void NotifyRealmChanged(IntPtr realmHandle)
        {
            var gch = GCHandle.FromIntPtr(realmHandle);
            ((Realm)gch.Target).NotifyChanged(EventArgs.Empty);
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

            // TODO cache these initializers but note complications with ObjectClasses
            var schemaInitializer = new SchemaInitializerHandle();

            if (config.ObjectClasses == null)
            {
                foreach (var realmObjectClass in RealmObjectClasses)
                {
                    var objectSchemaHandle = GenerateObjectSchema(realmObjectClass);
                    NativeSchema.initializer_add_object_schema(schemaInitializer, objectSchemaHandle);
                }
            }
            else
            {
                foreach (var selectedRealmObjectClass in config.ObjectClasses) {
                    if (selectedRealmObjectClass.BaseType != typeof(RealmObject))
                        throw new ArgumentException($"The class {selectedRealmObjectClass.Name} must descend from RealmObject");
                    
                    Debug.Assert(RealmObjectClasses.Contains(selectedRealmObjectClass));
                    var objectSchemaHandle = GenerateObjectSchema(selectedRealmObjectClass);
                    NativeSchema.initializer_add_object_schema(schemaInitializer, objectSchemaHandle);
                }
            }

            var schemaHandle = new SchemaHandle(schemaInitializer);

            var srHandle = new SharedRealmHandle();

            var readOnly = MarshalHelpers.BoolToIntPtr(config.ReadOnly);
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

            return new Realm(srHandle, config);
        } 


        private static IntPtr GenerateObjectSchema(Type objectClass)
        {           
            IntPtr objectSchemaPtr = IntPtr.Zero;
            if (ObjectSchemaCache.TryGetValue(objectClass, out objectSchemaPtr)) {
               return objectSchemaPtr;  // use cached schema                
            }

            objectSchemaPtr = NativeObjectSchema.create(objectClass.Name);
            ObjectSchemaCache[objectClass] = objectSchemaPtr;  // save for later lookup
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

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly Dictionary<Type, RealmObject.Metadata> Metadata;

        internal bool IsInTransaction => MarshalHelpers.IntPtrToBool(NativeSharedRealm.is_in_transaction(SharedRealmHandle));

        private Realm(SharedRealmHandle sharedRealmHandle, RealmConfiguration config)
        {
            SharedRealmHandle = sharedRealmHandle;
            Config = config;
            // update OUR config version number in case loaded one from disk
            Config.SchemaVersion = NativeSharedRealm.get_schema_version(sharedRealmHandle);

            Metadata = (config.ObjectClasses ?? RealmObjectClasses).ToDictionary(t => t, CreateRealmObjectMetadata);
        }

        private RealmObject.Metadata CreateRealmObjectMetadata(Type realmObjectType)
        {
            var table = this.GetTable(realmObjectType);
            var helper = (Weaving.IRealmObjectHelper)Activator.CreateInstance(realmObjectType.GetCustomAttribute<WovenAttribute>().HelperType);
            var properties = realmObjectType.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public)
                                            .Where(p => p.GetCustomAttributes(false).OfType<WovenPropertyAttribute>().Any())
                                            .Select(p =>
                                            {
                                                var mapTo = p.GetCustomAttributes(false).OfType<MapToAttribute>().SingleOrDefault();
                                                if (mapTo != null)
                                                {
                                                    return mapTo.Mapping;
                                                }

                                                return p.Name;
                                            })
                                            .ToDictionary(name => name, name => NativeTable.get_column_index(table, name, (IntPtr)name.Length));

            return new RealmObject.Metadata
            {
                Table = table,
                Helper = helper,
                ColumnIndices = properties
            };
        }

        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/> 
        /// </summary>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        private event RealmChangedEventHandler _realmChanged;

        /// <summary>
        /// Triggered when a realm has changed (i.e. a transaction was committed)
        /// </summary>
        public event RealmChangedEventHandler RealmChanged
        {
            add
            {
                if (_realmChanged == null)
                {
                    var managedRealmHandle = GCHandle.Alloc(this, GCHandleType.Weak);
                    NativeSharedRealm.bind_to_managed_realm_handle(SharedRealmHandle, GCHandle.ToIntPtr(managedRealmHandle));
                }
                    
                _realmChanged += value;
            }

            remove
            {
                _realmChanged -= value;
            }
        }

        private void NotifyChanged(EventArgs e)
        {
            if (_realmChanged != null)
                _realmChanged(this, e);
        }

        /// <summary>
        /// Checks if database has been closed.
        /// </summary>
        /// <returns>True if closed.</returns>
        public bool IsClosed => SharedRealmHandle.IsClosed;


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
                // Note we expect this call to also do explicit native close first rather than relying on destruction
                // in case other handles preserve pointers - they will no longer work but don't stop closing
                SharedRealmHandle.Close();
            }
        }


        /// <summary>
        ///  Dispose automatically closes the Realm if not already closed.
        /// </summary>
        public void Dispose()
        {
            Close();
            if (stringGetBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(stringGetBuffer);
                stringGetBuffer = IntPtr.Zero;
                stringGetBufferLen = 0;
            }
                
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
            return MarshalHelpers.IntPtrToBool(NativeSharedRealm.is_same_instance(SharedRealmHandle, rhs.SharedRealmHandle));
        }


        /// <summary>
        /// Serves as a hash function for a Realm based on the core instance.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return (int)SharedRealmHandle.DangerousGetHandle();
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
                var tablePtr = NativeSharedRealm.get_table(SharedRealmHandle, tableName, (IntPtr)tableName.Length);
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
        public T CreateObject<T>() where T : RealmObject, new()
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot create Realm object outside write transactions");

            var objectType = typeof(T);
            if (Config.ObjectClasses != null && !Config.ObjectClasses.Contains(objectType))
                throw new ArgumentException($"The class {objectType.Name} is not in the limited set of classes for this realm");

            var metadata = Metadata[typeof(T)];

            var result = (T)metadata.Helper.CreateInstance();

            var rowPtr = NativeTable.add_empty_row(metadata.Table);
            var rowHandle = CreateRowHandle (rowPtr, SharedRealmHandle);

            result._Manage(this, rowHandle);
            return result;
        }

        internal RealmObject MakeObjectForRow(Type objectType, RowHandle rowHandle)
        {
            RealmObject ret = Metadata[objectType].Helper.CreateInstance();
            ret._Manage(this, rowHandle);
            return ret;
        }


        internal ResultsHandle MakeResultsForTable(Type tableType)
        {
            var tableHandle = Metadata[tableType].Table;
            var objSchema = Realm.ObjectSchemaCache[tableType];
            IntPtr resultsPtr = NativeResults.create_for_table(SharedRealmHandle, tableHandle, objSchema);
            return CreateResultsHandle(resultsPtr);
        }


        internal ResultsHandle MakeResultsForQuery(Type tableType, QueryHandle builtQuery, SortOrderHandle optionalSortOrder)
        {
            var objSchema = Realm.ObjectSchemaCache[tableType];
            IntPtr resultsPtr = IntPtr.Zero;               
            if (optionalSortOrder == null)
                resultsPtr = NativeResults.create_for_query(SharedRealmHandle, builtQuery, objSchema);
            else
                resultsPtr = NativeResults.create_for_query_sorted(SharedRealmHandle, builtQuery, objSchema, optionalSortOrder);
            return CreateResultsHandle(resultsPtr);
        }


        internal SortOrderHandle MakeSortOrderForTable(Type tableType)
        {
            var tableHandle = Metadata[tableType].Table;
            IntPtr sortOrderPtr = NativeSortOrder.create_for_table(tableHandle);
            return CreateSortOrderHandle(sortOrderPtr);
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
                if (obj.Realm.SharedRealmHandle == this.SharedRealmHandle)
                    throw new RealmObjectAlreadyManagedByRealmException("The object is already managed by this realm");

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }


            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot start managing a Realm object outside write transactions");

            var tableHandle = Metadata[typeof(T)].Table;

            var rowPtr = NativeTable.add_empty_row(tableHandle);
            var rowHandle = CreateRowHandle(rowPtr, SharedRealmHandle);

            obj._Manage(this, rowHandle);
            obj._CopyDataFromBackingFieldsToRow();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static ResultsHandle CreateResultsHandle(IntPtr resultsPtr)
        {
            var resultsHandle = new ResultsHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                resultsHandle.SetHandle(resultsPtr);
            }
            return resultsHandle;
        }


        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static RowHandle CreateRowHandle(IntPtr rowPtr, SharedRealmHandle sharedRealmHandle)
        {
            var rowHandle = new RowHandle(sharedRealmHandle);

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                rowHandle.SetHandle(rowPtr);
            }
            return rowHandle;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static SortOrderHandle CreateSortOrderHandle(IntPtr sortOrderPtr)
        {
            var sortOrderHandle = new SortOrderHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                sortOrderHandle.SetHandle(sortOrderPtr);
            }
            return sortOrderHandle;
        }

        /// <summary>
        /// Factory for a write Transaction. Essential object to create scope for updates.
        /// </summary>
        /// <example>
        /// using (var trans = myrealm.BeginWrite()) { 
        ///     var rex = myrealm.CreateObject<Dog>();
        ///     rex.Name = "Rex";
        ///     trans.Commit();
        /// }
        /// </example>
        /// <returns>A transaction in write mode, which is required for any creation or modification of objects persisted in a Realm.</returns>
        public Transaction BeginWrite()
        {
            return new Transaction(SharedRealmHandle);
        }

        /// <summary>
        /// Execute an action inside a temporary transaction. If no exception is thrown, the transaction will automatically
        /// be committed.
        /// </summary>
        /// <remarks>
        /// Creates its own temporary transaction and commits it after running the lambda passed to `action`. 
        /// Be careful of wrapping multiple single property updates in multiple `Write` calls. It is more efficient to update several properties 
        /// or even create multiple objects in a single Write, unless you need to guarantee finer-grained updates.
        /// </remarks>
        /// <example>
        /// realm.Write(() => 
        /// {
        ///     d = myrealm.CreateObject<Dog>();
        ///     d.Name = "Eddie";
        ///     d.Age = 5;
        /// });
        /// </example>
        /// <param name="action">Action to perform inside a transaction, creating, updating or removing objects.</param>
        public void Write(Action action)
        {
            using (var transaction = BeginWrite())
            {
                action();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Update a Realm and outstanding objects to point to the most recent data for this Realm.
        /// This is only necessary when you have a Realm on a non-runloop thread that needs manual refreshing.
        /// </summary>
        /// <returns>
        /// Whether the realm had any updates. Note that this may return true even if no data has actually changed.
        /// </returns>
        public bool Refresh()
        {
            return MarshalHelpers.IntPtrToBool(NativeSharedRealm.refresh(SharedRealmHandle));
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

            var tableHandle = Metadata[obj.GetType()].Table;
            NativeTable.remove_row(tableHandle, (RowHandle)obj.RowHandle);
        }

        /// <summary>
        /// Remove objects matcing a query from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        public void RemoveRange<T>(RealmResults<T> range) where T: RealmObject
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove Realm objects outside write transactions");

            NativeResults.clear(range.ResultsHandle);
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        public void RemoveAll<T>() where T: RealmObject
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove all Realm objects outside write transactions");

            RemoveRange(All<T>());
        }

        /// <summary>
        /// Remove all objects of all types managed by this realm.
        /// </summary>
        public void RemoveAll()
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove all Realm objects outside write transactions");

            var objectClasses = Config.ObjectClasses ?? RealmObjectClasses;

            foreach (var objectClass in objectClasses)
            {
                var resultsHandle = MakeResultsForTable(objectClass);
                NativeResults.clear(resultsHandle);
            }
        }
    }
}
