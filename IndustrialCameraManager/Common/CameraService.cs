using IndustrialCameraManager.Core;
using IndustrialCameraManager.Stream;
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
        private readonly CameraManager cameraManager;
        private readonly StreamManager streamManager;
        public ObservableCollection<ICameraInfo> CamInfoList => cameraManager.CamInfoList;

        public CameraService(CameraManager cameraManager, StreamManager streamManager)
        {
            this.cameraManager = cameraManager;
            this.streamManager = streamManager;
        }

        public void EnumerateCameras(CameraType type = CameraType.ALL)
        {
            cameraManager.EnumerateCameras(type);
        }

        public CameraResult OpenCamera(ICameraInfo info)
        {
            if (info == null)
                return CameraResult.Fail(-1, "No camera selected");

            try
            {
                var camera = cameraManager.GetOrCreate(info);
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

        public bool SubscribeFrameStream(string streamKey, string subKey, Func<IFrame, Task> processFrame)
        {
            if (processFrame == null) return false;
            if (string.IsNullOrEmpty(streamKey) || string.IsNullOrEmpty(subKey)) return false;
            if (!streamManager.GetStream(streamKey, out var stream)) return false;

            stream.Subscribe(subKey, processFrame);
            return true;
        }

        public bool UnsubscribeFrameStream(string streamKey, string subKey)
        {
            if (!string.IsNullOrEmpty(streamKey) || !string.IsNullOrEmpty(subKey)) return false;
            if (!streamManager.GetStream(streamKey, out var stream)) return false;

            return stream.Unsubscribe(subKey);
        }

        public CameraResult StartGrab(string serialNumber)
        {
            var result = cameraManager.GetCamera(serialNumber ?? "", out var camera);

            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            try
            {
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
            var result = cameraManager.GetCamera(serialNumber ?? "", out var camera);
            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            camera.StopGrab();
            return CameraResult.Success(1);
        }

        public CameraResult Close(string serialNumber)
        {
            var result = cameraManager.GetCamera(serialNumber ?? "", out var camera);
            if (!result)
                return CameraResult.Fail(-1, errorMsg);

            return camera.Close();
        }

        public CameraResult SetTriggerMode(string serialNumber, string triggerWay)
        {
            var result = cameraManager.GetCamera(serialNumber ?? "", out var camera);
            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            var setResult = camera.SetTrigger(triggerWay, true);
            return setResult;
        }

        public CameraResult ExecuteCommand(string serialNumber, string command)
        {
            var result = cameraManager.GetCamera(serialNumber ?? "", out var camera);
            if (!result || !camera.IsOpen)
                return CameraResult.Fail(-1, errorMsg);

            var setResult = camera.ExecuteCommand(command);
            return setResult;
        }

        public void DisposeCamera()
        {

        }
        public void Dispose()
        {
            cameraManager.Dispose();
            streamManager.Dispose();
        }
    }
}
