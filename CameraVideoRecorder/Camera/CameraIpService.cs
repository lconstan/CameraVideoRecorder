using CameraVideoRecorder.Arguments;
using System.Net.NetworkInformation;
using System.Net;

namespace CameraVideoRecorder.Camera
{
    internal class CameraIpService : ICameraIpService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private const string _cameraMacAddress = "04-17-b6-01-39-c5";

        private string _cameraIp;

        public CameraIpService(ICameraRecorderArgumentProvider argumentProvider)
        {
            _argumentProvider = argumentProvider;
        }

        public bool CanPingCamera()
        {
            _cameraIp = GetCameraIpAddress();

            if (string.IsNullOrEmpty(_cameraIp))
            {
                return false;
            }

            // Try a ping in case arp cache is not up to date

            using Ping _ping = new Ping();

            // Will throw if unable to ping
            PingReply reply = _ping.Send(_cameraIp);

            return reply?.Status == IPStatus.Success;
        }

        private string GetCameraIpAddress()
        {
            using System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();

            using var sr = new StringReader(strOutput);
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                
                if (!line.StartsWith("192.168"))
                {
                    continue;
                }

                if (line.Contains(_cameraMacAddress))
                {
                    return ParseIpAdress(line);
                }
            }

            return null;
        }

        private string ParseIpAdress(string line)
        {
            int index = 0;

            while (char.IsDigit(line[index]) || line[index] == '.')
            { 
                index++;
            }

            return line.Substring(0, index);
        }

        public string GetCameraIp()
        {
            return _cameraIp;
        }
    }
}
