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
 
/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a Realm.
    /// </summary>
    public class RealmObject
    {
        private Realm _realm;  // may not be used but wanted it included in definition of IsManaged below.

        /// <summary>
        /// Allows you to check if the object has been associated with a Realm, either at creation or via Realm.Manage.
        /// </summary>
        public bool IsManaged => _realm != null;



        #region Getters

        protected string GetStringValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return "";
        }

        protected char GetCharValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return ' ';
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected byte GetByteValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected short GetInt16Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected int GetInt32Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected long GetInt64Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected float GetSingleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0f;
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected double GetDoubleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0;
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected bool GetBooleanValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(DateTimeOffset);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetCharValue(string propertyName, char value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        /// <summary>
        /// Compare objects with identity query for persistent objects.
        /// </summary>
        /// <remarks>Persisted RealmObjects map their properties directly to the realm with no caching so multiple instances of a given object always refer to the same store.</remarks>
        /// <param name="obj"></param>
        /// <returns>True when objects are the same memory object or refer to the same persisted object.</returns>
        public override bool Equals(object obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

    }
}
