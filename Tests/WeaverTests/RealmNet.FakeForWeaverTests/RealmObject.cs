/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RealmNet
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

protected T GetValue<T>(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return default(T);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }
    }
}
