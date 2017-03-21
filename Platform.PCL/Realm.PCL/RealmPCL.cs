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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Realms.Exceptions;
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

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1409:RemoveUnnecessaryCode")]
        static Realm()
        {
            // TODO decide if this can be removed or if that would make signatures different
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Compacts a Realm file. A Realm file usually contains free/unused space. This method removes this free space and the file size is thereby reduced. Objects within the Realm files are untouched.
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Deletes all the files associated with a realm.
        /// </summary>
        /// <param name="configuration">A <see cref="RealmConfigurationBase"/> which supplies the realm path.</param>
        public static void DeleteRealm(RealmConfigurationBase configuration)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="RealmConfigurationBase"/> that controls this realm's path and other settings.
        /// </summary>
        /// <value>The Realm's configuration.</value>
        public RealmConfigurationBase Config { get; }

        /// <summary>
        /// Gets the <see cref="RealmSchema"/> instance that describes all the types that can be stored in this <see cref="Realm"/>.
        /// </summary>
        /// <value>The Schema of the Realm.</value>
        public RealmSchema Schema { get; }

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

        /// <summary>
        /// Triggered when a Realm-level exception has occurred.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets a value indicating whether the instance has been closed via <see cref="Dispose"/>. If <c>true</c>, you
        /// should not call methods on that instance.
        /// </summary>
        /// <value><c>true</c> if closed, <c>false</c> otherwise.</value>
        public bool IsClosed { get; }

        /// <summary>
        /// Dispose automatically closes the Realm if not already closed.
        /// </summary>
        public void Dispose()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(T);
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
        /// <see cref="Action"/> to perform inside a <see cref="Transaction"/>, creating, updating, or removing objects.
        /// </param>
        public void Write(Action action)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Update the <see cref="Realm"/> instance and outstanding objects to point to the most recent persisted version.
        /// </summary>
        /// <returns>
        /// Whether the <see cref="Realm"/> had any updates. Note that this may return true even if no data has actually changed.
        /// </returns>
        public bool Refresh()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must be a <see cref="RealmObject"/>.</typeparam>
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of class T, in this <see cref="Realm"/>.</returns>
        public IQueryable<T> All<T>() where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Get a view of all the objects of a particular type.
        /// </summary>
        /// <param name="className">The type of the objects as defined in the schema.</param>
        /// <remarks>Because the objects inside the view are accessed dynamically, the view cannot be queried into using LINQ or other expression predicates.</remarks>
        /// <returns>A queryable collection that without further filtering, allows iterating all objects of className, in this realm.</returns>
        public IQueryable<dynamic> All(string className)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of all types managed by this Realm.
        /// </summary>
        /// <exception cref="RealmInvalidTransactionException">
        /// If you invoke this when there is no write <see cref="Transaction"/> active on the <see cref="Realm"/>.
        /// </exception>
        public void RemoveAll()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

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
        public void Manage<T>(T obj, bool update) where T : RealmObject
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion
    }
}
