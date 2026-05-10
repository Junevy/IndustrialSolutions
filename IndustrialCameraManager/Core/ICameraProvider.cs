using System;
using System.Collections.Generic;

namespace IndustrialCameraManager.Core
{
    /// <summary>
    /// 工业相机的Provider类
    /// 用于：
    ///     1）枚举工业相机；
    ///     2）创建工业相机的实例。
    /// </summary>
    public interface ICameraProvider : IDisposable
    {
        ICamera Create(ICameraInfo info);
        IEnumerable<ICamera> CreateAll();

        IEnumerable<ICameraInfo> Enumerate();
        IEnumerable<ICameraInfo> Enumerate(CameraType type);
    }
}
