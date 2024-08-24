using System.Diagnostics;

namespace CameraVideoRecorder.Ffmpeg
{
    internal interface IFfmpegService
    {
        Task<Process> StartRecordingAsync(CancellationToken stoppingToken);

        Task StopRecordingAsync(Process p, CancellationToken stoppingToken);
    }
}
