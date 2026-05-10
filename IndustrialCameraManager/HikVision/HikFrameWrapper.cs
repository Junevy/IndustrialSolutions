using IndustrialCameraManager.Core;
using MvCameraControl;
using System;
using System.Drawing;
using System.Threading;

namespace IndustrialCameraManager.HikVision
{
    public class HikFrameWrapper(IFrameOut native) : IFrame
    {
        private readonly IFrameOut native = native;
        private int refCount = 1;


        public IntPtr PixelDataPtr => native.Image.PixelDataPtr;

        public byte[] Data => native.Image.PixelData;

        public int Stride => (int)(ImageSize / Height);

        public ImagePixelFormat PixelType => ConvertFormat(native.Image.PixelType);

        public ulong ImageSize => native.Image.ImageSize;

        public uint Width => native.Image.Width;

        public uint Height => native.Image.Height;

        public void Dispose()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
            {
                native.Dispose();
            }
        }

        public void AddRef()
        {
            Interlocked.Increment(ref refCount);
        }

        private ImagePixelFormat ConvertFormat(MvGvspPixelType pixelType)
        {
            return pixelType switch
            {
                MvGvspPixelType.PixelType_Gvsp_Mono8 => ImagePixelFormat.Mono8,
                MvGvspPixelType.PixelType_Gvsp_Mono10 => ImagePixelFormat.Mono10,
                MvGvspPixelType.PixelType_Gvsp_Mono12 => ImagePixelFormat.Mono12,
                MvGvspPixelType.PixelType_Gvsp_Mono14 => ImagePixelFormat.Mono14,
                MvGvspPixelType.PixelType_Gvsp_Mono16 => ImagePixelFormat.Mono16,

                MvGvspPixelType.PixelType_Gvsp_BayerRG8 => ImagePixelFormat.BayerRG8,
                MvGvspPixelType.PixelType_Gvsp_BayerRG10 => ImagePixelFormat.BayerRG10,
                MvGvspPixelType.PixelType_Gvsp_BayerRG12 => ImagePixelFormat.BayerRG12,

                MvGvspPixelType.PixelType_Gvsp_RGB8_Packed => ImagePixelFormat.RGB8,
                MvGvspPixelType.PixelType_Gvsp_BGR8_Packed => ImagePixelFormat.BGR8,

                _ => ImagePixelFormat.Unknown
            };
        }

        public Bitmap GetBitmap()
        {
            return native.Image.ToBitmap();
        }

    }
}
