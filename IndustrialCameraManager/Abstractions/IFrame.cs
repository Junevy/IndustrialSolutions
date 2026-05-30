using System;
using System.Drawing;

namespace IndustrialCameraManager.Abstractions
{
    /// <summary>
    /// 图像帧接口
    /// </summary>
    public interface IFrame : IDisposable
    {
        /// <summary>
        /// 图像像素数据指针，非托管内存
        /// </summary>
        IntPtr PixelDataPtr { get; }

        /// <summary>
        /// 图像数据数组，托管内存
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// 图像行步长，单位：字节
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// 图像宽度
        /// </summary>
        uint Width { get; }

        /// <summary>
        /// 图像高度
        /// </summary>
        uint Height { get; }

        /// <summary>
        /// 图像像素格式
        /// </summary>
        ImagePixelFormat PixelType { get; }

        /// <summary>
        /// 图像大小，单位：字节
        /// </summary>
        /// <value>
        /// 图像大小，单位：字节
        /// </value>
        ulong ImageSize { get; }

        /// <summary>
        /// 增加引用计数，防止图像帧被GC回收
        /// </summary>
        void AddRef();

        /// <summary>
        /// 获取图像帧的Bitmap表示
        /// </summary>
        /// <returns>
        /// 图像帧的Bitmap表示
        /// </returns>
        Bitmap GetBitmap();
    }
}
