
using Azure.Storage.Blobs;
using CameraVideoRecorder.Arguments;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

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

        public async Task PushToAzureAsync(CancellationToken ct)
        {
            _logger.LogInformation("Pushing to azure...");

            string outputPath = _argumentProvider.Arguments[ArgumentConstants.OutputPath];
            string lastFilePath = Directory.GetFiles(outputPath).SingleOrDefault();

            if (lastFilePath == null)
            {
                _logger.LogInformation("No files to push");
                return;
            }

            string zippedFilePath = lastFilePath.Replace(Path.GetExtension(lastFilePath), ".zip");

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("videorecordercontainer");
            BlobClient blobClient = blobContainerClient.GetBlobClient(Path.GetFileName(zippedFilePath));

            using (Stream stream = await blobClient.OpenWriteAsync(true))
            {
                using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false))
                {
                    using (var fileStream = File.OpenRead(lastFilePath))
                    {
                        var entry = zip.CreateEntry(Path.GetFileName(lastFilePath), CompressionLevel.Optimal);
                        using (var innerFile = entry.Open())
                        {
                            await fileStream.CopyToAsync(innerFile);
                        }
                    }
                }
            }

            _logger.LogInformation("Pushed to azure");
        }
    }
}
