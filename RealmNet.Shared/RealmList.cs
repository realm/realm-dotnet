/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace RealmNet
{
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

            public bool MoveNext()
            {
                index++;
                if (index >= enumerating.Count)
                    return false;
                return true;
            }

            public void Reset()
            {
                index = -1;  // by definition BEFORE first item
            }

            public void Dispose() 
            {
            }
        }


        public const int ITEM_NOT_FOUND = -1;

        private RealmObject _parent;  // we only make sense within an owning object
        private LinkListHandle _listHandle;

        internal void CompleteInit(RealmObject parent, LinkListHandle adoptedList)
        {
            _parent = parent;
            _listHandle = adoptedList;
        }

        #region implementing IList properties
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


        public void Add(T item)
        {
            this.AttachObjectIfNeeded(item);
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            NativeLinkList.add(_listHandle, (IntPtr)rowIndex);        
        }

        public void Clear()
        {
            NativeLinkList.clear(_listHandle);        
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != ITEM_NOT_FOUND;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)new RealmListEnumerator(this);
        }


        public int IndexOf(T item)
        {
            if (!item.IsManaged)
                throw new ArgumentException("Value does not belong to a realm", nameof(item));

            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            return (int)NativeLinkList.find(_listHandle, (IntPtr)rowIndex, (IntPtr)0);        
        }

        public void Insert(int index, T item)
        {
            if (index < 0)
                throw new IndexOutOfRangeException ();

            this.AttachObjectIfNeeded(item);
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            NativeLinkList.insert(_listHandle, (IntPtr)index, (IntPtr)rowIndex);        
        }

        public bool Remove(T item)
        {
            int index = IndexOf (item);
            if (index == ITEM_NOT_FOUND)
                return false;
            RemoveAt (index);
            return true;
        }

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

        private void AttachObjectIfNeeded(T obj)
        {
            if (!obj.IsManaged)
                _parent.Realm.Attach(obj);
        }

        #endregion
    }
}