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

        public const string L64 = "wrappers";
        public const string L32 = "wrappers";
    }
}