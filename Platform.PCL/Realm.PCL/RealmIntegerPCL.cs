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
        public RealmInteger<T> Increment()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        public RealmInteger<T> Decrement()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        public RealmInteger<T> Increment(T value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        #region Equals

        public override bool Equals(object obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public bool Equals(T other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public override int GetHashCode()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public override string ToString()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region IComparable

        public int CompareTo(RealmInteger<T> other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public int CompareTo(T other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        #endregion

        #region IFormattable

        public string ToString(string format, IFormatProvider formatProvider)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region Operators

        public static implicit operator T(RealmInteger<T> i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(T);
        }

        public static implicit operator RealmInteger<T>(T i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        public static RealmInteger<T> operator ++(RealmInteger<T> i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        public static RealmInteger<T> operator --(RealmInteger<T> i)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmInteger<T>);
        }

        public static bool operator ==(RealmInteger<T> first, RealmInteger<T> second)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public static bool operator !=(RealmInteger<T> first, RealmInteger<T> second)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        #endregion
    }
}