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
using CommandLine;

namespace SetupUnityPackage
{
    [Verb("tests", HelpText = "Setup the Tests.Unity project")]
    internal class TestOptions : OptionsBase
    {
        [Option("realm-package", Required = false, HelpText = "Path to the folder containing io.realm.unity.tgz to use. If not specified, a local install will be assumed.")]
        public string? RealmPackage { get; set; }

        public override string PackageBasePath => Path.Combine(Helpers.SolutionFolder, "Tests", "Tests.Unity", "Assets");

        public override ISet<string> IgnoredDependencies { get; } = new HashSet<string>
        {
            "Fody",
            "Realm",
            "Microsoft.CSharp",
            "System.Configuration.ConfigurationManager"
        };

        private static readonly IEnumerable<DependencyInfo> _testsDependencies = new[]
        {
            new DependencyInfo("Nito.AsyncEx.Context", "lib/netstandard2.0/Nito.AsyncEx.Context.dll"),
            new DependencyInfo("Nito.AsyncEx.Tasks", "lib/netstandard2.0/Nito.AsyncEx.Tasks.dll"),
            new DependencyInfo("Nito.Disposables", "lib/netstandard2.0/Nito.Disposables.dll"),
            new DependencyInfo("System.Collections.Immutable", "lib/netstandard2.0/System.Collections.Immutable.dll"),
        };

        public override PackageInfo[] Files { get; } = new[]
        {
            new PackageInfo("Realm.Tests", new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.Tests.dll", "Tests/Realm.Tests.dll" },
                { "lib/netstandard2.0/Realm.Tests.pdb", "Tests/Realm.Tests.pdb" },
            }, _testsDependencies)
        };
    }
}
