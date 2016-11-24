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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#if __IOS__
using ObjCRuntime;
#endif
using Realms.Native;

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

        static Realm()
        {
            NativeCommon.Initialize();
            NativeCommon.register_notify_realm_changed(NotifyRealmChanged);
            NativeCommon.register_notify_realm_object_changed(RealmObject.NotifyRealmObjectPropertyChanged);
        }

        #if __IOS__
        [MonoPInvokeCallback(typeof(NativeCommon.NotifyRealmCallback))]
        #endif
        private static void NotifyRealmChanged(IntPtr realmHandle)
        {
            var gch = GCHandle.FromIntPtr(realmHandle);
            ((Realm)gch.Target).NotifyChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="databasePath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        /// <remarks>If you specify a relative path, sandboxing by the OS may cause failure if you specify anything other than a subdirectory. <br />
        /// Instances are cached for a given absolute path and thread, so you may get back the same instance.
        /// </remarks>
        /// <returns>A realm instance, possibly from cache.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the file system returns an error preventing file creation.</exception>
        public static Realm GetInstance(string databasePath)
        {
            var config = RealmConfiguration.DefaultConfiguration;
            if (!string.IsNullOrEmpty(databasePath))
            {
                config = config.ConfigWithPath(databasePath);
            }

            return GetInstance(config);
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="config">Optional configuration.</param>
        /// <returns>A realm instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the file system returns an error, preventing file creation.</exception>
        public static Realm GetInstance(RealmConfiguration config = null)
        {
            return GetInstance(config ?? RealmConfiguration.DefaultConfiguration, null);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static Realm GetInstance(RealmConfiguration config, RealmSchema schema)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (schema == null)
            {
                if (config.ObjectClasses != null)
                {
                    schema = RealmSchema.CreateSchemaForClasses(config.ObjectClasses);
                }
                else
                {
                    schema = RealmSchema.Default;
                }
            }

            return config.CreateRealm(schema);
        }

        #endregion

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly Dictionary<string, RealmObject.Metadata> Metadata;

        internal bool IsInTransaction => SharedRealmHandle.IsInTransaction();

        /// <summary>
        /// Gets the <see cref="RealmSchema"/> instance that describes all the types that can be stored in this <see cref="Realm"/>.
        /// </summary>
        public RealmSchema Schema { get; }

        /// <summary>
        /// Gets the <see cref="RealmConfiguration"/> that controls this realm's path and other settings.
        /// </summary>
        public RealmConfiguration Config { get; private set; }

        internal Realm(SharedRealmHandle sharedRealmHandle, RealmConfiguration config, RealmSchema schema)
        {
            SharedRealmHandle = sharedRealmHandle;
            Config = config;

            Metadata = schema.ToDictionary(t => t.Name, CreateRealmObjectMetadata);
            Schema = schema;
        }

        private RealmObject.Metadata CreateRealmObjectMetadata(Schema.ObjectSchema schema)
        {
            var table = this.GetTable(schema);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null && !Config.Dynamic)
            {
                var wovenAtt = schema.Type.GetCustomAttribute<WovenAttribute>();
                if (wovenAtt == null)
                {
                    throw new RealmException($"Fody not properly installed. {schema.Type.FullName} is a RealmObject but has not been woven.");
                }

                helper = (Weaving.IRealmObjectHelper)Activator.CreateInstance(wovenAtt.HelperType);
            }
            else
            {
                helper = Dynamic.DynamicRealmObjectHelper.Instance;
            }

            // build up column index in a loop so can spot and cache primary key index on the way
            var initPropertyMap = new Dictionary<string, IntPtr>();
            var propertyIndex = -1;
            foreach (var prop in schema)
            {
                initPropertyMap[prop.Name] = (IntPtr)(++propertyIndex);
            }

            return new RealmObject.Metadata
            {
                Table = table,
                Helper = helper,
                PropertyIndices = initPropertyMap,
                Schema = schema
            };
        }

        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/> 
        /// </summary>
        /// <param name="sender">The Realm which has changed.</param>
        /// <param name="e">Currently an empty argument, in future may indicate more details about the change.</param>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        private event RealmChangedEventHandler _realmChanged;

        /// <summary>
        /// Triggered when a realm has changed (i.e. a transaction was committed).
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
            _realmChanged?.Invoke(this, e);
        }

        public event ErrorEventHandler Error;

        internal void NotifyError(Exception ex)
        {
            Debug.Assert(Error != null, "A realm-level exception has occurred. To handle and react to those, subscribe to the Realm.Error event.");

            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// Checks if database has been closed.
        /// </summary>
        /// <returns>True if closed.</returns>
        public bool IsClosed => SharedRealmHandle.IsClosed;

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
            {
                return;
            }

            if (disposing && !(SharedRealmHandle is UnownedRealmHandle))
            {
                SharedRealmHandle.CloseRealm();
            }

            SharedRealmHandle.Close();  // Note: this closes the *handle*, it does not trigger realm::Realm::close().
        }

        /// <summary>
        /// Generic override determines whether the specified <see cref="object"/> is equal to the current Realm.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public override bool Equals(object obj) => Equals(obj as Realm);

        /// <summary>
        /// Determines whether the specified Realm is equal to the current Realm.
        /// </summary>
        /// <param name="other">The Realm to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public bool Equals(Realm other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Config.Equals(other.Config) && IsClosed == other.IsClosed;
        }

        /// <summary>
        /// Determines whether this instance is the same core instance as the passed in argument.
        /// </summary>
        /// <remarks>
        /// You can, and should, have multiple instances open on different threads which have the same path and open the same Realm.
        /// </remarks>
        /// <returns><c>true</c> if this instance is the same core instance; otherwise, <c>false</c>.</returns>
        /// <param name="other">The Realm to compare with the current Realm.</param>
        public bool IsSameInstance(Realm other)
        {
            return SharedRealmHandle.IsSameInstance(other.SharedRealmHandle);
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
        public static void DeleteRealm(RealmConfiguration configuration)
        {
            // TODO add cache checking when implemented, https://github.com/realm/realm-dotnet/issues/308
            // when cache checking, uncomment in IntegrationTests.cs RealmInstanceTests.DeleteRealmFailsIfOpenSameThread and add a variant to test open on different thread
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
            try
            {
                /* Retain handle in a constrained execution region */
            }
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
        /// <remarks>Using CreateObject is more efficient than creating standalone objects, assigning their values, then using Add because it avoids copying properties to the realm.</remarks>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <returns>An object which is already managed.</returns>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        public T CreateObject<T>() where T : RealmObject, new()
        {
            RealmObject.Metadata metadata;
            var ret = CreateObject(typeof(T).Name, out metadata);
            if (typeof(T) != metadata.Schema.Type)
            {
                throw new ArgumentException($"The type {typeof(T).FullName} does not match the original type the schema was created for - {metadata.Schema.Type?.FullName}");
            }

            return (T)ret;
        }

        /// <summary>
        /// Factory for a managed object in a realm. Only valid within a Write transaction.
        /// </summary>
        /// <returns>A dynamically-accessed Realm object.</returns>
        /// <param name="className">The type of object to create as defined in the schema.</param>
        /// <remarks>
        /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version of a realm) the returned object will be purely dynamic.
        /// If the realm has been created from a typed schema as is the default case when calling <code>Realm.GetInstance()</code> the returned object will be an instance of a user-defined class, as if created by <code>Realm.CreateObject&lt;T&gt;()</code>.
        /// </remarks>
        public dynamic CreateObject(string className)
        {
            RealmObject.Metadata ignored;
            return CreateObject(className, out ignored);
        }

        private RealmObject CreateObject(string className, out RealmObject.Metadata metadata)
        {
            if (!Metadata.TryGetValue(className, out metadata))
            {
                throw new ArgumentException($"The class {className} is not in the limited set of classes for this realm");
            }

            var result = metadata.Helper.CreateInstance();

            var objectPtr = metadata.Table.AddEmptyObject(SharedRealmHandle);
            var objectHandle = CreateObjectHandle(objectPtr, SharedRealmHandle);
            result._SetOwner(this, objectHandle, metadata);
            return result;
        }

        internal RealmObject MakeObject(RealmObject.Metadata metadata, IntPtr objectPtr)
        {
            return MakeObject(metadata, CreateObjectHandle(objectPtr, SharedRealmHandle));
        }

        internal RealmObject MakeObject(string className, IntPtr objectPtr)
        {
            return MakeObject(Metadata[className], CreateObjectHandle(objectPtr, SharedRealmHandle));
        }

        internal RealmObject MakeObject(string className, ObjectHandle objectHandle)
        {
            return MakeObject(Metadata[className], objectHandle);
        }

        internal RealmObject MakeObject(RealmObject.Metadata metadata, ObjectHandle objectHandle)
        {
            var ret = metadata.Helper.CreateInstance();
            ret._SetOwner(this, objectHandle, metadata);
            return ret;
        }

        internal ResultsHandle MakeResultsForTable(RealmObject.Metadata metadata)
        {
            var resultsPtr = metadata.Table.CreateResults(SharedRealmHandle);
            return CreateResultsHandle(resultsPtr);
        }

        internal ResultsHandle MakeResultsForQuery(QueryHandle builtQuery, SortDescriptorBuilder optionalSortDescriptorBuilder)
        {
            var resultsPtr = IntPtr.Zero;
            if (optionalSortDescriptorBuilder == null)
            {
                resultsPtr = builtQuery.CreateResults(SharedRealmHandle);
            }
            else
            {
                resultsPtr = builtQuery.CreateSortedResults(SharedRealmHandle, optionalSortDescriptorBuilder);
            }

            return CreateResultsHandle(resultsPtr);
        }

        internal SortDescriptorBuilder CreateSortDescriptorForTable(RealmObject.Metadata metadata)
        {
            return new SortDescriptorBuilder(metadata);
        }

        /// <summary>
        /// This realm will start managing a RealmObject which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processed by the Fody weaver, so it has persistent properties.</typeparam>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <param name="update">If true, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm</exception>
        /// <remarks>
        /// If the object is already managed by this realm, this method does nothing.
        /// Cyclic graphs (<c>Parent</c> has <c>Child</c> that has a <c>Parent</c>) will result in undefined behavior. You have to break the cycle manually and assign relationships after all object have been managed.
        /// </remarks>
        public void Add<T>(T obj, bool update = false) where T : RealmObject
        {
            // This is not obsoleted because the compiler will always pick it for specific types, generating a bunch of warnings
            AddInternal(obj, typeof(T), update);
        }

        /// <summary>
        /// This realm will start managing a RealmObject which has been created as a standalone object.
        /// </summary>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <param name="update">If true, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm</exception>
        /// <remarks>
        /// If the object is already managed by this realm, this method does nothing.
        /// Cyclic graphs (<c>Parent</c> has <c>Child</c> that has a <c>Parent</c>) will result in undefined behavior. You have to break the cycle manually and assign relationships after all object have been managed.
        /// </remarks>
        public void Add(RealmObject obj, bool update = false)
        {
            AddInternal(obj, obj?.GetType(), update);
        }

        private void AddInternal(RealmObject obj, Type objectType, bool update)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            if (obj.IsManaged)
            {
                if (obj.Realm.SharedRealmHandle == this.SharedRealmHandle)
                {
                    // Already managed by this realm, so nothing to do.
                    return;
                }

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }

            var metadata = Metadata[objectType.Name];

            var objectPtr = IntPtr.Zero;

            object pkValue;
            if (update && metadata.Helper.TryGetPrimaryKeyValue(obj, out pkValue))
            {
                if (pkValue is string)
                {
                    objectPtr = metadata.Table.Find(SharedRealmHandle, (string)pkValue);
                }
                else if (pkValue == null)
                {
                    objectPtr = metadata.Table.Find(SharedRealmHandle, (long?)null);
                }
                else
                {
                    // We know it must be convertible to long, so optimistically do it.
                    objectPtr = metadata.Table.Find(SharedRealmHandle, Convert.ToInt64(pkValue));
                }
            }

            if (objectPtr == IntPtr.Zero)
            {
                objectPtr = metadata.Table.AddEmptyObject(SharedRealmHandle);
            }

            var objectHandle = CreateObjectHandle(objectPtr, SharedRealmHandle);

            obj._SetOwner(this, objectHandle, metadata);
            metadata.Helper.CopyToRealm(obj, update);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static ResultsHandle CreateResultsHandle(IntPtr resultsPtr)
        {
            var resultsHandle = new ResultsHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                /* Retain handle in a constrained execution region */
            }
            finally
            {
                resultsHandle.SetHandle(resultsPtr);
            }

            return resultsHandle;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static ObjectHandle CreateObjectHandle(IntPtr objectPtr, SharedRealmHandle sharedRealmHandle)
        {
            var objectHandle = new ObjectHandle(sharedRealmHandle);

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                /* Retain handle in a constrained execution region */
            }
            finally
            {
                objectHandle.SetHandle(objectPtr);
            }

            return objectHandle;
        }

        /// <summary>
        /// Factory for a write Transaction. Essential object to create scope for updates.
        /// </summary>
        /// <example><c>
        /// using (var trans = realm.BeginWrite()) 
        /// { 
        ///     var rex = realm.CreateObject&lt;Dog&gt;();
        ///     rex.Name = "Rex";
        ///     trans.Commit();
        /// }</c>
        /// </example>
        /// <returns>A transaction in write mode, which is required for any creation or modification of objects persisted in a Realm.</returns>
        public Transaction BeginWrite()
        {
            return new Transaction(this);
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
        /// <example><c>
        /// realm.Write(() => 
        /// {
        ///     d = realm.CreateObject&lt;Dog&gt;();
        ///     d.Name = "Eddie";
        ///     d.Age = 5;
        /// });</c>
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
        /// <returns>A standard <c>Task</c> so it can be used by <c>await</c>.</returns>
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
        /// This is only necessary when you have a Realm on a thread without a runloop that needs manual refreshing.
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
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of class T, in this realm.</returns>
        public IQueryable<T> All<T>() where T : RealmObject
        {
            var type = typeof(T);
            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(type.Name, out metadata) || metadata.Schema.Type != type)
            {
                throw new ArgumentException($"The class {type.Name} is not in the limited set of classes for this realm");
            }

            return new RealmResults<T>(this, metadata, true);
        }

        /// <summary>
        /// Get a view of all the objects of a particular type.
        /// </summary>
        /// <param name="className">The type of the objects as defined in the schema.</param>
        /// <remarks>Because the objects inside the view are accessed dynamically, the view cannot be queried into using LINQ or other expression predicates.</remarks>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of className, in this realm.</returns>
        public IQueryable<dynamic> All(string className)
        {
            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(className, out metadata))
            {
                throw new ArgumentException($"The class {className} is not in the limited set of classes for this realm");
            }

            return new RealmResults<dynamic>(this, metadata, true);
        }

        #region Quick Find using primary key

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search. An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.</param>
        /// <returns>Null or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        public T Find<T>(long? primaryKey) where T : RealmObject
        {
            var metadata = Metadata[typeof(T).Name];
            var objectPtr = metadata.Table.Find(SharedRealmHandle, primaryKey);
            if (objectPtr == IntPtr.Zero)
            {
                return null;
            }

            return (T)MakeObject(metadata, objectPtr);
        }

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        public T Find<T>(string primaryKey) where T : RealmObject
        {
            var metadata = Metadata[typeof(T).Name];
            var objectPtr = metadata.Table.Find(SharedRealmHandle, primaryKey);
            if (objectPtr == IntPtr.Zero)
            {
                return null;
            }

            return (T)MakeObject(metadata, objectPtr);
        }

        /// <summary>
        /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search. An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.</param>
        /// <returns>Null or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        public RealmObject Find(string className, long? primaryKey)
        {
            var metadata = Metadata[className];
            var objectPtr = metadata.Table.Find(SharedRealmHandle, primaryKey);
            if (objectPtr == IntPtr.Zero)
            {
                return null;
            }

            return MakeObject(metadata, objectPtr);
        }

        /// <summary>
        /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        public RealmObject Find(string className, string primaryKey)
        {
            var metadata = Metadata[className];
            var objectPtr = metadata.Table.Find(SharedRealmHandle, primaryKey);
            if (objectPtr == IntPtr.Zero)
            {
                return null;
            }

            return MakeObject(metadata, objectPtr);
        }

        #endregion Quick Find using primary key

        /// <summary>
        /// Removes a persistent object from this realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this realm.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="ArgumentNullException">If you invoke this with a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!obj.IsManaged)
            {
                throw new ArgumentException("Object is not managed by Realm, so it cannot be removed.", nameof(obj));
            }

            obj.ObjectHandle.RemoveFromRealm(SharedRealmHandle);
        }

        /// <summary>
        /// Remove objects matching a query from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        public void RemoveRange<T>(IQueryable<T> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            if (!(range is RealmResults<T>))
            {
                throw new ArgumentException("range should be the return value of .All or a LINQ query applied to it.", nameof(range));
            }

            var results = (RealmResults<T>)range;
            results.ResultsHandle.Clear(SharedRealmHandle);
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        public void RemoveAll<T>() where T : RealmObject
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
            foreach (var metadata in Metadata.Values)
            {
                var resultsHandle = MakeResultsForTable(metadata);
                resultsHandle.Clear(SharedRealmHandle);
            }
        }

        #region Obsolete methods

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="id">Id to be matched exactly, same as an == search. Int64 argument works for all integer properties supported as PrimaryKey.</param>
        /// <returns>Null or an object matching the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public T ObjectForPrimaryKey<T>(long id) where T : RealmObject
        {
            return Find<T>(id);
        }

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matdhing the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public T ObjectForPrimaryKey<T>(string id) where T : RealmObject
        {
            return Find<T>(id);
        }

        /// <summary>
        /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matdhing the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public RealmObject ObjectForPrimaryKey(string className, long id)
        {
            return Find(className, id);
        }

        /// <summary>
        /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matdhing the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public RealmObject ObjectForPrimaryKey(string className, string id)
        {
            return Find(className, id);
        }

        /// <summary>
        /// This realm will start managing a RealmObject which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processed by the Fody weaver, so it has persistent properties.</typeparam>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <param name="update">If true, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm</exception>
        [Obsolete("This method has been renamed. Use Add for the same results.")]
        public void Manage<T>(T obj, bool update = false) where T : RealmObject
        {
            Add(obj, update);
        }

        /// <summary>
        /// Closes the Realm if not already closed. Safe to call repeatedly.
        /// Note that this will close the file. Other references to the same database
        /// on the same thread will be invalidated.
        /// </summary>
        [Obsolete("This method has been deprecated. Instead, dispose the realm to close it.")]
        public void Close()
        {
            Dispose();
        }

        #endregion
    }
}
