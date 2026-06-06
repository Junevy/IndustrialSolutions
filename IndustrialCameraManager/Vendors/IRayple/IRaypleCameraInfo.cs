using IndustrialCameraManager.Abstractions;
using static MVSDK_Net.IMVDefine;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCameraInfo : ICameraInfo
    {
        public string SerialNumber { get; }

        public string ModelName { get; }

        public string UserDefinedName { get; private set; }

        public string Manufacturer { get; } = "iRAYPLE";

        public string CameraVersion { get; }

        internal IMV_DeviceInfo Native { get; }

        public string CameraKey { get; }

        public IRaypleCameraInfo(string serialNumber, string modelName, string userDefinedName,
            string cameraVersion, string cameraKey, IMV_DeviceInfo native)
        {
            this.SerialNumber = serialNumber;
            this.ModelName = modelName;
            this.UserDefinedName = userDefinedName;
            this.CameraVersion = cameraVersion;
            this.CameraKey = cameraKey;
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
