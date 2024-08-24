using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CameraVideoRecorder.Arguments;
using Microsoft.Extensions.Logging;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class VideoStorer : IVideoStorer
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<VideoStorer> _logger;
        private readonly TimeSpan _azureStorePeriod = TimeSpan.FromSeconds(10);

        public VideoStorer(ICameraRecorderArgumentProvider argumentProvider, BlobServiceClient blobServiceClient, ILogger<VideoStorer> logger)
        {
            _argumentProvider = argumentProvider;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task PushToAzureAsync(Stream stream, CancellationToken ct)
        {
            _logger.LogInformation("Pushing to azure...");

            AppendBlobClient blobClient = await CreateNewBlobAsync(ct);
            
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[80 * 1024]; // 80 KB buffer
            int bytesRead;
            DateTime lastWriteUtc = DateTime.UtcNow;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);

                if (DateTime.UtcNow - lastWriteUtc > _azureStorePeriod)
                {
                    if (lastWriteUtc.Day != DateTime.UtcNow.Day)
                    {
                        blobClient = await CreateNewBlobAsync(ct);
                    }

                    await PushToAzureAsync(blobClient, memoryStream, ct);

                    lastWriteUtc = DateTime.UtcNow;
                }
            }

            // Upload any remaining data in the memory stream
            if (memoryStream.Length > 0)
            {
                await PushToAzureAsync(blobClient, memoryStream, ct);
            }

            _logger.LogInformation("Upload completed successfully.");
        }

        private async Task<AppendBlobClient> CreateNewBlobAsync(CancellationToken ct)
        {
            string blobName = $"video_{DateTime.UtcNow.ToString("yyyy_mm_dd_HH_mm_ss")}.ts";

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("videorecordercontainer");

            AppendBlobClient blobClient = blobContainerClient.GetAppendBlobClient(blobName);

            if (!await blobClient.ExistsAsync(ct))
            {
                _logger.LogInformation("Creating a new append blob...");
                await blobClient.CreateAsync(cancellationToken: ct);
            }

            return blobClient;
        }

        private static async Task PushToAzureAsync(AppendBlobClient blobClient, MemoryStream memoryStream, CancellationToken ct)
        {
            memoryStream.Position = 0; // Reset stream to the beginning
            await blobClient.AppendBlockAsync(memoryStream, cancellationToken: ct);
            memoryStream.SetLength(0); // Clear the stream after upload
        }
    }
}
