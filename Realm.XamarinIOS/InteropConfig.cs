////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.IO;
using Foundation;
using UIKit;

namespace Realms
{
    /// <summary>
    /// Per-platform utility functions. A copy of this file exists in each platform project such as Realm.XamarinIOS.
    /// </summary>
    internal static class InteropConfig
    {
        /// <summary>
        /// Gets a value indicating whether the platform is 64 bit.
        /// </summary>
        public static bool Is64Bit
        {
#if REALM_32       
            get
            {
                Debug.Assert(IntPtr.Size == 4, "REALM_32 symbol is defined, but we're in a 64 bit process.");
                return false;
            }
#elif REALM_64
            get
            {
                Debug.Assert(IntPtr.Size == 8, "REALM_64 symbol is defined, but we're in a 32 bit process.");
                return true;
            }
#else
            get
            {
                return IntPtr.Size == 8;
            }
#endif
        }

        /// <summary>
        /// Name of the DLL used in native declarations, constant varying per-platform.
        /// </summary>
        public const string DLL_NAME = "__Internal";
    }
}