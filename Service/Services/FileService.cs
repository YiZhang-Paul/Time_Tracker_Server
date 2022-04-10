using Amazon.S3;
using Amazon.S3.Model;
using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
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

        public async Task<bool> UploadAvatar(long userId, Stream avatar)
        {
            var request = new PutObjectRequest
            {
                InputStream = avatar,
                BucketName = Configuration["Aws:S3BucketName"],
                Key = $"users/{userId}/avatar.jpg"
            };

            var result = await S3Client.PutObjectAsync(request).ConfigureAwait(false);

            return result.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
