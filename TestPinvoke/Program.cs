using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestPinvoke
{
    class Program
    {
        static void assertlong(string test,long expected,long returned)
        {
            if (expected== returned)
            {
                System.Console.WriteLine("{0} was {1} as expected",test ,expected);
            }
            else
            {
                System.Console.WriteLine("{0} was not {1} as expected,but {2} instead ",test, expected, returned);
            }
        }

        static void assertstring(string test, string expected, string returned)
        {
            if (expected == returned)
            {
                System.Console.WriteLine("{0} worked! expected {1}  returned {2}", test, expected, returned);
            }
            else
            {
                System.Console.WriteLine("{0} failed! expected {1}  returned {2}", test, expected, returned);
            }
        }

        static void Main(string[] args)
        {
            string txt="Calling c++ dll via P/Invoke";
            System.Console.WriteLine(txt);            
            long returned = (long)fnTightCSDLL();
            long expected = 423;
            assertlong("size_t function, recieved as uintptr and cast as long ", expected, returned);

            UIntPtr value = (UIntPtr)43;
            returned = (long)TestIntegerParam(value);
            expected = 43 * 2;
            assertlong("size_t function received as uintptr cast as long, parameter was uintptr cast as long, recieived as size_t", expected, returned);


            //getting a static string back from the C++ dll (no need to free it)            
            var valuePtr = TestConstantStringReturn();
            if (valuePtr == IntPtr.Zero )
            {
                System.Console.WriteLine("Unexpected string returned (zero pointer or zero length) ");
            }
            var csharpstring = Marshal.PtrToStringAnsi(valuePtr);
            assertstring("Char * return of a static string, no parametres", "Hello from the DLL!", csharpstring);
            
            //getting a heap allocated string back from the c++ dll (after having processed the string, we call the dll to free it)



            System.Console.ReadKey();

        }





        //this dll must be manually copied to the location of the testpinvoke program exefile
        [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        // as per mono suggestion here http://www.mono-project.com/Interop_with_Native_Libraries the .dll is omitted to allow other sufixes in other oses
        // cdecl was chosen because leveldb uses that in their c# binding that is supposed to work on both mono and windows
        private static extern UIntPtr  fnTightCSDLL ();  

       [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr TestIntegerParam(UIntPtr valuetodouble);

       [DllImport("tightCSDLL", CallingConvention = CallingConvention.Cdecl)]
       private static extern IntPtr TestConstantStringReturn();

    }
}
