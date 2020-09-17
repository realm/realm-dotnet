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
using System.Diagnostics.CodeAnalysis;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    /// <summary>
    /// A structure representing an integer value in the database. It offers API to increment the value, which produces
    /// correct merges during conflicts.
    /// </summary>
    /// <remarks>
    /// <see cref="RealmInteger{T}"/> is implicitly convertible to and from T/>.
    /// <br/>
    /// Calling <see cref="Increment()"/> on a managed <see cref="RealmObject"/>/<see cref="EmbeddedObject"/>'s property must be done in a write
    /// transaction. When calling <see cref="Increment()"/> on a <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> property, it will increment
    /// the property's value in the database, so the change will be reflected the next time this property is accessed.
    /// If the object is unmanaged, its property value will not be affected.
    /// </remarks>
    /// <typeparam name="T">
    /// The integer type, represented by this <see cref="RealmInteger{T}"/>. Supported types are <see cref="byte"/>,
    /// <see cref="short"/>, <see cref="int"/>, and <see cref="long"/>.
    /// </typeparam>
    /// <seealso href="https://realm.io/docs/realm-object-server/#counters"/>
    [Preserve(AllMembers = true, Conditional = false)]
    [SuppressMessage("Design", "CA1066:Implement IEquatable when overriding Object.Equals", Justification = "We already implement IEquatable<T> and RealmInteger<T> implicitly converts to T.")]
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
        private readonly ColumnKey _columnKey;

        private bool IsManaged => _objectHandle != null;

        internal RealmInteger(T value)
        {
            _value = value;
            _objectHandle = null;
            _columnKey = default;
        }

        internal RealmInteger(T value, ObjectHandle objectHandle, ColumnKey columnKey)
        {
            _value = value;
            _objectHandle = objectHandle;
            _columnKey = columnKey;
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
                _objectHandle.AddInt64(_columnKey, value.ToLong());
                var result = _objectHandle.GetPrimitive(_columnKey, Schema.PropertyType.Int).ToIntegral<T>();
                return new RealmInteger<T>(result, _objectHandle, _columnKey);
            }

            throw new NotSupportedException("Increment should only be called on RealmInteger properties of managed objects.");
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

        /// <summary>
        /// Indicates whether this instance represents the same numeric value as the provided object.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance represent the same numeric value; otherwise, false.</returns>
        public bool Equals(T other)
        {
            return CompareTo(other) == 0;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of the underlying numeric value.
        /// </summary>
        /// <returns>The string representation of the numeric value.</returns>
        public override string ToString()
        {
            return _value.ToString();
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Compares this instance to another <see cref="RealmInteger{T}"/> value.
        /// </summary>
        /// <param name="other">The value to compare to.</param>
        /// <returns>1 if this instance is greater than <c>other</c>, 0 if the two values are equal, and -1 if <c>other</c> is larger.</returns>
        public int CompareTo(RealmInteger<T> other)
        {
            return CompareTo(other._value);
        }

        /// <summary>
        /// Compares this instance to another numeric value.
        /// </summary>
        /// <param name="other">The value to compare to.</param>
        /// <returns>1 if this instance is greater than <c>other</c>, 0 if the two values are equal, and -1 if <c>other</c> is larger.</returns>
        public int CompareTo(T other)
        {
            return _value.CompareTo(other);
        }

        #endregion

        #region IConvertible

        private IConvertible ConvertibleValue => (IConvertible)_value;

        /// <summary>
        /// Returns the <see cref="TypeCode"/> of the value represented by this <see cref="RealmInteger{T}"/>.
        /// </summary>
        /// <returns>The enumerated constant that is the System.TypeCode of the class or value type that implements this interface.</returns>
        TypeCode IConvertible.GetTypeCode() => ConvertibleValue.GetTypeCode();

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Boolean value equivalent to the value of this instance.</returns>
        bool IConvertible.ToBoolean(IFormatProvider provider) => ConvertibleValue.ToBoolean(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit unsigned integer value equivalent to the value of this instance.</returns>
        byte IConvertible.ToByte(IFormatProvider provider) => ConvertibleValue.ToByte(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Unicode character value equivalent to the value of this instance.</returns>
        char IConvertible.ToChar(IFormatProvider provider) => ConvertibleValue.ToChar(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="DateTime"/> value using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="DateTime"/> value equivalent to the value of this instance.</returns>
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ConvertibleValue.ToDateTime(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="decimal"/> value using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="decimal"/> value equivalent to the value of this instance.</returns>
        decimal IConvertible.ToDecimal(IFormatProvider provider) => ConvertibleValue.ToDecimal(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A double-precision floating-point number equivalent to the value of this instance.</returns>
        double IConvertible.ToDouble(IFormatProvider provider) => ConvertibleValue.ToDouble(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 16-bit signed integer equivalent to the value of this instance.</returns>
        short IConvertible.ToInt16(IFormatProvider provider) => ConvertibleValue.ToInt16(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 32-bit signed integer equivalent to the value of this instance.</returns>
        int IConvertible.ToInt32(IFormatProvider provider) => ConvertibleValue.ToInt32(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 64-bit signed integer equivalent to the value of this instance.</returns>
        long IConvertible.ToInt64(IFormatProvider provider) => ConvertibleValue.ToInt64(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit signed integer equivalent to the value of this instance.</returns>
        sbyte IConvertible.ToSByte(IFormatProvider provider) => ConvertibleValue.ToSByte(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A single-precision floating-point number equivalent to the value of this instance.</returns>
        float IConvertible.ToSingle(IFormatProvider provider) => ConvertibleValue.ToSingle(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="string"/> value using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="string"/> value equivalent to the value of this instance.</returns>
        string IConvertible.ToString(IFormatProvider provider) => ConvertibleValue.ToString(provider);

        /// <summary>
        /// Converts the value of this instance to an <see cref="object"/> of the specified <see cref="Type"/>
        /// that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An <see cref="object"/> instance of type <paramref name="conversionType"/> whose value equivalent to the value of this instance.</returns>
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ConvertibleValue.ToType(conversionType, provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 16-bit unsigned integer equivalent to the value of this instance.</returns>
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ConvertibleValue.ToUInt16(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 32-bit unsigned integer equivalent to the value of this instance.</returns>
        uint IConvertible.ToUInt32(IFormatProvider provider) => ConvertibleValue.ToUInt32(provider);

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider "/>interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A 64-bit unsigned integer equivalent to the value of this instance.</returns>
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ConvertibleValue.ToUInt64(provider);

        #endregion

        #region IFormattable

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">
        /// The format to use. -or- A null reference to use the default format defined for the type of the <see cref="IFormattable"/> implementation.
        /// </param>
        /// <param name="formatProvider">
        /// The provider to use to format the value. -or- A null reference to obtain the numeric format
        /// information from the current locale setting of the operating system</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);

        #endregion

        #region Operators

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Argument is required for proper operator overloading.")]
        public static RealmInteger<T> operator ++(RealmInteger<T> source)
        {
            throw new NotSupportedException("++ is not supported, use Increment instead.");
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Argument is required for proper operator overloading.")]
        public static RealmInteger<T> operator --(RealmInteger<T> source)
        {
            throw new NotSupportedException("++ is not supported, use Decrement instead.");
        }

        public static implicit operator T(RealmInteger<T> i)
        {
            return i._value;
        }

        public static implicit operator RealmInteger<T>(T i)
        {
            return new RealmInteger<T>(i);
        }

        public static bool operator ==(RealmInteger<T> first, RealmInteger<T> second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(RealmInteger<T> first, RealmInteger<T> second)
        {
            return !(first == second);
        }

        public static bool operator <(RealmInteger<T> left, RealmInteger<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(RealmInteger<T> left, RealmInteger<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(RealmInteger<T> left, RealmInteger<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(RealmInteger<T> left, RealmInteger<T> right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion
    }
}