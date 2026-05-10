using IndustrialCameraManager.Core;
using IndustrialCameraManager.HikVision;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// CameraService 提供了一个统一的接口来管理和操作工业相机。
    /// 它封装了工业相机的枚举、打开、抓取和关闭等功能，并通过 CameraManager 来维护相机实例的生命周期。
    /// </summary>
    public class CameraService : IDisposable
    {
        private const string errorMsg = "Camera not open or found";
        private readonly CameraManager manager;
        public ObservableCollection<ICameraInfo> CamInfoList => manager.CamInfoList;

        public CameraService(CameraManager manager)
        {
            this.manager = manager;
        }
        public void EnumerateCameras(CameraType type = CameraType.ALL)
        {
            manager.EnumerateCameras(type);
        }

        public CameraResult OpenCamera(ICameraInfo info)
        {
            if (info == null)
                return CameraResult.Fail(-1, "No camera selected");

            try
            {
                var camera = manager.GetOrCreate(info);
                if (camera.IsOpen)
                    return CameraResult.Fail(-1, "The camera has been connected");

                return camera.Open();
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-2, e.Message);
                // 处理异常，例如显示错误消息
            }
        }

        public CameraResult StartGrab(string serialNumber, Func<IFrame, Task> processFrame = null)
        {
            var result = manager.GetCamera(serialNumber ?? "", out var camera);

            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            try
            {
                if (processFrame != null)
                {
                    if (camera is HikCamera hikCamera)
                        hikCamera.Stream.Subscribe(processFrame);
                }

                return camera.Grab();
            }
            catch (Exception e)
            {
                camera?.StopGrab();
                return CameraResult.Fail(-2, e.Message);
            }
        }

        public CameraResult StopGrab(string serialNumber)
        {
            var result = manager.GetCamera(serialNumber ?? "", out var camera);
            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            camera.StopGrab();
            return CameraResult.Success(1);
        }

        public CameraResult Close(string serialNumber)
        {
            var result = manager.GetCamera(serialNumber ?? "", out var camera);
            if (!result)
                return CameraResult.Fail(-1, errorMsg);

            return camera.Close();
        }

        public CameraResult SoftTrigger(string serialNumber)
        {
            var result = manager.GetCamera(serialNumber ?? "", out var camera);
            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            var setResult = camera.SetParam("TriggerSource", "Software");
            if (!setResult.IsSuccess)
                return setResult;

            return camera.SetParam("TriggerSoftware");
        }


        public void DisposeCamera()
        {

        }
        public void Dispose()
        {
            manager.Dispose();
        }
    }
}
