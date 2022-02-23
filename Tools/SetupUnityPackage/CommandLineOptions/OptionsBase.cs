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
using CommandLine;

namespace SetupUnityPackage
{
    internal abstract class OptionsBase
    {
        [Option("no-ilrepack", Default = false, Required = false, HelpText = "Specify whether to skip bundling dependencies into the main package. This should not be used for release builds")]
        public bool NoRepack { get; set; }

        public abstract PackageInfo[] Files { get; }

        public abstract string PackageBasePath { get; }

        public abstract ISet<string> IgnoredDependencies { get; }
    }
}
