/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 
 
 
using System;
using System.Collections.Generic;
// using System.Runtime.ConstrainedExecution;

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
            // TODO decide if this can be removed or if that would make signatures different
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Factory for a Realm instance for this thread.
        /// </summary>
        /// <param name="config">Optional configuration.</param>
        /// <returns>A realm instance.</returns>
        /// <exception cref="RealmFileAccessErrorException">Throws error if the filesystem has an error preventing file creation.</exception>
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static Realm GetInstance(RealmConfiguration config=null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }  // GetInstance


        private static IntPtr GenerateObjectSchema(Type objectClass)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        #endregion

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
        public bool IsClosed => false;


        /// <summary>
        ///  Closes the Realm if not already closed. Safe to call repeatedly.
        /// </summary>
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Close()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }


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
        /// <param name="rhs">The <see cref="System.Object"/> to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public override bool Equals(Object rhs)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }


        /// <summary>
        /// Determines whether the specified Realm is equal to the current Realm.
        /// </summary>
        /// <param name="rhs">The Realm to compare with the current Realm.</param>
        /// <returns><c>true</c> if the Realms are functionally equal.</returns>
        public  bool Equals(Realm rhs)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
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
        static public void DeleteRealm(RealmConfiguration configuration)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Execute an action inside a transaction. If no exception is thrown, the transaction will automatically
        /// be committed.
        /// </summary>
        /// <example>
        /// realm.Write(() => 
        /// {
        ///     d = myrealm.CreateObject<Dog>();
        ///     d.Name = "Eddie";
        ///     d.Age = 5;
        /// });
        /// </example>
        /// <param name="action">Action to perform inside transaction.</param>
        public void Write(Action action)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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
        /// <typeparam name="T">The Type T must not only be a RealmObject but also have been processd by the Fody weaver, so it has persistent properties.</typeparam>
        /// <returns>A RealmResults that without further filtering, allows iterating all objects of class T, in this realm.</returns>
        public RealmResults<T> All<T>() where T: RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Removes a persistent object from this realm, effectively deleting it.
        /// </summary>
        /// <param name="obj">Must be an object persisted in this realm.</param>
        /// <exception cref="RealmOutsideTransactionException">If you invoke this when there is no write Transaction active on the realm.</exception>
        /// <exception cref="System.ArgumentNullException">If you invoke this with a standalone object.</exception>
        public void Remove(RealmObject obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove objects matcing a query from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        /// <param name="range">The query to match for.</param>
        public void RemoveRange<T>(RealmResults<T> range) where T: RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of a type from the realm.
        /// </summary>
        /// <typeparam name="T">Type of the objects to remove.</typeparam>
        public void RemoveAll<T>() where T: RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Remove all objects of all types managed by this realm.
        /// </summary>
        public void RemoveAll()
        {
        }
    }
}
