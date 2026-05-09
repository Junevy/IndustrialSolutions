using System;
using System.Drawing;

namespace IndustrialCameraManager.Core
{
    public interface IFrame : IDisposable
    {
        IntPtr PixelDataPtr { get; }

        byte[] Data { get; }

        public int Stride { get; }

        uint Width { get; }

        uint Height { get; }

        ImagePixelFormat PixelType { get; }

        ulong ImageSize { get; }

        void AddRef();

        Bitmap GetBitmap();

    }
}
