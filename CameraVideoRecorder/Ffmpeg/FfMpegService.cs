
using CameraVideoRecorder.Arguments;
using CameraVideoRecorder.AzureIntegration;
using System.Diagnostics;

namespace CameraVideoRecorder.Ffmpeg
{
    internal class FfMpegService : IFfmpegService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ISecretProvider _secretProvider;
        private Process _process;

        private const string OutputFileName = "output_file_";
        private const string OutputFileExtension = ".mp4";
        private const string FfmpegExeName = "ffmpeg.exe";

        public FfMpegService(ICameraRecorderArgumentProvider argumentProvider, 
            ISecretProvider secretProvider)
        {
            _argumentProvider = argumentProvider;
            _secretProvider = secretProvider;
        }

        public async Task StartRecordingAsync(CancellationToken token)
        {
            string ipAddress = _argumentProvider.Arguments[ArgumentConstants.CameraIpAddress];

            string inputPath = _argumentProvider.Arguments[ArgumentConstants.FfmpegDirectoryPath];
            string inputFile = Path.Combine(inputPath, FfmpegExeName);

            string outputPath = _argumentProvider.Arguments[ArgumentConstants.OutputPath];
            string outputFile = Path.Combine(outputPath, OutputFileName + DateTime.UtcNow.ToString("yyyy_mm_dd_HH_mm_ss") + OutputFileExtension);

            string cameraLogin = await _secretProvider.GetSecretAsync(SecretType.CameraLogin);
            string cameraPassword = await _secretProvider.GetSecretAsync(SecretType.CameraPassword);

            foreach (string file in Directory.GetFiles(outputPath))
            {
                if (file.StartsWith(OutputFileName))
                {
                    File.Delete(file);
                }
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = inputFile,
                Arguments = $"-i rtsp://{cameraLogin}:{cameraPassword}@{ipAddress}/live0 -c copy {outputFile}",
                RedirectStandardInput = true
            };

            _process = Process.Start(processStartInfo);

            await Task.Delay(2_000, token);

            if (_process.HasExited)
            {
                throw new InvalidOperationException("Unable to start ffmpeg");
            }
        }

        public async Task StopRecordingAsync(CancellationToken ct)
        {
            if (_process == null || _process.HasExited)
            {
                return;
            }

            using (StreamWriter sw = _process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("q");
                }
            }

            await Task.Delay(2_000, ct);

            if (!_process.HasExited)
            {
                throw new InvalidOperationException("Unable to stop ffmpeg");
            }

            _process.Dispose();
        }
    }
}
