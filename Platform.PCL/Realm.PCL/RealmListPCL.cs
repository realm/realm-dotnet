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
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
////ASD using Realms.Dynamic;

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
    internal class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider where T : RealmObject
    {
        internal RealmList(Realm realm, ListHandle adoptedList, RealmObject.Metadata metadata) : base(realm, metadata)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected override CollectionHandleBase CreateHandle()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #region implementing IList properties

        public bool IsReadOnly
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }
        }

        [IndexerName("Item")]
        public new T this[int index]
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void Clear()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public bool Contains(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public int IndexOf(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public void Insert(int index, T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public bool Remove(T item)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return true;
        }

        public void RemoveAt(int index)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private void AddObjectToRealmIfNeeded(T obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        public void Move(T item, int targetIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}