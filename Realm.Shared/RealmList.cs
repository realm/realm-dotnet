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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Realms
{
    /// <summary>
    /// Used to declare to-many relationships and as the return type when you access such a relationship.
    /// </summary>
    /// <remarks>Relationships are ordered and preserve their order, hence the ability to use ordinal 
    /// indexes in calls such as Insert and RemoveAt.
    /// </remarks>
    /// 
    /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
    public class RealmList<T> : IList<T> where T : RealmObject
    {
        private class RealmListEnumerator : IEnumerator<T> 
        {
            private int index;
            private RealmList<T> enumerating;

            internal RealmListEnumerator(RealmList<T> parent)
            {
                index = -1;
                enumerating = parent;
            }

            /// <summary>
            /// Return the current related object when iterating a related set.
            /// </summary>
            /// <exception cref="IndexOutOfRangeException">When we are not currently pointing at a valid item, either MoveNext has not been called for the first time or have iterated through all the items.</exception>
            public T Current
            {
                get
                {
                    return enumerating[index];
                }
            }

            // also needed - https://msdn.microsoft.com/en-us/library/s793z9y2.aspx
            object IEnumerator.Current
            {
                get
                {
                    return enumerating[index];
                }
            }

            /// <summary>
            ///  Move the iterator to the next related object, starting "before" the first object.
            /// </summary>
            /// <returns>True only if can advance.</returns>
            public bool MoveNext()
            {
                index++;
                if (index >= enumerating.Count)
                    return false;
                return true;
            }

            /// <summary>
            /// Reset the iter to before the first object, so MoveNext will move to it.
            /// </summary>
            public void Reset()
            {
                index = -1;  // by definition BEFORE first item
            }

            /// <summary>
            /// Standard Dispose with no side-effects.
            /// </summary>
            public void Dispose() 
            {
            }
        }


        /// <summary>
        /// Value returned by IndexOf if an item is not found.
        /// </summary>
        public const int ITEM_NOT_FOUND = -1;

        private RealmObject _parent;  // we only make sense within an owning object
        private LinkListHandle _listHandle;

        internal RealmList(RealmObject parent, LinkListHandle adoptedList)
        {
            _parent = parent;
            _listHandle = adoptedList;
        }

        #region implementing IList properties
        /// <summary>
        /// Returns the count of related items.
        /// </summary>
        /// <returns>0 if there are no related items, including a "null" relationship never established, or the count of items.</returns>
        public int Count
        {
            get
            {
                if (_listHandle.IsInvalid)
                    return 0;
                return (int)NativeLinkList.size (_listHandle);
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
                if (index < 0)
                    throw new IndexOutOfRangeException ();
                var linkedRowPtr = NativeLinkList.get (_listHandle, (IntPtr)index);
                return (T)_parent.MakeRealmObject(typeof(T), linkedRowPtr);
            }

            set
            {
                throw new NotImplementedException();
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
            this.ManageObjectIfNeeded(item);
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            NativeLinkList.add(_listHandle, (IntPtr)rowIndex);        
        }

        /// <summary>
        /// Breaks the relationship to all related items, without deleting the items.
        /// </summary>
        public void Clear()
        {
            NativeLinkList.clear(_listHandle);        
        }

        /// <summary>
        /// Tests if an item exists in the related set.
        /// </summary>
        /// <param name="item">Object to be searched for in the related items.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>True if found, false if not found.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) != ITEM_NOT_FOUND;
        }

        /// <summary>
        /// Copies all the elements to a portion of an array.
        /// </summary>
        /// <param name="index">Ordinal zero-based starting index of the <b>destination</b> of thef related items being copied.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown if there is not enough room in array from arrayIndex onward.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();
            if ((arrayIndex + Count) > array.Length)
                throw new ArgumentException();            
            foreach (var obj in this) {
                array[arrayIndex++] = obj;
            }
        }


        /// <summary>
        /// Factory for an iterator to be called explicitly or used in a foreach loop.
        /// </summary>
        /// <returns>A RealmListEnumerator as the generic IEnumerator<T>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new RealmListEnumerator(this);
        }

        /// <summary>
        /// Finds an ordinal index for an item in a relationship.
        /// </summary>
        /// <param name="item">RealmObject being removed from the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>0-based index if the item was found in the related set, or RealmList.ITEM_NOT_FOUND.</returns>
        public int IndexOf(T item)
        {
            if (!item.IsManaged)
                throw new ArgumentException("Value does not belong to a realm", nameof(item));

            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            return (int)NativeLinkList.find(_listHandle, (IntPtr)rowIndex, (IntPtr)0);        
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
            if (index < 0)
                throw new IndexOutOfRangeException ();

            this.ManageObjectIfNeeded(item);
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            NativeLinkList.insert(_listHandle, (IntPtr)index, (IntPtr)rowIndex);        
        }

        /// <summary>
        /// Breaks the relationship to the specified item, without deleting the item.
        /// </summary>
        /// <param name="item">RealmObject being removed from the relationship.</param>
        /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
        /// <returns>True if the item was found and removed, false if it is not in the related set.</returns>
        public bool Remove(T item)
        {
            int index = IndexOf (item);
            if (index == ITEM_NOT_FOUND)
                return false;
            RemoveAt (index);
            return true;
        }

        /// <summary>
        /// Breaks the relationship to the item at the ordinal index, without deleting the item.
        /// </summary>
        /// <param name="index">Ordinal zero-based index of the related item.</param>
        /// <exception cref="IndexOutOfRangeException">When the index is out of range for the related items.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException ();
            NativeLinkList.erase(_listHandle, (IntPtr)index);        
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RealmListEnumerator(this);
        }

        private void ManageObjectIfNeeded(T obj)
        {
            if (!obj.IsManaged)
                _parent.Realm.Manage(obj);
        }

        #endregion
    }
}