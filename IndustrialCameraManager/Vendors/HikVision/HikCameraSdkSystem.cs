using IndustrialCameraManager.Abstractions;
using MvCameraControl;
using System.Threading;

namespace IndustrialCameraManager.Vendors.HikVision
{
    /// <summary>
    /// 海康相机SDK系统
    /// </summary>
    public class HikCameraSdkSystem : ICameraSdkSystem
    {
        /// <summary>
        /// 相机SDK系统是否已初始化
        /// </summary>
        private int isInitialized = 0;

        public void Dispose() => Release();

        public void Initialize()
        {
            if (Interlocked.CompareExchange(ref isInitialized, 1, 0) == 0)
                SDKSystem.Initialize();
        }

        public void Release()
        {
            if (Interlocked.CompareExchange(ref isInitialized, 0, 1) == 1)
                SDKSystem.Finalize();
        }
    }
}
