/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    public class RealmObject
    {
        public List<string> LogList = new List<string>();

        public void LogString(string s)
        {
            LogList.Add(s);
        }

        public  void LogCall(string parameters = "", [CallerMemberName] string caller = "")
        {
            var stackTrace = new StackTrace(1, false);
            var type = stackTrace.GetFrame(0).GetMethod().DeclaringType;
            LogString(type.Name + "." + caller + "(" + parameters + ")");
        }

        private bool _isManaged;
        public bool IsManaged
        {
            get
            {
                LogString("IsManaged");
                return _isManaged;
            }
            set { _isManaged = value;  }
        }

        protected string GetStringValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return "";
        }

        protected void SetStringValue(string propertyName, string value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetStringValueUnique(string propertyName, string value)
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

        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new RealmList<T>();
        }

        protected void SetListValue<T>(string propertyName, RealmList<T> value) where T : RealmObject
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
    }
}
