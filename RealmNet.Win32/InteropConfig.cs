/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Diagnostics;

namespace RealmNet
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


#if (DEBUG)
        private const string BuildName = "Debug";
#else
        private const string BuildName = "Release";
#endif

        //TODO eventually retire L32 and L64 for platform-conditional builds using DLL_NAME
#if REALM_32
        public const string DLL_NAME = "wrappersx86-" + BuildName;
#elif REALM_64
        public const string DLL_NAME = "wrappersx64-" + BuildName;
#else
        public const string DLL_NAME = "** error see InteropConfig.cs DLL_NAME";
#endif

        public static string GetDefaultDatabasePath()
        {
            const string dbFilename = "db.realm";
            var documentsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return System.IO.Path.Combine(documentsPath, dbFilename);
        }
    }
}