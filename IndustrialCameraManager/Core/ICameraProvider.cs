using System.Collections.Generic;

namespace IndustrialCameraManager.Core
{
    public interface ICameraProvider
    {
        ICamera Create(ICameraInfo info);
        IEnumerable<ICamera> CreateAll();

        IEnumerable<ICameraInfo> Enumerate();
        IEnumerable<ICameraInfo> Enumerate(CameraType type);

    }
}
