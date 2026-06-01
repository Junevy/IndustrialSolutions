using IndustrialCameraManager.Abstractions;
using System;
using System.Drawing;
using System.Threading;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleFrameWrapper : IFrame
    {
        private readonly object nativeFrame;
        private int refCount = 1;

        public IntPtr PixelDataPtr => GetPixelDataPtr();

        public byte[] Data => GetImageData();

        public int Stride => Height > 0 ? (int)(ImageSize / Height) : 0;

        public ImagePixelFormat PixelType => ConvertPixelFormat();

        public ulong ImageSize => GetImageSize();

        public uint Width => GetWidth();

        public uint Height => GetHeight();

        public IRaypleFrameWrapper(object nativeFrame)
        {
            this.nativeFrame = nativeFrame;
        }

        public void Dispose()
        {
            if (Interlocked.Decrement(ref refCount) == 0)
            {
                ReleaseNativeFrame();
            }
        }

        public void AddRef()
        {
            Interlocked.Increment(ref refCount);
        }

        public Bitmap GetBitmap()
        {
            var data = GetImageData();
            var width = (int)GetWidth();
            var height = (int)GetHeight();
            var stride = Stride;

            var bmp = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                for (int y = 0; y < height; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(
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

        private IntPtr GetPixelDataPtr()
        {
            return IntPtr.Zero;
        }

        private byte[] GetImageData()
        {
            return Array.Empty<byte>();
        }

        private ulong GetImageSize()
        {
            return 0;
        }

        private uint GetWidth()
        {
            return 0;
        }

        private uint GetHeight()
        {
            return 0;
        }

        private ImagePixelFormat ConvertPixelFormat()
        {
            return ImagePixelFormat.Unknown;
        }

        private void ReleaseNativeFrame()
        {
            (nativeFrame as IDisposable)?.Dispose();
        }
    }
}
