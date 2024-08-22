using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using CameraVideoRecorder.Camera;
using CameraVideoRecorder.Ffmpeg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CameraVideoRecorder.Recording
{
    internal class CameraRecorderHostedService : IHostedService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ICameraIpPinger _cameraIpPinger;
        private readonly IFfmpegService _ffmpegService;
        private readonly IVideoStorer _videoStorer;
        private readonly ILogger<CameraRecorderHostedService> _logger;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting...");

            _argumentProvider.ParseArguments();

            if (!_cameraIpPinger.CanPingCamera())
            {
                _logger.LogInformation("Unable to ping camera");
                return;
            }

            TimeSpan delayTimeSpan = GetDelay();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await _ffmpegService.StartRecordingAsync();

                    await Task.Delay(delayTimeSpan, _cancellationTokenSource.Token);

                    await _ffmpegService.StopRecordingAsync();
                    await _videoStorer.PushToAzureAsync();
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            await _ffmpegService.StopRecordingAsync();
            await _videoStorer.PushToAzureAsync();
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
    }
}
