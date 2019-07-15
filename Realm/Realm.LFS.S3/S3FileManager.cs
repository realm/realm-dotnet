using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Realms.LFS;
using System;
using System.IO;

namespace Realm.LFS.S3
{
    public class S3FileManager : RemoteFileManager
    {
        private readonly AmazonS3Client _s3Client;

        public S3FileManager(string accessKey, string secretKey, RegionEndpoint region)
        {
            var creds = new BasicAWSCredentials(accessKey, secretKey);
            _s3Client = new AmazonS3Client(creds, region);
        }

        protected override void DeleteFile(string id)
        {
            throw new NotImplementedException();
        }

        protected override Stream DownloadFile(string id)
        {
            _s3Client.GetObjectAsync(new GetObjectRequest
            {
                Key = id,
                
            })
        }

        protected override string UploadFile(string id, Stream file)
        {
            throw new NotImplementedException();
        }
    }
}
