using IndustrialCameraManager.Abstractions;
using MvCameraControl;
using System;
using System.Drawing;
using System.Threading;

namespace IndustrialCameraManager.Vendors.HikVision
{
    /// <summary>
    /// Hik相机图像帧包装器
    /// </summary>
    /// <param name="native">Hik相机图像帧</param>
    /// <returns>
    /// <see cref="IFrame" />
    /// </returns>
    public class HikFrameWrapper(IFrameOut native) : IFrame
    {
        /// <summary>
        /// 原始Hik相机图像帧
        /// </summary>
        private readonly IFrameOut native = native;
        private int refCount = 1;

        /// <summary>
        /// 注意非托管内存 PixelDataPtr的生命周期，可能会随着相机缓存队列满而释放，
        /// 建议使用更安全的属性 Data
        /// </summary>
        public IntPtr PixelDataPtr => native.Image.PixelDataPtr;

        public byte[] Data => native.Image.PixelData;

        public int Stride => Height > 0 ? (int)(ImageSize / Height) : 0;

        public ImagePixelFormat PixelType => ConvertFormat(native.Image.PixelType);

        public ulong ImageSize => native.Image.ImageSize;

        public uint Width => native.Image.Width;

        public uint Height => native.Image.Height;

        public void Dispose()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
                native.Dispose();
        }

        public void AddRef() => Interlocked.Increment(ref refCount);
        
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

        public Bitmap GetBitmap() => native.Image.ToBitmap();
    }
}
