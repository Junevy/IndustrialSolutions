using IndustrialCameraManager.Core;
using MvCameraControl;
using System.Collections.Generic;

namespace IndustrialCameraManager.HikVision
{
    public class HikCameraProvider : ICameraProvider
    {
        public ICamera Create(ICameraInfo info)
        {
            if (info is not HikCameraInfo hikInfo)
                throw new System.ArgumentException("Invalid camera info type.");

            return new HikCamera(hikInfo.Native);
        }

        public ICamera Create(string IpAddress, string netExport)
        {
            if (string.IsNullOrWhiteSpace(IpAddress) || string.IsNullOrWhiteSpace(netExport))
                throw new System.ArgumentException("IP address is invalid");

            return new HikCamera(IpAddress, netExport);
        }

        public IEnumerable<ICamera> CreateAll()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ICameraInfo> Enumerate(CameraType type)
        {
            DeviceTLayerType camType = DeviceTLayerType.MvGigEDevice;

            switch (type)
            {
                case CameraType.GigE: camType = DeviceTLayerType.MvGigEDevice; break;
                case CameraType.Usb: camType = DeviceTLayerType.MvUsbDevice; break;
                case CameraType.GenTL: camType = DeviceTLayerType.MvGenTLCXPDevice; break;
                default: break;
            }

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
                        native: cam
                        );
                }
            }
        }

        public IEnumerable<ICameraInfo> Enumerate()
        {
            int result = DeviceEnumerator.EnumDevices(
                DeviceTLayerType.MvGigEDevice
                | DeviceTLayerType.MvUsbDevice
                | DeviceTLayerType.MvGenTLCXPDevice
                | DeviceTLayerType.MvGenTLXoFDevice,
                out var camInfoList);

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
                        native: cam
                        );
                }
            }

        }
    }
}
