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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Realms.Dynamic;

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
    /// <typeparam name="T">Type of the RealmObject which is the target of the relationship.</typeparam>
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider where T : RealmObject
    {
        private Realm _realm;
        private ListHandle _listHandle;

        internal RealmList(Realm realm, ListHandle adoptedList, RealmObject.Metadata metadata) : base(realm, metadata)
        {
            _realm = realm;
            _listHandle = adoptedList;
        }

        internal override CollectionHandleBase CreateHandle()
        {
            return _listHandle;
        }

        #region implementing IList properties

        public bool IsReadOnly => (_realm?.Config as RealmConfiguration)?.IsReadOnly == true;

        [IndexerName("Item")]
        public new T this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                throw new NotSupportedException("Setting items directly is not supported.");
            }
        }

        #endregion

        #region implementing IList members

        public void Add(T item)
        {
            AddObjectToRealmIfNeeded(item);
            _listHandle.Add(item.ObjectHandle);
        }

        public void Clear()
        {
            _listHandle.Clear();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (arrayIndex + Count > array.Length)
            {
                throw new ArgumentException();
            }

            foreach (var obj in this)
            {
                array[arrayIndex++] = obj;
            }
        }

        public int IndexOf(T item)
        {
            if (!item.IsManaged)
            {
                throw new ArgumentException("Value does not belong to a realm", nameof(item));
            }

            return (int)_listHandle.Find(item.ObjectHandle);
        }

        public void Insert(int index, T item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            AddObjectToRealmIfNeeded(item);
            _listHandle.Insert((IntPtr)index, item.ObjectHandle);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _listHandle.Erase((IntPtr)index);
        }

        private void AddObjectToRealmIfNeeded(T obj)
        {
            if (!obj.IsManaged)
            {
                _realm.Add(obj);
            }
        }

        #endregion

        public void Move(T item, int targetIndex)
        {
            if (targetIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _listHandle.Move(item.ObjectHandle, (IntPtr)targetIndex);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);
    }
}