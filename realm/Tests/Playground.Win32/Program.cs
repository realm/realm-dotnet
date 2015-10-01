using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealmNet;

namespace Playground.Win32
{
    class Program
    {
        static void Main(string[] args)
        {
            var os1 = NativeObjectSchema.object_schema_new("123 hej med dig ☃ <- snemand!");
            var os2 = NativeObjectSchema.object_schema_new("Nummer 2");

            var osses = new[] { os1, os2 };

            NativeSchema.schema_new(osses, (IntPtr) osses.Length);
        }
    }
}
