using System;

namespace Interop.Config
{
    public static class InteropConfig
    {
        public static bool Is64Bit
        {
            //if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            get { return (IntPtr.Size == 8); }
        }

#if (DEBUG)
        private const string Buildmode = "d";
        private const string BuildName = "Debug";
#else
        private const string Buildmode = "r";
        private const string BuildName = "Release";
#endif

        //the .net library wil always use a c dll that is called wrappers[32/64][r/d]
        //this dll could have been built with vs2012 or 2010 - we don't really care as long as the C interface is the same, which it will be
        //if built from the same source.
        public const string L64 = "wrappersx64-" + BuildName;
        public const string L32 = "wrappersx86-" + BuildName;
    }
}