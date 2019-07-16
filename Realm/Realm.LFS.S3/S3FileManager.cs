using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Threading.Tasks;

namespace Realms.LFS.S3
{
    public class S3FileManager : RemoteFileManager
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucket;
        public S3FileManager(AWSCredentials credentials, RegionEndpoint region, string bucket = "realm-lfs-data")
        {
            _s3Client = new AmazonS3Client(credentials, region);
            _bucket = bucket;
        }

        protected override async Task DeleteFile(string id)
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = id
            });
        }

        protected override async Task DownloadFile(string id, string file)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.DownloadAsync(new TransferUtilityDownloadRequest
            {
                Key = id,
                BucketName = _bucket,
                FilePath = file,
            });
        }

        protected override async Task<string> UploadFile(string id, string file)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = _bucket,
                FilePath = file,
                Key = id,
                StorageClass = S3StorageClass.StandardInfrequentAccess,
                CannedACL = S3CannedACL.PublicRead,
            };

            await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

            return $"https://{_bucket}.s3.amazonaws.com/{id}";
        }
    }
}
