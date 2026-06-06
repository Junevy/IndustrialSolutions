using IndustrialCameraManager.Abstractions;
using MvCameraControl;

namespace IndustrialCameraManager.Vendors.HikVision
{
    public class HikCameraInfo : ICameraInfo
    {
        public string SerialNumber { get; }

        public string ModelName { get; }

        public string UserDefinedName { get; private set; }

        public string Manufacturer { get; }

        public string CameraVersion { get; }

        internal IDeviceInfo Native { get; }

        public CameraType InterfaceType => GetCameraType();

        public HikCameraInfo(string serialNumber, string modelName, string userDefinedName,
            string manufacturer, string cameraVersion, IDeviceInfo native)
        {
            this.SerialNumber = serialNumber;
            this.ModelName = modelName;
            this.UserDefinedName = userDefinedName;
            this.Manufacturer = manufacturer;
            this.CameraVersion = cameraVersion;
            this.Native = native;
        }

        private CameraType GetCameraType()
        {
            return Native.TLayerType switch
            {
                DeviceTLayerType.MvVirGigEDevice => CameraType.GigE,
                DeviceTLayerType.MvUsbDevice => CameraType.Usb,
                DeviceTLayerType.MvCameraLinkDevice => CameraType.CameraLink,
                DeviceTLayerType.MvGenTLCameraLinkDevice => CameraType.GenTL,
                DeviceTLayerType.MvGenTLCXPDevice => CameraType.Unknown,
                _ => CameraType.Unknown,
            };
        }

        public bool SetDefinedName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length > 64)
                return false;

            this.UserDefinedName = name;
            return true;
        }
    }
}
