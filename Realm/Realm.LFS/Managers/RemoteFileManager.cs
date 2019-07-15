using Nito.AsyncEx;
using Realms.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Realms.LFS
{
    public abstract class RemoteFileManager
    {
        private RealmConfigurationBase _config;
        private readonly ConcurrentQueue<string> _uploadQueue = new ConcurrentQueue<string>();
        private readonly AsyncContextThread _backgroundThread = new AsyncContextThread();

        private int _isProcessing = 0;

        protected RemoteFileManager()
        {
        }

        internal void Start(RealmConfigurationBase config)
        {
            _config = config;
            _backgroundThread.Factory.StartNew(EnqueueExisting);
        }

        internal void EnqueueUpload(FileData data)
        {
            if (data.Status == DataStatus.Remote)
            {
                Logger.Error($"Expected data with Id: {data.Id} to be local, but was already remote.");
                return;
            }

            _uploadQueue.Enqueue(data.Id);

            _backgroundThread.Factory.StartNew(ProcessQueue);
            ProcessQueue();
        }

        internal Task DownloadFile(FileData data, string destinationFile)
        {
            Argument.Ensure(data.Status == DataStatus.Remote, $"Expected remote data, got {data.Status}", nameof(data));

            return DownloadFile(GetId(data.Id), destinationFile);
        }

        protected abstract Task<string> UploadFile(string id, string file);

        protected abstract Task DownloadFile(string id, string file);

        protected abstract Task DeleteFile(string id);

        private void EnqueueExisting()
        {
            using (var realm = Realm.GetInstance(_config))
            {
                var unprocessedDatas = realm.All<FileData>().Filter($"StatusInt == {(int)DataStatus.Local}");
                foreach (var item in unprocessedDatas)
                {
                    _uploadQueue.Enqueue(item.Id);
                }
            }

            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (Interlocked.Exchange(ref _isProcessing, 1) != 0)
            {
                // Someone else is processing
                return;
            }

            try
            {
                while (_uploadQueue.TryDequeue(out var dataId))
                {
                    var filePath = FileManager.GetFilePath(_config, dataId);
                    var url = UploadFile(GetId(dataId), filePath).GetAwaiter().GetResult();

                    using (var realm = Realm.GetInstance(_config))
                    {
                        var data = realm.Find<FileData>(dataId);
                        using (var transaction = realm.BeginWrite())
                        {
                            if (data == null)
                            {
                                data = realm.Find<FileData>(dataId);
                            }

                            if (data == null)
                            {
                                transaction.Rollback();

                                Logger.Error($"Could not find data with Id: {dataId}");
                                DeleteFile(dataId).GetAwaiter().GetResult();
                            }
                            else
                            {
                                data.Url = url;
                                data.Status = DataStatus.Remote;
                                transaction.Commit();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        private string GetId(string dataId)
        {
            var realmHash = HashHelper.MD5(Path.GetFileNameWithoutExtension(_config.DatabasePath));
            return Path.Combine(realmHash, dataId);
        }
    }
}
