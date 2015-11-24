using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnitTests
{
    public class Logger
    {
        public static Logger Instance { get; set; }

        public List<string> LogList = new List<string>(); 

        public static void LogString(string s)
        {
            Instance.LogList.Add(s);
        }

        public static void LogCall(string parameters = "", [CallerMemberName] string caller = "")
        {
            var stackTrace = new StackTrace(1, false);
            var type = stackTrace.GetFrame(0).GetMethod().DeclaringType;
            LogString(type.Name + "." + caller + "(" + parameters + ")");
        }

        public static void Clear()
        {
            Instance.LogList.Clear();
        }
    }
}
