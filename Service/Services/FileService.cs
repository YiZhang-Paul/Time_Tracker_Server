using Amazon.S3;
using Amazon.S3.Model;
using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FileService : IFileService
    {
        private IConfiguration Configuration { get; }
        private AmazonS3Client S3Client { get; }

        public FileService(IConfiguration configuration, AmazonS3Client s3Client)
        {
            Configuration = configuration;
            S3Client = s3Client;
        }

        public async Task<string> GetAvatarUrl(long userId)
        {
            var key = GetAvatarKey(userId);

            if (!await IsValidKey(key).ConfigureAwait(false))
            {
                return null;
            }

            var request = new GetPreSignedUrlRequest
            {
                BucketName = Configuration["Aws:S3BucketName"],
                Key = key,
                Expires = DateTime.UtcNow.AddDays(30)
            };

            return S3Client.GetPreSignedURL(request);
        }

        public async Task<bool> UploadAvatar(long userId, Stream avatar)
        {
            var request = new PutObjectRequest
            {
                InputStream = avatar,
                BucketName = Configuration["Aws:S3BucketName"],
                Key = GetAvatarKey(userId)
            };

            var response = await S3Client.PutObjectAsync(request).ConfigureAwait(false);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        private string GetAvatarKey(long userId)
        {
            return $"users/{userId}/avatar.jpg";
        }

        private async Task<bool> IsValidKey(string key)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = Configuration["Aws:S3BucketName"],
                Prefix = key
            };

            var response = await S3Client.ListObjectsV2Async(request).ConfigureAwait(false);

            return response.S3Objects.Any();
        }
    }
}
