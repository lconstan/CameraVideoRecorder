﻿namespace CameraVideoRecorder.AzureIntegration
{
    internal interface IVideoStorer
    {
        Task PushToAzureAsync(CancellationToken stoppingToken);
    }
}
