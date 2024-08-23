using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using CameraVideoRecorder.Camera;
using CameraVideoRecorder.Ffmpeg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CameraVideoRecorder.Recording
{
    internal class CameraRecorderHostedService : BackgroundService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ICameraIpPinger _cameraIpPinger;
        private readonly IFfmpegService _ffmpegService;
        private readonly IVideoStorer _videoStorer;
        private readonly ILogger<CameraRecorderHostedService> _logger;

        public CameraRecorderHostedService(ICameraRecorderArgumentProvider argumentProvider,
            ICameraIpPinger cameraIpPinger,
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

        private static TimeSpan GetDelay()
        {
            var delayTimeSpan =
#if DEBUG
    TimeSpan.FromSeconds(10);
#else
                TimeSpan.FromHours(1);
#endif
            return delayTimeSpan;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting...");

            _argumentProvider.ParseArguments();

            if (!_cameraIpPinger.CanPingCamera())
            {
                _logger.LogInformation("Unable to ping camera");
                Environment.Exit(0);
            }

            TimeSpan delayTimeSpan = GetDelay();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool started = false;
                    while (!started)
                    {
                        started = await _ffmpegService.StartRecordingAsync(stoppingToken);
                    }

                    await Task.Delay(delayTimeSpan, stoppingToken);

                    await _ffmpegService.StopRecordingAsync(stoppingToken);
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Error while recording");

                    await _videoStorer.PushToAzureAsync(stoppingToken);
                }
            }
        }
    }
}
