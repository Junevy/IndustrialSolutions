using IndustrialCameraManager.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机的管理类，职责：
    ///     1）维护工业相机实例的生命周期，确保同一相机只创建一个实例；
    ///     2）返回指定工业相机的实例。
    /// </summary>
    public class CameraManager : IDisposable
    {
        private readonly ICameraProvider provider;
        // 存储相机实例的字典
        private readonly ConcurrentDictionary<string, ICamera> cameras = new();
        // 枚举的相机信息列表
        public ObservableCollection<ICameraInfo> CamInfoList { get; private set; } = new();

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

        public bool GetCamera(string serialNumber, out ICamera camera)
        {
            camera = null;

            if (string.IsNullOrEmpty(serialNumber))
                return false;

            return cameras.TryGetValue(serialNumber, out camera);
        }

        public bool DisposeCamera(string serialNumber)
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

        public void EnumerateCameras(CameraType type = CameraType.ALL)
        {
            IEnumerable<ICameraInfo> ls;
            if (type == CameraType.ALL) ls = provider.Enumerate();
            else ls = provider.Enumerate(type);
            FillCameraList(ls);
        }

        private void FillCameraList(IEnumerable<ICameraInfo> infoList)
        {
            CamInfoList.Clear();
            foreach (var info in infoList)
                CamInfoList.Add(info);
        }


        public void Dispose()
        {
            foreach (var cam in cameras.Values) cam.Dispose();
            cameras.Clear();
            provider.Dispose();
        }
    }
}
