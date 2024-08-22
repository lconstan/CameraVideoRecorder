namespace CameraVideoRecorder.Ffmpeg
{
    internal interface IFfmpegService
    {
        Task StartRecordingAsync(CancellationToken stoppingToken);

        Task StopRecordingAsync(CancellationToken stoppingToken);
    }
}
