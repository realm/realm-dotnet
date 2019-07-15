using System;

namespace Realms.LFS
{
    public class FileManagerOptions
    {
        public Placeholder Placeholder { get; set; }

        public string PersistenceLocation { get; set; }

        public Func<RemoteFileManager> RemoteManagerFactory { get; set; }

        public static Func<RemoteFileManager> DefaultRemoteManagerFactory { get; set; }
    }
}
