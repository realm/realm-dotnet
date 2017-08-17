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
using Realms.Helpers;
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
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public class RealmList<T> : RealmCollectionBase<T>, IList<T>, IDynamicMetaObjectProvider
    {
        private static readonly PropertyType _argumentType = PropertyTypeEx.ToPropertyType(typeof(T), out var _);

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
                throw new NotSupportedException("Setting items directly is not supported.");
            }
        }

        protected override T GetAtIndex(int index)
        {
            switch (_argumentType)
            {
                case PropertyType.Object | PropertyType.Nullable:
                    return base.GetAtIndex(index);
                case PropertyType.Int:
                    return Operator.Convert<long, T>(_listHandle.GetInt64AtIndex(index));
                case PropertyType.Int | PropertyType.Nullable:
                    return Operator.Convert<long?, T>(_listHandle.GetNullableInt64AtIndex(index));
                default:
                    throw new NotImplementedException("");
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

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public override bool Contains(object value) => Contains((T)value);

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
            switch (_argumentType)
            {
                case PropertyType.Object | PropertyType.Nullable:
                    var obj = Operator.Convert<T, RealmObject>(item);
                    if (!obj.IsManaged)
                    {
                        throw new ArgumentException("Value does not belong to a realm", nameof(item));
                    }

                    return _listHandle.Find(obj.ObjectHandle);
                case PropertyType.Int:
                    return _listHandle.Find(Operator.Convert<T, long>(item));
                case PropertyType.Int | PropertyType.Nullable:
                    return _listHandle.Find(Operator.Convert<T, long?>(item));
                default:
                    throw new NotImplementedException("");
            }
        }

        public override int IndexOf(object value) => IndexOf((T)value);

        public void Insert(int index, T item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            Execute(item, obj =>
            {
                AddObjectToRealmIfNeeded(obj);
                _listHandle.Insert(index, obj.ObjectHandle);
            },
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
                throw new ArgumentOutOfRangeException();
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

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);

        private static void Execute(T item,
            Action<RealmObject> objectHandler,
            Action<long> intHandler,
            Action<long?> nullableIntHandler)
        {
            switch (_argumentType)
            {
                case PropertyType.Object | PropertyType.Nullable:
                    objectHandler(Operator.Convert<T, RealmObject>(item));
                    break;
                case PropertyType.Int:
                    intHandler(Operator.Convert<T, long>(item));
                    break;
                case PropertyType.Int | PropertyType.Nullable:
                    nullableIntHandler(Operator.Convert<T, long?>(item));
                    break;
                default:
                    throw new NotImplementedException("");
            }
        }
    }
}