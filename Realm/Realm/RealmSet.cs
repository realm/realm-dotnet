////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Helpers;

namespace Realms
{
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmSet<T> : RealmCollectionBase<T>, ISet<T>, IDynamicMetaObjectProvider
    {
        private readonly SetHandle _setHandle;
        private readonly Func<T, bool> _add;
        private readonly Func<T, bool> _remove;
        private readonly Func<T, bool> _contains;

        internal RealmSet(Realm realm, SetHandle adoptedSet, RealmObjectBase.Metadata metadata)
            : base(realm, metadata)
        {
            _setHandle = adoptedSet;
            switch (_argumentType)
            {
                case RealmValueType.Object:
                    _add = AddObject;
                    _remove = GetObjectExecutor(_setHandle.Remove);
                    _contains = GetObjectExecutor(_setHandle.Contains);
                    break;
                default:
                    _add = (item) => _setHandle.Add(Operator.Convert<T, RealmValue>(item));
                    _remove = (item) => _setHandle.Remove(Operator.Convert<T, RealmValue>(item));
                    _contains = (item) => _setHandle.Contains(Operator.Convert<T, RealmValue>(item));
                    break;
            }
        }

        public bool Add(T item) => _add(item);

        public bool Remove(T item) => _remove(item);

        public override int IndexOf(T value)
        {
            throw new NotSupportedException();
        }

        public override bool Contains(T value) => _contains(value);

        private bool AddObject(T item)
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

                    return _setHandle.Add(realmObj.ObjectHandle);
                case EmbeddedObject embeddedObj:
                    if (embeddedObj.IsManaged)
                    {
                        throw new RealmException("Can't add, set, or insert an embedded object that is already managed.");
                    }

                    var handle = _setHandle.AddEmbedded();
                    Realm.ManageEmbedded(embeddedObj, handle);
                    return true;
                default:
                    throw new NotSupportedException($"Adding, setting, or inserting {item.GetType()} in a list of objects is not supported, because it doesn't inherit from RealmObject or EmbeddedObject.");
            }
        }

        private static Func<T, bool> GetObjectExecutor(Func<ObjectHandle, bool> handler)
        {
            return (item) =>
            {
                Argument.NotNull(item, nameof(item));

                var obj = Operator.Convert<T, RealmObjectBase>(item);
                if (!obj.IsManaged)
                {
                    throw new ArgumentException("Item does not belong to a realm", nameof(item));
                }

                return handler(obj.ObjectHandle);
            };
        }

        void ICollection<T>.Add(T item) => Add(item);

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmSet(expression, this);

        internal override RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmSet<T>(realm, (SetHandle)handle, Metadata);

        internal override CollectionHandleBase CreateHandle() => _setHandle;

        #region Set methods

        public void ExceptWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call ExceptWith in native
                throw new NotImplementedException();
            }

            if (Count == 0)
            {
                return;
            }

            foreach (var item in other)
            {
                Remove(item);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call IntersectWith in native
                throw new NotImplementedException();
            }

            // intersection of anything with empty set is empty set, so return if count is 0
            if (Count == 0)
            {
                return;
            }

            var otherSet = GetCollection(other);

            // Special case for when other is empty - just clear the set.
            if (otherSet.Count == 0)
            {
                Clear();
                return;
            }

            foreach (var item in this)
            {
                if (!otherSet.Contains(item))
                {
                    Remove(item);
                }
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) => IsSubsetCore(other, proper: true);

        public bool IsSubsetOf(IEnumerable<T> other) => IsSubsetCore(other, proper: false);

        public bool IsProperSubsetOf(IEnumerable<T> other) => IsSupersetCore(other, proper: true);

        public bool IsSupersetOf(IEnumerable<T> other) => IsSupersetCore(other, proper: false);

        public bool Overlaps(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call Overlaps in native
                throw new NotImplementedException();
            }

            // Special case - empty set doesn't overlap with anything.
            if (Count == 0)
            {
                return false;
            }

            foreach (var item in other)
            {
                if (Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call Overlaps in native
                throw new NotImplementedException();
            }

            var otherSet = GetSet(other);
            if (otherSet.Count != Count)
            {
                return false;
            }

            foreach (var item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            // We already know that counts are the same
            return true;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call Overlaps in native
                throw new NotImplementedException();
            }

            // We create a new hashset because we're going to be manipulating the in-memory collection
            // as that's cheaper.
            var otherSet = new HashSet<T>(other);

            // Special case - this is a no-op
            if (otherSet.Count == 0)
            {
                return;
            }

            // Special case - SymmetricExceptWith for empty set is equivalent to Union
            foreach (var item in this)
            {
                if (otherSet.Contains(item))
                {
                    Remove(item);
                    otherSet.Remove(item);
                }
            }

            // We removed all duplicates, just add the remainder
            UnionWith(otherSet);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call UnionWith in native
                return;
            }

            foreach (var item in other)
            {
                Add(item);
            }
        }

        private bool IsSubsetCore(IEnumerable<T> other, bool proper)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call IsSubsetOf in native
                throw new NotImplementedException();
            }

            // The empty set is a subset of any set.
            if (Count == 0)
            {
                return true;
            }

            var otherSet = GetSet(other);

            var maximumCount = otherSet.Count;
            if (proper)
            {
                // Proper subset means at least one element in addition to the subset
                maximumCount--;
            }

            // Special case - we can't have a subset if we have more elements than the max count.
            if (Count > maximumCount)
            {
                return false;
            }

            foreach (var item in this)
            {
                if (!otherSet.Contains(item))
                {
                    return false;
                }
            }

            // We've already established that we have less than the max element count, so we're a subset.
            return true;
        }

        private bool IsSupersetCore(IEnumerable<T> other, bool proper)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmCollectionBase<T> realmCollection)
            {
                // Call IsSupersetOf in native
                throw new NotImplementedException();
            }

            // The empty set is a subset of any set.
            if (Count == 0)
            {
                return true;
            }

            var otherSet = GetSet(other);

            var maximumCount = Count;
            if (proper)
            {
                // Proper subset means at least one element in addition to the subset
                maximumCount--;
            }

            // Special case - we can't have a subset if we have more elements than the max count.
            if (otherSet.Count > maximumCount)
            {
                return false;
            }

            foreach (var item in otherSet)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            // We've already established that we have less than the max element count, so we're a subset.
            return true;
        }

        private static ISet<T> GetSet(IEnumerable<T> enumerable)
        {
            if (enumerable is ISet<T> set)
            {
                return set;
            }

            return new HashSet<T>(enumerable);
        }

        private static ICollection<T> GetCollection(IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T> collection)
            {
                return collection;
            }

            return new List<T>(enumerable);
        }

        #endregion
    }
}
