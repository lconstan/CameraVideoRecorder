using CameraVideoRecorder.Arguments;
using System.Net.NetworkInformation;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CameraVideoRecorder.Camera
{
    internal class CameraIpService : ICameraIpService
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;
        private readonly ILogger<CameraIpService> _logger;
        private const string _cameraMacAddress = "04-17-b6-01-39-c5";

        private string _cameraIp;

        public CameraIpService(ICameraRecorderArgumentProvider argumentProvider, ILogger<CameraIpService> logger)
        {
            _argumentProvider = argumentProvider;
            _logger = logger;
        }

        public bool CanPingCamera()
        {
            _cameraIp = GetCameraIpAddress();

            _logger.LogInformation("Found ip {ip}", _cameraIp);

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetIpAddressWindows();
            }

            return GetIpAddressLinux();
        }

        private string GetIpAddressLinux()
        {
            string strOutput = RunProcessAndGetOutput("nmap", "-sn 192.168.1.0/24");

            using var sr = new StringReader(strOutput);
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                if (!line.Contains("Indoorcam"))
                {
                    continue;
                }

                return line.Replace("Nmap scan report for Indoorcam (", "").Replace(")", "");
            }

            return null;
        }

        private string GetIpAddressWindows()
        {
            string strOutput = RunProcessAndGetOutput("arp", "-a");
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
                    int index = 0;

                    while (char.IsDigit(line[index]) || line[index] == '.')
                    {
                        index++;
                    }

                    return line.Substring(0, index);
                }
            }

            return null;
        }

        private static string RunProcessAndGetOutput(string command, string parameter = null)
        {
            using var pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = command;

            if (parameter != null)
            {
                pProcess.StartInfo.Arguments = parameter;
            }

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();

            return pProcess.StandardOutput.ReadToEnd();
        }

        public string GetCameraIp()
        {
            return _cameraIp;
        }
    }
}
