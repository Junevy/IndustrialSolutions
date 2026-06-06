using IndustrialCameraManager.Abstractions;
using System.Threading;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCameraSdkSystem : ICameraSdkSystem
    {
        private int disposed;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
            {
                Interlocked.Exchange(ref disposed, 1);
                Release();
            }
        }

        public void Initialize()
        {
            Interlocked.Exchange(ref disposed, 0);
        }

        public void Release()
        {
        }
    }
}
