using CameraVideoRecorder.Arguments;
using System.Net.NetworkInformation;

namespace CameraVideoRecorder.Camera
{
    internal class CameraIpPinger : ICameraIpPinger
    {
        private readonly ICameraRecorderArgumentProvider _argumentProvider;

        public CameraIpPinger(ICameraRecorderArgumentProvider argumentProvider)
        {
            _argumentProvider = argumentProvider;
        }

        public bool CanPingCamera()
        {
            string ipAddress = _argumentProvider.Arguments[ArgumentConstants.CameraIpAddress];
            using Ping _ping = new Ping();

            // Will throw if unable to ping
            PingReply reply = _ping.Send(ipAddress);

            return reply?.Status == IPStatus.Success;
        }
    }
}
