
using Azure.Storage.Blobs;
using CameraVideoRecorder.OutputFile;

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

        public async Task PushToAzureAsync()
        {
            string lastFilePath = _outputFileRepository.GetLastFilePath();

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("videorecordercontainer");
            BlobClient blobClient = blobContainerClient.GetBlobClient(lastFilePath);

            await blobClient.UploadAsync(lastFilePath, true);
        }
    }
}
