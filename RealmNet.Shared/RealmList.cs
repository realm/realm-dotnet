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
    public class RealmListEnumerator<T> : IEnumerator
    {

        public object Current
        {
            get
            {
                return null;  // TODO return real object
                // throw new NotImplementedException();
            }
        }

        public bool MoveNext()
        {
            return false;  // TODO return real iteration status
            //throw new NotImplementedException();
        }

        public void Reset()
        {
            // TODO clear the relatinships
            // throw new NotImplementedException();
        }
    }

    public class RealmList<T> : IList<T> where T : RealmObject
    {
        public const int ITEM_NOT_FOUND = -1;

        private RealmObject _parent;  // we only make sense within an owning object
        private LinkListHandle _listHandle;

        internal void CompleteInit(RealmObject parent, LinkListHandle adoptedList)
        {
            _parent = parent;
            _listHandle = adoptedList;

            if (!parent.GetType ().GetCustomAttributes (typeof(WovenAttribute), true).Any ())
            {
                var modelName = parent.GetType ().Name;
                Debug.WriteLine ("WARNING! The parent type " + modelName + " is a RealmObject but it has not been woven.");
            }
            // suppressing the following warning as it is apparently ALWAYS triggered - the type bound here is NOT the woven object
            /*
            if (!typeof(T).GetType().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The list contains a type " + typeof(T).Name + " which is a RealmObject but it has not been woven.");
            */
        }

        #region implementing IList members
        public T this[int index]
        {
            get
            {
                var linkedRowPtr = NativeLinkList.get (_listHandle, (IntPtr)index);
                return (T)_parent.MakeRealmObject(typeof(T), linkedRowPtr);
            }

            set
            {
                throw new NotImplementedException();
            }
        }

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
            get
            {
                return false;  // TODO decide if this is realistic
            }
        }

        public void Add(T item)
        {
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            NativeLinkList.add(_listHandle, (IntPtr)rowIndex);        
        }

        public void Clear()
        {
            // TODO clear relationship
            //throw new NotImplementedException();
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
            return null;  // TODO return real iter
            //throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            var rowIndex = ((RealmObject)item).RowHandle.RowIndex;
            return (int)NativeLinkList.find(_listHandle, (IntPtr)rowIndex, (IntPtr)0);        
        }

        public void Insert(int index, T item)
        {
            // TODO return real object
            //throw new NotImplementedException();
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
            NativeLinkList.erase(_listHandle, (IntPtr)index);        
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;  // TODO return real object iter
            //throw new NotImplementedException();
        }

        #endregion
        /*
                protected T GetValue<T>(string propertyName)
                {
                    return _coreProvider.GetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex);
                }

                protected void SetValue<T>(string propertyName, T value)
                {
                    _coreProvider.SetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex, value);
                }
        */


    }
}