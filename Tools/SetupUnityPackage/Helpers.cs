////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.IO;

namespace SetupUnityPackage
{
    internal static class Helpers
    {
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }

        public static string BuildFolder { get; } = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;

        public static string SolutionFolder { get; } = BuildFolder[..BuildFolder.IndexOf(Path.Combine("Tools", "SetupUnityPackage"), StringComparison.InvariantCulture)];

        public static string PackagesFolder { get; } = Path.Combine(SolutionFolder, "Realm", "packages");

        public static void CopyFiles(string from, string to, Func<string, bool>? shouldIncludeFile = null)
        {
            Directory.CreateDirectory(to);

            var testFiles = Directory.EnumerateFiles(from, "*.*", SearchOption.AllDirectories);
            foreach (var file in testFiles)
            {
                var relativePath = Path.GetRelativePath(from, file);
                if (shouldIncludeFile?.Invoke(relativePath) == false)
                {
                    continue;
                }

                var targetPath = Path.Combine(to, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(file, targetPath, overwrite: true);
            }
        }
    }
}
