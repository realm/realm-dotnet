////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// A structure representing an integer value in the database. It offers API to increment the value, which produces
    /// correct merges during conflicts.
    /// </summary>
    /// <remarks>
    /// <see cref="RealmInteger{T}"/> is implicitly convertible to and from <see cref="T"/>.
    /// <br/>
    /// Calling <see cref="Increment()"/> on a managed <see cref="RealmObject"/>'s property must be done in a write
    /// transaction. When calling <see cref="Increment()"/> on a <see cref="RealmObject"/> property, it will increment
    /// the property's value in the database, so the change will be reflected the next time this property is accessed.
    /// If the object is unmanaged, its property value will not be affected.
    /// </remarks>
    /// <typeparam name="T">
    /// The integer type, represented by this <see cref="RealmInteger{T}"/>. Supported types are <see cref="byte"/>,
    /// <see cref="short"/>, <see cref="int"/>, and <see cref="long"/>.
    /// </typeparam>
    /// <seealso href="https://realm.io/docs/realm-object-server/#counters"/>
    [Preserve(AllMembers = true, Conditional = false)]
    public struct RealmInteger<T> :
        IEquatable<T>,
        IComparable<RealmInteger<T>>,
        IComparable<T>,
        IFormattable
        where T : struct, IComparable<T>, IFormattable
    {
        /// <summary>
        /// Increments the integer value by 1. Inverse of <see cref="Decrement"/>.
        /// </summary>
        /// <returns>The incremented value.</returns>
        public RealmInteger<T> Increment()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        /// <summary>
        /// Decrements the integer value by 1. Inverse of <see cref="Increment()"/>.
        /// </summary>
        /// <returns>The decremented value.</returns>
        public RealmInteger<T> Decrement()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        /// <summary>
        /// Increment the integer value by a specified amount.
        /// </summary>
        /// <returns>The incremented value.</returns>
        /// <param name="value">Value by which to increment.</param>
        public RealmInteger<T> Increment(T value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        #region Equals

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <inheritdoc />
        public bool Equals(T other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region IComparable

        /// <inheritdoc />
        public int CompareTo(RealmInteger<T> other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        /// <inheritdoc />
        public int CompareTo(T other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        #endregion

        #region IFormattable

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region Operators

        /// <inheritdoc />
        public static implicit operator T(RealmInteger<T> i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(T);
        }

        /// <inheritdoc />
        public static implicit operator RealmInteger<T>(T i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        /// <inheritdoc />
        public static bool operator ==(RealmInteger<T> first, RealmInteger<T> second)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <inheritdoc />
        public static bool operator !=(RealmInteger<T> first, RealmInteger<T> second)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        #endregion
    }
}