namespace CameraVideoRecorder.Camera
{
    internal interface ICameraIpService
    {
        bool CanPingCamera();

        string GetCameraIpAddress();
    }
}
