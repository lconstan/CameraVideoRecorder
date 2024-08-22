namespace CameraVideoRecorder.OutputFile
{
    internal class OutputFileRepository : IOutputFileRepository
    {
        private string _filePath;

        public string GetLastFilePath()
        {
            return _filePath;
        }

        public void StoreFilePath(string filePath)
        {
            _filePath = filePath;
        }
    }
}
