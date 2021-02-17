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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Helpers;
using Realms.Native;
using Realms.Schema;
using Realms.Sync;

namespace Realms
{
    /// <summary>
    /// A Realm instance (also referred to as a Realm) represents a Realm database.
    /// </summary>
    /// <remarks>
    /// <b>Warning</b>: Non-frozen Realm instances are not thread safe and can not be shared across threads.
    /// You must call <see cref="GetInstance(RealmConfigurationBase)"/> on each thread in which you want to interact with the Realm.
    /// </remarks>
    public class Realm : IDisposable
    {
        #region static

        // This is imperfect solution because having a realm open on a different thread wouldn't prevent deleting the file.
        // Theoretically we could use trackAllValues: true, but that would create locking issues.
        private static readonly ThreadLocal<IDictionary<string, WeakReference<State>>> _states = new ThreadLocal<IDictionary<string, WeakReference<State>>>(DictionaryConstructor<string, WeakReference<State>>);

        // TODO: due to a Mono bug, this needs to be a function rather than a lambda
        private static IDictionary<TKey, TValue> DictionaryConstructor<TKey, TValue>() => new Dictionary<TKey, TValue>();

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
        /// If the configuration is <see cref="SyncConfiguration"/>, the realm will be downloaded and fully
        /// synchronized with the server prior to the completion of the returned Task object.
        /// Otherwise this method will perform any migrations on a background thread before returning an
        /// opened instance to the calling thread.
        /// </remarks>
        /// <returns>
        /// An awaitable <see cref="Task{T}"/> that is completed once the remote realm is fully synchronized or
        /// after migrations are executed if it's a local realm.
        /// </returns>
        /// <param name="config">A configuration object that describes the realm.</param>
        /// <param name="cancellationToken">An optional cancellation token that can be used to cancel the work.</param>
        public static Task<Realm> GetInstanceAsync(RealmConfigurationBase config = null, CancellationToken cancellationToken = default)
        {
            if (config == null)
            {
                config = RealmConfiguration.DefaultConfiguration;
            }

            RealmSchema schema = GetSchema(config);

            return config.CreateRealmAsync(schema, cancellationToken);
        }

        internal static Realm GetInstance(RealmConfigurationBase config, RealmSchema schema)
        {
            Argument.NotNull(config, nameof(config));

            if (schema == null)
            {
                schema = GetSchema(config);
            }

            return config.CreateRealm(schema);
        }

        internal static RealmSchema GetSchema(RealmConfigurationBase config)
        {
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

            return schema;
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
            using var realm = GetInstance(config);
            return realm.SharedRealmHandle.Compact();
        }

        /// <summary>
        /// Deletes all the files associated with a realm.
        /// </summary>
        /// <param name="configuration">A <see cref="RealmConfigurationBase"/> which supplies the realm path.</param>
        public static void DeleteRealm(RealmConfigurationBase configuration)
        {
            Argument.NotNull(configuration, nameof(configuration));

            var fullpath = configuration.DatabasePath;
            if (IsRealmOpen(fullpath))
            {
                throw new RealmPermissionDeniedException("Unable to delete Realm because it is still open.");
            }

            var filesToDelete = new[] { string.Empty, ".log_a", ".log_b", ".log", ".lock", ".note" }
                .Select(ext => fullpath + ext)
                .Where(File.Exists);

            foreach (var file in filesToDelete)
            {
                File.Delete(file);
            }

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

        #endregion static

        [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "A State can be shared between multiple Realm instances. It is disposed when the native instance and its BindingContext is destroyed")]
        private State _state;

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly RealmMetadata Metadata;

        /// <summary>
        /// Gets an object encompassing the dynamic API for this Realm instance.
        /// </summary>
        [Preserve]
        public Dynamic DynamicApi { get; }

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
        /// Gets a value indicating whether this Realm is frozen. Frozen Realms are immutable
        /// and will not update when writes are made to the database. Unlike live Realms, frozen
        /// Realms can be used across threads.
        /// </summary>
        /// <see cref="Freeze"/>
        public bool IsFrozen { get; }

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

            if (config.EnableCache)
            {
                var statePtr = sharedRealmHandle.GetManagedStateHandle();
                if (statePtr != IntPtr.Zero)
                {
                    _state = GCHandle.FromIntPtr(statePtr).Target as State;
                }
            }

            if (_state == null)
            {
                _state = new State();
                sharedRealmHandle.SetManagedStateHandle(GCHandle.ToIntPtr(_state.GCHandle));

                if (config.EnableCache)
                {
                    _states.Value[config.DatabasePath] = new WeakReference<State>(_state);
                }
            }

            _state.AddRealm(this);

            SharedRealmHandle = sharedRealmHandle;
            Metadata = new RealmMetadata(schema.Select(CreateRealmObjectMetadata));
            Schema = schema;
            IsFrozen = SharedRealmHandle.IsFrozen;
            DynamicApi = new Dynamic(this);
        }

        private RealmObjectBase.Metadata CreateRealmObjectMetadata(ObjectSchema schema)
        {
            var tableKey = SharedRealmHandle.GetTableKey(schema.Name);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null && !Config.IsDynamic)
            {
                var wovenAtt = schema.Type.GetCustomAttribute<WovenAttribute>();
                if (wovenAtt == null)
                {
                    throw new RealmException($"Fody not properly installed. {schema.Type.FullName} is a RealmObjectBase but has not been woven.");
                }

                helper = (Weaving.IRealmObjectHelper)Activator.CreateInstance(wovenAtt.HelperType);
            }
            else
            {
                helper = DynamicRealmObjectHelper.Instance(schema.IsEmbedded);
            }

