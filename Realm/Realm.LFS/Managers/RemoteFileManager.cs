using Nito.AsyncEx;
using Realms.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Realms.LFS
{
    public abstract class RemoteFileManager
    {
        private const int DefaultRetries = 3;
        private const int MaxRetryDelay = 60000;
        private const int MinRetryDelay = 1000;

        private RealmConfigurationBase _config;
        private readonly ConcurrentQueue<(string dataId, int retries)> _uploadQueue = new ConcurrentQueue<(string, int)>();
        private readonly AsyncContextThread _backgroundThread = new AsyncContextThread();
        private TaskCompletionSource<object> _completionTcs;

        private int _retryDelay = 500;

        private int _isProcessing = 0;

        internal event EventHandler<FileUploadedEventArgs> OnFileUploaded;

        protected RemoteFileManager()
        {
        }

        internal void Start(RealmConfigurationBase config)
        {
            _config = config;
            _backgroundThread.Factory.Run(EnqueueExisting);
        }

        internal void EnqueueUpload(string dataId)
        {
            _uploadQueue.Enqueue((dataId, DefaultRetries));

            _backgroundThread.Factory.Run(ProcessQueue);
        }

        internal Task DownloadFile(FileData data, string destinationFile)
        {
            Argument.Ensure(data.Status == DataStatus.Remote, $"Expected remote data, got {data.Status}", nameof(data));

            return DownloadFile(GetId(data.Id), destinationFile);
        }

        internal Task WaitForUploads()
        {
            _completionTcs = new TaskCompletionSource<object>();

            _backgroundThread.Factory.Run(ProcessQueue);

            return _completionTcs.Task;
        }

        protected abstract Task<string> UploadFile(string id, string file);

        protected abstract Task DownloadFile(string id, string file);

        protected abstract Task DeleteFile(string id);

        private async Task EnqueueExisting()
        {
            using (var realm = Realm.GetInstance(_config))
            {
                var unprocessedDatas = realm.All<FileData>().Filter($"StatusInt == {(int)DataStatus.Local}");
                foreach (var item in unprocessedDatas)
                {
                    _uploadQueue.Enqueue((item.Id, DefaultRetries));
                }
            }

            await ProcessQueue();
        }

        private async Task ProcessQueue()
        {
            if (Interlocked.Exchange(ref _isProcessing, 1) != 0)
            {
                // Someone else is processing
                return;
            }

            var retryHash = new HashSet<string>();

            try
            {
                using (var realm = Realm.GetInstance(_config))
                {
                    while (_uploadQueue.TryDequeue(out var item))
                    {
                        var (result, filePath) = await UploadItem(realm, item.dataId);
                        switch (result)
                        {
                            case UploadStatus.Success:
                                OnFileUploaded?.Invoke(this, new FileUploadedEventArgs
                                {
                                    FileDataId = item.dataId,
                                    FilePath = filePath,
                                    RealmPath = _config.DatabasePath,
                                });
                                break;
                            case UploadStatus.Failure:
                                if (item.retries > 0)
                                {
                                    _uploadQueue.Enqueue((item.dataId, item.retries - 1));
                                }
                                else
                                {
                                    retryHash.Add(item.dataId);
                                }
                                break;
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
                if (_completionTcs != null)
                {
                    _completionTcs.TrySetResult(null);
                }


                var nextDelay = _retryDelay * (retryHash.Count == 0 ? 0.5 : 2);
                _retryDelay = (int)Math.Max(MinRetryDelay, Math.Min(MaxRetryDelay, nextDelay));

                Interlocked.Exchange(ref _isProcessing, 0);
            }

            if (retryHash.Count > 0)
            {
                await Task.Delay(_retryDelay);
                foreach (var item in retryHash)
                {
                    _uploadQueue.Enqueue((item, DefaultRetries));

                    // Don't await
                    _ = _backgroundThread.Factory.Run(ProcessQueue);
                }
            }
        }

        private async Task<(UploadStatus status, string filePath)> UploadItem(Realm realm, string dataId)
        {
            var filePath = FileManager.GetFilePath(_config, dataId);
            if (!File.Exists(filePath))
            {
                return (UploadStatus.NotApplicable, null);
            }

            var data = realm.Find<FileData>(dataId);
            if (data == null || data.Status == DataStatus.Remote)
            {
                return (UploadStatus.NotApplicable, null);
            }

            try
            {
                var url = await UploadFile(GetId(dataId), filePath);

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

                        await DeleteFile(dataId);
                        return (UploadStatus.NotApplicable, null);
                    }

                    data.Url = url;
                    data.Status = DataStatus.Remote;
                    transaction.Commit();

                    return (UploadStatus.Success, filePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return (UploadStatus.Failure, null);
            }
        }

        private string GetId(string dataId)
        {
            var realmHash = HashHelper.MD5(Path.GetFileNameWithoutExtension(_config.DatabasePath));
            return $"{realmHash}/{dataId}";
        }

        enum UploadStatus
        {
            Success,
            Failure,
            NotApplicable
        }
    }
}
