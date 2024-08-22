
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CameraVideoRecorder.OutputFile;
using System.IO.Compression;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class VideoStorer : IVideoStorer
    {
        private readonly IOutputFileRepository _outputFileRepository;
        private readonly BlobServiceClient _blobServiceClient;

        public VideoStorer(IOutputFileRepository outputFileRepository, BlobServiceClient blobServiceClient)
        {
            _outputFileRepository = outputFileRepository;
            _blobServiceClient = blobServiceClient;
        }

        public async Task PushToAzureAsync(CancellationToken ct)
        {
            string lastFilePath = _outputFileRepository.GetLastFilePath();
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
