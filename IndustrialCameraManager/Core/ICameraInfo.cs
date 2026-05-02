namespace IndustrialCameraManager.Core
{
    public interface ICameraInfo
    {
        string SerialNumber { get; }
        string ModelName { get; }
        string UserDefinedName { get; }
        string Manufacturer { get; }
        string CameraVersion { get; }

        //string IpAddress { get; }


    }
}
