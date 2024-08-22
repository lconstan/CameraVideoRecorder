namespace CameraVideoRecorder.Ffmpeg
{
    internal interface IFfmpegService
    {
        Task StartRecordingAsync();

        Task StopRecordingAsync();
    }
}
