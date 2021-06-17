using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SetupUnityPackage
{
    [Verb("tests", HelpText = "Setup the Tests.Unity project")]
    public class TestOptions : OptionsBase
    {
        public override string PackageBasePath => Path.Combine(Helpers.SolutionFolder, "Tests", "Tests.Unity", "Assets");

        public override ISet<string> IgnoredDependencies { get; } = new HashSet<string>
        {
            "Realm.Fody",
            "Fody",
            "Realm",
            "Microsoft.CSharp",
            "System.Configuration.ConfigurationManager"
        };

        public override PackageInfo[] Files { get; } = new[]
        {
            new PackageInfo("Realm.Tests", DependencyMode.IncludeIfRequested, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/Realm.Tests.dll", "Tests/Realm.Tests.dll" },
                { "lib/netstandard2.0/Realm.Tests.pdb", "Tests/Realm.Tests.pdb" },
            }),
            new PackageInfo("*", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/(?<file>.*\\.dll)", "Dependencies/$file" }
            })
        };
    }
}
