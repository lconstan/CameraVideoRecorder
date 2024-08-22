namespace CameraVideoRecorder.AzureIntegration
{
    internal interface ISecretProvider
    {
        Task<string> GetSecretAsync(SecretType secretType);
    }
}
