/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using UIKit;
using Foundation;
using System.IO;

namespace Realms
{
    /// <summary>
    /// Provide per-platform configuration, IOS version documented to show typical content.
    /// </summary>
    public static class InteropConfig
    {
    
        /// <summary>
        /// Compile-time test of platform being 64bit.
        /// </summary>
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
            get { return (IntPtr.Size == 8); }
#endif
        }

        /// <summary>
        /// Name of the DLL used in native declarations, constant varying per-platform.
        /// </summary>
        public const string DLL_NAME = "__Internal";

        /// <summary>
        /// Get platform-specific default path for documents with default filename attached.
        /// </summary>
        /// <returns>A full path</returns>
        public static string GetDefaultDatabasePath()
        {
            const string dbFilename = "db.realm";
            string libDir;
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))    // For iOS 8.0 or higher
            {
                libDir = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User) [0].Path;
            }
            else
            {
                var docDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                libDir = Path.GetFullPath(System.IO.Path.Combine (docDir, "..", "Library")); 
            }
            return Path.Combine(libDir, dbFilename);
        }
    }
}