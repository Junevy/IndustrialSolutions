using IndustrialCameraManager.Core;
using MvCameraControl;

namespace IndustrialCameraManager.HikVision
{
    public class HikCameraInfo : ICameraInfo
    {
        public string SerialNumber { get; }

        public string ModelName { get; }

        public string UserDefinedName { get; private set; }

        public string Manufacturer { get; }

        public string CameraVersion { get; }

        internal IDeviceInfo Native { get; }

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
