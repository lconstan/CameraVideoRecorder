using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CameraVideoRecorder.Ffmpeg
{
    internal class FfMpegService : IFfmpegService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ISecretProvider _secretProvider;
        private readonly ILogger<FfMpegService> _logger;

        private const string OutputFileName = "output_file_";
        private const string OutputFileExtension = ".ts";
        private const string FfmpegExeName = "ffmpeg.exe";

        public FfMpegService(ICameraRecorderArgumentProvider argumentProvider,
            ISecretProvider secretProvider,
            ILogger<FfMpegService> logger)
        {
            _argumentProvider = argumentProvider;
            _secretProvider = secretProvider;
            _logger = logger;
        }

        public async Task<Process> StartRecordingAsync(CancellationToken token)
        {
            string ipAddress = _argumentProvider.Arguments[ArgumentConstants.CameraIpAddress];

            string inputPath = _argumentProvider.Arguments[ArgumentConstants.FfmpegDirectoryPath];
            string inputFile = Path.Combine(inputPath, FfmpegExeName);

            string cameraLogin = await _secretProvider.GetSecretAsync(SecretType.CameraLogin);
            string cameraPassword = await _secretProvider.GetSecretAsync(SecretType.CameraPassword);

            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = inputFile,
                Arguments = $"-i rtsp://{cameraLogin}:{cameraPassword}@{ipAddress}/live0 -f mpegts pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            return Process.Start(processStartInfo);
        }

        public async Task StopRecordingAsync(Process process, CancellationToken ct)
        {
            if (process == null || process.HasExited)
            {
                return;
            }

            using (StreamWriter sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("q");
                }
            }

            await Task.Delay(2_000, ct);

            if (!process.HasExited)
            {
                _logger.LogInformation("Killing process, camera seems offline");
                process.Kill();

                await Task.Delay(2_000, ct);

                if (!process.HasExited)
                {
                    throw new InvalidOperationException("Unable to stop ffmpeg");
                }
            }

            process.Dispose();
        }
    }
}
