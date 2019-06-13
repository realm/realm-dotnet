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
using Realms.Helpers;
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

        // This is imperfect solution because having a realm open on a different thread wouldn't prevent deleting the file.
        // Theoretically we could use trackAllValues: true, but that would create locking issues.
        private static readonly ThreadLocal<IDictionary<string, WeakReference<State>>> _states = new ThreadLocal<IDictionary<string, WeakReference<State>>>(DictionaryConstructor<string, WeakReference<State>>);

        // TODO: due to a Mono bug, this needs to be a function rather than a lambda
        private static IDictionary<T, U> DictionaryConstructor<T, U>() => new Dictionary<T, U>();

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
            return GetInstance(new RealmConfiguration(databasePath));
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

        /// <summary>
        /// Factory for asynchronously obtaining a <see cref="Realm"/> instance.
        /// </summary>
        /// <remarks>
        /// If the configuration points to a remote realm belonging to a Realm Object Server
        /// the realm will be downloaded and fully synchronized with the server prior to the completion
        /// of the returned Task object.
        /// Otherwise this method behaves identically to <see cref="GetInstance(RealmConfigurationBase)"/>
        /// and immediately returns a completed Task.
        /// </remarks>
        /// <returns>A <see cref="Task{Realm}"/> that is completed once the remote realm is fully synchronized or immediately if it's a local realm.</returns>
        /// <param name="config">A configuration object that describes the realm.</param>
        public static Task<Realm> GetInstanceAsync(RealmConfigurationBase config = null)
        {
            if (config == null)
            {
                config = RealmConfiguration.DefaultConfiguration;
            }

            RealmSchema schema;
            if (config.ObjectClasses != null)
            {
                schema = RealmSchema.CreateSchemaForClasses(config.ObjectClasses);
            }
            else if (config.IsDynamic)
            {
                schema = RealmSchema.Empty;
            }
            else
            {
                schema = RealmSchema.Default;
            }

            return config.CreateRealmAsync(schema);
        }

        internal static Realm GetInstance(RealmConfigurationBase config, RealmSchema schema)
        {
            Argument.NotNull(config, nameof(config));

            if (schema == null)
            {
                if (config.ObjectClasses != null)
                {
                    schema = RealmSchema.CreateSchemaForClasses(config.ObjectClasses);
                }
                else
                {
                    schema = config.IsDynamic ? RealmSchema.Empty : RealmSchema.Default;
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
            var fullpath = configuration.DatabasePath;
            if (IsRealmOpen(fullpath))
            {
                throw new RealmPermissionDeniedException("Unable to delete Realm because it is still open.");
            }

            File.Delete(fullpath);
            File.Delete(fullpath + ".log_a");  // eg: name at end of path is EnterTheMagic.realm.log_a
            File.Delete(fullpath + ".log_b");
            File.Delete(fullpath + ".log");
            File.Delete(fullpath + ".lock");
            File.Delete(fullpath + ".note");

            if (Directory.Exists($"{fullpath}.management"))
            {
                Directory.Delete($"{fullpath}.management", recursive: true);
            }
        }

        private static bool IsRealmOpen(string path)
        {
            return _states.Value.TryGetValue(path, out var reference) &&
                   reference.TryGetTarget(out var state) &&
                   state.GetLiveRealms().Any();
        }

        #endregion

        private State _state;

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly Dictionary<string, RealmObject.Metadata> Metadata;

        /// <summary>
        /// Gets a value indicating whether there is an active <see cref="Transaction"/> is in transaction.
        /// </summary>
        /// <value><c>true</c> if is in transaction; otherwise, <c>false</c>.</value>
        public bool IsInTransaction
        {
            get
            {
                ThrowIfDisposed();

                return SharedRealmHandle.IsInTransaction();
            }
        }

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
            Config = config;

            State state = null;

            if (config.EnableCache)
            {
                var statePtr = sharedRealmHandle.GetManagedStateHandle();
                if (statePtr != IntPtr.Zero)
                {
                    state = GCHandle.FromIntPtr(statePtr).Target as State;
                }
            }

            if (state == null)
            {
                state = new State();
                sharedRealmHandle.SetManagedStateHandle(GCHandle.ToIntPtr(state.GCHandle));

                if (config.EnableCache)
                {
                    _states.Value[config.DatabasePath] = new WeakReference<State>(state);
                }
            }

            state.AddRealm(this);

            _state = state;

            SharedRealmHandle = sharedRealmHandle;
            Metadata = schema.ToDictionary(t => t.Name, CreateRealmObjectMetadata);
            Schema = schema;
        }

        private RealmObject.Metadata CreateRealmObjectMetadata(ObjectSchema schema)
        {
            var table = SharedRealmHandle.GetTable(schema.Name);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null && !Config.IsDynamic)
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
        public void Dispose()
        {
            if (!IsClosed)
            {
                // only mutate the state on explicit disposal
                // otherwise we do so on the finalizer thread
                if (Config.EnableCache && _state.RemoveRealm(this))
                {
                    _states.Value.Remove(Config.DatabasePath);
                }

                _state = null;
                SharedRealmHandle.Close();  // Note: this closes the *handle*, it does not trigger realm::Realm::close().
            }
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

            Argument.NotNull(other, nameof(other));

            return SharedRealmHandle == other.SharedRealmHandle || SharedRealmHandle.IsSameInstance(other.SharedRealmHandle);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            ThrowIfDisposed();

            return (int)((long)SharedRealmHandle.DangerousGetHandle() % int.MaxValue);
        }

        /// <summary>
        /// Factory for a managed object in a realm. Only valid within a write <see cref="Transaction"/>.
        /// </summary>
        /// <returns>A dynamically-accessed Realm object.</returns>
        /// <param name="className">The type of object to create as defined in the schema.</param>
        /// <param name="primaryKey">
        /// The primary key of object to be created. If the object doesn't have primary key defined, this argument
        /// is ignored.
        /// </param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If you pass <c>null</c> for an object with string primary key.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If you pass <c>primaryKey</c> with type that is different from the type, defined in the schema.
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
        /// object will be an instance of a user-defined class.
        /// </para>
        /// </remarks>
        public dynamic CreateObject(string className, object primaryKey)
        {
            ThrowIfDisposed();

            return CreateObject(className, primaryKey, out var _);
        }

        private RealmObject CreateObject(string className, object primaryKey, out RealmObject.Metadata metadata)
        {
            Argument.Ensure(Metadata.TryGetValue(className, out metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));

            var result = metadata.Helper.CreateInstance();

            ObjectHandle objectHandle;
            var pkProperty = metadata.Schema.PrimaryKeyProperty;
            if (pkProperty.HasValue)
            {
                objectHandle = SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty.Value, primaryKey, metadata.Table, className, update: false, isNew: out var _);
            }
            else
            {
                objectHandle = SharedRealmHandle.CreateObject(metadata.Table);
            }

            result._SetOwner(this, objectHandle, metadata);
            result.OnManaged();
            return result;
        }

        internal RealmObject MakeObject(RealmObject.Metadata metadata, ObjectHandle objectHandle)
        {
            var ret = metadata.Helper.CreateInstance();
            ret._SetOwner(this, objectHandle, metadata);
            ret.OnManaged();
            return ret;
        }

        internal ResultsHandle MakeResultsForQuery(QueryHandle builtQuery, SortDescriptorBuilder optionalSortDescriptorBuilder)
        {
            var resultsPtr = IntPtr.Zero;
            if (optionalSortDescriptorBuilder == null)
            {
                return builtQuery.CreateResults(SharedRealmHandle);
            }

            return builtQuery.CreateSortedResults(SharedRealmHandle, optionalSortDescriptorBuilder);
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
            Argument.NotNull(obj, nameof(obj));
            Argument.NotNull(objectType, nameof(objectType));

            if (obj.IsManaged)
            {
                if (IsSameInstance(obj.Realm))
                {
                    // Already managed by this realm, so nothing to do.
                    return;
                }

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }

            var objectName = objectType.GetTypeInfo().GetMappedOrOriginalName();
            Argument.Ensure(Metadata.TryGetValue(objectName, out var metadata), $"The class {objectType.Name} is not in the limited set of classes for this realm", nameof(objectType));

            ObjectHandle objectHandle;
            bool isNew;
            if (metadata.Helper.TryGetPrimaryKeyValue(obj, out var primaryKey))
            {
                var pkProperty = metadata.Schema.PrimaryKeyProperty.Value;
                objectHandle = SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty, primaryKey, metadata.Table, objectName, update, out isNew);
            }
            else
            {
                isNew = true; // Objects without PK are always new
                objectHandle = SharedRealmHandle.CreateObject(metadata.Table);
            }

            obj._SetOwner(this, objectHandle, metadata);

            // If an object is newly created, we don't need to invoke setters of properties with default values.
            metadata.Helper.CopyToRealm(obj, update, isNew);
            obj.OnManaged();
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
        public Task WriteAsync(Action<Realm> action)
        {
            // Can't use async/await due to mono inliner bugs
            ThrowIfDisposed();

            Argument.NotNull(action, nameof(action));

            // If we are on UI thread will be set but often also set on long-lived workers to use Post back to UI thread.
            if (AsyncHelper.HasValidContext)
            {
                async Task doWorkAsync()
                {
                    await Task.Run(() =>
                    {
                        using (var realm = GetInstance(Config))
                        {
                            realm.Write(() => action(realm));
                        }
                    });
                    var didRefresh = await RefreshAsync();

                    // TODO: figure out why this assertion fails in `AsyncTests.AsyncWrite_ShouldExecuteOnWorkerThread`
                    // System.Diagnostics.Debug.Assert(didRefresh, "Expected RefreshAsync to return true.");
                }
                return doWorkAsync();
            }
            else
            {
                // If running on background thread, execute synchronously.
                Write(() => action(this));
                return Task.CompletedTask;
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
        /// Asynchronously wait for the <see cref="Realm"/> instance and outstanding objects to get updated
        /// to point to the most recent persisted version.
        /// </summary>
        /// <remarks>
        /// On worker threads (where the SynchronizationContext) is null, this will call the blocking <see cref="Refresh"/>
        /// method instead. On the main thread (or other threads that have SynchronizationContext), this will wait until
        /// the instance automatically updates to resolve the task. Note that you must keep a reference to the Realm
        /// until the returned task is resolved.
        /// </remarks>
        /// <returns>
        /// Whether the <see cref="Realm"/> had any updates. Note that this may return true even if no data has actually changed.
        /// </returns>
        public Task<bool> RefreshAsync()
        {
            if (!SharedRealmHandle.HasChanged())
            {
                return Task.FromResult(false);
            }

            if (!AsyncHelper.HasValidContext)
            {
                return Task.FromResult(Refresh());
            }

            var tcs = new TaskCompletionSource<bool>();

            RealmChanged += handler;
            return tcs.Task;

            void handler(object sender, EventArgs e)
            {
                ((Realm)sender).RealmChanged -= handler;
                tcs.TrySetResult(true);
            }
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
            Argument.Ensure(
                Metadata.TryGetValue(type.GetTypeInfo().GetMappedOrOriginalName(), out var metadata) && metadata.Schema.Type.AsType() == type,
                $"The class {type.Name} is not in the limited set of classes for this realm", nameof(T));

            return new RealmResults<T>(this, metadata);
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

            Argument.Ensure(Metadata.TryGetValue(className, out var metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));

            return new RealmResults<RealmObject>(this, metadata);
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

            var metadata = Metadata[typeof(T).GetTypeInfo().GetMappedOrOriginalName()];
            if (metadata.Table.TryFind(SharedRealmHandle, primaryKey, out var objectHandle))
            {
                return (T)MakeObject(metadata, objectHandle);
            }

            return null;
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

            var metadata = Metadata[typeof(T).GetTypeInfo().GetMappedOrOriginalName()];
            if (metadata.Table.TryFind(SharedRealmHandle, primaryKey, out var objectHandle))
            {
                return (T)MakeObject(metadata, objectHandle);
            }

            return null;
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
            if (metadata.Table.TryFind(SharedRealmHandle, primaryKey, out var objectHandle))
            {
                return MakeObject(metadata, objectHandle);
            }

            return null;
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
            if (metadata.Table.TryFind(SharedRealmHandle, primaryKey, out var objectHandle))
            {
                return MakeObject(metadata, objectHandle);
            }

            return null;
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
            var objectHandle = new ObjectHandle(SharedRealmHandle, objectPtr);

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
        /// <typeparam name="T">The type of the objects, contained in the collection.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="IList{T}"/> resolved for the current thread or <c>null</c>
        /// if the list's parent object has been deleted after the reference was created.
        /// </returns>
        public IList<T> ResolveReference<T>(ThreadSafeReference.List<T> reference)
        {
            var listPtr = SharedRealmHandle.ResolveReference(reference);
            var listHandle = new ListHandle(SharedRealmHandle, listPtr);
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
            var resultsHandle = new ResultsHandle(SharedRealmHandle, resultsPtr);
            return new RealmResults<T>(this, reference.Metadata, resultsHandle);
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

            Argument.NotNull(obj, nameof(obj));
            Argument.Ensure(obj.IsManaged, "Object is not managed by Realm, so it cannot be removed.", nameof(obj));

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

            Argument.NotNull(range, nameof(range));
            Argument.Ensure(range is RealmResults<T>, "range should be the return value of .All or a LINQ query applied to it.", nameof(range));

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
                var resultsHandle = metadata.Table.CreateResults(SharedRealmHandle);
                resultsHandle.Clear(SharedRealmHandle);
                resultsHandle.Close();
            }
        }

        /// <summary>
        /// Writes a compacted copy of the Realm to the path in the specified config. If the configuration object has
        /// non-null <see cref="RealmConfigurationBase.EncryptionKey"/>, the copy will be encrypted with that key.
        /// </summary>
        /// <remarks>
        /// The destination file cannot already exist.
        /// <para/>
        /// If this is called from within a transaction it writes the current data, and not the data as it was when
        /// the last transaction was committed.
        /// </remarks>
        /// <param name="config">Configuration, specifying the path and optionally the encryption key for the copy.</param>
        public void WriteCopy(RealmConfigurationBase config)
        {
            SharedRealmHandle.WriteCopy(config.DatabasePath, config.EncryptionKey);
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

        internal class State
        {
            private readonly List<WeakReference<Realm>> _weakRealms = new List<WeakReference<Realm>>();

            public readonly GCHandle GCHandle;
            public readonly Queue<Action> AfterTransactionQueue = new Queue<Action>();

            public State()
            {
                // this is freed in a native callback when the CSharpBindingContext is destroyed
                GCHandle = GCHandle.Alloc(this);
            }

            internal void NotifyChanged(EventArgs e)
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
                Debug.Assert(!_weakRealms.Any(r =>
                {
                    return r.TryGetTarget(out var other) && ReferenceEquals(realm, other);
                }), "Trying to add a duplicate Realm to the RealmState.");

                _weakRealms.Add(new WeakReference<Realm>(realm));
            }

            public bool RemoveRealm(Realm realm)
            {
                var weakRealm = _weakRealms.SingleOrDefault(r =>
                {
                    return r.TryGetTarget(out var other) && ReferenceEquals(realm, other);
                });
                _weakRealms.Remove(weakRealm);

                if (!_weakRealms.Any())
                {
                    realm.SharedRealmHandle.CloseRealm();
                    return true;
                }

                return false;
            }

            public IEnumerable<Realm> GetLiveRealms()
            {
                var realms = new List<Realm>();

                _weakRealms.RemoveAll(r =>
                {
                    if (r.TryGetTarget(out var realm))
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
