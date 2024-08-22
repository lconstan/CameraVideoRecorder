namespace CameraVideoRecorder.Arguments
{
    internal interface ICameraRecorderArgumentProvider
    {
        Dictionary<string, string> Arguments { get; }
        void ParseArguments();
    }
}
