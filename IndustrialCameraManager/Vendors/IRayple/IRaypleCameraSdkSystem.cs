using IndustrialCameraManager.Abstractions;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCameraSdkSystem : ICameraSdkSystem
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Release();
            }
        }

        public void Initialize()
        {
            _disposed = false;
        }

        public void Release()
        {
        }
    }
}
