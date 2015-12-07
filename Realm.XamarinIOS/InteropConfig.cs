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
    /// Per-platform utility functions. A copy of this file exists in each platform project such as Realm.XamarinIOS
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
        /// Get the location to be used for realms if a full path is not specified
        /// </summary>
        /// <returns>A path to the standard Documents directory for the platform</returns>
        public static string GetDefaultDatabasePath()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

    }
}