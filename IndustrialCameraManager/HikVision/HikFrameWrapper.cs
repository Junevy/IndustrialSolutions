using IndustrialCameraManager.Core;
using MvCameraControl;
using System;

namespace IndustrialCameraManager.HikVision
{
    public class HikFrameWrapper : IFrame
    {
        private readonly IFrameOut native;

        public HikFrameWrapper(IFrameOut native)
        {
            this.native = native;
        }

        public IntPtr PixelDataPtr => native.Image.PixelDataPtr;

        public ImagePixelFormat PixelType => ConvertFormat(native.Image.PixelType);

        public ulong ImageSize => native.Image.ImageSize;

        public uint Width => native.Image.Width;

        public uint Height => native.Image.Height;

        public void Dispose()
        {
            native.Dispose();
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
    }
}
