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
using System.Linq;
using System.Runtime.CompilerServices;

namespace Realms
{
    public class RealmObject : INotifyPropertyChanged
    {
        public List<string> LogList = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void LogString(string s)
        {
            LogList.Add(s);
        }

        private void LogCall(string parameters = "", [CallerMemberName] string caller = "")
        {
            LogString("RealmObject." + caller + "(" + parameters + ")");
        }

        private bool _isManaged;

        public bool IsManaged
        {
            get
            {
                LogString("IsManaged");
                return _isManaged;
            }

            set
            {
                _isManaged = value;
            }
        }

        private Realm _realm;

        public Realm Realm
        {
            get
            {
                LogString("Realm");
                return _realm;
            }
        }

        protected string GetStringValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return string.Empty;
        }

        protected void SetStringValue(string propertyName, string value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected char GetCharValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return char.MinValue;
        }

        protected void SetCharValue(string propertyName, char value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetNullableCharValueUnique(string propertyName, char? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected byte GetByteValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetNullableByteValueUnique(string propertyName, byte? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected short GetInt16Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetNullableInt16ValueUnique(string propertyName, short? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected int GetInt32Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetNullableInt32ValueUnique(string propertyName, int? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected long GetInt64Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetNullableInt64ValueUnique(string propertyName, long? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected float GetSingleValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected double GetDoubleValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected bool GetBooleanValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return false;
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return DateTimeOffset.MinValue;
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return DateTimeOffset.MinValue;
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return char.MinValue;
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return 0;
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return false;
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new RealmList<T>();
        }

        protected void SetListValue<T>(string propertyName, IList<T> value) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return default(T);
        }

        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new byte[0];
        }

        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected IQueryable<T> GetBacklinks<T>(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return Enumerable.Empty<T>().AsQueryable();
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
