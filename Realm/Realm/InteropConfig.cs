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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Realms.Logging;

namespace Realms
{
    internal static class InteropConfig
    {
        /// <summary>
        /// Name of the DLL used in native declarations, constant varying per-platform.
        /// </summary>
        public const string DLL_NAME = "realm-wrappers";

        public static readonly string Platform;

        public static readonly Version SDKVersion = typeof(InteropConfig).Assembly.GetName().Version;

        private static readonly List<string> _potentialStorageFolders = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            Path.Combine(Directory.GetCurrentDirectory(), "Documents")
        };

        private static readonly Lazy<string> _defaultStorageFolder = new Lazy<string>(() =>
        {
            if (TryGetUWPFolder(out var folder))
            {
                return folder;
            }

            foreach (var potentialFolder in _potentialStorageFolders)
            {
                if (TryGetDatabaseFolder(() => potentialFolder, out folder))
                {
                    return folder;
                }
            }

            throw new InvalidOperationException("Couldn't determine a writable folder where to store realm file. Specify absolute path manually.");
        });

        private static string _customStorageFolder;

        public static string DefaultStorageFolder
        {
            get => _customStorageFolder ?? _defaultStorageFolder.Value;
            set => _customStorageFolder = value;
        }

        public static void AddPotentialStorageFolder(string folder)
        {
#if DEBUG
            if (_defaultStorageFolder.IsValueCreated)
            {
                throw new NotSupportedException("Adding a potential storage folder after the lazy value has been created.");
            }
#endif

            _potentialStorageFolders.Insert(0, folder);
        }

        static InteropConfig()
        {
            Platform = TryInitializeUnity() ? "Realm Unity" : "Realm .NET";

            AppDomain.CurrentDomain.DomainUnload += (_, __) =>
            {
                Logger.LogDefault(LogLevel.Info, "Realm: Domain is unloading, force closing all Realm instances.");

                try
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    SharedRealmHandle.CloseAllRealms();

                    sw.Stop();
                    Logger.LogDefault(LogLevel.Info, $"Realm: Closed all native instances in {sw.ElapsedMilliseconds} ms.");
                }
                catch (Exception ex)
                {
                    Logger.LogDefault(LogLevel.Error, $"Realm: Failed to close all native instances. You may need to restart your app. Error: {ex}");
                }
            };
        }

        private static bool TryInitializeUnity()
        {
            try
            {
                var fileHelper = Type.GetType($"UnityUtils.Initializer, Realm.UnityUtils, Version={SDKVersion}, Culture=neutral, PublicKeyToken=null");
                if (fileHelper == null)
                {
                    return false;
                }

                var initialize = fileHelper.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
                initialize.Invoke(null, null);

                return true;
            }
            catch
            {
            }

            return false;
        }

        private static bool TryGetUWPFolder(out string folder) => TryGetDatabaseFolder(() =>
        {
            // On UWP, the sandbox folder is obtained by:
            // ApplicationData.Current.LocalFolder.Path
            var applicationData = Type.GetType("Windows.Storage.ApplicationData, Windows, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime");
            if (applicationData == null)
            {
                return null;
            }

            var currentProperty = applicationData.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
            var localFolderProperty = applicationData.GetProperty("LocalFolder", BindingFlags.Public | BindingFlags.Instance);
            var pathProperty = localFolderProperty.PropertyType.GetProperty("Path", BindingFlags.Public | BindingFlags.Instance);

            var currentApplicationData = currentProperty.GetValue(null);
            var localFolder = localFolderProperty.GetValue(currentApplicationData);
            return (string)pathProperty.GetValue(localFolder);
        }, out folder);

        private static bool TryGetDatabaseFolder(Func<string> getter, out string folder)
        {
            try
            {
                var result = getter();

                if (result != null && !Directory.Exists(result))
                {
                    Directory.CreateDirectory(result);
                }

                if (result != null && IsDirectoryWritable(result))
                {
                    folder = result;
                    return true;
                }
            }
            catch
            {
            }

            folder = null;
            return false;
        }

        private static bool IsDirectoryWritable(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            try
            {
                using (File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
