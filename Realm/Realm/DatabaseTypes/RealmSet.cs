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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using Realms.Dynamic;
using Realms.Helpers;

namespace Realms
{
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "IList conformance is needed for UWP databinding. IList<T> is not necessary.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmSet<T> : RealmCollectionBase<T>, ISet<T>, IDynamicMetaObjectProvider
    {
        private readonly SetHandle _setHandle;

        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "We need to be on the generic class so that the weaver can use it.")]
        public static IEqualityComparer<T> Comparer { get; }

        static RealmSet()
        {
            if (typeof(T) == typeof(byte[]))
            {
                Comparer = (IEqualityComparer<T>)new BinaryEqualityComparer();
            }
            else if (typeof(T) == typeof(RealmValue))
            {
                Comparer = (IEqualityComparer<T>)new RealmValueEqualityComparer();
            }
            else
            {
                Comparer = EqualityComparer<T>.Default;
            }
        }

        internal RealmSet(Realm realm, SetHandle adoptedSet, Metadata? metadata)
            : base(realm, metadata)
        {
            _setHandle = adoptedSet;
        }

        public bool Add(T value)
        {
            if (Metadata is not null && value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var realmValue = Operator.Convert<T, RealmValue>(value);

            AddToRealmIfNecessary(realmValue);
            return _setHandle.Add(realmValue);
        }

        public bool Remove(T value)
        {
            var realmValue = Operator.Convert<T, RealmValue>(value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return false;
            }

            return _setHandle.Remove(realmValue);
        }

        // IndexOf is not available on ISet<T>, but is available on IList and IRealmCollection.
        // We provide an implementation here because in data-binding scenarios, the binding engine
        // may cast the collection to IList and attempt to invoke IndexOf on it - most notably when
        // binding to a ListView and changing the SelectedItem.
        public override int IndexOf([AllowNull] T value)
        {
            var realmValue = Operator.Convert<T?, RealmValue>(value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return -1;
            }

            return _setHandle.Find(realmValue);
        }

        public override bool Contains([AllowNull] T value)
        {
            var realmValue = Operator.Convert<T?, RealmValue>(value);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return false;
            }

            return _setHandle.Contains(realmValue);
        }

        void ICollection<T>.Add(T item) => Add(item);

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) => new MetaRealmSet(expression, this);

        internal RealmResults<T> ToResults()
        {
            var resultsHandle = _setHandle.ToResults();
            return new RealmResults<T>(Realm, resultsHandle, Metadata);
        }

        internal override RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmSet<T>(realm, (SetHandle)handle, Metadata);

        internal override CollectionHandleBase GetOrCreateHandle() => _setHandle;

        protected override T GetValueAtIndex(int index) => _setHandle.GetValueAtIndex(index, Realm).As<T>();

        #region Set methods

        public void ExceptWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                _setHandle.ExceptWith(realmCollection.Handle.Value);
                return;
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

            if (other is RealmSet<T> realmCollection)
            {
                _setHandle.IntersectWith(realmCollection.Handle.Value);
                return;
            }

            // intersection of anything with empty set is empty set, so return if count is 0
            var count = Count;
            if (count == 0)
            {
                return;
            }

            var otherSet = GetSet(other);

            // Special case for when other is empty - just clear the set.
            if (otherSet.Count == 0)
            {
                Clear();
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var item = this[i];
                if (!otherSet.Contains(item))
                {
                    Remove(item);
                    i--;
                    count--;
                }
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                _setHandle.SymmetricExceptWith(realmCollection.Handle.Value);
                return;
            }

            // We create a new hashset because we're going to be manipulating the in-memory collection
            // as that's cheaper.
            var otherSet = new HashSet<T>(other, Comparer);

            // Special case - this is a no-op
            if (otherSet.Count == 0)
            {
                return;
            }

            var count = Count;
            for (var i = 0; i < count; i++)
            {
                var item = this[i];
                if (otherSet.Remove(item))
                {
                    Remove(item);
                    i--;
                    count--;
                }
            }

            // We removed all duplicates, just add the remainder
            UnionWith(otherSet);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                _setHandle.UnionWith(realmCollection.Handle.Value);
                return;
            }

            foreach (var item in other)
            {
                Add(item);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other) => IsSubsetCore(other, proper: false);

        public bool IsProperSubsetOf(IEnumerable<T> other) => IsSubsetCore(other, proper: true);

        public bool IsSupersetOf(IEnumerable<T> other) => IsSupersetCore(other, proper: false);

        public bool IsProperSupersetOf(IEnumerable<T> other) => IsSupersetCore(other, proper: true);

        public bool Overlaps(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                return _setHandle.Overlaps(realmCollection.Handle.Value);
            }

            // Special case - empty set doesn't overlap with anything.
            if (Count == 0)
            {
                return false;
            }

            return other.Any(Contains);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                return _setHandle.SetEquals(realmCollection.Handle.Value);
            }

            var otherSet = GetSet(other);
            if (otherSet.Count != Count)
            {
                return false;
            }

            return otherSet.All(Contains);
        }

        private bool IsSubsetCore(IEnumerable<T> other, bool proper)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                return _setHandle.IsSubsetOf(realmCollection.Handle.Value, proper);
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

            return this.All(otherSet.Contains);
        }

        private bool IsSupersetCore(IEnumerable<T> other, bool proper)
        {
            Argument.NotNull(other, nameof(other));

            if (other is RealmSet<T> realmCollection)
            {
                return _setHandle.IsSupersetOf(realmCollection.Handle.Value, proper);
            }

            var count = Count;

            // The empty set can't be a proper superset
            if (count == 0 && proper)
            {
                return false;
            }

            var otherSet = GetSet(other);

            var minimumCount = otherSet.Count;
            if (proper)
            {
                // Proper superset means at least one element in addition to the subset
                minimumCount++;
            }

            // Special case - we can't have a superset if we have less elements than the min count.
            if (count < minimumCount)
            {
                return false;
            }

            return otherSet.All(Contains);
        }

        private static ISet<T> GetSet(IEnumerable<T> collection)
        {
            if (collection is HashSet<T> set && set.Comparer == Comparer)
            {
                return set;
            }

            return new HashSet<T>(collection, Comparer);
        }

        #endregion

        private class BinaryEqualityComparer : EqualityComparer<byte[]>
        {
            public override bool Equals(byte[]? x, byte[]? y)
            {
                if (x == null || y == null)
                {
                    return x == y;
                }

                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x.Length != y.Length)
                {
                    return false;
                }

                return x.SequenceEqual(y);
            }

            public override int GetHashCode(byte[] obj)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj));
                }

                return obj.Length;
            }
        }

        private class RealmValueEqualityComparer : EqualityComparer<RealmValue>
        {
            public override bool Equals(RealmValue x, RealmValue y)
            {
                // We're converting numeric types to Decimal128 as it can hold the entire range
                // of long, float, and double
                if (x.Type.IsNumeric() && y.Type.IsNumeric())
                {
                    var decimalX = x.As<Decimal128>();
                    var decimalY = x.As<Decimal128>();
                    return decimalX == decimalY;
                }

                return x == y;
            }

            public override int GetHashCode(RealmValue obj)
            {
                // We're getting the hashcode of numeric types by casting them to double
                // because Decimal128's hashcode function is incorrect: https://jira.mongodb.org/browse/CSHARP-3288
                if (obj.Type.IsNumeric())
                {
                    return obj.As<double>().GetHashCode();
                }

                return obj.GetHashCode();
            }
        }
    }
}
