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
    [Preserve(AllMembers = true, Conditional = false)]
    public struct RealmInteger<T> :
        IEquatable<T>,
        IComparable<RealmInteger<T>>,
        IComparable<T>,
        IFormattable
        where T : struct, IComparable<T>, IFormattable
    {
        private readonly T _value;

        internal RealmInteger(T value)
        {
            _value = value;
        }

        public RealmInteger<T> Increment()
        {
            return Increment(default(T));
        }

        public RealmInteger<T> Decrement()
        {
            return Increment(default(T));
        }

        public RealmInteger<T> Increment(T value)
        {
            return new RealmInteger<T>(default(T));
        }

        #region Equals

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

        public bool Equals(T other)
        {
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        #endregion

        #region IComparable

        public int CompareTo(RealmInteger<T> other)
        {
            return CompareTo(other._value);
        }

        public int CompareTo(T other)
        {
            return _value.CompareTo(other);
        }

        #endregion

        #region IFormattable

        public string ToString(string format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);

        #endregion

        #region Operators

        public static implicit operator T(RealmInteger<T> i)
        {
            return i._value;
        }

        public static implicit operator RealmInteger<T>(T i)
        {
            return new RealmInteger<T>(i);
        }

        public static RealmInteger<T> operator ++(RealmInteger<T> i)
        {
            return i.Increment();
        }

        public static RealmInteger<T> operator --(RealmInteger<T> i)
        {
            return i.Decrement();
        }

        public static bool operator ==(RealmInteger<T> first, RealmInteger<T> second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(RealmInteger<T> first, RealmInteger<T> second)
        {
            return !(first == second);
        }

        #endregion
    }
}