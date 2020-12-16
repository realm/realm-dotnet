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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// Return type for a managed object property when you declare a to-many relationship with IList.
    /// </summary>
    /// <remarks>Relationships are ordered and preserve their order, hence the ability to use ordinal
    /// indexes in calls such as Insert and RemoveAt.
    /// </remarks>
    /// <typeparam name="T">Type of the <see cref="RealmObject"/>, <see cref="EmbeddedObject"/>, or primitive which is contained by the list.</typeparam>
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider, IRealmList
    {
        private readonly ListHandle _listHandle;

        internal RealmList(Realm realm, ListHandle adoptedList, RealmObjectBase.Metadata metadata) : base(realm, metadata)
        {
            _listHandle = adoptedList;
        }

        internal override CollectionHandleBase GetOrCreateHandle() => _listHandle;

        ListHandle IRealmList.NativeHandle => _listHandle;

        RealmObjectBase.Metadata IRealmList.Metadata => Metadata;

        [IndexerName("Item")]
        public new T this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                var realmValue = Operator.Convert<T, RealmValue>(value);

                if (_isEmbedded)
                {
                    Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.SetEmbedded(index));
                    return;
                }

                if (_argumentType == RealmValueType.Object)
                {
                    AddToRealmIfNecessary(realmValue);
                }

                _listHandle.Set(index, realmValue);
            }
        }

        #region implementing IList members

        public void Add(T value)
        {
            var realmValue = Operator.Convert<T, RealmValue>(value);

            if (_isEmbedded)
            {
                Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.AddEmbedded());
                return;
            }

            if (_argumentType == RealmValueType.Object)
            {
                AddToRealmIfNecessary(realmValue);
            }

            _listHandle.Add(realmValue);
        }

        public override int IndexOf(T value)
        {
            var realmValue = Operator.Convert<T, RealmValue>(value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsRealmObject().IsManaged)
            {
                throw new ArgumentException("Value does not belong to a realm", nameof(value));
            }

            return _listHandle.Find(realmValue);
        }

        public void Insert(int index, T value)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var realmValue = Operator.Convert<T, RealmValue>(value);

            if (_isEmbedded)
            {
                Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.InsertEmbedded(index));
                return;
            }

            if (_argumentType == RealmValueType.Object)
            {
                AddToRealmIfNecessary(realmValue);
            }

            _listHandle.Insert(index, realmValue);
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
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _listHandle.Erase((IntPtr)index);
        }

        #endregion

        public void Move(int sourceIndex, int targetIndex)
        {
            if (targetIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetIndex));
            }

            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            _listHandle.Move((IntPtr)sourceIndex, (IntPtr)targetIndex);
        }

        internal override RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmList<T>(realm, (ListHandle)handle, Metadata);

        protected override T GetValueAtIndex(int index) => _listHandle.GetValueAtIndex(index, Metadata, Realm).As<T>();

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);

        private void AddToRealmIfNecessary(in RealmValue value)
        {
            var robj = value.AsRealmObject<RealmObject>();
            if (!robj.IsManaged)
            {
                Realm.Add(robj);
            }
        }

        private static EmbeddedObject EnsureUnmanagedEmbedded(in RealmValue value)
        {
            var result = value.AsRealmObject<EmbeddedObject>();
            if (result.IsManaged)
            {
                throw new RealmException("Can't add, set, or insert an embedded object that is already managed.");
            }

            return result;
        }
    }

    /// <summary>
    /// IRealmList is only implemented by RealmList and serves to expose the ListHandle without knowing the generic param.
    /// </summary>
    internal interface IRealmList
    {
        /// <summary>
        /// Gets the native handle for that list.
        /// </summary>
        ListHandle NativeHandle { get; }

        /// <summary>
        /// Gets the metadata for the objects contained in the list.
        /// </summary>
        RealmObjectBase.Metadata Metadata { get; }
    }
}