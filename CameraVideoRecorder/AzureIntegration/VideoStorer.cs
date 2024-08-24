
using Azure.Storage.Blobs;
using CameraVideoRecorder.Arguments;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.Metadata;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class VideoStorer : IVideoStorer
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<VideoStorer> _logger;

        public VideoStorer(ICameraRecorderArgumentProvider argumentProvider, BlobServiceClient blobServiceClient, ILogger<VideoStorer> logger)
        {
            _argumentProvider = argumentProvider;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public Task PushToAzureAsync(Process p, CancellationToken ct)
        {
            _logger.LogInformation("Pushing to azure...");

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("videorecordercontainer");
            BlobClient blobClient = blobContainerClient.GetBlobClient(Path.GetFileName($"video_{DateTime.UtcNow.ToString("yyyy_mm_dd_HH_mm_ss")}.ts"));

            return blobClient.UploadAsync(p.StandardOutput.BaseStream, true, ct);
        }
    }
}
