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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Extensions;
using Realms.Helpers;
using Realms.Logging;
using Realms.Native;
using Realms.Schema;
using Realms.Sync;

namespace Realms
{
    /// <summary>
    /// A Realm instance (also referred to as a Realm) represents a Realm database.
    /// <br/>
    /// <b>Warning</b>: Non-frozen Realm instances are not thread safe and can not be shared across threads.
    /// You must call <see cref="GetInstance(RealmConfigurationBase)"/> on each thread in which you want to interact with the Realm.
    /// </summary>
    public class Realm : IDisposable
    {
        #region static

        /// <summary>
        /// Gets or sets a value indicating whether to use the legacy representation when storing Guid values in the database.
        /// </summary>
        /// <remarks>
        /// In versions prior to 10.10.0, the .NET SDK had a bug where it would store Guid values with architecture-specific byte ordering
        /// (little-endian for most modern CPUs) while the database query engine and Sync would always treat them as big-endian. This manifests
        /// as different string representations between the SDK and the database - e.g. "f2952191-a847-41c3-8362-497f92cb7d24" instead of
        /// "912195f2-47a8-c341-8362-497f92cb7d24" (notice the swapped bytes in the first 3 components). Starting with 10.10.0, big-endian
        /// representation is the default one and a seamless one-time migration is provided for local (non-sync) Realms. The first time a
        /// Realm is opened, all properties holding a Guid value will be updated from little-endian to big-endian format and the .NET SDK
        /// will treat them as such. There should be no noticeable change when reading/writing data from the SDK, but you should see consistent
        /// values when accessing the Realm file from Realm Studio or other SDKs.
        /// <br/>
        /// For synchronized Realms, such a migration is impossible due to the distributed nature of the data. Therefore, the assumption
        /// is that the Guid representation in Atlas if authoritative and the SDK values should be updated to match it. THIS MEANS THAT THE
        /// SDK WILL START REPORTING A DIFFERENT STRING REPRESENTATION OF EXISTING GUID DATA COMPARED TO pre-10.10.0. If you are querying
        /// 3rd party systems from the SDK, you might see unexpected results. To preserve the existing behavior (little-endian in the SDK,
        /// big-endian in Atlas), set this value to <c>true</c> and reach out to the Realm support team (https://support.mongodb.com) for
        /// help with migrating your data to the new format.
        /// </remarks>
        [Obsolete("It is strongly advised to migrate to the new Guid representation as soon as possible to avoid data inconsistency.")]
        public static bool UseLegacyGuidRepresentation { get; set; }

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
        public static Realm GetInstance(RealmConfigurationBase? config = null)
        {
            config ??= RealmConfiguration.DefaultConfiguration;

            return config.CreateRealm();
        }

