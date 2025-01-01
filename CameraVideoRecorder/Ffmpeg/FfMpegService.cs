using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using CameraVideoRecorder.Camera;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CameraVideoRecorder.Ffmpeg
{
    internal class FfMpegService : IFfmpegService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ISecretProvider _secretProvider;
        private readonly ICameraIpService _cameraIpService;
        private readonly ILogger<FfMpegService> _logger;

        private const string OutputFileName = "output_file_";
        private const string OutputFileExtension = ".ts";
        private const string FfmpegExeName = "ffmpeg.exe";

        public FfMpegService(ICameraRecorderArgumentProvider argumentProvider,
            ISecretProvider secretProvider,
            ICameraIpService cameraIpService,
            ILogger<FfMpegService> logger)
        {
            _argumentProvider = argumentProvider;
            _secretProvider = secretProvider;
            _cameraIpService = cameraIpService;
            _logger = logger;
        }

        public async Task<Process> StartRecordingAsync(CancellationToken token)
        {
            string ipAddress = _cameraIpService.GetCameraIpAddress();

            string inputPath = _argumentProvider.Arguments[ArgumentConstants.FfmpegDirectoryPath];
            string inputFile = Path.Combine(inputPath, FfmpegExeName);

            string cameraLogin = await _secretProvider.GetSecretAsync(SecretType.CameraLogin);
            string cameraPassword = await _secretProvider.GetSecretAsync(SecretType.CameraPassword);

            ProcessStartInfo ffmpegProcessStartInfo = new ProcessStartInfo()
            {
                FileName = inputFile,
                Arguments = $"-i rtsp://{cameraLogin}:{cameraPassword}@{ipAddress}/live0 -f mpegts pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Process? ffmpegProcess = Process.Start(ffmpegProcessStartInfo);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                using (var reader = ffmpegProcess.StandardError)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        _logger.LogInformation(line);
                    }
                }
            }, token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return ffmpegProcess;
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
