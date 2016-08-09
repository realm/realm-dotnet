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

/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;

namespace Realms
{
    /// <summary>
    /// Return type for a managed object property when you declare a to-many relationship with IList. 
    /// </summary>
    /// <remarks>Relationships are ordered and preserve their order, hence the ability to use ordinal 
    /// indexes in calls such as Insert and RemoveAt.
    /// </remarks>
    /// <remarks>Although originally used in declarations, whilst that still compiles, 
    /// it is <b>not</b> recommended as the IList approach both supports standalone objects and is 
    /// implemented with a faster binding.
    /// </remarks>
    /// 
    /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
    public class RealmList<T> : IList<T>, IRealmList, IDynamicMetaObjectProvider, ICopyValuesFrom where T : RealmObject
    {

        /// <summary>
        /// Value returned by IndexOf if an item is not found.
        /// </summary>
        public const int ITEM_NOT_FOUND = -1;

        #region implementing IList properties
        /// <summary>
        /// Returns the count of related items.
        /// </summary>
        /// <returns>0 if there are no related items, including a "null" relationship never established, or the count of items.</returns>
        public int Count
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return 0;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the item at the ordinal index.
        /// </summary>
        /// <param name="index">Ordinal zero-based index of the related items.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>A related item, if exception not thrown.</returns>
        /// <exception cref="IndexOutOfRangeException">When the index is out of range for the related items.</exception>
        public T this[int index]
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return default(T);
            }

            set
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }
        #endregion

        #region implementing IList members


        /// <summary>
        /// Makes a relationship to an item, appending it at the end of the sorted relationship.
        /// </summary>
        /// <param name="item">RealmObject being added to the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        public void Add(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Breaks the relationship to all related items, without deleting the items.
        /// </summary>
        public void Clear()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Tests if an item exists in the related set.
        /// </summary>
        /// <param name="item">Object to be searched for in the related items.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>True if found, false if not found.</returns>
        public bool Contains(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Copies all the elements to a portion of an array.
        /// </summary>
        /// <param name="array">Preallocated destination into which we copy.</param>
        /// <param name="arrayIndex">Ordinal zero-based starting index of the <b>destination</b> of the related items being copied.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown if there is not enough room in array from arrayIndex onward.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }


        /// <summary>
        /// Factory for an iterator to be called explicitly or used in a foreach loop.
        /// </summary>
        /// <returns>A RealmListEnumerator as the generic IEnumerator<T>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Finds an ordinal index for an item in a relationship.
        /// </summary>
        /// <param name="item">RealmObject being removed from the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>0-based index if the item was found in the related set, or RealmList.ITEM_NOT_FOUND.</returns>
        public int IndexOf(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        /// <summary>
        /// Makes a relationship to an item, inserting at a specified location ahead of whatever else was in that location.
        /// </summary>
        /// <param name="index">Ordinal zero-based index at which to insert the related items.</param>
        /// <param name="item">RealmObject being inserted into the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <exception cref="IndexOutOfRangeException">When the index is out of range for the related items.</exception>
        public void Insert(int index, T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Breaks the relationship to the specified item, without deleting the item.
        /// </summary>
        /// <param name="item">RealmObject being removed from the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>True if the item was found and removed, false if it is not in the related set.</returns>
        public bool Remove(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Breaks the relationship to the item at the ordinal index, without deleting the item.
        /// </summary>
        /// <param name="index">Ordinal zero-based index of the related item.</param>
        /// <exception cref="IndexOutOfRangeException">When the index is out of range for the related items.</exception>
        public void RemoveAt(int index)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        private void ManageObjectIfNeeded(T obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        void ICopyValuesFrom.CopyValuesFrom(IEnumerable<RealmObject> values)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #region implementing IRealmList properties
        Realm IRealmList.Realm { get; }

        LinkListHandle IRealmList.Handle { get; }

        #endregion

    }

    internal class LinkListHandle
    {
    }


    [Preserve(AllMembers = true)]
    internal interface IRealmList
    {
        Realm Realm { get; }
        LinkListHandle Handle { get; }
    }

}