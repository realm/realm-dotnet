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
using System.Reflection;

namespace Realms
{
    internal static class InteropConfig
    {
        private static readonly Lazy<string> _defaultStorageFolder = new Lazy<string>(() =>
        {
            var specialFolderType = typeof(Environment).GetNestedType("SpecialFolder", BindingFlags.Public);

            if (specialFolderType != null)
            {
                var getFolderPath = typeof(Environment).GetMethod("GetFolderPath", new[] { specialFolderType });

                if (getFolderPath != null)
                {
                    var personalField = specialFolderType.GetField("Personal");
                    return (string)getFolderPath.Invoke(null, new[] { personalField.GetValue(null) });
                }
            }

            throw new NotSupportedException();
        });

        /// <summary>
        /// Name of the DLL used in native declarations, constant varying per-platform.
        /// </summary>
        public const string DLL_NAME = "realm-wrappers";

        public static readonly bool Is64BitProcess = IntPtr.Size == 8;

        public static string DefaultStorageFolder => _defaultStorageFolder.Value;
    }
}