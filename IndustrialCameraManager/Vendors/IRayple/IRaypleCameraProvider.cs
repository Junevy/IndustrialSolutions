using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using System.Collections.Generic;
using System.Linq;
using MVSDK_Net;
using static MVSDK_Net.IMVDefine;
using System.Runtime.InteropServices;
using IndustrialCameraManager.Vendors.HikVision;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCameraProvider : ICameraProvider
    {
        private readonly StreamManager streamManager;

        public IRaypleCameraProvider(StreamManager streamManager)
        {
            this.streamManager = streamManager;
        }

        public ICamera Create(ICameraInfo info)
        {
            if (info is not IRaypleCameraInfo irInfo)
                throw new System.ArgumentException("Invalid camera info type.");

            var stream = streamManager.GetOrCreateStream(info.SerialNumber);

            return new IRaypleCamera(irInfo.Native, stream);
        }

        public ICamera Create(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new System.ArgumentException("IP address is invalid");

            var stream = streamManager.GetOrCreateStream(ipAddress);
            return new IRaypleCamera(ipAddress, stream);
        }

        public IEnumerable<ICamera> CreateAll() => Enumerate().Select(info => Create(info));

        public IEnumerable<ICameraInfo> Enumerate()
        {
            return Enumerate(CameraType.ALL);
        }

        public IEnumerable<ICameraInfo> Enumerate(CameraType type)
        {
            IMV_EInterfaceType camType = type switch
            {
                CameraType.GigE => IMV_EInterfaceType.interfaceTypeGige,
                CameraType.Usb => IMV_EInterfaceType.interfaceTypeUsb3,
                CameraType.GenTL => IMV_EInterfaceType.interfaceTypeAll,
                CameraType.CameraLink => IMV_EInterfaceType.interfaceTypeCL,
                _ => IMV_EInterfaceType.interfaceTypeAll
            };

            IMV_DeviceList deviceList = new IMVDefine.IMV_DeviceList();
            var result = MyCamera.IMV_EnumDevices(ref deviceList, (uint)camType);
            if (result == IMV_OK && deviceList.nDevNum > 0)
            {
                for (int i = 0; i < deviceList.nDevNum; i++)
                {
                    IMV_DeviceInfo camInfo = (IMV_DeviceInfo)Marshal.PtrToStructure(
                            deviceList.pDevInfo + Marshal.SizeOf(typeof(IMV_DeviceInfo)) * i,
                            typeof(IMV_DeviceInfo));
                    yield return ToCameraInfo(camInfo);
                }
            }
        }

        private IRaypleCameraInfo ToCameraInfo(IMV_DeviceInfo native)
        {
            try
            {
                return new IRaypleCameraInfo(
                    native.serialNumber,
                    native.modelName,
                    native.cameraName,
                    "iRayple",
                    native.deviceVersion,
                    native);
            }
            catch
            {
                return null;
            }
        }
    }
}