            var initPropertyMap = new Dictionary<string, IntPtr>(schema.Count);
            var persistedProperties = -1;
            var computedProperties = -1;

            // We're taking advantage of the fact OS keeps property indices aligned
            // with the property indices in ObjectSchema
            foreach (var prop in schema)
            {
                var index = prop.Type.IsComputed() ? ++computedProperties : ++persistedProperties;
                initPropertyMap[prop.Name] = (IntPtr)index;
            }

            return new RealmObjectBase.Metadata(tableKey, helper, initPropertyMap, schema);
        }

        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/>.
        /// </summary>
        /// <param name="sender">The <see cref="Realm"/> which has changed.</param>
        /// <param name="e">Currently an empty argument, in future may indicate more details about the change.</param>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event RealmChangedEventHandler _realmChanged;

        /// <summary>
        /// Triggered when a Realm has changed (i.e. a <see cref="Transaction"/> was committed).
        /// </summary>
        public event RealmChangedEventHandler RealmChanged
        {
            add
            {
                ThrowIfFrozen("It is not possible to add/remove a change listener to a frozen Realm since it never changes.");
                _realmChanged += value;
            }

            remove
            {
                ThrowIfFrozen("It is not possible to add/remove a change listener to a frozen Realm since it never changes.");
                _realmChanged -= value;
            }
        }

        private void NotifyChanged(EventArgs e)
        {
            _realmChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Triggered when a Realm-level exception has occurred.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        internal void NotifyError(Exception ex)
        {
            if (Error == null)
            {
                Console.Error.WriteLine("A realm-level exception has occurred. To handle and react to those, subscribe to the Realm.Error event.");
            }

            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// Gets a value indicating whether the instance has been closed via <see cref="Dispose()"/>. If <c>true</c>, you
        /// should not call methods on that instance.
        /// </summary>
        /// <value><c>true</c> if closed, <c>false</c> otherwise.</value>
        public bool IsClosed => SharedRealmHandle.IsClosed;

        /// <summary>
        /// Disposes the current instance and closes the native Realm if this is the last remaining
        /// instance holding a reference to it.
        /// </summary>
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

        /// <summary>
        /// Returns a frozen (immutable) snapshot of this Realm.
        /// <para/>
        /// A frozen Realm is an immutable snapshot view of a particular version of a
        /// Realm's data. Unlike normal <see cref="Realm"/> instances, it does not live-update to
        /// reflect writes made to the Realm, and can be accessed from any thread. Writing
        /// to a frozen Realm is not allowed, and attempting to begin a write transaction
        /// will throw an exception.
        /// <para/>
        /// All objects and collections read from a frozen Realm will also be frozen.
        /// <para/>
        /// Note: Keeping a large number of frozen Realms with different versions alive can have a negative impact on the filesize
        /// of the underlying database. In order to avoid such a situation it is possible to set <see cref="RealmConfigurationBase.MaxNumberOfActiveVersions"/>.
        /// </summary>
        /// <returns>A frozen <see cref="Realm"/> instance.</returns>
        public Realm Freeze()
        {
            if (IsFrozen)
            {
                return this;
            }

            var handle = SharedRealmHandle.Freeze();
            return new Realm(handle, Config, Schema);
        }

        private void ThrowIfDisposed()
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException(typeof(Realm).FullName, "Cannot access a closed Realm.");
            }
        }

        private void ThrowIfFrozen(string message)
        {
            if (IsFrozen)
            {
                throw new RealmFrozenException(message);
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

        internal RealmObjectBase MakeObject(RealmObjectBase.Metadata metadata, ObjectHandle objectHandle)
        {
            var ret = metadata.Helper.CreateInstance();
            ret.SetOwner(this, objectHandle, metadata);
            ret.OnManaged();
            return ret;
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
        /// </remarks>
        /// <returns>The passed object, so that you can write <c>var person = realm.Add(new Person { Id = 1 });</c>.</returns>
        public T Add<T>(T obj, bool update = false)
            where T : RealmObject
        {
            ThrowIfDisposed();
            Argument.NotNull(obj, nameof(obj));

            // This is not obsoleted because the compiler will always pick it for specific types, generating a bunch of warnings
            AddInternal(obj, typeof(T), update);
            return obj;
        }

        /// <summary>
        /// Add a collection of standalone <see cref="RealmObject"/>s to this <see cref="Realm"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="RealmObject"/> but also have been processed by the Fody weaver,
        /// so it has persistent properties.
        /// </typeparam>
        /// <param name="objs">A collection of <see cref="RealmObject"/> instances that will be added to this <see cref="Realm"/>.</param>
        /// <param name="update">If <c>true</c>, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the collection contains items that are already managed by this <see cref="Realm"/>, they will be ignored.
        /// This method modifies the objects in-place, meaning that after it has run, all items in <c>objs</c> will be managed.
        /// </remarks>
        public void Add<T>(IEnumerable<T> objs, bool update = false)
            where T : RealmObject
        {
            ThrowIfDisposed();
            Argument.NotNull(objs, nameof(objs));
            Argument.Ensure(objs.All(o => o != null), $"{nameof(objs)} must not contain null values.", nameof(objs));

            foreach (var obj in objs)
            {
                AddInternal(obj, typeof(T), update);
            }
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
            Argument.NotNull(obj, nameof(obj));

            AddInternal(obj, obj.GetType(), update);
            return obj;
        }

        internal void ManageEmbedded(EmbeddedObject obj, ObjectHandle handle)
        {
            var objectType = obj.GetType();
            var objectName = objectType.GetTypeInfo().GetMappedOrOriginalName();
            Argument.Ensure(Metadata.TryGetValue(objectName, out var metadata), $"The class {objectType.Name} is not in the limited set of classes for this realm", nameof(obj));

            obj.SetOwner(this, handle, metadata);

            // If an object is newly created, we don't need to invoke setters of properties with default values.
            metadata.Helper.CopyToRealm(obj, update: false, skipDefaults: true);
            obj.OnManaged();
        }

        private void AddInternal(RealmObject obj, Type objectType, bool update)
        {
            if (!ShouldAddNewObject(obj))
            {
                return;
            }

            var objectName = objectType.GetTypeInfo().GetMappedOrOriginalName();
            Argument.Ensure(Metadata.TryGetValue(objectName, out var metadata), $"The class {objectType.Name} is not in the limited set of classes for this realm", nameof(objectType));

            ObjectHandle objectHandle;
            bool isNew;
            if (metadata.Helper.TryGetPrimaryKeyValue(obj, out var primaryKey))
            {
                var pkProperty = metadata.Schema.PrimaryKeyProperty.Value;
                objectHandle = SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty, primaryKey, metadata.TableKey, objectName, update, out isNew);
            }
            else
            {
                isNew = true; // Objects without PK are always new
                objectHandle = SharedRealmHandle.CreateObject(metadata.TableKey);
            }

            obj.SetOwner(this, objectHandle, metadata);

            // If an object is newly created, we don't need to invoke setters of properties with default values.
            metadata.Helper.CopyToRealm(obj, update, isNew);
            obj.OnManaged();
        }

        private bool ShouldAddNewObject(RealmObjectBase obj)
        {
            Argument.NotNull(obj, nameof(obj));

            if (obj.IsManaged)
            {
                if (IsSameInstance(obj.Realm))
                {
                    // Already managed by this realm, so nothing to do.
                    return false;
                }

                throw new RealmObjectManagedByAnotherRealmException("Cannot start to manage an object with a realm when it's already managed by another realm");
            }

            return true;
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
            ThrowIfFrozen("Starting a write transaction on a frozen Realm is not allowed.");

            return new Transaction(this);
        }

        /// <summary>
        /// Execute an action inside a temporary <see cref="Transaction"/>. If no exception is thrown, the <see cref="Transaction"/>
        /// will be committed.
        /// </summary>
        /// <remarks>
        /// Creates its own temporary <see cref="Transaction"/> and commits it after running the lambda passed to <paramref name="action"/>.
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
        /// Action to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        public void Write(Action action)
        {
            Argument.NotNull(action, nameof(action));

            Write(() =>
            {
                action();
                return true;
            });
        }

        /// <summary>
        /// Execute a delegate inside a temporary <see cref="Transaction"/>. If no exception is thrown, the <see cref="Transaction"/>
        /// will be committed.
        /// </summary>
        /// <remarks>
        /// Creates its own temporary <see cref="Transaction"/> and commits it after running the lambda passed to <paramref name="function"/>.
        /// Be careful of wrapping multiple single property updates in multiple <see cref="Write"/> calls.
        /// It is more efficient to update several properties or even create multiple objects in a single <see cref="Write"/>,
        /// unless you need to guarantee finer-grained updates.
        /// </remarks>
        /// <example>
        /// <code>
        /// var dog = realm.Write(() =>
        /// {
        ///     return realm.Add(new Dog
        ///     {
        ///         Name = "Eddie",
        ///         Age = 5
        ///     });
        /// });
        /// </code>
        /// </example>
        /// <param name="function">
        /// Delegate with one return value to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <typeparam name="T">The type returned by the input delegate.</typeparam>
        /// <returns>The return value of <paramref name="function"/>.</returns>
        public T Write<T>(Func<T> function)
        {
            ThrowIfDisposed();
            Argument.NotNull(function, nameof(function));

            using var transaction = BeginWrite();
            var result = function();
            transaction.Commit();
            return result;
        }

        /// <summary>
        /// Execute an action inside a temporary <see cref="Transaction"/> on a worker thread, <b>if</b> called from UI thread. If no exception is thrown,
        /// the <see cref="Transaction"/> will be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this Realm on a worker thread and executes <paramref name="action"/> inside a write <see cref="Transaction"/>.
        /// <see cref="Realm"/>s and <see cref="RealmObject"/>s/<see cref="EmbeddedObject"/>s are thread-affine, so capturing any such objects in
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
        /// Action to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public Task WriteAsync(Action<Realm> action)
        {
            Argument.NotNull(action, nameof(action));

            return WriteAsync(tempRealm =>
            {
                action(tempRealm);
                return true;
            });
        }

        /// <summary>
        /// Execute a delegate inside a temporary <see cref="Transaction"/> on a worker thread, <b>if</b> called from UI thread. If no exception is thrown,
        /// the <see cref="Transaction"/> will be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this Realm on a worker thread and executes <paramref name="function"/> inside a write <see cref="Transaction"/>.
        /// <see cref="Realm"/>s and <see cref="RealmObject"/>s/<see cref="EmbeddedObject"/>s are thread-affine, so capturing any such objects in
        /// the <c>action</c> delegate will lead to errors if they're used on the worker thread. Note that it checks the
        /// <see cref="SynchronizationContext"/> to determine if <c>Current</c> is null, as a test to see if you are on the UI thread
        /// and will otherwise just call Write without starting a new thread. So if you know you are invoking from a worker thread, just call Write instead.
        /// </remarks>
        /// <example>
        /// <code>
        /// var dog = await realm.WriteAsync(tempRealm =&gt;
        /// {
        ///     return tempRealm.Add(new Dog
        ///     {
        ///         Breed = "Dalmatian",
        ///     });
        /// });
        /// </code>
        /// <b>Note</b> that inside the action, we use <c>tempRealm</c>.
        /// </example>
        /// <param name="function">
        /// Delegate with one return value to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <typeparam name="T">The type returned by the input delegate.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> with return type <typeparamref name="T"/>.</returns>
        public async Task<T> WriteAsync<T>(Func<Realm, T> function)
        {
            ThrowIfDisposed();

            Argument.NotNull(function, nameof(function));

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.HasValidContext)
            {
                return Write(() => function(this));
            }

            // If we are on UI thread the SynchronizationContext will be set (often also set on long-lived workers to use Post back to UI thread).
            var result = await Task.Run(() =>
            {
                using var realm = GetInstance(Config);
                var writeAction = realm.Write(() => function(realm));
                if (writeAction is RealmObjectBase rob && rob.IsManaged && rob.IsValid)
                {
                    return (object)ThreadSafeReference.Create(rob);
                }

                return writeAction;
            });

            await RefreshAsync();

            if (result is ThreadSafeReference.Object<RealmObjectBase> tsr)
            {
                return (T)(object)ResolveReference(tsr);
            }

            return (T)result;
        }

        /// <summary>
        /// Execute a delegate inside a temporary <see cref="Transaction"/> on a worker thread, <b>if</b> called from UI thread. If no exception is thrown,
        /// the <see cref="Transaction"/> will be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this Realm on a worker thread and executes <paramref name="function"/> inside a write <see cref="Transaction"/>.
        /// <see cref="Realm"/>s and <see cref="RealmObject"/>s/<see cref="EmbeddedObject"/>s are thread-affine, so capturing any such objects in
        /// the <c>action</c> delegate will lead to errors if they're used on the worker thread. Note that it checks the
        /// <see cref="SynchronizationContext"/> to determine if <c>Current</c> is null, as a test to see if you are on the UI thread
        /// and will otherwise just call Write without starting a new thread. So if you know you are invoking from a worker thread, just call Write instead.
        /// </remarks>
        /// <example>
        /// <code>
        /// var dogs = await realm.WriteAsync(tempRealm =&gt;
        /// {
        ///     tempRealm.Add(new Dog
        ///     {
        ///         Breed = "Dalmatian",
        ///     });
        ///
        ///     tempRealm.Add(new Dog
        ///     {
        ///         Breed = "Poddle",
        ///     });
        ///
        ///     return tempRealm.All&lt;Dog&gt;();
        /// });
        /// </code>
        /// <b>Note</b> that inside the action, we use <c>tempRealm</c>.
        /// </example>
        /// <param name="function">
        /// Delegate with return type <see cref="IQueryable{T}"/> to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <typeparam name="T">The type of data in the <see cref="IQueryable{T}"/>.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> with return type <see cref="IQueryable{T}"/>.</returns>
        public async Task<IQueryable<T>> WriteAsync<T>(Func<Realm, IQueryable<T>> function)
            where T : RealmObjectBase
        {
            ThrowIfDisposed();

            Argument.NotNull(function, nameof(function));

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.HasValidContext)
            {
                return Write(() => function(this));
            }

            // If we are on UI thread the SynchronizationContext will be set (often also set on long-lived workers to use Post back to UI thread).
            var result = await Task.Run(() =>
            {
                using var realm = GetInstance(Config);
                var writeResult = realm.Write(() => function(realm));
                if (writeResult is RealmResults<T> rr && rr.IsValid && rr.IsManaged)
                {
                    return (object)ThreadSafeReference.Create(writeResult);
                }

                return writeResult;
            });

            await RefreshAsync();

            if (result is ThreadSafeReference.Query<T> tsr)
            {
                return ResolveReference(tsr);
            }

            return (IQueryable<T>)result;
        }

        /// <summary>
        /// Execute a delegate inside a temporary <see cref="Transaction"/> on a worker thread, <b>if</b> called from UI thread. If no exception is thrown,
        /// the <see cref="Transaction"/> will be committed.
        /// </summary>
        /// <remarks>
        /// Opens a new instance of this Realm on a worker thread and executes <paramref name="function"/> inside a write <see cref="Transaction"/>.
        /// <see cref="Realm"/>s and <see cref="RealmObject"/>s/<see cref="EmbeddedObject"/>s are thread-affine, so capturing any such objects in
        /// the <c>action</c> delegate will lead to errors if they're used on the worker thread. Note that it checks the
        /// <see cref="SynchronizationContext"/> to determine if <c>Current</c> is null, as a test to see if you are on the UI thread
        /// and will otherwise just call Write without starting a new thread. So if you know you are invoking from a worker thread, just call Write instead.
        /// </remarks>
        /// <example>
        /// <code>
        /// var markDogs = await realm.WriteAsync(tempRealm =&gt;
        /// {
        ///     var mark = tempRealm.All&lt;Person&gt;().Single(d =&gt; d.Name == "Mark");
        ///
        ///     mark.Dogs.Add(new Dog
        ///     {
        ///         Breed = "Dalmatian",
        ///     });
        ///
        ///     mark.Dogs.Add(new Dog
        ///     {
        ///         Breed = "Poodle",
        ///     });
        ///
        ///     return mark.Dogs;
        /// });
        /// </code>
        /// <b>Note</b> that inside the action, we use <c>tempRealm</c>.
        /// </example>
        /// <param name="function">
        /// Delegate with return type <see cref="IList{T}"/> to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <typeparam name="T">The type of data in the <see cref="IList{T}"/>.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> with return type <see cref="IList{T}"/>.</returns>
        public async Task<IList<T>> WriteAsync<T>(Func<Realm, IList<T>> function)
        {
            ThrowIfDisposed();

            Argument.NotNull(function, nameof(function));

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.HasValidContext)
            {
                return Write(() => function(this));
            }

            // If we are on UI thread the SynchronizationContext will be set (often also set on long-lived workers to use Post back to UI thread).
            var result = await Task.Run(() =>
            {
                using var realm = GetInstance(Config);
                var writeResult = realm.Write(() => function(realm));
                if (writeResult is RealmList<T> rl && rl.IsValid && rl.IsManaged)
                {
                    return (object)ThreadSafeReference.Create(writeResult);
                }

                return writeResult;
            });

            await RefreshAsync();

            if (result is ThreadSafeReference.List<T> tsr)
            {
                return ResolveReference(tsr);
            }

            return (IList<T>)result;
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

            RealmChanged += Handler;
            return tcs.Task;

            void Handler(object sender, EventArgs e)
            {
                ((Realm)sender).RealmChanged -= Handler;
                tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of class T, in this <see cref="Realm"/>.</returns>
        public IQueryable<T> All<T>()
            where T : RealmObject
        {
            ThrowIfDisposed();

            var type = typeof(T);
            Argument.Ensure(
                Metadata.TryGetValue(type.GetTypeInfo().GetMappedOrOriginalName(), out var metadata) && metadata.Schema.Type.AsType() == type,
                $"The class {type.Name} is not in the limited set of classes for this realm", nameof(T));

            return new RealmResults<T>(this, metadata);
        }

        // This should only be used for tests
        internal IQueryable<T> AllEmbedded<T>()
            where T : EmbeddedObject
        {
            ThrowIfDisposed();

            var type = typeof(T);
            Argument.Ensure(
                Metadata.TryGetValue(type.GetTypeInfo().GetMappedOrOriginalName(), out var metadata) && metadata.Schema.Type.AsType() == type,
                $"The class {type.Name} is not in the limited set of classes for this realm", nameof(T));

            return new RealmResults<T>(this, metadata);
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
        public T Find<T>(long? primaryKey)
            where T : RealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T Find<T>(string primaryKey)
            where T : RealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T Find<T>(ObjectId? primaryKey)
            where T : RealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T Find<T>(Guid? primaryKey)
            where T : RealmObject => FindCore<T>(primaryKey);

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The RealmObjectBase instance will own its handle.")]
        private T FindCore<T>(RealmValue primaryKey)
            where T : RealmObject
        {
            ThrowIfDisposed();

            var metadata = Metadata[typeof(T).GetTypeInfo().GetMappedOrOriginalName()];
            if (SharedRealmHandle.TryFindObject(metadata.TableKey, primaryKey, out var objectHandle))
            {
                return (T)MakeObject(metadata, objectHandle);
            }

            return null;
        }

        #endregion Quick Find using primary key

        #region Thread Handover

        /// <summary>
        /// Returns the same object as the one referenced when the <see cref="ThreadSafeReference.Object{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the reference.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> resolved for the current thread or <c>null</c>
        /// if the object has been deleted after the reference was created.
        /// </returns>
        public T ResolveReference<T>(ThreadSafeReference.Object<T> reference)
            where T : RealmObjectBase
        {
            Argument.NotNull(reference, nameof(reference));

            var objectPtr = SharedRealmHandle.ResolveReference(reference);
            var objectHandle = new ObjectHandle(SharedRealmHandle, objectPtr);

            if (!objectHandle.IsValid)
            {
                objectHandle.Dispose();
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
            Argument.NotNull(reference, nameof(reference));

            var listPtr = SharedRealmHandle.ResolveReference(reference);
            var listHandle = new ListHandle(SharedRealmHandle, listPtr);
            if (!listHandle.IsValid)
            {
                listHandle.Dispose();
                return null;
            }

            return new RealmList<T>(this, listHandle, reference.Metadata);
        }

        /// <summary>
        /// Returns the same collection as the one referenced when the <see cref="ThreadSafeReference.Set{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="ISet{T}"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the elements, contained in the collection.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="ISet{T}"/> resolved for the current thread or <c>null</c>
        /// if the set's parent object has been deleted after the reference was created.
        /// </returns>
        public ISet<T> ResolveReference<T>(ThreadSafeReference.Set<T> reference)
        {
            Argument.NotNull(reference, nameof(reference));

            var setPtr = SharedRealmHandle.ResolveReference(reference);
            var setHandle = new SetHandle(SharedRealmHandle, setPtr);
            if (!setHandle.IsValid)
            {
                setHandle.Dispose();
                return null;
            }

            return new RealmSet<T>(this, setHandle, reference.Metadata);
        }

        /// <summary>
        /// Returns the same collection as the one referenced when the <see cref="ThreadSafeReference.Dictionary{TValue}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="IDictionary{String, TValue}"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="TValue">The type of the values contained in the dictionary.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="IDictionary{String, TValue}"/> resolved for the current thread or <c>null</c>
        /// if the set's parent object has been deleted after the reference was created.
        /// </returns>
        public IDictionary<string, TValue> ResolveReference<TValue>(ThreadSafeReference.Dictionary<TValue> reference)
        {
            Argument.NotNull(reference, nameof(reference));

            var dictionaryPtr = SharedRealmHandle.ResolveReference(reference);
            var dictionaryHandle = new DictionaryHandle(SharedRealmHandle, dictionaryPtr);
            if (!dictionaryHandle.IsValid)
            {
                dictionaryHandle.Dispose();
                return null;
            }

            return new RealmDictionary<TValue>(this, dictionaryHandle, reference.Metadata);
        }

        /// <summary>
        /// Returns the same query as the one referenced when the <see cref="ThreadSafeReference.Query{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="IQueryable{T}"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the query.</typeparam>
        /// <returns>A thread-confined instance of the original <see cref="IQueryable{T}"/> resolved for the current thread.</returns>
        public IQueryable<T> ResolveReference<T>(ThreadSafeReference.Query<T> reference)
            where T : RealmObjectBase
        {
            Argument.NotNull(reference, nameof(reference));

            var resultsPtr = SharedRealmHandle.ResolveReference(reference);
            var resultsHandle = new ResultsHandle(SharedRealmHandle, resultsPtr);
            return new RealmResults<T>(this, resultsHandle, reference.Metadata);
        }

        #endregion Thread Handover

        /// <summary>
        /// Removes a persistent object from this Realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this Realm.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">If <c>obj</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If you pass a standalone object.</exception>
        public void Remove(RealmObjectBase obj)
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
        public void RemoveRange<T>(IQueryable<T> range)
            where T : RealmObjectBase
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
        public void RemoveAll<T>()
            where T : RealmObject
        {
            ThrowIfDisposed();

            RemoveRange(All<T>());
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
                using var resultsHandle = SharedRealmHandle.CreateResults(metadata.TableKey);
                resultsHandle.Clear(SharedRealmHandle);
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
            Argument.NotNull(config, nameof(config));

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

        #endregion Transactions

        internal class RealmMetadata
        {
            private readonly Dictionary<string, RealmObjectBase.Metadata> stringToRealmObjectMetadataDict;
            private readonly Dictionary<TableKey, RealmObjectBase.Metadata> tableKeyToRealmObjectMetadataDict;

            public IEnumerable<RealmObjectBase.Metadata> Values => stringToRealmObjectMetadataDict.Values;

            public RealmMetadata(IEnumerable<RealmObjectBase.Metadata> objectsMetadata)
            {
                stringToRealmObjectMetadataDict = new Dictionary<string, RealmObjectBase.Metadata>();
                tableKeyToRealmObjectMetadataDict = new Dictionary<TableKey, RealmObjectBase.Metadata>();

                foreach (var objectMetadata in objectsMetadata)
                {
                    stringToRealmObjectMetadataDict[objectMetadata.Schema.Name] = objectMetadata;
                    tableKeyToRealmObjectMetadataDict[objectMetadata.TableKey] = objectMetadata;
                }
            }

            public bool TryGetValue(string objectType, out RealmObjectBase.Metadata metadata) =>
                stringToRealmObjectMetadataDict.TryGetValue(objectType, out metadata);

            public bool TryGetValue(TableKey tablekey, out RealmObjectBase.Metadata metadata) =>
                tableKeyToRealmObjectMetadataDict.TryGetValue(tablekey, out metadata);

            public RealmObjectBase.Metadata this[string objectType] => stringToRealmObjectMetadataDict[objectType];

            public RealmObjectBase.Metadata this[TableKey tablekey] => tableKeyToRealmObjectMetadataDict[tablekey];
        }

        internal class State : IDisposable
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

            public void Dispose()
            {
                GCHandle.Free();
            }
        }

        /// <summary>
        /// A class that exposes the dynamic API for a <see cref="Realm"/> instance.
        /// </summary>
        [Preserve(AllMembers = true)]
        public class Dynamic
        {
            private readonly Realm _realm;

            internal Dynamic(Realm realm)
            {
                _realm = realm;
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
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public dynamic CreateObject(string className, object primaryKey)
            {
                _realm.ThrowIfDisposed();

                Argument.Ensure(_realm.Metadata.TryGetValue(className, out var metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));

                var result = metadata.Helper.CreateInstance();

                ObjectHandle objectHandle;
                var pkProperty = metadata.Schema.PrimaryKeyProperty;
                if (pkProperty.HasValue)
                {
                    objectHandle = _realm.SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty.Value, primaryKey, metadata.TableKey, className, update: false, isNew: out var _);
                }
                else
                {
                    objectHandle = _realm.SharedRealmHandle.CreateObject(metadata.TableKey);
                }

                result.SetOwner(_realm, objectHandle, metadata);
                result.OnManaged();
                return result;
            }

            /// <summary>
            /// Factory for a managed embedded object in a realm. Only valid within a write <see cref="Transaction"/>.
            /// Embedded objects need to be owned immediately which is why they can only be created for a specific property.
            /// </summary>
            /// <param name="parent">
            /// The parent <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> that will own the newly created
            /// embedded object.
            /// </param>
            /// <param name="propertyName">The property to which the newly created embedded object will be assigned.</param>
            /// <returns>A dynamically-accessed embedded object.</returns>
            public dynamic CreateEmbeddedObjectForProperty(RealmObjectBase parent, string propertyName)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(parent, nameof(parent));
                Argument.Ensure(parent.IsManaged && parent.IsValid, "The object passed as parent must be managed and valid to create an embedded object.", nameof(parent));
                Argument.Ensure(parent.Realm.IsSameInstance(_realm), "The object passed as parent is managed by a different Realm", nameof(parent));
                Argument.Ensure(parent.ObjectMetadata.Schema.TryFindProperty(propertyName, out var property), $"The schema for class {parent.GetType().Name} does not contain a property {propertyName}.", nameof(propertyName));
                Argument.Ensure(_realm.Metadata.TryGetValue(property.ObjectType, out var metadata), $"The class {property.ObjectType} linked to by {parent.GetType().Name}.{propertyName} is not in the limited set of classes for this realm", nameof(propertyName));
                Argument.Ensure(metadata.Schema.IsEmbedded, $"The class {property.ObjectType} linked to by {parent.GetType().Name}.{propertyName} is not embedded", nameof(propertyName));

                var obj = metadata.Helper.CreateInstance();
                var handle = parent.ObjectHandle.CreateEmbeddedObjectForProperty(parent.ObjectMetadata.PropertyIndices[propertyName]);

                obj.SetOwner(_realm, handle, metadata);
                obj.OnManaged();

                return obj;
            }

            /// <summary>
            /// Creates an embedded object and adds it to the specified list. This also assigns correct ownership of the newly created embedded object.
            /// </summary>
            /// <param name="list">The list to which the object will be added.</param>
            /// <returns>The newly created object, after it has been added to the back of the list.</returns>
            /// <remarks>
            /// Lists of embedded objects cannot directly add objects as that would require constructing an unowned embedded object, which is not possible. This is why
            /// <see cref="AddEmbeddedObjectToList"/>, <see cref="InsertEmbeddedObjectInList"/>, and <see cref="SetEmbeddedObjectInList"/> have to be used instead of
            /// <see cref="ICollection{T}.Add"/>, <see cref="IList{T}.Insert"/>, and <see cref="IList{T}.this[int]"/>.
            /// </remarks>
            /// <seealso cref="InsertEmbeddedObjectInList"/>
            /// <seealso cref="SetEmbeddedObjectInList"/>
            [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Argument is validated in PerformEmbeddedListOperation.")]
            public dynamic AddEmbeddedObjectToList(IRealmCollection<EmbeddedObject> list)
            {
                return PerformEmbeddedListOperation(list, listHandle => listHandle.AddEmbedded());
            }

            /// <summary>
            /// Creates an embedded object and inserts it in the specified list at the specified index. This also assigns correct ownership of the newly created embedded object.
            /// </summary>
            /// <param name="list">The list in which the object will be inserted.</param>
            /// <param name="index">The index at which the object will be inserted.</param>
            /// <returns>The newly created object, after it has been inserted in the list.</returns>
            /// <remarks>
            /// Lists of embedded objects cannot directly add objects as that would require constructing an unowned embedded object, which is not possible. This is why
            /// <see cref="AddEmbeddedObjectToList"/>, <see cref="InsertEmbeddedObjectInList"/>, and <see cref="SetEmbeddedObjectInList"/> have to be used instead of
            /// <see cref="ICollection{T}.Add"/>, <see cref="IList{T}.Insert"/>, and <see cref="IList{T}.this[int]"/>.
            /// </remarks>
            /// <seealso cref="InsertEmbeddedObjectInList"/>
            /// <seealso cref="SetEmbeddedObjectInList"/>
            [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Argument is validated in PerformEmbeddedListOperation.")]
            public dynamic InsertEmbeddedObjectInList(IRealmCollection<EmbeddedObject> list, int index)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return PerformEmbeddedListOperation(list, listHandle => listHandle.InsertEmbedded(index));
            }

            /// <summary>
            /// Creates an embedded object and sets it in the specified list at the specified index. This also assigns correct ownership of the newly created embedded object.
            /// </summary>
            /// <param name="list">The list in which the object will be set.</param>
            /// <param name="index">The index at which the object will be set.</param>
            /// <returns>The newly created object, after it has been set to the specified index in the list.</returns>
            /// <remarks>
            /// Lists of embedded objects cannot directly add objects as that would require constructing an unowned embedded object, which is not possible. This is why
            /// <see cref="AddEmbeddedObjectToList"/>, <see cref="InsertEmbeddedObjectInList"/>, and <see cref="SetEmbeddedObjectInList"/> have to be used instead of
            /// <see cref="ICollection{T}.Add"/>, <see cref="IList{T}.Insert"/>, and <see cref="IList{T}.this[int]"/>.
            /// <para/>
            /// Setting an object at an index will remove the existing object from the list and unown it. Since unowned embedded objects are automatically deleted,
            /// the old object that the list contained at <paramref name="index"/> will get deleted when the transaction is committed.
            /// </remarks>
            /// <seealso cref="InsertEmbeddedObjectInList"/>
            /// <seealso cref="SetEmbeddedObjectInList"/>
            [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Argument is validated in PerformEmbeddedListOperation.")]
            public dynamic SetEmbeddedObjectInList(IRealmCollection<EmbeddedObject> list, int index)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return PerformEmbeddedListOperation(list, listHandle => listHandle.SetEmbedded(index));
            }

            /// <summary>
            /// Creates an embedded object and adds it to the specified dictionary. This also assigns correct ownership of the newly created embedded object.
            /// </summary>
            /// <param name="dictionary">The dictionary to which the object will be added.</param>
            /// <param name="key">The key for which the object will be added.</param>
            /// <returns>The newly created object, after it has been added to the dictionary.</returns>
            /// <remarks>
            /// Dictionaries containing embedded objects cannot directly add objects as that would require constructing an unowned embedded object, which is not possible. This is why
            /// <see cref="AddEmbeddedObjectToDictionary"/> and <see cref="SetEmbeddedObjectInDictionary"/> have to be used instead of
            /// <see cref="IDictionary{String, TValue}.Add"/> and <see cref="IDictionary{String, TValue}.this[String]"/>.
            /// </remarks>
            /// <seealso cref="SetEmbeddedObjectInDictionary"/>
            [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Argument is validated in PerformEmbeddedListOperation.")]
            public dynamic AddEmbeddedObjectToDictionary(object dictionary, string key)
            {
                Argument.NotNull(key, nameof(key));

                return PerformEmbeddedDictionaryOperation(dictionary, handle => handle.AddEmbedded(key));
            }

            /// <summary>
            /// Creates an embedded object and sets it in the specified dictionary for the specified key. This also assigns correct ownership of the newly created embedded object.
            /// </summary>
            /// <param name="dictionary">The dictionary in which the object will be set.</param>
            /// <param name="key">The key for which the object will be set.</param>
            /// <returns>The newly created object, after it has been assigned to the specified key in the dictionary.</returns>
            /// <remarks>
            /// Dictionaries containing embedded objects cannot directly add objects as that would require constructing an unowned embedded object, which is not possible. This is why
            /// <see cref="AddEmbeddedObjectToDictionary"/> and <see cref="SetEmbeddedObjectInDictionary"/> have to be used instead of
            /// <see cref="IDictionary{String, TValue}.Add"/> and <see cref="IDictionary{String, TValue}.this[String]"/>.
            /// </remarks>
            /// <seealso cref="AddEmbeddedObjectToDictionary"/>
            [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Argument is validated in PerformEmbeddedListOperation.")]
            public dynamic SetEmbeddedObjectInDictionary(object dictionary, string key)
            {
                Argument.NotNull(key, nameof(key));

                return PerformEmbeddedDictionaryOperation(dictionary, handle => handle.SetEmbedded(key));
            }

            /// <summary>
            /// Get a view of all the objects of a particular type.
            /// </summary>
            /// <param name="className">The type of the objects as defined in the schema.</param>
            /// <remarks>Because the objects inside the view are accessed dynamically, the view cannot be queried into using LINQ or other expression predicates.</remarks>
            /// <returns>A queryable collection that without further filtering, allows iterating all objects of className, in this realm.</returns>
            public IQueryable<dynamic> All(string className)
            {
                _realm.ThrowIfDisposed();

                Argument.Ensure(_realm.Metadata.TryGetValue(className, out var metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));
                Argument.Ensure(!metadata.Schema.IsEmbedded, $"The class {className} represents an embedded object and thus cannot be queried directly.", nameof(className));

                return new RealmResults<RealmObject>(_realm, metadata);
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
                _realm.ThrowIfDisposed();

                var query = (RealmResults<RealmObject>)All(className);
                query.ResultsHandle.Clear(_realm.SharedRealmHandle);
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
            public dynamic Find(string className, long? primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public dynamic Find(string className, string primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">
            /// Primary key to be matched exactly, same as an == search.
            /// </param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public dynamic Find(string className, ObjectId? primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">
            /// Primary key to be matched exactly, same as an == search.
            /// </param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="RealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public dynamic Find(string className, Guid? primaryKey) => FindCore(className, primaryKey);

            [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The RealmObjectBase instance will own its handle.")]
            private dynamic FindCore(string className, RealmValue primaryKey)
            {
                _realm.ThrowIfDisposed();

                var metadata = _realm.Metadata[className];
                if (_realm.SharedRealmHandle.TryFindObject(metadata.TableKey, primaryKey, out var objectHandle))
                {
                    return _realm.MakeObject(metadata, objectHandle);
                }

                return null;
            }

            private dynamic PerformEmbeddedListOperation(IRealmCollection<EmbeddedObject> list, Func<ListHandle, ObjectHandle> getHandle)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(list, nameof(list));

                if (!(list is IRealmCollectionBase<ListHandle> realmList))
                {
                    throw new ArgumentException($"Expected list to be IList<EmbeddedObject> but was ${list.GetType().FullName} instead.", nameof(list));
                }

                var obj = realmList.Metadata.Helper.CreateInstance();

                obj.SetOwner(_realm, getHandle(realmList.NativeHandle), realmList.Metadata);
                obj.OnManaged();

                return obj;
            }

            private dynamic PerformEmbeddedDictionaryOperation(object dictionary, Func<DictionaryHandle, ObjectHandle> getHandle)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(dictionary, nameof(dictionary));

                if (!(dictionary is IRealmCollectionBase<DictionaryHandle> realmDict))
                {
                    throw new ArgumentException($"Expected dictionary to be IDictionary<string, EmbeddedObject> but was ${dictionary.GetType().FullName} instead.", nameof(dictionary));
                }

                var obj = realmDict.Metadata.Helper.CreateInstance();

                obj.SetOwner(_realm, getHandle(realmDict.NativeHandle), realmDict.Metadata);
                obj.OnManaged();

                return obj;
            }
        }
    }
}
