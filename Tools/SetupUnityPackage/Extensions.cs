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

using System.Collections.Generic;
using System.IO;

namespace SetupUnityPackage
{
    public static class Helpers
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

        public static string BuildFolder { get; } = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        public static string SolutionFolder { get; } = BuildFolder.Substring(0, BuildFolder.IndexOf(Path.Combine("Tools", "SetupUnityPackage")));

        public static string PackagesFolder { get; } = Path.Combine(SolutionFolder, "Realm", "packages");
    }
}
