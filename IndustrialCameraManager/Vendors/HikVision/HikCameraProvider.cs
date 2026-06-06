using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using MvCameraControl;
using System.Collections.Generic;
using System.Linq;

namespace IndustrialCameraManager.Vendors.HikVision
{
    public class HikCameraProvider(StreamManager streamManager) : ICameraProvider
    {
        private readonly StreamManager streamManager = streamManager;

        public ICamera Create(ICameraInfo info)
        {
            if (info is not HikCameraInfo hikInfo)
                throw new System.ArgumentException("Invalid camera info type.");

            var stream = streamManager.GetOrCreateStream(info.SerialNumber);
            return new HikCamera(hikInfo.Native, stream);
        }

        public ICamera Create(string IpAddress, string netExport)
        {
            if (string.IsNullOrWhiteSpace(IpAddress) || string.IsNullOrWhiteSpace(netExport))
                throw new System.ArgumentException("IP address is invalid");

            var stream = streamManager.GetOrCreateStream(IpAddress);
            return new HikCamera(IpAddress, netExport, stream);
        }

        public IEnumerable<ICamera> CreateAll() => Enumerate().Select(info => Create(info));

        public IEnumerable<ICameraInfo> Enumerate(CameraType type)
        {
            DeviceTLayerType camType = type switch
            {
                CameraType.GigE => DeviceTLayerType.MvGigEDevice,
                CameraType.Usb => DeviceTLayerType.MvUsbDevice,
                CameraType.CameraLink => DeviceTLayerType.MvCameraLinkDevice,
                CameraType.GenTL => DeviceTLayerType.MvGenTLCXPDevice,
                _ => DeviceTLayerType.MvGigEDevice
                                            | DeviceTLayerType.MvUsbDevice
                                            | DeviceTLayerType.MvGenTLCXPDevice
                                            | DeviceTLayerType.MvGenTLXoFDevice
            };

            int result = DeviceEnumerator.EnumDevices(camType, out var camInfoList);

            if (result == MvError.MV_OK)
            {
                foreach (var cam in camInfoList)
                {
                    yield return new HikCameraInfo(
                        serialNumber: cam.SerialNumber,
                        modelName: cam.ModelName,
                        userDefinedName: cam.UserDefinedName,
                        manufacturer: "HikVision",
                        cameraVersion: cam.DeviceVersion,
                        native: cam);
                }
            }
        }

        public IEnumerable<ICameraInfo> Enumerate() => Enumerate(CameraType.ALL);
    }
}
