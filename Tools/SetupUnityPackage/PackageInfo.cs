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
using System.Linq;

namespace SetupUnityPackage
{
    internal class PackageInfo : PackageInfoBase
    {
        private readonly IDictionary<string, string> _paths;

        public IEnumerable<DependencyInfo>? Dependencies { get; }

        public PackageInfo(string id, IDictionary<string, string> paths, IEnumerable<DependencyInfo>? dependencies = null) : base(id)
        {
            Dependencies = dependencies;
            _paths = paths;
        }

        public override IEnumerable<(string PackagePath, string OnDiskPath)> GetFilesToExtract(string basePath)
        {
            foreach (var kvp in _paths)
            {
                yield return (kvp.Key, Path.Combine(basePath, kvp.Value));
            }
        }

        public string MainPackagePath => _paths.Single(kvp => kvp.Key.EndsWith($"{Id}.dll", StringComparison.OrdinalIgnoreCase)).Value;
    }
}
