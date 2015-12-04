using System;

namespace Realm
{
    public static class InteropConfig
    {
        public const bool Is64Bit = true;
        public const string L32 = "(unimplemented)";
        public const string L64 = "libwrappers";
        
        //TODO eventually retire L32 and L64 for platform-conditional builds using DLL_NAME
        public const string DLL_NAME = "libwrappers";
        
    }
}

