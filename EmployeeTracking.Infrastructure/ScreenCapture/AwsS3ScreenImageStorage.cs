using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using EmployeeTracking.Application.ScreenCapture;
using EmployeeTracking.Domain.Common;
using Microsoft.Extensions.Options;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class AwsS3ScreenImageStorage : IScreenImageStorage
    {
        private readonly AwsStorageOptions _options;

        public AwsS3ScreenImageStorage(IOptions<ScreenCaptureOptions> options) => _options = options.Value.Aws;

        public async Task<string> SaveAsync(Guid employeeId, Stream content, string contentType, CancellationToken ct = default)
        {
            var key = $"{employeeId:N}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.jpg";
            using var client = Client();
            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _options.Bucket,
                Key = key,
                InputStream = content,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType
            }, ct);
            return key;
        }

        public async Task<byte[]?> ReadAsync(string storageKey, CancellationToken ct = default)
        {
            using var client = Client();
            try
            {
                using var response = await client.GetObjectAsync(_options.Bucket, storageKey, ct);
                using var memory = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memory, ct);
                return memory.ToArray();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private AmazonS3Client Client()
        {
            if (string.IsNullOrWhiteSpace(_options.Bucket)
                || string.IsNullOrWhiteSpace(_options.Region)
                || string.IsNullOrWhiteSpace(_options.AccessKey)
                || string.IsNullOrWhiteSpace(_options.SecretKey))
                throw new DomainException("AWS screen storage is not configured.");

            return new AmazonS3Client(_options.AccessKey, _options.SecretKey, RegionEndpoint.GetBySystemName(_options.Region));
        }
    }
}
