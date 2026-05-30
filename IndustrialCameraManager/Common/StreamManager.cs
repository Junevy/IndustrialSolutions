using IndustrialCameraManager.Abstractions;
using System;
using System.Collections.Concurrent;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机图像帧数据流管理类，用于管理相机的图像数据流
    /// </summary>
    public class StreamManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, ICameraStream> streams = new();

        /// <summary>
        /// 释放所有图像数据流
        /// </summary>
        public void Dispose()
        {
            foreach (var stream in streams.Values) { stream.Dispose(); }
            streams.Clear();
        }

        /// <summary>
        /// 获取或创建指定的相机的图像数据流
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 相机图像数据流
        /// </returns>
        /// <exception cref="ArgumentNullException">serialNumber 为空</exception>
        public ICameraStream GetOrCreateStream(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
                throw new ArgumentNullException(nameof(serialNumber));

            return streams.GetOrAdd(serialNumber, _ => new CameraStream());
        }

        /// <summary>
        /// 获取指定的相机的图像数据流
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="stream">相机图像数据流</param>
        /// <returns>
        /// 是否成功获取到图像数据流
        /// </returns>
        /// <exception cref="ArgumentNullException">serialNumber 为空</exception>
        public bool GetStream(string serialNumber, out ICameraStream stream)
        {
            stream = null;

            if (string.IsNullOrEmpty(serialNumber))
                return false;

            return streams.TryGetValue(serialNumber, out stream);
        }
    }
}
