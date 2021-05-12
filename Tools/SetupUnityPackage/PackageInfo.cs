using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet.Packaging;

namespace SetupUnityPackage
{
    public class PackageInfo
    {
        public string Id { get; }

        public DependencyMode DependencyMode { get; }

        private readonly IDictionary<string, string> _paths;

        internal PackageInfo(string id, DependencyMode mode, IDictionary<string, string> paths = null)
        {
            Id = id;
            DependencyMode = mode;
            _paths = paths ?? new Dictionary<string, string>();
        }

        public IEnumerable<(string PackagePath, string OnDiskPath)> GetFilesToExtract(PackageArchiveReader reader)
        {
            if (_paths.Keys.Any(p => p.Contains("*")))
            {
                var packageFiles = reader.GetFiles();
                foreach (var kvp in _paths)
                {
                    var regex = new Regex(kvp.Key);
                    foreach (var file in packageFiles)
                    {
                        var match = regex.Match(file);
                        if (!match.Success)
                        {
                            continue;
                        }

                        var onDiskPath = kvp.Value;
                        foreach (Group group in match.Groups)
                        {
                            onDiskPath = onDiskPath.Replace($"${group.Name}", group.Value);
                        }

                        yield return (file, onDiskPath);
                    }
                }
            }
            else
            {
                foreach (var kvp in _paths)
                {
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }

        public override string ToString()
        {
            return $"{Id} - {DependencyMode}";
        }
    }
}
