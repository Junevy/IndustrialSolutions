using System;

namespace IndustrialCameraManager.Core
{
    /// <summary>
    /// SDK 的初始化与生命周期控制接口
    /// </summary>
    public interface ICameraSdkSystem : IDisposable
    {
        void Initialize();

        void Release();
    }
}
