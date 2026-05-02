using IndustrialCameraManager.Core;
using System;
using System.Collections.Concurrent;

namespace IndustrialCameraManager.Common
{
    public class CameraManager : IDisposable
    {
        private readonly ICameraProvider provider;
        private readonly ConcurrentDictionary<string, ICamera> cameras = new();

        public CameraManager(ICameraProvider provider)
        {
            this.provider = provider;
        }

        public ICamera GetOrCreate(ICameraInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var key = info.SerialNumber; // 唯一标识

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(info));

            if (cameras.TryGetValue(key, out var existing))
                return existing;

            var cam = provider.Create(info);

            cameras[key] = cam;

            return cam;
        }

        public void Dispose()
        {
            foreach (var cam in cameras.Values)
            {
                cam.Dispose();
            }

            cameras.Clear();
        }
    }
}
