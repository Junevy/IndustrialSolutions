using System;

namespace IndustrialCameraManager.Core
{
    public interface IFrame : IDisposable
    {
        IntPtr PixelDataPtr { get; }

        uint Width { get; }

        uint Height { get; }

        ImagePixelFormat PixelType { get; }

        ulong ImageSize { get; }
    }
}
