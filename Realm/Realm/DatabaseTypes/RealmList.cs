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
using System.Reflection;
using System.Runtime.CompilerServices;
using Realms.Dynamic;
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
    public class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider, IRealmCollectionBase<ListHandle>
    {
        private readonly ListHandle _listHandle;

        internal RealmList(Realm realm, ListHandle adoptedList, Metadata? metadata) : base(realm, metadata)
        {
            _listHandle = adoptedList;
        }

        internal override CollectionHandleBase GetOrCreateHandle() => _listHandle;

        ListHandle IRealmCollectionBase<ListHandle>.NativeHandle => _listHandle;

        [IndexerName("Item")]
        public new T this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                ValidateIndex(index);
                var realmValue = ValidateValueToInsert(value);

                if (realmValue.Type.IsCollection())
                {
                    CreateInternalCollectionAndPopulate(realmValue, _listHandle.SetCollection(index, realmValue.Type));
                    return;
                }

                if (_isEmbedded)
                {
                    if (IsDynamic)
                    {
                        throw new NotSupportedException("Can't set embedded objects directly. Instead use Realm.DynamicApi.SetEmbeddedObjectInList.");
                    }

                    Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.SetEmbedded(index));
                    return;
                }

                AddToRealmIfNecessary(realmValue);
                _listHandle.Set(index, realmValue);
            }
        }

        #region implementing IList members

        public void Add(T value)
        {
            var realmValue = ValidateValueToInsert(value);

            if (realmValue.Type.IsCollection())
            {
                CreateInternalCollectionAndPopulate(realmValue, _listHandle.AddCollection(realmValue.Type));
                return;
            }

            if (_isEmbedded)
            {
                if (IsDynamic)
                {
                    throw new NotSupportedException("Can't add embedded objects directly. Instead use Realm.DynamicApi.AddEmbeddedObjectToList.");
                }

                Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.AddEmbedded());
                return;
            }

            AddToRealmIfNecessary(realmValue);
            _listHandle.Add(realmValue);
        }

        public override int IndexOf([AllowNull] T value)
        {
            var realmValue = Operator.Convert<T?, RealmValue>(value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return -1;
            }

            if (realmValue.Type.IsCollection())
            {
                return -1;
            }

            return _listHandle.Find(realmValue);
        }

        public void Insert(int index, T value)
        {
            ValidateIndex(index);
            var realmValue = ValidateValueToInsert(value);

            if (realmValue.Type.IsCollection())
            {
                CreateInternalCollectionAndPopulate(realmValue, _listHandle.InsertCollection(index, realmValue.Type));
                return;
            }

            if (_isEmbedded)
            {
                if (IsDynamic)
                {
                    throw new NotSupportedException("Can't insert embedded objects directly. Instead use Realm.DynamicApi.InsertEmbeddedObjectInList.");
                }

                Realm.ManageEmbedded(EnsureUnmanagedEmbedded(realmValue), _listHandle.InsertEmbedded(index));
                return;
            }

            AddToRealmIfNecessary(realmValue);
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

        public override void RemoveAt(int index)
        {
            ValidateIndex(index);

            _listHandle.Erase(index);
        }

        #endregion

        public void Move(int sourceIndex, int targetIndex)
        {
            ValidateIndex(sourceIndex, nameof(sourceIndex));
            ValidateIndex(targetIndex, nameof(targetIndex));

            _listHandle.Move((IntPtr)sourceIndex, (IntPtr)targetIndex);
        }

        internal RealmResults<T> ToResults()
        {
            var resultsHandle = _listHandle.ToResults();
            return new RealmResults<T>(Realm, resultsHandle, Metadata);
        }

        internal override RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmList<T>(realm, (ListHandle)handle, Metadata);

        protected override T GetValueAtIndex(int index) => _listHandle.GetValueAtIndex(index, Realm).As<T>();

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);

        private static void ValidateIndex(int index, string name = "index")
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private RealmValue ValidateValueToInsert(T value)
        {
            // Lists of objects may not contain null
            if (Metadata is not null && value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Operator.Convert<T, RealmValue>(value);
        }
    }
}
