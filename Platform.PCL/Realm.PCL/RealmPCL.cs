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

// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
//// using System.Runtime.ConstrainedExecution;

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

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1409:RemoveUnnecessaryCode")]
        static Realm()
        {
            // TODO decide if this can be removed or if that would make signatures different
        }

        /// <summary>
        /// Gets the <see cref="RealmConfiguration"/> that controls this realm's path and other settings.
        /// </summary>
        public RealmConfiguration Config { get; private set; }

        /// <summary>
        /// Gets the <see cref="RealmSchema"/> instance that describes all the types that can be stored in this <see cref="Realm"/>.
        /// </summary>
        public RealmSchema Schema { get; }

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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="config">Optional configuration.</param>
        /// <returns>A realm instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the file system returns an error, preventing file creation.</exception>
        //// [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static Realm GetInstance(RealmConfiguration config = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }  // GetInstance

        #endregion

        /// <summary>
        /// Handler type used by <see cref="RealmChanged"/> 
        /// </summary>
        /// <param name="sender">The Realm which has changed.</param>
        /// <param name="e">Currently an empty argument, in future may indicate more details about the change.</param>
        public delegate void RealmChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Triggered when a realm has changed (i.e. a transaction was committed)
        /// </summary>
        public event RealmChangedEventHandler RealmChanged;

        /// <summary>
        /// Checks if database has been closed.
        /// </summary>
        /// <returns>True if closed.</returns>
        public bool IsClosed => false;

        /// <summary>
        ///  Dispose automatically closes the Realm if not already closed.
        /// </summary>
        public void Dispose()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Generic override determines whether the specified <see cref="System.Object"/> is equal to the current Realm.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public override bool Equals(object obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Determines whether the specified Realm is equal to the current Realm.
        /// </summary>
        /// <param name="other">The Realm to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public bool Equals(Realm other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
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
        /// Serves as a hash function for a Realm based on the core instance.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        /// <summary>
        ///  Deletes all the files associated with a realm. Hides knowledge of the auxiliary filenames from the programmer.
        /// </summary>
        /// <param name="configuration">A configuration which supplies the realm path.</param>
        public static void DeleteRealm(RealmConfiguration configuration)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
        public void Add<T>(T obj, bool update) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Update a Realm and outstanding objects to point to the most recent data for this Realm.
        /// </summary>
        /// <returns>
        /// Whether the realm had any updates. Note that this may return true even if no data has actually changed.
        /// </returns>
        public bool Refresh()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Extract an iterable set of objects for direct use or further query.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of class T, in this realm.</returns>
        public IQueryable<T> All<T>() where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Get a view of all the objects of a particular type
        /// </summary>
        /// <param name="className">The type of the objects as defined in the schema.</param>
        /// <remarks>Because the objects inside the view are accessed dynamically, the view cannot be queried into using LINQ or other expression predicates.</remarks>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of className, in this realm.</returns>
        public IQueryable<dynamic> All(string className)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Fast lookup of an object from a class which has a PrimaryKey property.
        /// </summary>
        /// <typeparam name="T">The Type T must be a RealmObject.</typeparam>
        /// <param name="primaryKey">Primary key to be matched exactly, same as an == search. An argument of type <c>long?</c> works for all integer properties, supported as PrimaryKey.</param>
        /// <returns>Null or an object matching the primary key.</returns>
        /// <exception cref="RealmClassLacksPrimaryKeyException">If the RealmObject class T lacks an [PrimaryKey].</exception>
        public T Find<T>(long? primaryKey) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Removes a persistent object from this realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this realm.</param>
        /// <exception cref="RealmInvalidTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="System.ArgumentNullException">If you invoke this with a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove objects matching a query from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        public void RemoveRange<T>(IQueryable<T> range)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        public void RemoveAll<T>() where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <param name="className">Type of the objects to remove as defined in the schema.</param>
        public void RemoveAll(string className)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of all types managed by this realm.
        /// </summary>
        public void RemoveAll()
        {
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
        /// <exception cref="RealmObjectAlreadyManagedByRealmException">You can't manage the same object twice. This exception is thrown, rather than silently detecting the mistake, to help you debug your code</exception>
        /// <exception cref="RealmObjectManagedByAnotherRealmException">You can't manage an object with more than one realm</exception>
        [Obsolete("This method has been renamed. Use Add for the same results.")]
        public void Manage<T>(T obj, bool update) where T : RealmObject
        {
            Add(obj, update);
        }

        /// <summary>
        ///  Closes the Realm if not already closed. Safe to call repeatedly.
        /// </summary>
        //// [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [Obsolete("This method has been deprecated. Instead, dispose the realm to close it.")]
        public void Close()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion
    }
}
