namespace CameraVideoRecorder.OutputFile
{
    internal interface IOutputFileRepository
    {
        void StoreFilePath(string filePath);

        string GetLastFilePath();
    }
}
