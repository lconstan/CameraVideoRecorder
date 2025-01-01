using System.Diagnostics;
using CameraVideoRecorder.AzureIntegration;
using CameraVideoRecorder.Camera;
using CameraVideoRecorder.Ffmpeg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: Fody.ConfigureAwait(false)]

namespace CameraVideoRecorder.HostedService
{
    internal class CameraRecorderHostedService : BackgroundService
    {
        private readonly ICameraIpService _cameraIpPinger;
        private readonly IFfmpegService _ffmpegService;
        private readonly IVideoStorer _videoStorer;
        private readonly ILogger<CameraRecorderHostedService> _logger;

        public CameraRecorderHostedService(ICameraIpService cameraIpPinger,
            IFfmpegService ffmpegService,
            IVideoStorer videoStorer,
            ILogger<CameraRecorderHostedService> logger)
        {
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
                    try
                    {
                        await _ffmpegService.StopRecordingAsync(ffmpegProcess, stoppingToken);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Critical error while stopping recording");
                    }
                }
            }
        }
    }
}
