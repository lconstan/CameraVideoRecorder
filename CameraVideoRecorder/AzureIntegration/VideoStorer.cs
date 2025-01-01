using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class VideoStorer : IVideoStorer
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<VideoStorer> _logger;

        private static readonly TimeSpan AzureStorePeriod = TimeSpan.FromSeconds(10);

        public VideoStorer(BlobServiceClient blobServiceClient, ILogger<VideoStorer> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task PushToAzureAsync(Stream stream, CancellationToken ct)
        {
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[80 * 1024]; // 80 KB buffer
            int bytesRead;
            DateTime lastWriteUtc = DateTime.UtcNow;

            AppendBlobClient blobClient = null;

            while ((bytesRead = await ReadStreamWithTimeoutAsync(stream, buffer, ct)) > 0)
            {
                if (blobClient == null || lastWriteUtc.Day != DateTime.UtcNow.Day)
                {
                    blobClient = await CreateNewBlobAsync(ct);
                }

                await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);

                if (DateTime.UtcNow - lastWriteUtc > AzureStorePeriod)
                {
                    await PushToAzureAsync(blobClient, memoryStream, ct);

                    lastWriteUtc = DateTime.UtcNow;
                }
            }

            _logger.LogInformation("Finishing video storing");

            // Upload any remaining data in the memory stream
            if (memoryStream.Length > 0)
            {
                await PushToAzureAsync(blobClient, memoryStream, ct);
            }
        }

        private static async Task<int> ReadStreamWithTimeoutAsync(Stream stream, byte[] buffer, CancellationToken ct)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(AzureStorePeriod);

            try
            {
                return await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
        }

        private async Task PushToAzureAsync(AppendBlobClient blobClient, MemoryStream memoryStream, CancellationToken ct)
        {
            memoryStream.Position = 0; // Reset stream to the beginning
            await blobClient.AppendBlockAsync(memoryStream, cancellationToken: ct);
            memoryStream.SetLength(0); // Clear the stream after upload
        }

        private async Task<AppendBlobClient> CreateNewBlobAsync(CancellationToken ct)
        {
            string blobName = $"video_{DateTime.UtcNow:yyyy_MM_dd_HH_mm}.ts";

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("videorecordercontainer");

            AppendBlobClient blobClient = blobContainerClient.GetAppendBlobClient(blobName);

            if (!await blobClient.ExistsAsync(ct))
            {
                _logger.LogInformation("Creating a new append blob {name}", blobName);
                await blobClient.CreateAsync(cancellationToken: ct);
            }

            return blobClient;
        }

    }
}
