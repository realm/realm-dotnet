/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

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

        // This is always the "name" of the dll, regardless of bit-width.
        public const string DLL_NAME = "__Internal";

        public static string GetDefaultDatabasePath()
        {
            const string dbFilename = "db.realm";
            string libDir;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                libDir = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User) [0].Path;
            }
            else
            {
                var docDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                libDir = System.IO.Path.GetFullPath(System.IO.Path.Combine (docDir, "..", "Library")); 
            }
            return System.IO.Path.Combine(libDir, dbFilename);
        }
    }
}