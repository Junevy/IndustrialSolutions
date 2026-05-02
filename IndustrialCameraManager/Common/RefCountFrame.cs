using IndustrialCameraManager.Core;
using System;
using System.Threading;

namespace IndustrialCameraManager.Common
{
    public class RefCountFrame : IFrame
    {
        private int refCount = 1;
        private readonly IFrame inner;

        public RefCountFrame(IFrame inner)
        {
            this.inner = inner;
        }

        public void AddRef()
        {
            Interlocked.Increment(ref refCount);
        }

        public void Dispose()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
            {
                inner.Dispose();
            }
        }

        public IntPtr PixelDataPtr => inner.PixelDataPtr;

        public uint Width => inner.Width;

        public uint Height => inner.Height;

        public ImagePixelFormat PixelType => inner.PixelType;

        public ulong ImageSize => inner.ImageSize;

    }
}
