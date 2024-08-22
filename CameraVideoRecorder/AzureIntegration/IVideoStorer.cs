namespace CameraVideoRecorder.AzureIntegration
{
    internal interface IVideoStorer
    {
        Task PushToAzureAsync();
    }
}
