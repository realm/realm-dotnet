using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;
using Realms.LFS;
using System.Threading.Tasks;

namespace Realm.LFS.Azure
{
    public class AzureFileManager : RemoteFileManager
    {
        private readonly CloudBlobContainer _container;
        public AzureFileManager(string connectionString, string container)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            _container = client.GetContainerReference(container);
            _container.CreateIfNotExists();
        }

        protected override async Task DeleteFile(string id)
        {
            var blob = _container.GetBlockBlobReference(id);
            await blob.DeleteIfExistsAsync();
        }

        protected override async Task DownloadFile(string id, string file)
        {
            var context = new SingleTransferContext();
            var blob = _container.GetBlockBlobReference(id);
            await TransferManager.DownloadAsync(blob, file, null, context);
        }

        protected override async Task<string> UploadFile(string id, string file)
        {
            var context = new SingleTransferContext();
            var blob = _container.GetBlockBlobReference(id);
            await TransferManager.UploadAsync(file, blob, null, context);
            return blob.Uri.AbsoluteUri;
        }
    }
}
