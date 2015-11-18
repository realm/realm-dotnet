/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet
{
    public static class InteropConfig
    {
        public static bool Is64Bit {
#if REALM_32       
            get {
                return false;
            }
#elif REALM_64
            get {
                return true;
            }
#else
            //if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            get { return (IntPtr.Size == 8); }
#endif
        }

        //TODO eventually retire L32 and L64 for platform-conditional builds using DLL_NAME
#if REALM_32
        public const string DLL_NAME = "wrappers";
#elif REALM_64
        public const string DLL_NAME = "UNIMPLEMENTED 64BIT";
#else
        public const string DLL_NAME = "** error see InteropConfig.cs DLL_NAME";
#endif
    }
}