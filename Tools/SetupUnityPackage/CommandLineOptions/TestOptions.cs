using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace SetupUnityPackage
{
    [Verb("tests", HelpText = "Setup the Tests.Unity project")]
    public class TestOptions : OptionsBase
    {
        public override string PackageBasePath => Path.Combine(Helpers.SolutionFolder, "Tests", "Tests.Unity", "Assets");

        private static readonly ISet<string> _ignoredTestFiles = new HashSet<string>
        {
            "Program.cs"
        };

        public override ISet<string> IgnoredDependencies { get; } = new HashSet<string>
        {
            "Realm.Fody",
            "Fody",
            "Realm",
            "Microsoft.CSharp"
        };

        public override PackageInfo[] Files { get; } = new[]
        {
            new PackageInfo("Realm.Tests", DependencyMode.IncludeIfRequested),
            new PackageInfo("*", DependencyMode.IsDependency, new Dictionary<string, string>
            {
                { "lib/netstandard2.0/(?<file>.*\\.dll)", "Dependencies/$file" }
            })
        };

        public static bool ShouldIncludeTestFile(string relativePath)
            => relativePath.EndsWith(".cs") &&
               !_ignoredTestFiles.Contains(relativePath) &&
               !relativePath.StartsWith("obj") &&
               !relativePath.StartsWith("bin");
    }
}
