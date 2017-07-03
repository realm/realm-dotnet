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
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// A structure representing an integer value in the database. It offers API to increment the value, which produces
    /// correct merges during conflicts.
    /// </summary>
    /// <remarks>
    /// Supported integer types are <see cref="byte"/>, <see cref="short"/>, <see cref="int"/>, and <see cref="long"/>.
    /// <br/>
    /// <see cref="RealmInteger{T}"/> is implicitly convertible to and from <see cref="T"/>.
    /// <br/>
    /// Calling <see cref="Increment()"/> on a managed <see cref="RealmObject"/>'s property must be done in a write
    /// transaction. When calling <see cref="Increment()"/> on a <see cref="RealmObject"/> property, it will increment
    /// the property's value in the database, so the change will be reflected the next time this property is accessed.
    /// If the object is unmanaged, its property value will not be affected.
    /// </remarks>
    /// <seealso href="https://realm.io/docs/realm-object-server/#counters"/>
    [Preserve(AllMembers = true, Conditional = false)]
    public struct RealmInteger<T> :
        IEquatable<T>,
        IComparable<RealmInteger<T>>,
        IComparable<T>,
        IConvertible,
        IFormattable
        where T : struct, IComparable<T>, IFormattable
    {
        private readonly T _value;
        private readonly ObjectHandle _objectHandle;
        private readonly IntPtr _propertyIndex;

        private bool IsManaged => _objectHandle != null;

        internal RealmInteger(T value)
        {
            _value = value;
            _objectHandle = null;
            _propertyIndex = IntPtr.Zero;
        }

        internal RealmInteger(T value, ObjectHandle objectHandle, IntPtr propertyIndex)
        {
            _value = value;
            _objectHandle = objectHandle;
            _propertyIndex = propertyIndex;
        }

        /// <summary>
        /// Increments the integer value by 1. Inverse of <see cref="Decrement"/>.
        /// </summary>
        /// <returns>The incremented value.</returns>
        public RealmInteger<T> Increment()
        {
            return Increment(Operator.Convert<int, T>(1));
        }

        /// <summary>
        /// Decrements the integer value by 1. Inverse of <see cref="Increment()"/>.
        /// </summary>
        /// <returns>The decremented value.</returns>
        public RealmInteger<T> Decrement()
        {
            return Increment(Operator.Convert<int, T>(-1));
        }

        /// <summary>
        /// Increment the integer value by a specified amount.
        /// </summary>
        /// <returns>The incremented value.</returns>
        /// <param name="value">Value by which to increment.</param>
        public RealmInteger<T> Increment(T value)
        {
            if (IsManaged)
            {
                _objectHandle.AddInt64(_propertyIndex, value.ToLong());
                var result = Operator.Convert<long, T>(_objectHandle.GetInt64(_propertyIndex));
                return new RealmInteger<T>(result, _objectHandle, _propertyIndex);
            }

            return new RealmInteger<T>(Operator.Add(value, _value));
        }

        #region Equals

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is RealmInteger<T> realmInteger)
            {
                return Equals(realmInteger);
            }

            if (obj is T value)
            {
                return Equals(value);
            }

            return false;
        }

        /// <inheritdoc />
		public bool Equals(T other)
        {
            return CompareTo(other) == 0;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString();
        }

        #endregion

        #region IComparable

        /// <inheritdoc />
        public int CompareTo(RealmInteger<T> other)
        {
            return CompareTo(other._value);
        }

        /// <inheritdoc />
        public int CompareTo(T other)
        {
            return _value.CompareTo(other);
        }

        #endregion

        #region IConvertible

        private IConvertible ConvertibleValue => (IConvertible)_value;

        TypeCode IConvertible.GetTypeCode() => ConvertibleValue.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ConvertibleValue.ToBoolean(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ConvertibleValue.ToByte(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ConvertibleValue.ToChar(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ConvertibleValue.ToDateTime(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ConvertibleValue.ToDecimal(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ConvertibleValue.ToDouble(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ConvertibleValue.ToInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ConvertibleValue.ToInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ConvertibleValue.ToInt64(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ConvertibleValue.ToSByte(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ConvertibleValue.ToSingle(provider);

        string IConvertible.ToString(IFormatProvider provider) => ConvertibleValue.ToString(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ConvertibleValue.ToType(conversionType, provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ConvertibleValue.ToUInt16(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ConvertibleValue.ToUInt32(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ConvertibleValue.ToUInt64(provider);

        #endregion

        #region IFormattable

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);

        #endregion

        #region Operators

        /// <inheritdoc />
        public static implicit operator T(RealmInteger<T> i)
        {
            return i._value;
        }

        /// <inheritdoc />
        public static implicit operator RealmInteger<T>(T i)
        {
            return new RealmInteger<T>(i);
        }

        /// <inheritdoc />
        public static bool operator ==(RealmInteger<T> first, RealmInteger<T> second)
        {
            return first.Equals(second);
        }

        /// <inheritdoc />
        public static bool operator !=(RealmInteger<T> first, RealmInteger<T> second)
        {
            return !(first == second);
        }

        #endregion
    }
}