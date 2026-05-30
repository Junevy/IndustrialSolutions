using IndustrialCameraManager.Abstractions;
using System;
using System.Collections.Concurrent;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机管理器
    /// </summary>
    public class CameraManager : IDisposable
    {
        /// <summary>
        /// 缓存的相机实例
        /// </summary>
        private readonly ConcurrentDictionary<string, ICamera> cameras = new();

        /// <summary>
        /// 注册相机实例
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="camera">相机实例</param>
        /// <exception cref="ArgumentNullException">
        /// <c>serialNumber</c> 或 <c>camera</c> 为 <c>null</c>
        /// </exception>
        public void Register(string serialNumber, ICamera camera)
        {
            if (string.IsNullOrEmpty(serialNumber))
                throw new ArgumentNullException(nameof(serialNumber));

            cameras[serialNumber] = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        /// <summary>
        /// 尝试获取相机实例
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="camera">相机实例</param>
        /// <returns>
        /// 是否成功获取相机实例
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>serialNumber</c> 为 <c>null</c>
        /// </exception>
        public bool TryGet(string serialNumber, out ICamera camera)
        {
            camera = null;

            if (string.IsNullOrEmpty(serialNumber))
                return false;

            return cameras.TryGetValue(serialNumber, out camera);
        }

        /// <summary>
        /// 移除相机实例
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 是否成功移除相机实例
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <c>serialNumber</c> 为 <c>null</c>
        /// </exception>
        public bool Remove(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
                throw new ArgumentNullException(nameof(serialNumber));

            if (cameras.TryRemove(serialNumber, out var cam))
            {
                cam.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放所有相机实例
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <c>serialNumber</c> 为 <c>null</c>
        /// </exception>
        public void Dispose()
        {
            foreach (var cam in cameras.Values)
                cam.Dispose();

            cameras.Clear();
        }
    }
}
