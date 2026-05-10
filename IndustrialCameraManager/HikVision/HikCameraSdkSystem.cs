using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using MvCameraControl;
using System.Threading;

namespace IndustrialCameraManager.HikVision
{
    public class HikCameraSdkSystem : ICameraSdkSystem
    {
        private static int isInitialized = 0;
        private readonly CameraManager manager;

        public HikCameraSdkSystem(CameraManager manager)
        {
            this.manager = manager;
            Initialize();
        }

        public void Dispose()
        {
            Release();
        }

        public void Initialize()
        {
            if (Interlocked.CompareExchange(ref isInitialized, 1, 0) == 0)
            {
                SDKSystem.Initialize();
            }
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref isInitialized) == 0)
            {
                SDKSystem.Finalize();
                manager.Dispose();
            }
        }
    }
}
