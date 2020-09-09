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
using Realms.Helpers;
using Realms.Native;
using Realms.Schema;

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
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider
    {
        private readonly Realm _realm;
        private readonly ListHandle _listHandle;

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

        public override bool IsReadOnly => (_realm?.Config as RealmConfiguration)?.IsReadOnly == true;

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

                Execute(value, obj =>
                {
                    AddObjectToRealmIfNeeded(obj);
                    _listHandle.Set(index, obj.ObjectHandle);
                },
                v => _listHandle.Set(index, v),
                v => _listHandle.Set(index, v),
                v => _listHandle.Set(index, v));
            }
        }

        #endregion

        #region implementing IList members

        public void Add(T item)
        {
            Execute(item, obj =>
            {
                AddObjectToRealmIfNeeded(obj);
                _listHandle.Add(obj.ObjectHandle);
            },
            _listHandle.Add,
            _listHandle.Add,
            _listHandle.Add);
        }

        public override int Add(object value)
        {
            Add((T)value);
            return Count;
        }

        public override void Clear()
        {
            _listHandle.Clear();
        }

        public bool Contains(T item) => Contains((object)item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            Argument.NotNull(array, nameof(array));

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (arrayIndex + Count > array.Length)
            {
                throw new ArgumentException($"Specified array doesn't have enough capacity to perform the copy. Needed: {arrayIndex + Count}, available: {array.Length}", nameof(array));
            }

            foreach (var obj in this)
            {
                array[arrayIndex++] = obj;
            }
        }

        public override int IndexOf(T value)
        {
            switch (_argumentType)
            {
                case PropertyType.Object | PropertyType.Nullable:
                    Argument.NotNull(value, nameof(value));

                    var obj = Operator.Convert<T, RealmObject>(value);
                    if (!obj.IsManaged)
                    {
                        throw new ArgumentException("Value does not belong to a realm", nameof(value));
                    }

                    return _listHandle.Find(obj.ObjectHandle);
                case PropertyType.String:
                case PropertyType.String | PropertyType.Nullable:
                    return _listHandle.Find(Operator.Convert<T, string>(value));
                case PropertyType.Data:
                case PropertyType.Data | PropertyType.Nullable:
                    return _listHandle.Find(Operator.Convert<T, byte[]>(value));
                default:
                    return _listHandle.Find(PrimitiveValue.Create(value, _argumentType));
            }
        }

        public void Insert(int index, T item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Execute(item, obj =>
            {
                AddObjectToRealmIfNeeded(obj);
                _listHandle.Insert(index, obj.ObjectHandle);
            },
            value => _listHandle.Insert(index, value),
            value => _listHandle.Insert(index, value),
            value => _listHandle.Insert(index, value));
        }

        public override void Insert(int index, object value) => Insert(index, (T)value);

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

        public override void Remove(object value) => Remove((T)value);

        public override void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _listHandle.Erase((IntPtr)index);
        }

        private void AddObjectToRealmIfNeeded(RealmObject obj)
        {
            if (!obj.IsManaged)
            {
                _realm.Add(obj);
            }
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

        public override IRealmCollection<T> Freeze()
        {
            if (IsFrozen)
            {
                return this;
            }

            var frozenRealm = Realm.Freeze();
            var frozenHandle = _listHandle.Freeze(frozenRealm.SharedRealmHandle);
            return new RealmList<T>(frozenRealm, frozenHandle, Metadata);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);

        private static void Execute(T item,
            Action<RealmObject> objectHandler,
            Action<PrimitiveValue> primitiveHandler,
            Action<string> stringHandler,
            Action<byte[]> binaryHandler)
        {
            switch (_argumentType)
            {
                case PropertyType.Object | PropertyType.Nullable:
                    objectHandler(Operator.Convert<T, RealmObject>(item));
                    break;
                case PropertyType.String:
                case PropertyType.String | PropertyType.Nullable:
                    stringHandler(Operator.Convert<T, string>(item));
                    break;
                case PropertyType.Data:
                case PropertyType.Data | PropertyType.Nullable:
                    binaryHandler(Operator.Convert<T, byte[]>(item));
                    break;
                default:
                    primitiveHandler(PrimitiveValue.Create(item, _argumentType));
                    break;
            }
        }
    }
}