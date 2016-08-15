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
using System.Threading.Tasks;

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

        // shared string buffer for getter because can only be getting on this one thread per Realm
        internal IntPtr stringGetBuffer = IntPtr.Zero;
        internal int stringGetBufferLen;

        static Realm()
        {
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
        public static Realm GetInstance(RealmConfiguration config = null)
        {
            return GetInstance(config, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static Realm GetInstance(RealmConfiguration config, RealmSchema schema)
        {
            config = config ?? RealmConfiguration.DefaultConfiguration;

            var srHandle = new SharedRealmHandle();

            if (schema == null)
            {
                if (config.ObjectClasses != null)
                {
                    schema = RealmSchema.CreateSchemaForClasses(config.ObjectClasses, new SchemaHandle(srHandle));
                }
                else
                {
                    schema = RealmSchema.Default.CloneForAdoption(srHandle);
                }
            }
            else
            {
                schema = schema.CloneForAdoption(srHandle);
            }

            var srPtr = IntPtr.Zero;
            try {
                srPtr = srHandle.Open(schema.Handle, 
                    config.DatabasePath, 
                    config.ReadOnly, false, 
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
                srPtr = srHandle.Open(schema.Handle, 
                    config.DatabasePath, 
                    config.ReadOnly, false, 
                    config.EncryptionKey,
                    config.SchemaVersion);
            }

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                srHandle.SetHandle(srPtr);
            }

            return new Realm(srHandle, config, schema);
        } 

        #endregion

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly Dictionary<string, RealmObject.Metadata> Metadata;

        internal bool IsInTransaction => SharedRealmHandle.IsInTransaction();

        /// <summary>
        /// The <see cref="RealmSchema"/> instance that describes all the types that can be stored in this <see cref="Realm"/>.
        /// </summary>
        public RealmSchema Schema { get; }

        private Realm(SharedRealmHandle sharedRealmHandle, RealmConfiguration config, RealmSchema schema)
        {
            SharedRealmHandle = sharedRealmHandle;
            Config = config;
            // update OUR config version number in case loaded one from disk
            Config.SchemaVersion = sharedRealmHandle.GetSchemaVersion();

            Metadata = schema.ToDictionary(t => t.Name, CreateRealmObjectMetadata);
            Schema = schema;
        }

        private RealmObject.Metadata CreateRealmObjectMetadata(Schema.ObjectSchema schema)
        {
            var table = this.GetTable(schema);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null)
            {
                var wovenAtt = schema.Type.GetCustomAttribute<WovenAttribute>();
                if (wovenAtt == null)
                    throw new RealmException($"Fody not properly installed. {schema.Type.FullName} is a RealmObject but has not been woven.");
                helper = (Weaving.IRealmObjectHelper)Activator.CreateInstance(wovenAtt.HelperType);
            }
            else
            {
                helper = Dynamic.DynamicRealmObjectHelper.Instance;
            }

            return new RealmObject.Metadata
            {
                Table = table,
                Helper = helper,
                ColumnIndices = schema.ToDictionary(p => p.Name, p => NativeTable.GetColumnIndex(table, p.Name)),
                Schema = schema
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
                    SharedRealmHandle.BindToManagedRealmHandle(GCHandle.ToIntPtr(managedRealmHandle));
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
        /// Closes the Realm if not already closed. Safe to call repeatedly.
        /// Note that this will close the file. Other references to the same database
        /// on the same thread will be invalidated.
        /// </summary>
        public void Close()
        {
            if (IsClosed)
                return;

            Dispose();
        }

        ~Realm()
        {
            Dispose(false);
        }

        /// <summary>
        ///  Dispose automatically closes the Realm if not already closed.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsClosed)
                throw new ObjectDisposedException(nameof(Realm));
            
            if (disposing)
            {
                SharedRealmHandle.CloseRealm();
            }

            SharedRealmHandle.Close();  // Note: this closes the *handle*, it does not trigger realm::Realm::close().

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
            return SharedRealmHandle.IsSameInstance(rhs.SharedRealmHandle);
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
        private TableHandle GetTable(Schema.ObjectSchema schema)
        {
            var result = new TableHandle();
            var tableName = schema.Name;

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                var tablePtr = SharedRealmHandle.GetTable(tableName);
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
            RealmObject.Metadata metadata;
            var ret = CreateObject(typeof(T).Name, out metadata);
            if (typeof(T) != metadata.Schema.Type)
                throw new ArgumentException($"The type {typeof(T).FullName} does not match the original type the schema was created for - {metadata.Schema.Type?.FullName}");

            return (T)ret;
        }

        /// <summary>
        /// Factory for a managed object in a realm. Only valid within a Write transaction.
        /// </summary>
        /// <returns>A dynamically-accessed Realm object.</returns>
        /// <param name="className">The type of object to create as defined in the schema.</param>
        /// <remarks>
        /// If the realm instance has been created from an untyped schema (such as when migrating from an older version of a realm) the returned object will be purely dynamic.
        /// If the realm has been created from a typed schema as is the default case when calling <code>Realm.GetInstance()</code> the returned object will be an instance of a user-defined class, as if created by <code>Realm.CreateObject&lt;T&gt;()</code>.
        /// </remarks>
        public dynamic CreateObject(string className)
        {
            RealmObject.Metadata ignored;
            return CreateObject(className, out ignored);
        }

        private RealmObject CreateObject(string className, out RealmObject.Metadata metadata)
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot create Realm object outside write transactions");

            if (!Metadata.TryGetValue(className, out metadata))
                throw new ArgumentException($"The class {className} is not in the limited set of classes for this realm");

            var result = metadata.Helper.CreateInstance();

            var rowPtr = NativeTable.AddEmptyRow(metadata.Table);
            var rowHandle = CreateRowHandle(rowPtr, SharedRealmHandle);

            result._Manage(this, rowHandle, metadata);
            return result;
        }


        internal RealmObject MakeObjectForRow(RealmObject.Metadata metadata, IntPtr rowPtr)
        {
            return MakeObjectForRow(metadata, CreateRowHandle(rowPtr, SharedRealmHandle));
        }


        internal RealmObject MakeObjectForRow(string className, IntPtr rowPtr)
        {
            return MakeObjectForRow(Metadata [className], CreateRowHandle(rowPtr, SharedRealmHandle));
        }


        internal RealmObject MakeObjectForRow(string className, RowHandle row)
        {
            return MakeObjectForRow(Metadata[className], row);
        }


        internal RealmObject MakeObjectForRow(RealmObject.Metadata metadata, RowHandle row)
        {
            RealmObject ret = metadata.Helper.CreateInstance();
            ret._Manage(this, row, metadata);
            return ret;
        }


        internal ResultsHandle MakeResultsForTable(RealmObject.Metadata metadata)
        {
            var resultsPtr = NativeTable.CreateResults(metadata.Table, SharedRealmHandle, metadata.Schema.Handle);
            return CreateResultsHandle(resultsPtr);
        }


        internal ResultsHandle MakeResultsForQuery(Schema.ObjectSchema schema, QueryHandle builtQuery, SortOrderHandle optionalSortOrder)
        {
            var resultsPtr = IntPtr.Zero;               
            if (optionalSortOrder == null)
                resultsPtr = builtQuery.CreateResults(SharedRealmHandle, schema.Handle);
            else
                resultsPtr = builtQuery.CreateSortedResults(SharedRealmHandle, schema.Handle, optionalSortOrder);
            return CreateResultsHandle(resultsPtr);
        }


        internal SortOrderHandle MakeSortOrderForTable(RealmObject.Metadata metadata)
        {
            var result = new SortOrderHandle();
            result.CreateForTable(metadata.Table);
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
                if (obj.Realm.SharedRealmHandle == this.SharedRealmHandle)
                    throw new RealmObjectAlreadyManagedByRealmException("The object is already managed by this realm");

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }


            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot start managing a Realm object outside write transactions");

            var metadata = Metadata[typeof(T).Name];
            var tableHandle = metadata.Table;

            var rowPtr = NativeTable.AddEmptyRow(tableHandle);
            var rowHandle = CreateRowHandle(rowPtr, SharedRealmHandle);

            obj._Manage(this, rowHandle, metadata);
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
        /// Execute an action inside a temporary transaction on a worker thread. If no exception is thrown, the transaction will automatically
        /// be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this realm on a worker thread and executes <c>action</c> inside a write transaction.
        /// Realms and realm objects are thread-affine, so capturing any such objects in the <c>action</c> delegate will lead to errors
        /// if they're used on the worker thread.
        /// </remarks>
        /// <example>
        /// await realm.WriteAsync(tempRealm =&gt; 
        /// {
        ///     var pongo = tempRealm.All&lt;Dog&gt;().Single(d =&gt; d.Name == "Pongo");
        ///     var missis = tempRealm.All&lt;Dog&gt;().Single(d =&gt; d.Name == "Missis");
        ///     for (var i = 0; i &lt; 15; i++)
        ///     {
        ///         var pup = tempRealm.CreateObject&lt;Dog&gt;();
        ///         pup.Breed = "Dalmatian";
        ///         pup.Mum = missis;
        ///         pup.Dad = pongo;
        ///     }
        /// });
        /// </example>
        /// <param name="action">Action to perform inside a transaction, creating, updating or removing objects.</param>
        public Task WriteAsync(Action<Realm> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // avoid capturing `this` in the lambda
            var configuration = Config;
            return Task.Run(() =>
            {
                using (var realm = GetInstance(configuration))
                using (var transaction = realm.BeginWrite())
                {
                    action(realm);
                    transaction.Commit();
                }
            });
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
            return SharedRealmHandle.Refresh();
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of class T, in this realm.</returns>
        public RealmResults<T> All<T>() where T: RealmObject
        {
            var type = typeof(T);
            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(type.Name, out metadata) || metadata.Schema.Type != type)
                throw new ArgumentException($"The class {type.Name} is not in the limited set of classes for this realm");

            return new RealmResults<T>(this, metadata, true);
        }

        /// <summary>
        /// Get a view of all the objects of a particular type
        /// </summary>
        /// <param name="className">The type of the objects as defined in the schema.</param>
        /// <remarks>Because the objects inside the view are accessed dynamically, the view cannot be queried into using LINQ or other expression predicates.</remarks>
        public RealmResults<dynamic> All(string className)
        {
            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(className, out metadata))
                throw new ArgumentException($"The class {className} is not in the limited set of classes for this realm");

            return new RealmResults<dynamic>(this, metadata, true);
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

            var tableHandle = obj.ObjectMetadata.Table;
            NativeTable.RemoveRow(tableHandle, (RowHandle)obj.RowHandle);
        }

        /// <summary>
        /// Remove objects matcing a query from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        public void RemoveRange<T>(RealmResults<T> range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));

            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove Realm objects outside write transactions");

            range.ResultsHandle.Clear();
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        public void RemoveAll<T>() where T: RealmObject
        {
            RemoveRange(All<T>());
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <param name="className">Type of the objects to remove as defined in the schema.</param>
        public void RemoveAll(string className)
        {
            RemoveRange(All(className));
        }

        /// <summary>
        /// Remove all objects of all types managed by this realm.
        /// </summary>
        public void RemoveAll()
        {
            if (!IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot remove all Realm objects outside write transactions");

            foreach (var metadata in Metadata.Values)
            {
                var resultsHandle = MakeResultsForTable(metadata);
                resultsHandle.Clear();
            }
        }
    }
}
