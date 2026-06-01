using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using System.Collections.Generic;
using System.Linq;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCameraProvider : ICameraProvider
    {
        private readonly StreamManager _streamManager;

        public IRaypleCameraProvider(StreamManager streamManager)
        {
            _streamManager = streamManager;
        }

        public ICamera Create(ICameraInfo info)
        {
            if (info is not IRaypleCameraInfo irInfo)
                throw new System.ArgumentException("Invalid camera info type.");

            var stream = _streamManager.GetOrCreateStream(info.SerialNumber);
            return new IRaypleCamera(irInfo.Native, stream);
        }

        public ICamera Create(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new System.ArgumentException("IP address is invalid");

            var stream = _streamManager.GetOrCreateStream(ipAddress);
            return new IRaypleCamera(ipAddress, stream);
        }

        public IEnumerable<ICamera> CreateAll() => Enumerate().Select(info => Create(info));

        public IEnumerable<ICameraInfo> Enumerate()
        {
            return Enumerate(CameraType.ALL);
        }

        public IEnumerable<ICameraInfo> Enumerate(CameraType type)
        {
            var devices = EnumerateDevicesInternal(type);

            foreach (var dev in devices)
            {
                var info = ToCameraInfo(dev);
                if (info != null)
                    yield return info;
            }
        }

        private static List<object> EnumerateDevicesInternal(CameraType type)
        {
            return new List<object>();
        }

        private static IRaypleCameraInfo ToCameraInfo(object native)
        {
            try
            {
                dynamic dev = native;
                return new IRaypleCameraInfo(
                    serialNumber: dev.SerialNumber ?? string.Empty,
                    modelName: dev.ModelName ?? string.Empty,
                    userDefinedName: dev.UserDefinedName ?? string.Empty,
                    cameraVersion: dev.CameraVersion ?? string.Empty,
                    cameraKey: dev.CameraKey ?? string.Empty,
                    native: native
                );
            }
            catch
            {
                return null;
            }
        }
    }
}
