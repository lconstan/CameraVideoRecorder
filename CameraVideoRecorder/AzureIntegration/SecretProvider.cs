using Azure.Security.KeyVault.Secrets;

namespace CameraVideoRecorder.AzureIntegration
{
    internal class SecretProvider : ISecretProvider
    {
        private readonly SecretClient _secretClient;

        public SecretProvider(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        public async Task<string> GetSecretAsync(SecretType secretType)
        {
            var secret = await _secretClient.GetSecretAsync(secretType.ToString());

            return secret.Value.Value;
        }
    }
}
