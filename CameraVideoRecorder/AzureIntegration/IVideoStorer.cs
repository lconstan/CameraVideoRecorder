using System.Diagnostics;

namespace CameraVideoRecorder.AzureIntegration
{
    internal interface IVideoStorer
    {
        Task PushToAzureAsync(Process p, CancellationToken stoppingToken);
    }
}
