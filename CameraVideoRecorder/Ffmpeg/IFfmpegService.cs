namespace CameraVideoRecorder.Ffmpeg
{
    internal interface IFfmpegService
    {
        Task<bool> StartRecordingAsync(CancellationToken stoppingToken);

        Task StopRecordingAsync(CancellationToken stoppingToken);
    }
}
