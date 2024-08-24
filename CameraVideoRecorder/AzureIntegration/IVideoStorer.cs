using System.Diagnostics;

namespace CameraVideoRecorder.AzureIntegration
{
    internal interface IVideoStorer
    {
        Task PushToAzureAsync(Stream stream, CancellationToken stoppingToken);
    }
}
