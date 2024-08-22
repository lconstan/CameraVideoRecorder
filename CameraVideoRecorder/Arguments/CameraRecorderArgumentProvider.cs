using Microsoft.Extensions.Logging;

namespace CameraVideoRecorder.Arguments
{
    internal class CameraRecorderArgumentProvider : ICameraRecorderArgumentProvider
    {
        private readonly ILogger<CameraRecorderArgumentProvider> _logger;

        public Dictionary<string, string> Arguments { get; } = new Dictionary<string, string>();

        public CameraRecorderArgumentProvider(ILogger<CameraRecorderArgumentProvider> logger)
        {
            _logger = logger;
        }

        public void ParseArguments()
        {
            var args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                string[] parts = arg.Split('=');

                if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];
                    Arguments[key] = value;

                    _logger.LogInformation($"Parsed {key} - {value}");
                }
                else
                {
                    Arguments[arg] = null;
                }
            }

            CheckArgument(ArgumentConstants.CameraIpAddress);
        }

        public void CheckArgument(string argumentName)
        {
            if (!Arguments.ContainsKey(argumentName))
            {
                throw new InvalidOperationException($"No argument {argumentName} provided");
            }
        }
    }
}
