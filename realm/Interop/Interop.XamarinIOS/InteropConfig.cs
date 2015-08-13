using System;

namespace Interop.Config
{
    public static class InteropConfig
    {
        public static bool Is64Bit
        {
#if REALM_32       
            get {
                Debug.Assert(IntPtr.Size == 4);
                return false;
            }
#elif REALM_64
            get {
                Debug.Assert(IntPtr.Size == 8);
                return true;
            }
#else
            //if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            get { return (IntPtr.Size == 8); }
#endif
		}
        public const string L32 = "(unimplemented)";
        public const string L64 = "__Internal";
        
        //TODO eventually retire L32 and L64 for platform-conditional builds using DLL_NAME
#if REALM_32
        public const string DLL_NAME = "(unimplemented) 32BIT";
#elif REALM_64
        public const string DLL_NAME = "__Internal";
#else
        public const string DLL_NAME = "** error see InteropConfig.cs DLL_NAME";
#endif
    }
}