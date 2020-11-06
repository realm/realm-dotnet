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
using Realms.Native;

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
        private readonly Action<T> _add;
        private readonly Action<int, T> _set;
        private readonly Action<int, T> _insert;
        private readonly Func<T, int> _indexOf;

        internal RealmList(Realm realm, ListHandle adoptedList, RealmObjectBase.Metadata metadata) : base(realm, metadata)
        {
            _listHandle = adoptedList;

            switch (_argumentType)
            {
                case RealmValueType.Object:
                    _add = GetObjectExecutor(_listHandle.Add, _listHandle.AddEmbedded);
                    _set = GetObjectExecutor(_listHandle.Set, _listHandle.SetEmbedded);
                    _insert = GetObjectExecutor(_listHandle.Insert, _listHandle.InsertEmbedded);
                    _indexOf = (value) =>
                    {
                        Argument.NotNull(value, nameof(value));

                        var obj = Operator.Convert<T, RealmObjectBase>(value);
                        if (!obj.IsManaged)
                        {
                            throw new ArgumentException("Value does not belong to a realm", nameof(value));
                        }

                        return _listHandle.Find(obj.ObjectHandle);
                    };

                    break;
                case RealmValueType.String:
                    _add = (item) => _listHandle.Add(Operator.Convert<T, string>(item));
                    _set = (index, item) => _listHandle.Set(index, Operator.Convert<T, string>(item));
                    _insert = (index, item) => _listHandle.Insert(index, Operator.Convert<T, string>(item));
                    _indexOf = (value) => _listHandle.Find(Operator.Convert<T, string>(value));
                    break;
                case RealmValueType.Data:
                    _add = (item) => _listHandle.Add(Operator.Convert<T, byte[]>(item));
                    _set = (index, item) => _listHandle.Set(index, Operator.Convert<T, byte[]>(item));
                    _insert = (index, item) => _listHandle.Insert(index, Operator.Convert<T, byte[]>(item));
                    _indexOf = (value) => _listHandle.Find(Operator.Convert<T, byte[]>(value));
                    break;
                default:
                    _add = (item) => _listHandle.Add(PrimitiveValue.Create(item, _argumentType));
                    _set = (index, item) => _listHandle.Set(index, PrimitiveValue.Create(item, _argumentType));
                    _insert = (index, item) => _listHandle.Insert(index, PrimitiveValue.Create(item, _argumentType));
                    _indexOf = (value) => _listHandle.Find(PrimitiveValue.Create(value, _argumentType));
                    break;
            }
        }

        internal override CollectionHandleBase CreateHandle() => _listHandle;

        ListHandle IRealmList.NativeHandle => _listHandle;

        RealmObjectBase.Metadata IRealmList.Metadata => Metadata;

        #region implementing IList properties

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

                _set(index, value);
            }
        }

        #endregion

        #region implementing IList members

        public void Add(T item) => _add(item);

        public override int IndexOf(T value) => _indexOf(value);

        public void Insert(int index, T item)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _insert(index, item);
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

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmList(expression, this);

        private Action<T> GetObjectExecutor(Action<ObjectHandle> objectHandler, Func<ObjectHandle> embeddedHandler)
        {
            return (item) =>
            {
                switch (item)
                {
                    case null:
                        throw new NotSupportedException("Adding, setting, or inserting <null> in a list of objects is not supported.");
                    case RealmObject realmObj:
                        if (!realmObj.IsManaged)
                        {
                            Realm.Add(realmObj);
                        }

                        objectHandler(realmObj.ObjectHandle);
                        break;
                    case EmbeddedObject embeddedObj:
                        if (embeddedObj.IsManaged)
                        {
                            throw new RealmException("Can't add, set, or insert an embedded object that is already managed.");
                        }

                        var handle = embeddedHandler();
                        Realm.ManageEmbedded(embeddedObj, handle);
                        break;
                    default:
                        throw new NotSupportedException($"Adding, setting, or inserting {item.GetType()} in a list of objects is not supported, because it doesn't inherit from RealmObject or EmbeddedObject.");
                }
            };
        }

        private Action<int, T> GetObjectExecutor(Action<int, ObjectHandle> objectHandler, Func<int, ObjectHandle> embeddedHandler)
        {
            return (index, item) =>
            {
                switch (item)
                {
                    case null:
                        throw new NotSupportedException("Adding, setting, or inserting <null> in a list of objects is not supported.");
                    case RealmObject realmObj:
                        if (!realmObj.IsManaged)
                        {
                            Realm.Add(realmObj);
                        }

                        objectHandler(index, realmObj.ObjectHandle);
                        break;
                    case EmbeddedObject embeddedObj:
                        if (embeddedObj.IsManaged)
                        {
                            throw new RealmException("Can't add, set, or insert an embedded object that is already managed.");
                        }

                        var handle = embeddedHandler(index);
                        Realm.ManageEmbedded(embeddedObj, handle);
                        break;
                    default:
                        throw new NotSupportedException($"Adding, setting, or inserting {item.GetType()} in a list of objects is not supported, because it doesn't inherit from RealmObject or EmbeddedObject.");
                }
            };
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