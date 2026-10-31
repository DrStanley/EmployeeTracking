using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EmployeeTracking.Application.ScreenCapture;
using EmployeeTracking.Domain.Common;
using Microsoft.Extensions.Options;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class AzureBlobScreenImageStorage : IScreenImageStorage
    {
        private readonly AzureStorageOptions _options;

        public AzureBlobScreenImageStorage(IOptions<ScreenCaptureOptions> options) => _options = options.Value.Azure;

        public async Task<string> SaveAsync(Guid employeeId, Stream content, string contentType, CancellationToken ct = default)
        {
            var container = Container();
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
            var key = $"{employeeId:N}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.jpg";
            await container.GetBlobClient(key).UploadAsync(content, new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType
            }, cancellationToken: ct);
            return key;
        }

        public async Task<byte[]?> ReadAsync(string storageKey, CancellationToken ct = default)
        {
            var blob = Container().GetBlobClient(storageKey);
            if (!await blob.ExistsAsync(ct)) return null;
            var download = await blob.DownloadContentAsync(ct);
            return download.Value.Content.ToArray();
        }

        private BlobContainerClient Container()
        {
            if (string.IsNullOrWhiteSpace(_options.ConnectionString) || string.IsNullOrWhiteSpace(_options.Container))
                throw new DomainException("Azure screen storage is not configured.");
            return new BlobContainerClient(_options.ConnectionString, _options.Container);
        }
    }
}
