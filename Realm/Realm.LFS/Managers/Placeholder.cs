using System;
using System.IO;

namespace Realms.LFS
{
    public class Placeholder
    {
        private const string PlaceholderId = "__placeholder";

        private readonly Func<string, Stream> _generator;

        private Placeholder(Func<string, Stream> generator)
        {
            _generator = generator;
        }

        public static Placeholder FromStream(Stream stream)
        {
            FileManager.WriteFile(FileLocation.System, PlaceholderId, stream);
            return new Placeholder((_) => FileManager.ReadFile(FileLocation.System, PlaceholderId));
        }

        public static Placeholder FromFile(string file)
        {
            FileManager.CopyFile(FileLocation.System, PlaceholderId, file);
            return new Placeholder((_) => FileManager.ReadFile(FileLocation.System, PlaceholderId));
        }

        public static Placeholder FromGenerator(Func<string, Stream> generator)
        {
            return new Placeholder(generator);
        }

        public Stream GeneratePlaceholder(string name) => _generator(name);
    }
}
