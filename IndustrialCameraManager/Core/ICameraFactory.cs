namespace IndustrialCameraManager.Core
{
    public interface ICameraFactory
    {
        ICamera Create(ICameraInfo info);
    }
}
