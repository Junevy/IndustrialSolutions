using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using IndustrialCameraManager.Stream;
using MvCameraControl;
using System.Collections.Generic;

namespace IndustrialCameraManager.HikVision
{
    /// <summary>
    /// 海康工业相机的Provider类
    /// 用于：
    ///     1）枚举海康工业相机；
    ///     2）创建海康工业相机的实例。
    /// </summary>
    public class HikCameraProvider : ICameraProvider
    {
        private readonly StreamManager manager;

        public HikCameraProvider(StreamManager manager)
        {
            this.manager = manager;
        }

        public ICamera Create(ICameraInfo info)
        {
            if (info is not HikCameraInfo hikInfo)
                throw new System.ArgumentException("Invalid camera info type.");

            var stream = manager.GetOrCreateStream(info.SerialNumber);
            return new HikCamera(hikInfo.Native, stream);
        }

        public ICamera Create(string IpAddress, string netExport)
        {
            if (string.IsNullOrWhiteSpace(IpAddress) || string.IsNullOrWhiteSpace(netExport))
                throw new System.ArgumentException("IP address is invalid");

            var stream = manager.GetOrCreateStream(IpAddress);
            return new HikCamera(IpAddress, netExport, stream);
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

        public void Dispose()
        {
            manager.Dispose();
        }
    }
}
