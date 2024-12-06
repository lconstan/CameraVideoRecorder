using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using CameraVideoRecorder.Camera;
using CameraVideoRecorder.Ffmpeg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

[assembly: Fody.ConfigureAwait(false)]

namespace CameraVideoRecorder.Recording
{
    internal class CameraRecorderHostedService : BackgroundService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ICameraIpService _cameraIpPinger;
        private readonly IFfmpegService _ffmpegService;
        private readonly IVideoStorer _videoStorer;
        private readonly ILogger<CameraRecorderHostedService> _logger;

        public CameraRecorderHostedService(ICameraRecorderArgumentProvider argumentProvider,
            ICameraIpService cameraIpPinger,
            IFfmpegService ffmpegService,
            IVideoStorer videoStorer,
            ILogger<CameraRecorderHostedService> logger)
        {
            _argumentProvider = argumentProvider;
            _cameraIpPinger = cameraIpPinger;
            _ffmpegService = ffmpegService;
            _videoStorer = videoStorer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting...");

            if (!_cameraIpPinger.CanPingCamera())
            {
                _logger.LogInformation("Unable to ping camera");
                Environment.Exit(0);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                Process ffmpegProcess = null;

                try
                {
                    ffmpegProcess = await _ffmpegService.StartRecordingAsync(stoppingToken);

                    await _videoStorer.PushToAzureAsync(ffmpegProcess.StandardOutput.BaseStream, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while recording");
                }
                finally
                {
                    await _ffmpegService.StopRecordingAsync(ffmpegProcess, stoppingToken);
                }
            }
        }
    }
}
