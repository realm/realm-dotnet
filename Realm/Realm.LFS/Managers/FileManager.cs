using Realms.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Realms.LFS
{
    public static class FileManager
    {
        private static string _persistenceLocation;
        private static Placeholder _placeholder;
        private static Func<RemoteFileManager> _remoteManagerFactory;
        private static ConcurrentDictionary<string, RemoteFileManager> _remoteManagers = new ConcurrentDictionary<string, RemoteFileManager>();

        public static event EventHandler<FileUploadedEventArgs> OnFileUploaded;

        public static void Initialize(FileManagerOptions options)
        {
            _persistenceLocation = options.PersistenceLocation;
            _placeholder = options.Placeholder;
            _remoteManagerFactory = options.RemoteManagerFactory ?? FileManagerOptions.DefaultRemoteManagerFactory;

            Argument.Ensure(_remoteManagerFactory != null, "Either a RemoteManagerFactory or DefaultRemoteManagerFactory must be provided.", nameof(options));
        }

        public static Task WaitForUploads(RealmConfigurationBase config)
        {
            return GetManager(config).WaitForUploads();
        }

        internal static Stream ReadFile(FileLocation location, string id)
        {
            var path = Path.Combine(GetPath(location), id);
            return ReadFileCore(path, generatePlaceholder: false);
        }

        internal static async Task<Stream> ReadFile(FileData data)
        {
            var dataPath = GetFilePath(data);
            if (!File.Exists(dataPath))
            {
                // If it's supposed to be local but file is missing, generate a placeholder
                // This also handles the unamanaged case.
                if (data.Status == DataStatus.Local)
                {
                    return _placeholder.GeneratePlaceholder(data.Name);
                }

                var tempPath = dataPath + ".temp";
                await GetManager(data.Realm.Config).DownloadFile(data, tempPath);

                File.Move(tempPath, dataPath);
            }

            return File.OpenRead(dataPath);
        }

        internal static string GetFilePath(RealmConfigurationBase config, string id)
        {
            return Path.Combine(GetPath(config), id);
        }

        internal static string GetFilePath(FileData data)
        {
            if (data.IsManaged)
            {
                return GetFilePath(data.Realm.Config, data.Id);
            }

            return Path.Combine(GetPath(FileLocation.Temporary), data.Id);
        }

        internal static bool FileExists(FileLocation location, string id)
        {
            var path = Path.Combine(GetPath(location), id);
            return File.Exists(path);
        }

        private static Stream ReadFileCore(string path, string nameForPlaceholder = null, bool generatePlaceholder = true)
        {
            if (File.Exists(path))
            {
                return File.OpenRead(path);
            }

            if (generatePlaceholder)
            {
                return _placeholder.GeneratePlaceholder(nameForPlaceholder);
            }

            return Stream.Null;
        }

        internal static void WriteFile(FileLocation location, string id, Stream stream)
        {
            var filePath = Path.Combine(GetPath(location), id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fs = File.OpenWrite(filePath))
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                stream.CopyTo(fs);
            }
        }

        internal static void CopyFile(FileLocation location, string id, string file)
        {
            var targetFile = Path.Combine(GetPath(location), id);
            File.Copy(file, targetFile, overwrite: true);
        }

        internal static void UploadFile(FileLocation fromLocation, FileData data)
        {
            Argument.Ensure(data.IsManaged, "Expected data to be managed.", nameof(data));

            var sourceFile = Path.Combine(GetPath(fromLocation), data.Id);
            if (File.Exists(sourceFile))
            {
                var targetFile = Path.Combine(GetPath(data.Realm.Config), data.Id);
                File.Move(sourceFile, targetFile);
                GetManager(data.Realm.Config).EnqueueUpload(data.Id);
            }
        }

        private static string GetPath(FileLocation location)
        {
            var folderPath = Path.Combine(_persistenceLocation, location.ToString());
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private static string GetPath(RealmConfigurationBase config)
        {
            var realmFolder = Path.GetDirectoryName(config.DatabasePath);
            var folderPath = Path.Combine(realmFolder, ".lfs");
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private static RemoteFileManager GetManager(RealmConfigurationBase config)
        {
            return _remoteManagers.GetOrAdd(config.DatabasePath, (key) =>
            {
                var result = _remoteManagerFactory();
                result.Start(config);
                result.OnFileUploaded += (s, e) =>
                {
                    OnFileUploaded?.Invoke(s, e);
                };

                // TODO: trigger cleanup event with a list of files that can be deleted.

                return result;
            });
        }
    }
}
