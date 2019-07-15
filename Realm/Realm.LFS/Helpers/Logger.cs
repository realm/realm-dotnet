using System;

namespace Realms.LFS
{
    public static class Logger
    {
        public static void Info(string value)
        {
            Console.WriteLine(value);
        }

        public static void Error(string value)
        {
            Console.Error.WriteLine(value);
        }
    }
}
