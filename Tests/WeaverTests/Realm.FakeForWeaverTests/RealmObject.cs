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
