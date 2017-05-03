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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// A Realm instance (also referred to as a Realm) represents a Realm database.
    /// </summary>
    /// <remarks>
    /// <b>Warning</b>: Realm instances are not thread safe and can not be shared across threads.
    /// You must call <see cref="GetInstance(RealmConfigurationBase)"/> on each thread in which you want to interact with the Realm. 
    /// </remarks>
    public class Realm : IDisposable
    {
        #region static

        static Realm()
        {
            NativeCommon.Initialize();

            NativeCommon.NotifyRealmCallback notifyRealm = RealmState.NotifyRealmChanged;
            GCHandle.Alloc(notifyRealm);

            NativeCommon.register_notify_realm_changed(notifyRealm);

            SynchronizationContextEventLoopSignal.Install();
        }

        /// <summary>
        /// Factory for obtaining a <see cref="Realm"/> instance for this thread.
        /// </summary>
        /// <param name="databasePath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        /// <remarks>
        /// If you specify a relative path, sandboxing by the OS may cause failure if you specify anything other than a subdirectory.
        /// </remarks>
        /// <returns>A <see cref="Realm"/> instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">
        /// Thrown if the file system returns an error preventing file creation.
        /// </exception>
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
        /// Factory for obtaining a <see cref="Realm"/> instance for this thread.
        /// </summary>
        /// <param name="config">Optional configuration.</param>
        /// <returns>A <see cref="Realm"/> instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">
        /// Thrown if the file system returns an error preventing file creation.
        /// </exception>
        public static Realm GetInstance(RealmConfigurationBase config = null)
        {
            return GetInstance(config ?? RealmConfiguration.DefaultConfiguration, null);
        }

        internal static Realm GetInstance(RealmConfigurationBase config, RealmSchema schema)
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

        /// <summary>
        /// Compacts a Realm file. A Realm file usually contains free/unused space. This method removes this free space and the file size is thereby reduced. Objects within the Realm file are untouched.
        /// </summary>
        /// <remarks>
        /// The realm file must not be open on other threads.
        /// The file system should have free space for at least a copy of the Realm file.
        /// This method must not be called inside a transaction.
        /// The Realm file is left untouched if any file operation fails.
        /// </remarks>
        /// <param name="config">Optional configuration.</param>
        /// <returns><c>true</c> if successful, <c>false</c> if any file operation failed.</returns>
        public static bool Compact(RealmConfigurationBase config = null)
        {
            using (var realm = GetInstance(config))
            {
                return realm.SharedRealmHandle.Compact();
            }
        }

        /// <summary>
        /// Deletes all the files associated with a realm.
        /// </summary>
        /// <param name="configuration">A <see cref="RealmConfigurationBase"/> which supplies the realm path.</param>
        public static void DeleteRealm(RealmConfigurationBase configuration)
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

        internal static ResultsHandle CreateResultsHandle(IntPtr resultsPtr)
        {
            var resultsHandle = new ResultsHandle();
            resultsHandle.SetHandle(resultsPtr);
            return resultsHandle;
        }

        internal static ObjectHandle CreateObjectHandle(IntPtr objectPtr, SharedRealmHandle sharedRealmHandle)
        {
            var objectHandle = new ObjectHandle(sharedRealmHandle);
            objectHandle.SetHandle(objectPtr);
            return objectHandle;
        }

        #endregion

        private RealmState _state;

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly Dictionary<string, RealmObject.Metadata> Metadata;

        internal bool IsInTransaction => SharedRealmHandle.IsInTransaction();

        /// <summary>
        /// Gets the <see cref="RealmSchema"/> instance that describes all the types that can be stored in this <see cref="Realm"/>.
        /// </summary>
        /// <value>The Schema of the Realm.</value>
        public RealmSchema Schema { get; }

        /// <summary>
        /// Gets the <see cref="RealmConfigurationBase"/> that controls this realm's path and other settings.
        /// </summary>
        /// <value>The Realm's configuration.</value>
        public RealmConfigurationBase Config { get; }

        internal Realm(SharedRealmHandle sharedRealmHandle, RealmConfigurationBase config, RealmSchema schema)
        {
            RealmState state = null;

            var statePtr = sharedRealmHandle.GetManagedStateHandle();
            if (statePtr != IntPtr.Zero)
            {
                state = GCHandle.FromIntPtr(statePtr).Target as RealmState;
            }

            if (state == null)
            {
                state = new RealmState();
                sharedRealmHandle.SetManagedStateHandle(GCHandle.ToIntPtr(state.GCHandle));
            }

            state.AddRealm(this);
            _state = state;

            SharedRealmHandle = sharedRealmHandle;
            Config = config;

            Metadata = schema.ToDictionary(t => t.Name, CreateRealmObjectMetadata);
            Schema = schema;
        }

        private RealmObject.Metadata CreateRealmObjectMetadata(ObjectSchema schema)
        {
            var table = this.GetTable(schema);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null && !Config.Dynamic)
            {
                var wovenAtt = schema.Type.GetTypeInfo().GetCustomAttribute<WovenAttribute>();
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

            var initPropertyMap = new Dictionary<string, IntPtr>(schema.Count);
            var persistedProperties = -1;
            var computedProperties = -1;
            foreach (var prop in schema)
            {
                var index = prop.Type.IsComputed() ? ++computedProperties : ++persistedProperties;
                initPropertyMap[prop.Name] = (IntPtr)index;
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
        /// <param name="sender">The <see cref="Realm"/> which has changed.</param>
        /// <param name="e">Currently an empty argument, in future may indicate more details about the change.</param>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Triggered when a Realm has changed (i.e. a <see cref="Transaction"/> was committed).
        /// </summary>
        public event RealmChangedEventHandler RealmChanged;

        private void NotifyChanged(EventArgs e)
        {
            RealmChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Triggered when a Realm-level exception has occurred.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        internal void NotifyError(Exception ex)
        {
            if (Error == null)
            {
                ErrorMessages.OutputError(ErrorMessages.RealmNotifyErrorNoSubscribers);
            }

            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// Gets a value indicating whether the instance has been closed via <see cref="Dispose()"/>. If <c>true</c>, you
        /// should not call methods on that instance.
        /// </summary>
        /// <value><c>true</c> if closed, <c>false</c> otherwise.</value>
        public bool IsClosed => SharedRealmHandle.IsClosed;

        /// <inheritdoc />
        ~Realm()
        {
            Dispose(false);
        }

        /// <inheritdoc />
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

            if (disposing)
            {
                // only mutate the state on explicit disposal
                // otherwise we do so on the finalizer thread
                _state.RemoveRealm(this);
            }
            _state = null;

            SharedRealmHandle.Close();  // Note: this closes the *handle*, it does not trigger realm::Realm::close().
        }

        private void ThrowIfDisposed()
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException(typeof(Realm).FullName, "Cannot access a closed Realm.");
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as Realm);

        private bool Equals(Realm other)
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
            ThrowIfDisposed();

            return SharedRealmHandle.IsSameInstance(other.SharedRealmHandle);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            ThrowIfDisposed();

            return (int)((long)SharedRealmHandle.DangerousGetHandle() % int.MaxValue);
        }

        private TableHandle GetTable(ObjectSchema schema)
        {
            var result = new TableHandle();
            var tableName = schema.Name;

            var tablePtr = SharedRealmHandle.GetTable(tableName);
            result.SetHandle(tablePtr);

            return result;
        }

        /// <summary>
        /// Factory for a managed object in a realm. Only valid within a write <see cref="Transaction"/>.
        /// </summary>
        /// <returns>A dynamically-accessed Realm object.</returns>
        /// <param name="className">The type of object to create as defined in the schema.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>WARNING:</b> if the dynamic object has a PrimaryKey then that must be the <b>first property set</b>
        /// otherwise other property changes may be lost.
        /// </para>
        /// <para>
        /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
        /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
        /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
        /// object will be an instance of a user-defined class, as if created by <see cref="CreateObject{T}"/>.
        /// </para>
        /// </remarks>
        public dynamic CreateObject(string className)
        {
            ThrowIfDisposed();

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
        
        /// <summary>
        /// This <see cref="Realm"/> will start managing a <see cref="RealmObject"/> which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="RealmObject"/> but also have been processed by the Fody weaver,
        /// so it has persistent properties.
        /// </typeparam>
        /// <param name="obj">Must be a standalone object, <c>null</c> not allowed.</param>
        /// <param name="update">If <c>true</c>, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the object is already managed by this <see cref="Realm"/>, this method does nothing.
        /// This method modifies the object in-place, meaning that after it has run, <c>obj</c> will be managed. 
        /// Returning it is just meant as a convenience to enable fluent syntax scenarios.
        /// Cyclic graphs (<c>Parent</c> has <c>Child</c> that has a <c>Parent</c>) will result in undefined behavior.
        /// You have to break the cycle manually and assign relationships after all object have been managed.
        /// </remarks>
        /// <returns>The passed object, so that you can write <c>var person = realm.Add(new Person { Id = 1 });</c></returns>
        public T Add<T>(T obj, bool update = false) where T : RealmObject
        {
            ThrowIfDisposed();

            // This is not obsoleted because the compiler will always pick it for specific types, generating a bunch of warnings
            AddInternal(obj, typeof(T), update);
            return obj;
        }

        /// <summary>
        /// This <see cref="Realm"/> will start managing a <see cref="RealmObject"/> which has been created as a standalone object.
        /// </summary>
        /// <param name="obj">Must be a standalone object, <c>null</c> not allowed.</param>
        /// <param name="update">If <c>true</c>, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the object is already managed by this <see cref="Realm"/>, this method does nothing.
        /// This method modifies the object in-place, meaning that after it has run, <c>obj</c> will be managed.
        /// Cyclic graphs (<c>Parent</c> has <c>Child</c> that has a <c>Parent</c>) will result in undefined behavior.
        /// You have to break the cycle manually and assign relationships after all object have been managed.
        /// </remarks>
        /// <returns>The passed object.</returns>
        public RealmObject Add(RealmObject obj, bool update = false)
        {
            ThrowIfDisposed();

            AddInternal(obj, obj?.GetType(), update);
            return obj;
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

            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(objectType.Name, out metadata))
            {
                throw new ArgumentException($"The class {objectType.Name} is not in the limited set of classes for this realm");
            }

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

            var setPrimaryKey = false;
            if (objectPtr == IntPtr.Zero)
            {
                objectPtr = metadata.Table.AddEmptyObject(SharedRealmHandle);
                setPrimaryKey = true;
            }

            var objectHandle = CreateObjectHandle(objectPtr, SharedRealmHandle);

            obj._SetOwner(this, objectHandle, metadata);
            metadata.Helper.CopyToRealm(obj, update, setPrimaryKey);
        }

        /// <summary>
        /// Factory for a write <see cref="Transaction"/>. Essential object to create scope for updates.
        /// </summary>
        /// <example>
        /// <code>
        /// using (var trans = realm.BeginWrite()) 
        /// { 
        ///     realm.Add(new Dog
        ///     {
        ///         Name = "Rex"
        ///     });
        ///     trans.Commit();
        /// }
        /// </code>
        /// </example>
        /// <returns>A transaction in write mode, which is required for any creation or modification of objects persisted in a <see cref="Realm"/>.</returns>
        public Transaction BeginWrite()
        {
            ThrowIfDisposed();

            return new Transaction(this);
        }

        /// <summary>
        /// Execute an action inside a temporary <see cref="Transaction"/>. If no exception is thrown, the <see cref="Transaction"/> 
        /// will be committed.
        /// </summary>
        /// <remarks>
        /// Creates its own temporary <see cref="Transaction"/> and commits it after running the lambda passed to <c>action</c>. 
        /// Be careful of wrapping multiple single property updates in multiple <see cref="Write"/> calls. 
        /// It is more efficient to update several properties or even create multiple objects in a single <see cref="Write"/>,
        /// unless you need to guarantee finer-grained updates.
        /// </remarks>
        /// <example>
        /// <code>
        /// realm.Write(() => 
        /// {
        ///     realm.Add(new Dog
        ///     {
        ///         Name = "Eddie",
        ///         Age = 5
        ///     });
        /// });
        /// </code>
        /// </example>
        /// <param name="action">
        /// Action to perform inside a <see cref="Transaction"/>, creating, updating or removing objects.
        /// </param>
        public void Write(Action action)
        {
            ThrowIfDisposed();

            using (var transaction = BeginWrite())
            {
                action();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Execute an action inside a temporary <see cref="Transaction"/> on a worker thread, <b>if</b> called from UI thread. If no exception is thrown,
        /// the <see cref="Transaction"/> will be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this Realm on a worker thread and executes <c>action</c> inside a write <see cref="Transaction"/>.
        /// <see cref="Realm"/>s and <see cref="RealmObject"/>s are thread-affine, so capturing any such objects in 
        /// the <c>action</c> delegate will lead to errors if they're used on the worker thread. Note that it checks the
        /// <see cref="SynchronizationContext"/> to determine if <c>Current</c> is null, as a test to see if you are on the UI thread
        /// and will otherwise just call Write without starting a new thread. So if you know you are invoking from a worker thread, just call Write instead.
        /// </remarks>
        /// <example>
        /// <code>
        /// await realm.WriteAsync(tempRealm =&gt; 
        /// {
        ///     var pongo = tempRealm.All&lt;Dog&gt;().Single(d =&gt; d.Name == "Pongo");
        ///     var missis = tempRealm.All&lt;Dog&gt;().Single(d =&gt; d.Name == "Missis");
        ///     for (var i = 0; i &lt; 15; i++)
        ///     {
        ///         tempRealm.Add(new Dog
        ///         {
        ///             Breed = "Dalmatian",
        ///             Mum = missis,
        ///             Dad = pongo
        ///         });
        ///     }
        /// });
        /// </code>
        /// <b>Note</b> that inside the action, we use <c>tempRealm</c>.
        /// </example>
        /// <param name="action">
        /// Action to perform inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task WriteAsync(Action<Realm> action)
        {
            ThrowIfDisposed();

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // If we are on UI thread will be set but often also set on long-lived workers to use Post back to UI thread.
            if (SynchronizationContext.Current != null)
            {
                await Task.Run(() =>
                {
                    using (var realm = GetInstance(Config))
                    {
                        realm.Write(() => action(realm));
                    }
                });

                Refresh();
            }
            else
            {
                // If running on background thread, execute synchronously.
                Write(() => action(this));
            }
        }

        /// <summary>
        /// Update the <see cref="Realm"/> instance and outstanding objects to point to the most recent persisted version.
        /// </summary>
        /// <returns>
        /// Whether the <see cref="Realm"/> had any updates. Note that this may return true even if no data has actually changed.
        /// </returns>
        public bool Refresh()
        {
            ThrowIfDisposed();

            return SharedRealmHandle.Refresh();
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of class T, in this <see cref="Realm"/>.</returns>
        public IQueryable<T> All<T>() where T : RealmObject
        {
            ThrowIfDisposed();

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
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of className, in this realm.</returns>
        public IQueryable<dynamic> All(string className)
        {
            ThrowIfDisposed();

            RealmObject.Metadata metadata;
            if (!Metadata.TryGetValue(className, out metadata))
            {
                throw new ArgumentException($"The class {className} is not in the limited set of classes for this realm");
            }

            return new RealmResults<RealmObject>(this, metadata, true);
        }

        #region Quick Find using primary key

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <param name="primaryKey">
        /// Primary key to be matched exactly, same as an == search.
        /// An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.
        /// </param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T Find<T>(long? primaryKey) where T : RealmObject
        {
            ThrowIfDisposed();

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
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T Find<T>(string primaryKey) where T : RealmObject
        {
            ThrowIfDisposed();

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
        /// <param name="primaryKey">
        /// Primary key to be matched exactly, same as an == search. 
        /// An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.
        /// </param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public RealmObject Find(string className, long? primaryKey)
        {
            ThrowIfDisposed();

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
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public RealmObject Find(string className, string primaryKey)
        {
            ThrowIfDisposed();

            var metadata = Metadata[className];
            var objectPtr = metadata.Table.Find(SharedRealmHandle, primaryKey);
            if (objectPtr == IntPtr.Zero)
            {
                return null;
            }

            return MakeObject(metadata, objectPtr);
        }

        #endregion Quick Find using primary key

        #region Thread Handover

        /// <summary>
        /// Returns the same object as the one referenced when the <see cref="ThreadSafeReference.Object{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="RealmObject"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the reference.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="RealmObject"/> resolved for the current thread or <c>null</c>
        /// if the object has been deleted after the reference was created.
        /// </returns>
        public T ResolveReference<T>(ThreadSafeReference.Object<T> reference) where T : RealmObject
        {
            var objectPtr = SharedRealmHandle.ResolveReference(reference);
            var objectHandle = CreateObjectHandle(objectPtr, SharedRealmHandle);

            if (!objectHandle.IsValid)
            {
                return null;
            }

            return (T)MakeObject(reference.Metadata, objectHandle);
        }

        /// <summary>
        /// Returns the same collection as the one referenced when the <see cref="ThreadSafeReference.List{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="IList{T}"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the collection.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="IList{T}"/> resolved for the current thread or <c>null</c>
        /// if the list's parent object has been deleted after the reference was created.
        /// </returns>
        public IList<T> ResolveReference<T>(ThreadSafeReference.List<T> reference) where T : RealmObject
        {
            var listPtr = SharedRealmHandle.ResolveReference(reference);
            var listHandle = new ListHandle(SharedRealmHandle);
            listHandle.SetHandle(listPtr);
            if (!listHandle.IsValid)
            {
                return null;
            }

            return new RealmList<T>(this, listHandle, reference.Metadata);
        }

        /// <summary>
        /// Returns the same query as the one referenced when the <see cref="ThreadSafeReference.Query{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="IQueryable{T}"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the query.</typeparam>
        /// <returns>A thread-confined instance of the original <see cref="IQueryable{T}"/> resolved for the current thread.</returns>
        public IQueryable<T> ResolveReference<T>(ThreadSafeReference.Query<T> reference) where T : RealmObject
        {
            var resultsPtr = SharedRealmHandle.ResolveReference(reference);
            var resultsHandle = new ResultsHandle();
            resultsHandle.SetHandle(resultsPtr);
            return new RealmResults<T>(this, resultsHandle, reference.Metadata);
        }

        #endregion

        /// <summary>
        /// Removes a persistent object from this Realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this Realm.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">If <c>obj</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If you pass a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            ThrowIfDisposed();

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
        /// Remove objects matching a query from the Realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <c>range</c> is not the result of <see cref="All{T}"/> or subsequent LINQ filtering.
        /// </exception>
        /// <exception cref="ArgumentNullException">If <c>range</c> is <c>null</c>.</exception>
        public void RemoveRange<T>(IQueryable<T> range) where T : RealmObject
        {
            ThrowIfDisposed();

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
        /// Remove all objects of a type from the Realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the type T is not part of the limited set of classes in this Realm's <see cref="Schema"/>.
        /// </exception>
        public void RemoveAll<T>() where T : RealmObject
        {
            ThrowIfDisposed();

            RemoveRange(All<T>());
        }

        /// <summary>
        /// Remove all objects of a type from the Realm.
        /// </summary>
        /// <param name="className">Type of the objects to remove as defined in the schema.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If you pass <c>className</c> that does not belong to this Realm's schema.
        /// </exception>
        public void RemoveAll(string className)
        {
            ThrowIfDisposed();

            var query = (RealmResults<RealmObject>)All(className);
            query.ResultsHandle.Clear(SharedRealmHandle);
        }

        /// <summary>
        /// Remove all objects of all types managed by this Realm.
        /// </summary>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        public void RemoveAll()
        {
            ThrowIfDisposed();

            foreach (var metadata in Metadata.Values)
            {
                var resultsHandle = MakeResultsForTable(metadata);
                resultsHandle.Clear(SharedRealmHandle);
            }
        }

        #region Transactions

        internal void DrainTransactionQueue()
        {
            _state.DrainTransactionQueue();
        }

        internal void ExecuteOutsideTransaction(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (IsInTransaction)
            {
                _state.AfterTransactionQueue.Enqueue(action);
            }
            else
            {
                action();
            }
        }

        #endregion

        #region Obsolete methods

        /// <summary>
        /// <b>Deprecated</b> Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="id">Id to be matched exactly, same as an == search. <see cref="Int64"/> argument works for all integer properties supported as PrimaryKey.</param>
        /// <returns>Null or an object matching the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public T ObjectForPrimaryKey<T>(long id) where T : RealmObject
        {
            return Find<T>(id);
        }

        /// <summary>
        /// <b>Deprecated</b> Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matching the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public T ObjectForPrimaryKey<T>(string id) where T : RealmObject
        {
            return Find<T>(id);
        }

        /// <summary>
        /// <b>Deprecated</b> Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matching the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public RealmObject ObjectForPrimaryKey(string className, long id)
        {
            return Find(className, id);
        }

        /// <summary>
        /// <b>Deprecated</b> Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
        /// </summary>
        /// <param name="className">Name of class in dynamic situation.</param>
        /// <param name="id">Id to be matched exactly, same as an == search.</param>
        /// <returns>Null or an object matching the id.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class lacks an [PrimaryKey].</exception>
        [Obsolete("This method has been renamed. Use Find for the same results.")]
        public RealmObject ObjectForPrimaryKey(string className, string id)
        {
            return Find(className, id);
        }

        /// <summary>
        /// <b>Deprecated</b> This realm will start managing a RealmObject which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processed by the Fody weaver, so it has persistent properties.</typeparam>
        /// <param name="obj">Must be a standalone object, null not allowed.</param>
        /// <param name="update">If true, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm.</exception>
        [Obsolete("This method has been renamed. Use Add for the same results.")]
        public void Manage<T>(T obj, bool update = false) where T : RealmObject
        {
            Add(obj, update);
        }

        /// <summary>
        /// <b>Deprecated</b> Closes the Realm if not already closed. Safe to call repeatedly.
        /// Note that this will close the file. Other references to the same database
        /// on the same thread will be invalidated.
        /// </summary>
        [Obsolete("This method has been deprecated. Instead, dispose the realm to close it.")]
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// <b>Deprecated</b> Factory for a managed object in a realm. Only valid within a write <see cref="Transaction"/>.
        /// </summary>
        /// <remarks>Scheduled for removal in the next major release, as it is dangerous to call CreateObject and then assign a PrimaryKey.</remarks>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <returns>An object which is already managed.</returns>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        [Obsolete("Please create an object with new and pass to Add instead")]
        public T CreateObject<T>() where T : RealmObject, new()
        {
            ThrowIfDisposed();

            RealmObject.Metadata metadata;
            var ret = CreateObject(typeof(T).Name, out metadata);
            if (typeof(T) != metadata.Schema.Type)
            {
                throw new ArgumentException($"The type {typeof(T).FullName} does not match the original type the schema was created for - {metadata.Schema.Type?.FullName}");
            }

            return (T)ret;
        }

        #endregion

        private class RealmState
        {
            #region static

            [NativeCallback(typeof(NativeCommon.NotifyRealmCallback))]
            public static void NotifyRealmChanged(IntPtr stateHandle)
            {
                var gch = GCHandle.FromIntPtr(stateHandle);
                ((RealmState)gch.Target).NotifyChanged(EventArgs.Empty);
            }

            #endregion

            private readonly List<WeakReference<Realm>> weakRealms = new List<WeakReference<Realm>>();

            public readonly GCHandle GCHandle;
            public readonly Queue<Action> AfterTransactionQueue = new Queue<Action>();
            
            public RealmState()
            {
                // this is freed in a native callback when the CSharpBindingContext is destroyed
                GCHandle = GCHandle.Alloc(this);
            }

            private void NotifyChanged(EventArgs e)
            {
                foreach (var realm in GetLiveRealms())
                {
                    realm.NotifyChanged(e);
                }
            }

            public void AddRealm(Realm realm)
            {
                // We only want to have each realm once. That should be the case as AddRealm
                // is only called in the Realm ctor, but let's check just in case.
                Debug.Assert(!weakRealms.Any(r =>
                {
                    Realm other;
                    return r.TryGetTarget(out other) && ReferenceEquals(realm, other);
                }), "Trying to add a duplicate Realm to the RealmState.");

                weakRealms.Add(new WeakReference<Realm>(realm));
            }

            public void RemoveRealm(Realm realm)
            {
                var weakRealm = weakRealms.SingleOrDefault(r =>
                {
                    Realm other;
                    return r.TryGetTarget(out other) && ReferenceEquals(realm, other);
                });
                weakRealms.Remove(weakRealm);

                if (!weakRealms.Any())
                {
                    realm.SharedRealmHandle.CloseRealm();
                }
            }

            private IEnumerable<Realm> GetLiveRealms()
            {
                var realms = new List<Realm>();

                weakRealms.RemoveAll(r =>
                {
                    Realm realm;
                    if (r.TryGetTarget(out realm))
                    {
                        realms.Add(realm);
                        return false;
                    }

                    return true;
                });

                return realms;
            }

            internal void DrainTransactionQueue()
            {
                while (AfterTransactionQueue.Count > 0)
                {
                    var action = AfterTransactionQueue.Dequeue();
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        foreach (var realm in GetLiveRealms())
                        {
                            realm.NotifyError(ex);
                        }
                    }
                }
            }
        }
    }
}
