namespace IndustrialCameraManager.HikVision
{
    public class HikCameraFactory : ICameraFactory
    {
        public ICamera Create(ICameraInfo info)
        {
            var native = info.Native as HikCameraInfo;
            return new HikCamera(native);
        }
    }
}
