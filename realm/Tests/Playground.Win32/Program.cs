using System;
using System.Collections.Generic;
using System.IO;
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
            var path = Path.GetTempFileName();
            Console.WriteLine("Path: " + path);

            //var os1 = NativeObjectSchema.object_schema_new("123 hej med dig ☃ <- snemand!");
            //var os2 = NativeObjectSchema.object_schema_new("Nummer 2");
            //var osses = new[] { os1, os2 };
            //NativeSchema.schema_new(osses, (IntPtr) osses.Length);

            var s = NativeSchema.generate();

            var sh = new SchemaHandle();
            sh.SetHandle(s);

            var sr = NativeSharedRealm.open(sh, path, (IntPtr)0, (IntPtr)0, "");
            var srh = new SharedRealmHandle();
            srh.SetHandle(sr);

            Console.WriteLine("Has table 'no': " + NativeSharedRealm.has_table(srh, "no"));
            Console.WriteLine("Has table 'os1': " + NativeSharedRealm.has_table(srh, "os1"));
        }
    }
}
