using System;
using System.Reflection;
using NUnitLite;

namespace Tests.NetCore
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args);
        }
    }
}
