
using Azure.Storage.Blobs;
using CameraVideoRecorder.Arguments;
using System.IO.Compression;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class VideoStorer : IVideoStorer
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly BlobServiceClient _blobServiceClient;

        public VideoStorer(ICameraRecorderArgumentProvider argumentProvider, BlobServiceClient blobServiceClient)
        {
            _argumentProvider = argumentProvider;
            _blobServiceClient = blobServiceClient;
        }

        public async Task PushToAzureAsync(CancellationToken ct)
        {
            string outputPath = _argumentProvider.Arguments[ArgumentConstants.OutputPath];
            string lastFilePath = Directory.GetFiles(outputPath).Single();

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
        }
    }
}
