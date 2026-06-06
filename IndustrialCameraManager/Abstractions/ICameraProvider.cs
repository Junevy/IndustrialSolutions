using System.Collections.Generic;

namespace IndustrialCameraManager.Abstractions
{
    /// <summary>
    /// Camera提供器接口
    /// </summary>
    public interface ICameraProvider
    {
        ICamera Create(ICameraInfo info);

        IEnumerable<ICamera> CreateAll();

        IEnumerable<ICameraInfo> Enumerate();

        IEnumerable<ICameraInfo> Enumerate(CameraType type);
    }
}