        /// <summary>
        /// Factory for asynchronously obtaining a <see cref="Realm"/> instance.
        /// </summary>
        /// <remarks>
        /// If the configuration is <see cref="SyncConfigurationBase"/>, the realm will be downloaded and fully
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
        public static Task<Realm> GetInstanceAsync(RealmConfigurationBase? config = null, CancellationToken cancellationToken = default)
        {
            config ??= RealmConfiguration.DefaultConfiguration;

            return config.CreateRealmAsync(cancellationToken);
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
        public static bool Compact(RealmConfigurationBase? config = null)
        {
            using var realm = GetInstance(config);
            if (config is SyncConfigurationBase)
            {
                // For synchronized Realms, shutdown the session, otherwise Compact will fail.
                var session = realm.SyncSession;
                session.CloseHandle(waitForShutdown: true);
            }

            return realm.SharedRealmHandle.Compact();
        }

        /// <summary>
        /// Deletes all files associated with a given Realm if the Realm exists and is not open.
        /// </summary>
        /// <remarks>
        /// The Realm file must not be open on other threads.<br/>
        /// All but the .lock file will be deleted by this.
        /// </remarks>
        /// <param name="configuration">A <see cref="RealmConfigurationBase"/> which supplies the realm path.</param>
        /// <exception cref="RealmInUseException">Thrown if the Realm is still open.</exception>
        public static void DeleteRealm(RealmConfigurationBase configuration)
        {
            Argument.NotNull(configuration, nameof(configuration));

            SharedRealmHandle.DeleteFiles(configuration.DatabasePath);
        }

        #endregion static

        private WeakReference<SubscriptionSet>? _subscriptionRef;

        private State _state;
        private WeakReference<Session>? _sessionRef;
        private Transaction? _activeTransaction;

        internal readonly SharedRealmHandle SharedRealmHandle;
        internal readonly RealmMetadata Metadata;
        internal readonly bool IsInMigration;

        /// <summary>
        /// Gets an object encompassing the dynamic API for this Realm instance.
        /// </summary>
        /// <value>A <see cref="Dynamic"/> instance that wraps this Realm.</value>
        [Preserve]
        public Dynamic DynamicApi => new(this);

        /// <summary>
        /// Gets a value indicating whether there is an active write transaction associated
        /// with this Realm.
        /// </summary>
        /// <value><c>true</c> if the Realm is in transaction; <c>false</c> otherwise.</value>
        /// <seealso cref="BeginWrite"/>
        /// <seealso cref="Transaction"/>
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
        /// <value><c>true</c> if the Realm is frozen and immutable; <c>false</c> otherwise.</value>
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

        /// <summary>
        /// Gets the <see cref="Session"/> for this <see cref="Realm"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown if the Realm has not been opened with a <see cref="FlexibleSyncConfiguration"/> or
        /// <see cref="PartitionSyncConfiguration"/>.
        /// </exception>
        /// <value>
        /// The <see cref="Session"/> that is responsible for synchronizing with MongoDB Atlas
        /// if the Realm instance was created with a <see cref="FlexibleSyncConfiguration"/> or
        /// <see cref="PartitionSyncConfiguration"/>. If this is a local or in-memory Realm, a
        /// <see cref="NotSupportedException"/> will be thrown.
        /// </value>
        public Session SyncSession
        {
            get
            {
                ThrowIfDisposed();

                if (Config is not SyncConfigurationBase)
                {
                    throw new NotSupportedException("Realm.SyncSession is only valid for synchronized Realms (i.e. ones that are opened with FlexibleSyncConfiguration or PartitionSyncConfiguration).");
                }

                if (_sessionRef is null || !_sessionRef.TryGetTarget(out var session) || session.IsClosed)
                {
                    var sessionHandle = SharedRealmHandle.GetSession();
                    session = new Session(sessionHandle);

                    if (_sessionRef is null)
                    {
                        _sessionRef = new WeakReference<Session>(session);
                    }
                    else
                    {
                        _sessionRef.SetTarget(session);
                    }
                }

                return session;
            }
        }

        /// <summary>
        /// Gets the <see cref="SubscriptionSet"/> representing the active subscriptions for this <see cref="Realm"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the Realm has not been opened with a <see cref="FlexibleSyncConfiguration"/>.</exception>
        /// <value>
        /// The <see cref="SubscriptionSet"/> containing the query subscriptions that the server is using to decide which objects to
        /// synchronize with the local <see cref="Realm"/>. If the Realm was not created with a <see cref="FlexibleSyncConfiguration"/>,
        /// this will throw a <see cref="NotSupportedException"/>.
        /// </value>
        public SubscriptionSet Subscriptions
        {
            get
            {
                ThrowIfDisposed();

                if (Config is not FlexibleSyncConfiguration)
                {
                    throw new NotSupportedException("Realm.Subscriptions is only valid for flexible sync Realms (i.e. ones that are opened with FlexibleSyncConfiguration).");
                }

                // If the last subscription ref is alive and its version matches the current subscription
                // version, we return it. Otherwise, we create a new set and replace the existing one.
                if (_subscriptionRef != null && _subscriptionRef.TryGetTarget(out var existingSet))
                {
                    var currentVersion = SharedRealmHandle.GetSubscriptionsVersion();
                    if (existingSet.Version >= currentVersion)
                    {
                        return existingSet;
                    }
                }

                var handle = SharedRealmHandle.GetSubscriptions();
                var set = new SubscriptionSet(handle);
                _subscriptionRef = new WeakReference<SubscriptionSet>(set);
                return set;
            }
        }

        internal Realm(SharedRealmHandle sharedRealmHandle, RealmConfigurationBase config, RealmSchema schema, RealmMetadata? metadata = null, bool isInMigration = false)
        {
            Config = config;
            IsInMigration = isInMigration;

            State? state = null;
            if (config.EnableCache && sharedRealmHandle.OwnsNativeRealm)
            {
                var statePtr = sharedRealmHandle.GetManagedStateHandle();
                if (statePtr != IntPtr.Zero)
                {
                    state = GCHandle.FromIntPtr(statePtr).Target as State;
                }
            }

            if (state is null)
            {
                state = new State();
                sharedRealmHandle.SetManagedStateHandle(state);
            }

            _state = state;
            _state.AddRealm(this);

            SharedRealmHandle = sharedRealmHandle;
            Metadata = metadata ?? new RealmMetadata(schema.Select(CreateRealmObjectMetadata));
            Schema = schema;
            IsFrozen = SharedRealmHandle.IsFrozen;
        }

        private Metadata CreateRealmObjectMetadata(ObjectSchema schema)
        {
            var tableKey = SharedRealmHandle.GetTableKey(schema.Name);
            Weaving.IRealmObjectHelper helper;

            if (schema.Type != null && !Config.IsDynamic)
            {
                var wovenAtt = schema.Type.GetCustomAttribute<WovenAttribute>();
                if (wovenAtt is null)
                {
                    throw new RealmException($"Fody not properly installed. {schema.Type.FullName} is a RealmObjectBase but has not been woven.");
                }

                helper = (Weaving.IRealmObjectHelper)Activator.CreateInstance(wovenAtt.HelperType)!;
            }
            else
            {
                helper = DynamicRealmObjectHelper.Instance(schema);
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

            return new Metadata(tableKey, helper, initPropertyMap, schema);
        }

        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/>.
        /// </summary>
        /// <param name="sender">The <see cref="Realm"/> which has changed.</param>
        /// <param name="e">Currently an empty argument, in the future may indicate more details about the change.</param>
        public delegate void RealmChangedEventHandler(Realm sender, EventArgs e);

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event RealmChangedEventHandler? _realmChanged;

        /// <summary>
        /// Triggered when a Realm has changed (i.e. a <see cref="Transaction"/> was committed).
        /// </summary>
        public event RealmChangedEventHandler? RealmChanged
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
        public event EventHandler<ErrorEventArgs>? Error;

        internal void NotifyError(Exception ex)
        {
            if (Error is null)
            {
                Logger.LogDefault(LogLevel.Error, "A realm-level exception has occurred. To handle and react to those, subscribe to the Realm.Error event.");
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
                _activeTransaction?.Dispose();
                _state.RemoveRealm(this, closeOnEmpty: SharedRealmHandle.OwnsNativeRealm);

                _state = null!;
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
            return new Realm(handle, Config, Schema, Metadata);
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
        public override bool Equals(object? obj) => Equals(obj as Realm);

        private bool Equals(Realm? other)
        {
            if (other is null)
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

        internal IRealmObjectBase MakeObject(Metadata metadata, ObjectHandle objectHandle)
        {
            var ret = metadata.Helper.CreateInstance();
            ret.CreateAndSetAccessor(objectHandle, this, metadata);
            return ret;
        }

        internal RealmMetadata MergeSchema(RealmSchema schema)
        {
            Metadata.Add(schema.Select(CreateRealmObjectMetadata));
            return Metadata;
        }

        /// <summary>
        /// This <see cref="Realm"/> will start managing an <see cref="IRealmObject"/> which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="IRealmObject"/> but also have been processed by the Fody weaver,
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
        /// This method modifies the object in-place, meaning that after it has run, <paramref name="obj"/> will be managed.
        /// Returning it is just meant as a convenience to enable fluent syntax scenarios.
        /// </remarks>
        /// <returns>The passed object, so that you can write <c>var person = realm.Add(new Person { Id = 1 });</c>.</returns>
        public T Add<T>(T obj, bool update = false)
            where T : IRealmObject
        {
            ThrowIfDisposed();
            Argument.NotNull(obj, nameof(obj));
            Argument.Ensure(obj.IsValid, "Cannot add the object to the realm because it has been removed.", nameof(obj));

            // This is not obsoleted because the compiler will always pick it for specific types, generating a bunch of warnings
            AddInternal(obj, obj.GetType(), update);
            return obj;
        }

        /// <summary>
        /// Add a collection of standalone <see cref="IRealmObject">RealmObjects</see> to this <see cref="Realm"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="IRealmObject"/> but also have been processed by the Fody weaver,
        /// so it has persistent properties.
        /// </typeparam>
        /// <param name="objs">A collection of <see cref="IRealmObject"/> instances that will be added to this <see cref="Realm"/>.</param>
        /// <param name="update">If <c>true</c>, and an object with the same primary key already exists, performs an update.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the collection contains items that are already managed by this <see cref="Realm"/>, they will be ignored.
        /// This method modifies the objects in-place, meaning that after it has run, all items in <paramref name="objs"/> will be managed.
        /// </remarks>
        public void Add<T>(IEnumerable<T> objs, bool update = false)
            where T : IRealmObject
        {
            ThrowIfDisposed();
            Argument.NotNull(objs, nameof(objs));

            foreach (var obj in objs)
            {
                Argument.Ensure(obj != null, $"{nameof(objs)} must not contain null objects.", nameof(objs));
                Argument.Ensure(obj.IsValid, $"{nameof(objs)} must not contain removed objects.", nameof(objs));
            }

            foreach (var obj in objs)
            {
                AddInternal(obj, obj.GetType(), update);
            }
        }

        /// <summary>
        /// This <see cref="Realm"/> will start managing an <see cref="IAsymmetricObject"/> which has been created as a standalone object.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="IAsymmetricObject"/> but also have been processed by the Fody weaver,
        /// so it has persistent properties.
        /// </typeparam>
        /// <param name="obj">Must be a standalone <see cref="IAsymmetricObject"/>, <c>null</c> not allowed.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the object is already managed by this <see cref="Realm"/>, this method does nothing.
        /// This method modifies the object in-place,
        /// meaning that after it has run, <see cref="IAsymmetricObject"/> will be managed.
        /// Once an <see cref="IAsymmetricObject"/> becomes managed dereferencing any property
        /// of the original <see cref="IAsymmetricObject"/> reference throws an exception.
        /// </remarks>
        public void Add<T>(T obj)
            where T : IAsymmetricObject
        {
            ThrowIfDisposed();
            Argument.NotNull(obj, nameof(obj));
            Argument.Ensure(!obj.IsManaged, $"{nameof(obj)} must not be already managed by a Realm.", nameof(obj));

            AddInternal(obj, obj.GetType(), update: false);
        }

        /// <summary>
        /// Add a collection of standalone <see cref="IAsymmetricObject">AsymmetricObjects</see> to this <see cref="Realm"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The Type T must not only be a <see cref="IAsymmetricObject"/> but also have been processed by the Fody weaver,
        /// so it has persistent properties.
        /// </typeparam>
        /// <param name="objs">A collection of <see cref="IAsymmetricObject"/> instances that will be added to this <see cref="Realm"/>.</param>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">
        /// You can't manage an object with more than one <see cref="Realm"/>.
        /// </exception>
        /// <remarks>
        /// If the collection contains items that are already managed by this <see cref="Realm"/>, they will be ignored.
        /// This method modifies the objects in-place, meaning that after it has run, all items in the collection will be managed.
        /// Once an <see cref="IAsymmetricObject"/> becomes managed and the transaction is committed,
        /// dereferencing any property of the original <see cref="IAsymmetricObject"/> reference throw an exception.
        /// Hence, none of the properties of the elements in the collection can be dereferenced anymore after the transaction.
        /// </remarks>
        public void Add<T>(IEnumerable<T> objs)
            where T : IAsymmetricObject
        {
            ThrowIfDisposed();
            Argument.NotNull(objs, nameof(objs));
            foreach (var obj in objs)
            {
                Argument.Ensure(obj != null, $"{nameof(objs)} must not contain null values.", nameof(objs));
                Argument.Ensure(!obj.IsManaged, $"{nameof(objs)} must not contain already managed objects by a Realm.", nameof(objs));
            }

            foreach (var obj in objs)
            {
                AddInternal(obj, obj.GetType(), update: false);
            }
        }

        internal void ManageEmbedded(IEmbeddedObject obj, ObjectHandle handle)
        {
            var objectType = obj.GetType();
            var objectName = objectType.GetMappedOrOriginalName();
            Argument.Ensure(Metadata.TryGetValue(objectName, out var metadata), $"The class {objectType.Name} is not in the limited set of classes for this realm", nameof(obj));

            obj.CreateAndSetAccessor(handle, this, metadata, copyToRealm: true, update: false, skipDefaults: true);
        }

        private void AddInternal<T>(T obj, Type objectType, bool update)
            where T : IRealmObjectBase
        {
            if (objectType.IsEmbeddedObject())
            {
                throw new ArgumentException("EmbeddedObjects can't be added as standalone objects.");
            }

            if (!ShouldAddNewObject(obj))
            {
                return;
            }

            var objectName = objectType.GetMappedOrOriginalName();
            Argument.Ensure(Metadata.TryGetValue(objectName, out var metadata), $"The class {objectType.Name} is not in the limited set of classes for this realm", nameof(objectType));

            ObjectHandle objectHandle;
            bool isNew;
            if (metadata.Helper.TryGetPrimaryKeyValue(obj, out var primaryKey))
            {
                var pkProperty = metadata.Schema.PrimaryKeyProperty!.Value;
                objectHandle = SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty, primaryKey, metadata.TableKey, objectName, update, out isNew);
            }
            else
            {
                isNew = true; // Objects without PK are always new
                objectHandle = SharedRealmHandle.CreateObject(metadata.TableKey);
            }

            obj.CreateAndSetAccessor(objectHandle, this, metadata, copyToRealm: true, update: update, skipDefaults: isNew);
        }

        private bool ShouldAddNewObject(IRealmObjectBase obj)
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
        /// Begins a write transaction for this Realm.
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

            SharedRealmHandle.BeginTransaction();

            Debug.Assert(_activeTransaction is null, "There should be no active transaction");
            return _activeTransaction = new Transaction(this);
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
        /// Asynchronously begins a write transaction for this Realm.
        /// </summary>
        /// <remarks>
        /// This method asynchronously acquires the write lock and then dispatches the continuation on the original
        /// thread the Realm was opened on. The transaction can then be committed either asynchronously or
        /// synchronously.
        /// <br/>
        /// When invoked on a thread without SynchronizationContext (i.e. typically background threads), this method
        /// calls <see cref="BeginWrite"/> and executes synchronously.
        /// </remarks>
        /// <example>
        /// <code>
        /// using (var trans = await realm.BeginWriteAsync())
        /// {
        ///     realm.Add(new Dog
        ///     {
        ///         Name = "Rex"
        ///     });
        ///     await trans.CommitAsync();
        ///     // or just
        ///     // trans.Commit();
        /// }
        /// </code>
        /// </example>
        /// <param name="cancellationToken">
        /// Optional cancellation token to stop waiting to start a write transaction.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/> that returns a transaction in write mode.
        /// A transaction is required for any creation, deletion or modification of objects persisted in a <see cref="Realm"/>.</returns>
        public async Task<Transaction> BeginWriteAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfFrozen("Starting a write transaction on a frozen Realm is not allowed.");

            if (!AsyncHelper.TryGetValidContext(out var synchronizationContext))
            {
                return BeginWrite();
            }

            await SharedRealmHandle.BeginTransactionAsync(synchronizationContext, cancellationToken);

            Debug.Assert(_activeTransaction is null, "There should be no active transaction");
            return _activeTransaction = new Transaction(this);
        }

        /// <summary>
        /// Execute an action inside a temporary <see cref="Transaction"/>. If no exception is thrown, the <see cref="Transaction"/> will be committed.
        /// <b>If</b> the method is not called from a thread with a <see cref="SynchronizationContext"/> (like the UI thread), it behaves synchronously.
        /// </summary>
        /// <example>
        /// <code>
        /// await realm.WriteAsync(() =&gt;
        /// {
        ///     realm.Add(new Dog
        ///     {
        ///         Breed = "Dalmatian",
        ///     });
        /// });
        /// </code>
        /// </example>
        /// <param name="action">
        /// Action to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token to stop waiting to start a write transaction.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/> that indicates that the transaction has been committed successfully.</returns>
        public Task WriteAsync(Action action, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            Argument.NotNull(action, nameof(action));

            return WriteAsync(() =>
            {
                action();
                return true;
            }, cancellationToken);
        }

        /// <summary>
        /// Execute a delegate inside a temporary <see cref="Transaction"/>. If no exception is thrown, the <see cref="Transaction"/> will be committed.
        /// <b>If</b> the method is not called from a thread with a <see cref="SynchronizationContext"/> (like the UI thread), it behaves synchronously.
        /// </summary>
        /// <example>
        /// <code>
        /// var dog = await realm.WriteAsync(() =&gt;
        /// {
        ///     return realm.Add(new Dog
        ///     {
        ///         Breed = "Dalmatian",
        ///     });
        /// });
        /// </code>
        /// </example>
        /// <param name="function">
        /// Delegate with one return value to execute inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional cancellation token to stop waiting to start a write transaction.
        /// </param>
        /// <typeparam name="T">The type returned by the input delegate.</typeparam>
        /// <returns>
        /// An awaitable <see cref="Task"/> that indicates that the transaction has been committed successfully. The result of
        /// the task is the result returned by invoking <paramref name="function"/>.
        /// </returns>
        public async Task<T> WriteAsync<T>(Func<T> function, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Argument.NotNull(function, nameof(function));

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.TryGetValidContext(out _))
            {
                return Write(function);
            }

            using var transaction = await BeginWriteAsync(cancellationToken);
            var result = function();

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            await transaction.CommitAsync(CancellationToken.None);
            return result;
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
            ThrowIfDisposed();

            if (!AsyncHelper.TryGetValidContext(out _))
            {
                return Task.FromResult(Refresh());
            }

            return SharedRealmHandle.RefreshAsync();
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must be an <see cref="IRealmObject"/>.</typeparam>
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of class T, in this <see cref="Realm"/>.</returns>
        /// <remarks>
        /// The returned collection is evaluated lazily and no objects are being held in memory by the collection itself, which means
        /// this call is very cheap even for huge number of items.
        /// </remarks>
        public IQueryable<T> All<T>()
            where T : IRealmObject
        {
            ThrowIfDisposed();

            var type = typeof(T);
            Argument.Ensure(
                Metadata.TryGetValue(type.GetMappedOrOriginalName(), out var metadata) && metadata.Schema.Type == type,
                $"The class {type.Name} is not in the limited set of classes for this realm", nameof(T));

            return new RealmResults<T>(this, metadata);
        }

        // This should only be used for tests
        internal IQueryable<T> AllEmbedded<T>()
            where T : IEmbeddedObject
        {
            ThrowIfDisposed();

            var type = typeof(T);
            Argument.Ensure(
                Metadata.TryGetValue(type.GetMappedOrOriginalName(), out var metadata) && metadata.Schema.Type == type,
                $"The class {type.Name} is not in the limited set of classes for this realm", nameof(T));

            return new RealmResults<T>(this, metadata);
        }

        #region Quick Find using primary key

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="IRealmObject"/>.</typeparam>
        /// <param name="primaryKey">
        /// Primary key to be matched exactly, same as an == search.
        /// An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.
        /// </param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T? Find<T>(long? primaryKey)
            where T : IRealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="IRealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T? Find<T>(string? primaryKey)
            where T : IRealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="IRealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T? Find<T>(ObjectId? primaryKey)
            where T : IRealmObject => FindCore<T>(primaryKey);

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="IRealmObject"/>.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
        /// <returns><c>null</c> or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">
        /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
        /// </exception>
        public T? Find<T>(Guid? primaryKey)
            where T : IRealmObject => FindCore<T>(primaryKey);

        internal T? FindCore<T>(RealmValue primaryKey)
            where T : IRealmObject
        {
            ThrowIfDisposed();

            var metadata = Metadata[typeof(T).GetMappedOrOriginalName()];
            if (SharedRealmHandle.TryFindObject(metadata.TableKey, primaryKey, out var objectHandle))
            {
                return (T)MakeObject(metadata, objectHandle);
            }

            return default;
        }

        #endregion Quick Find using primary key

        #region Thread Handover

        /// <summary>
        /// Returns the same object as the one referenced when the <see cref="ThreadSafeReference.Object{T}"/> was first created,
        /// but resolved for the current Realm for this thread.
        /// </summary>
        /// <param name="reference">The thread-safe reference to the thread-confined <see cref="IRealmObject"/>/<see cref="IEmbeddedObject"/> to resolve in this <see cref="Realm"/>.</param>
        /// <typeparam name="T">The type of the object, contained in the reference.</typeparam>
        /// <returns>
        /// A thread-confined instance of the original <see cref="IRealmObject"/>/<see cref="IEmbeddedObject"/> resolved for the current thread or <c>null</c>
        /// if the object has been deleted after the reference was created.
        /// </returns>
        public T? ResolveReference<T>(ThreadSafeReference.Object<T> reference)
            where T : IRealmObjectBase
        {
            Argument.NotNull(reference, nameof(reference));

            var objectPtr = SharedRealmHandle.ResolveReference(reference);
            var objectHandle = new ObjectHandle(SharedRealmHandle, objectPtr);

            if (!objectHandle.IsValid)
            {
                objectHandle.Dispose();
                return default;
            }

            if (!Metadata.TryGetValue(reference.Metadata!.Schema.Name, out var metadata))
            {
                metadata = reference.Metadata;
            }

            return (T)MakeObject(metadata, objectHandle);
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
        public IList<T>? ResolveReference<T>(ThreadSafeReference.List<T> reference)
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
        public ISet<T>? ResolveReference<T>(ThreadSafeReference.Set<T> reference)
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
        /// if the dictionary's parent object has been deleted after the reference was created.
        /// </returns>
        public IDictionary<string, TValue>? ResolveReference<TValue>(ThreadSafeReference.Dictionary<TValue> reference)
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
            where T : IRealmObjectBase
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
        /// <exception cref="ArgumentException">If you pass an unmanaged object.</exception>
        public void Remove(IRealmObjectBase obj)
        {
            ThrowIfDisposed();

            Argument.NotNull(obj, nameof(obj));
            Argument.Ensure(obj.IsManaged, "Object is not managed by Realm, so it cannot be removed.", nameof(obj));

            obj.GetObjectHandle()!.RemoveFromRealm(SharedRealmHandle);
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
        /// If <paramref name="range"/> is not the result of <see cref="All{T}"/> or subsequent LINQ filtering.
        /// </exception>
        /// <exception cref="ArgumentNullException">If <paramref name="range"/> is <c>null</c>.</exception>
        public void RemoveRange<T>(IQueryable<T> range)
            where T : IRealmObjectBase
        {
            ThrowIfDisposed();

            Argument.NotNull(range, nameof(range));
            var results = Argument.EnsureType<RealmResults<T>>(range, "range should be the return value of .All or a LINQ query applied to it.", nameof(range));
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
            where T : IRealmObject
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

            SharedRealmHandle.RemoveAll();
        }

        /// <summary>
        /// Writes a compacted copy of the Realm to the path in the specified config. If the configuration object has
        /// non-null <see cref="RealmConfiguration.EncryptionKey"/>, the copy will be encrypted with that key.
        /// </summary>
        /// <remarks>
        /// 1. The destination file cannot already exist.
        /// 2. When using a local Realm and this is called from within a transaction it writes the current data,
        ///    and not the data as it was when the last transaction was committed.
        /// 3. When using Sync, it is required that all local changes are synchronized with the server before the copy can be written.
        ///    This is to be sure that the file can be used as a starting point for a newly installed application.
        ///    The function will throw if there are pending uploads.
        /// </remarks>
        /// <param name="config">Configuration, specifying the path and optionally the encryption key for the copy.</param>
        public void WriteCopy(RealmConfigurationBase config)
        {
            Argument.NotNull(config, nameof(config));

            if (Config is PartitionSyncConfiguration originalConfig && config is PartitionSyncConfiguration copiedConfig && originalConfig.Partition != copiedConfig.Partition)
            {
                throw new NotSupportedException($"Changing the partition to synchronize on is not supported when writing a Realm copy. Original partition: {originalConfig.Partition}, passed partition: {copiedConfig.Partition}");
            }

            SharedRealmHandle.WriteCopy(config);
        }

        #region Transactions

        internal void DrainTransactionQueue()
        {
            _activeTransaction = null;
            _state.DrainTransactionQueue();
        }

        internal void ExecuteOutsideTransaction(Action action)
        {
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
            private readonly Dictionary<string, Metadata> stringToRealmObjectMetadataDict;
            private readonly Dictionary<TableKey, Metadata> tableKeyToRealmObjectMetadataDict;

            public IEnumerable<Metadata> Values => stringToRealmObjectMetadataDict.Values;

            public RealmMetadata(IEnumerable<Metadata> objectsMetadata)
            {
                stringToRealmObjectMetadataDict = new Dictionary<string, Metadata>();
                tableKeyToRealmObjectMetadataDict = new Dictionary<TableKey, Metadata>();

                Add(objectsMetadata);
            }

            public bool TryGetValue(string? objectType, [MaybeNullWhen(false)] out Metadata metadata)
            {
                if (objectType != null && stringToRealmObjectMetadataDict.TryGetValue(objectType, out metadata))
                {
                    return true;
                }

                metadata = null;
                return false;
            }

            public bool TryGetValue(TableKey tablekey, [MaybeNullWhen(false)] out Metadata metadata) =>
                tableKeyToRealmObjectMetadataDict.TryGetValue(tablekey, out metadata);

            public Metadata this[string objectType] => stringToRealmObjectMetadataDict[objectType];

            public Metadata this[TableKey tablekey] => tableKeyToRealmObjectMetadataDict[tablekey];

            public void Add(IEnumerable<Metadata> objectsMetadata)
            {
                foreach (var objectMetadata in objectsMetadata)
                {
                    if (stringToRealmObjectMetadataDict.ContainsKey(objectMetadata.Schema.Name))
                    {
                        Argument.AssertDebug($"Trying to add object schema to the string mapping that is already present: {objectMetadata.Schema.Name}");
                    }
                    else
                    {
                        stringToRealmObjectMetadataDict[objectMetadata.Schema.Name] = objectMetadata;
                    }

                    if (tableKeyToRealmObjectMetadataDict.ContainsKey(objectMetadata.TableKey))
                    {
                        Argument.AssertDebug($"Trying to add object schema to the table key mapping that is already present: {objectMetadata.Schema.Name} - {objectMetadata.TableKey}");
                    }
                    else
                    {
                        tableKeyToRealmObjectMetadataDict[objectMetadata.TableKey] = objectMetadata;
                    }
                }
            }
        }

        internal class State
        {
            private readonly List<WeakReference<Realm>> _weakRealms = new();

            public readonly Queue<Action> AfterTransactionQueue = new();

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
                Debug.Assert(!GetLiveRealms().Any(other => ReferenceEquals(realm, other)), "Trying to add a duplicate Realm to the RealmState.");

                _weakRealms.Add(new WeakReference<Realm>(realm));
            }

            /// <summary>
            /// Remove a Realm from the list of Realms tracked by this state. This is only called when
            /// Dispose is called on the Realm file and will not execute for garbage collected Realm
            /// instances. This is fine because for GC-ed instances the lifecycle is as:
            /// 1. Instance is GC-ed, its fields are GC-ed.
            /// 2. The SharedRealmHandled is GC-ed, which causes Unbind to be called.
            /// 3. The native pointer is deleted, which calls RealmCoordinator::unregister_realm.
            /// 4. Once the last instance is deleted, the CSharpBindingContext destructor is called, which frees the state GCHandle.
            /// 5. The State is now eligible for collection, and its fields will be GC-ed.
            /// </summary>
            public void RemoveRealm(Realm realm, bool closeOnEmpty)
            {
                _weakRealms.RemoveAll(r =>
                {
                    return !r.TryGetTarget(out var other) || ReferenceEquals(realm, other);
                });

                if (closeOnEmpty && !_weakRealms.Any())
                {
                    realm.SharedRealmHandle.CloseRealm();
                }
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

        /// <summary>
        /// A class that exposes the dynamic API for a <see cref="Realm"/> instance.
        /// </summary>
        [Preserve(AllMembers = true)]
        public readonly struct Dynamic
        {
            private readonly Realm _realm;

            internal Dynamic(Realm realm)
            {
                _realm = realm;
            }

            /// <summary>
            /// Factory for a managed object without a primary key in a realm. Only valid within a write <see cref="Transaction"/>.
            /// </summary>
            /// <returns>A dynamically-accessed Realm object.</returns>
            /// <param name="className">The type of object to create as defined in the schema.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If you use this method on an object that has primary key defined.
            /// </exception>
            /// <remarks>
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public IRealmObjectBase CreateObject(string className) => CreateObjectCore(className, primaryKey: null);

            /// <summary>
            /// Factory for a managed object without a primary key in a realm. Only valid within a write <see cref="Transaction"/>.
            /// </summary>
            /// <returns>A dynamically-accessed Realm object.</returns>
            /// <param name="className">The type of object to create as defined in the schema.</param>
            /// <param name="primaryKey">The primary key of the object to create.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If the type of the <paramref name="primaryKey"/> is different from the one specified in the schema.
            /// </exception>
            /// <remarks>
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public IRealmObjectBase CreateObject(string className, long? primaryKey) => CreateObjectCore(className, primaryKey);

            /// <summary>
            /// Factory for a managed object without a primary key in a realm. Only valid within a write <see cref="Transaction"/>.
            /// </summary>
            /// <returns>A dynamically-accessed Realm object.</returns>
            /// <param name="className">The type of object to create as defined in the schema.</param>
            /// <param name="primaryKey">The primary key of the object to create.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If the type of the <paramref name="primaryKey"/> is different from the one specified in the schema.
            /// </exception>
            /// <remarks>
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public IRealmObjectBase CreateObject(string className, string? primaryKey) => CreateObjectCore(className, primaryKey);

            /// <summary>
            /// Factory for a managed object without a primary key in a realm. Only valid within a write <see cref="Transaction"/>.
            /// </summary>
            /// <returns>A dynamically-accessed Realm object.</returns>
            /// <param name="className">The type of object to create as defined in the schema.</param>
            /// <param name="primaryKey">The primary key of the object to create.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If the type of the <paramref name="primaryKey"/> is different from the one specified in the schema.
            /// </exception>
            /// <remarks>
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public IRealmObjectBase CreateObject(string className, ObjectId? primaryKey) => CreateObjectCore(className, primaryKey);

            /// <summary>
            /// Factory for a managed object without a primary key in a realm. Only valid within a write <see cref="Transaction"/>.
            /// </summary>
            /// <returns>A dynamically-accessed Realm object.</returns>
            /// <param name="className">The type of object to create as defined in the schema.</param>
            /// <param name="primaryKey">The primary key of the object to create.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If the type of the <paramref name="primaryKey"/> is different from the one specified in the schema.
            /// </exception>
            /// <remarks>
            /// If the realm instance has been created from an un-typed schema (such as when migrating from an older version
            /// of a realm) the returned object will be purely dynamic. If the realm has been created from a typed schema as
            /// is the default case when calling <see cref="GetInstance(RealmConfigurationBase)"/> the returned
            /// object will be an instance of a user-defined class.
            /// </remarks>
            public IRealmObjectBase CreateObject(string className, Guid? primaryKey) => CreateObjectCore(className, primaryKey);

            /// <summary>
            /// Factory for a managed embedded object in a realm. Only valid within a write <see cref="Transaction"/>.
            /// Embedded objects need to be owned immediately which is why they can only be created for a specific property.
            /// </summary>
            /// <param name="parent">
            /// The parent <see cref="IRealmObject"/> or <see cref="IEmbeddedObject"/> that will own the newly created
            /// embedded object.
            /// </param>
            /// <param name="propertyName">The property to which the newly created embedded object will be assigned.</param>
            /// <returns>A dynamically-accessed embedded object.</returns>
            public IEmbeddedObject CreateEmbeddedObjectForProperty(IRealmObjectBase parent, string propertyName)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(parent, nameof(parent));
                Argument.Ensure(parent.IsManaged && parent.IsValid, "The object passed as parent must be managed and valid to create an embedded object.", nameof(parent));
                Argument.Ensure(parent.Realm.IsSameInstance(_realm), "The object passed as parent is managed by a different Realm", nameof(parent));
                Argument.Ensure(parent.GetObjectMetadata()!.Schema.TryFindProperty(propertyName, out var property), $"The schema for class {parent.GetType().Name} does not contain a property {propertyName}.", nameof(propertyName));
                Argument.Ensure(_realm.Metadata.TryGetValue(property.ObjectType, out var metadata), $"The class {property.ObjectType} linked to by {parent.GetType().Name}.{propertyName} is not in the limited set of classes for this realm", nameof(propertyName));
                Argument.Ensure(metadata.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject, $"The class {property.ObjectType} linked to by {parent.GetType().Name}.{propertyName} is not embedded", nameof(propertyName));

                var obj = (IEmbeddedObject)metadata.Helper.CreateInstance();
                var handle = parent.GetObjectHandle()!.CreateEmbeddedObjectForProperty(propertyName, parent.GetObjectMetadata()!);

                obj.CreateAndSetAccessor(handle, _realm, metadata);

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
            public IEmbeddedObject AddEmbeddedObjectToList(object list)
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
            public IEmbeddedObject InsertEmbeddedObjectInList(object list, int index)
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
            /// Setting an object at an index will remove the existing object from the list and un-own it. Since unowned embedded objects are automatically deleted,
            /// the old object that the list contained at <paramref name="index"/> will get deleted when the transaction is committed.
            /// </remarks>
            /// <seealso cref="InsertEmbeddedObjectInList"/>
            /// <seealso cref="SetEmbeddedObjectInList"/>
            public IEmbeddedObject SetEmbeddedObjectInList(object list, int index)
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
            public IEmbeddedObject AddEmbeddedObjectToDictionary(object dictionary, string key)
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
            public IEmbeddedObject SetEmbeddedObjectInDictionary(object dictionary, string key)
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
            public IQueryable<IRealmObject> All(string className)
            {
                _realm.ThrowIfDisposed();

                Argument.Ensure(_realm.Metadata.TryGetValue(className, out var metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));
                Argument.Ensure(metadata.Schema.BaseType != ObjectSchema.ObjectType.EmbeddedObject, $"The class {className} represents an embedded object and thus cannot be queried directly.", nameof(className));
                Argument.Ensure(metadata.Schema.BaseType != ObjectSchema.ObjectType.AsymmetricObject, $"The class {className} represents an asymmetric object and thus cannot be queried.", nameof(className));

                return new RealmResults<IRealmObject>(_realm, metadata);
            }

            /// <summary>
            /// Remove all objects of a type from the Realm.
            /// </summary>
            /// <param name="className">Type of the objects to remove as defined in the schema.</param>
            /// <exception cref="RealmInvalidTransactionException">
            /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// If you pass <paramref name="className"/> that does not belong to this Realm's schema.
            /// </exception>
            public void RemoveAll(string className)
            {
                _realm.ThrowIfDisposed();

                var query = (RealmResults<IRealmObject>)All(className);
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
            /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public IRealmObject? Find(string className, long? primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">Primary key to be matched exactly, same as an == search.</param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public IRealmObject? Find(string className, string? primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">
            /// Primary key to be matched exactly, same as an == search.
            /// </param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public IRealmObject? Find(string className, ObjectId? primaryKey) => FindCore(className, primaryKey);

            /// <summary>
            /// Fast lookup of an object for dynamic use, from a class which has a PrimaryKey property.
            /// </summary>
            /// <param name="className">Name of class in dynamic situation.</param>
            /// <param name="primaryKey">
            /// Primary key to be matched exactly, same as an == search.
            /// </param>
            /// <returns><c>null</c> or an object matching the primary key.</returns>
            /// <exception cref="RealmClassLacksPrimaryKeyException">
            /// If the <see cref="IRealmObject"/> class T lacks <see cref="PrimaryKeyAttribute"/>.
            /// </exception>
            public IRealmObject? Find(string className, Guid? primaryKey) => FindCore(className, primaryKey);

            internal IRealmObject? FindCore(string className, RealmValue primaryKey)
            {
                _realm.ThrowIfDisposed();

                var metadata = _realm.Metadata[className];
                if (_realm.SharedRealmHandle.TryFindObject(metadata.TableKey, primaryKey, out var objectHandle))
                {
                    return (IRealmObject)_realm.MakeObject(metadata, objectHandle);
                }

                return null;
            }

            private IRealmObjectBase CreateObjectCore(string className, RealmValue? primaryKey)
            {
                _realm.ThrowIfDisposed();

                Argument.Ensure(_realm.Metadata.TryGetValue(className, out var metadata), $"The class {className} is not in the limited set of classes for this realm", nameof(className));

                var result = metadata.Helper.CreateInstance();

                var pkProperty = metadata.Schema.PrimaryKeyProperty;

                ObjectHandle objectHandle;
                if (pkProperty.HasValue)
                {
                    Argument.Ensure(primaryKey.HasValue, $"The class {className} has primary key defined, but you didn't pass one.", nameof(primaryKey));

                    objectHandle = _realm.SharedRealmHandle.CreateObjectWithPrimaryKey(pkProperty.Value, primaryKey.Value, metadata.TableKey, className, update: false, isNew: out var _);
                }
                else
                {
                    Argument.Ensure(!primaryKey.HasValue, $"The class {className} doesn't have a primary key defined, but you passed {primaryKey}.", nameof(primaryKey));

                    objectHandle = _realm.SharedRealmHandle.CreateObject(metadata.TableKey);
                }

                result.CreateAndSetAccessor(objectHandle, _realm, metadata);

                return result;
            }

            private IEmbeddedObject PerformEmbeddedListOperation(object list, Func<ListHandle, ObjectHandle> getHandle)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(list, nameof(list));

                if (list is not IRealmCollectionBase<ListHandle> realmList)
                {
                    throw new ArgumentException($"Expected list to be IList<EmbeddedObject> but was ${list.GetType().FullName} instead.", nameof(list));
                }

                Argument.Ensure(realmList.Metadata != null, $"Supplied list cannot contain embedded objects because its type is: {list.GetType().FullName}.", nameof(list));

                var obj = (IEmbeddedObject)realmList.Metadata.Helper.CreateInstance();

                obj.CreateAndSetAccessor(getHandle(realmList.NativeHandle), _realm, realmList.Metadata);

                return obj;
            }

            private IEmbeddedObject PerformEmbeddedDictionaryOperation(object dictionary, Func<DictionaryHandle, ObjectHandle> getHandle)
            {
                _realm.ThrowIfDisposed();

                Argument.NotNull(dictionary, nameof(dictionary));

                if (dictionary is not IRealmCollectionBase<DictionaryHandle> realmDict)
                {
                    throw new ArgumentException($"Expected dictionary to be IDictionary<string, EmbeddedObject> but was ${dictionary.GetType().FullName} instead.", nameof(dictionary));
                }

                Argument.Ensure(realmDict.Metadata != null, $"Supplied dictionary cannot contain embedded objects because its type is: {dictionary.GetType().FullName}.", nameof(dictionary));

                var obj = (IEmbeddedObject)realmDict.Metadata.Helper.CreateInstance();

                obj.CreateAndSetAccessor(getHandle(realmDict.NativeHandle), _realm, realmDict.Metadata);

                return obj;
            }
        }
    }
}
