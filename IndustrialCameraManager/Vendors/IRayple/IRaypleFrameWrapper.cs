using IndustrialCameraManager.Abstractions;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using static MVSDK_Net.IMVDefine;

namespace IndustrialCameraManager.Vendors.IRayple
{
    /// <summary>
    /// IRayple 工业相机帧包装器
    ///     注意非托管内存 PixelDataPtr的生命周期，可能会随着相机缓存队列满而释放，
    ///     建议使用更安全的属性 Data
    /// </summary>
    public class IRaypleFrameWrapper : IFrame
    {
        private readonly IMV_Frame native;
        private int refCount = 1;

        public IntPtr PixelDataPtr => native.pData;

        public byte[] Data { get; private set; }

        public int Stride => Height > 0 ? (int)(ImageSize / Height) : 0;

        public ImagePixelFormat PixelType => ConvertFormat(native.frameInfo.pixelFormat);

        public ulong ImageSize => native.frameInfo.size;

        public uint Width => native.frameInfo.width;

        public uint Height => native.frameInfo.height;


        public IRaypleFrameWrapper(IMV_Frame nativeFrame)
        {
            this.native = nativeFrame;
            this.Data = GetImageData();
        }

        public void Dispose()
        {
            Data = null;
        }

        public void AddRef() => Interlocked.Increment(ref refCount);

        public Bitmap GetBitmap()
        {
            var data = this.Data;
            var width = (int)Width;
            var height = (int)Height;
            var stride = Stride;

            var bmp = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(
                        data, y * stride,
                        IntPtr.Add(bmpData.Scan0, y * bmpData.Stride),
                        bmpData.Stride);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        private byte[] GetImageData()
        {
            byte[] data = new byte[native.frameInfo.size];
            Marshal.Copy(native.pData, data, 0, (int)native.frameInfo.size);
            return data;
        }

        private ImagePixelFormat ConvertFormat(IMV_EPixelType type)
        {
            return type switch
            {
                IMV_EPixelType.gvspPixelMono8 => ImagePixelFormat.Mono8,
                IMV_EPixelType.gvspPixelMono10 => ImagePixelFormat.Mono10,
                IMV_EPixelType.gvspPixelMono12 => ImagePixelFormat.Mono12,
                IMV_EPixelType.gvspPixelMono16 => ImagePixelFormat.Mono16,

                IMV_EPixelType.gvspPixelRGB8 => ImagePixelFormat.RGB8,
                IMV_EPixelType.gvspPixelBGR8 => ImagePixelFormat.BGR8,

                IMV_EPixelType.gvspPixelBayBG8 => ImagePixelFormat.BayerRG8,
                IMV_EPixelType.gvspPixelBayBG10 => ImagePixelFormat.BayerRG10,
                IMV_EPixelType.gvspPixelBayBG12 => ImagePixelFormat.BayerRG12,

                _ => ImagePixelFormat.Unknown
            };
        }

    }
}
