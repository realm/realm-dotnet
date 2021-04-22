using System.Collections.Generic;

namespace SetupUnityPackage
{
    public abstract class OptionsBase
    {
        public abstract PackageInfo[] Files { get; }

        public abstract string PackageBasePath { get; }

        public abstract ISet<string> IgnoredDependencies { get; }
    }
}
