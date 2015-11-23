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
                throw new NotImplementedException();
            }
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class RealmList<T> : IList<T> where T : RealmObject
    {
        private RealmObject _parent;  // we only make sense within an owning object
        private LinkListHandle _listHandle;

        internal void CompleteInit(RealmObject parent, LinkListHandle adoptedList)
        {
            _parent = parent;
            _listHandle = adoptedList;
            var modelName = parent.GetType().Name;

            if (!parent.GetType().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The parent type " + modelName + " is a RealmObject but it has not been woven.");

            if (!typeof(T).GetType().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The list contains a type " + typeof(T).Name + " which is a RealmObject but it has not been woven.");
        }

        #region implementing IList members
        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            var ro = item as RealmObject;
            Debug.Assert (ro != null, "can't have instantiated collection without being Realm objects");
            var rowIndex = ro.RowHandle.RowIndex;
            NativeLinkList.add(_listHandle, (IntPtr)rowIndex);        
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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