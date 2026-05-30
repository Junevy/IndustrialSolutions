using IndustrialCameraManager.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机服务类
    /// </summary>
    public class CameraService(ICameraProvider provider, CameraManager cameraManager, StreamManager streamManager)
    {
        private const string ErrorMsg = "Camera not open or found";
        private readonly ICameraProvider provider = provider;
        private readonly CameraManager cameraManager = cameraManager;
        private readonly StreamManager streamManager = streamManager;

        /// <summary>
        /// 枚举相机
        /// </summary>
        /// <param name="type">相机类型</param>
        /// <returns>
        /// 相机信息列表
        /// </returns>
        public IEnumerable<ICameraInfo> EnumerateCameras(CameraType type = CameraType.ALL)
            => provider.Enumerate(type);

        /// <summary>
        /// 打开相机
        /// </summary>
        /// <param name="info">相机信息</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult OpenCamera(ICameraInfo info)
        {
            if (info == null)
                return CameraResult.Fail(-1, "The camera info is null");

            if (string.IsNullOrEmpty(info.SerialNumber))
                return CameraResult.Fail(-1, "The camera serial number is empty");

            try
            {
                if (!cameraManager.TryGet(info.SerialNumber, out var camera))
                {
                    camera = provider.Create(info);
                    cameraManager.Register(info.SerialNumber, camera);
                }

                if (camera.IsOpen)
                    return CameraResult.Fail(-1, "The camera has been opened");

                return camera.Open();
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-2, e.Message);
            }
        }

        /// <summary>
        /// 订阅指定相机的图像帧数据
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="subKey">订阅者标识</param>
        /// <param name="processFrame">处理图像帧的回调函数</param>
        /// <returns>
        /// 是否成功订阅
        /// </returns>
        public bool SubscribeFrameStream(string serialNumber, string subKey, Func<IFrame, Task> processFrame)
        {
            if (processFrame == null) return false;
            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(subKey)) return false;
            if (!streamManager.GetStream(serialNumber, out var stream)) return false;

            stream.Subscribe(subKey, processFrame);
            return true;
        }

        /// <summary>
        /// 取消订阅指定相机的图像帧数据
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="subKey">订阅者标识</param>
        /// <returns>
        /// 是否成功取消订阅
        /// </returns>
        public bool UnsubscribeFrameStream(string serialNumber, string subKey)
        {
            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(subKey)) return false;
            if (!streamManager.GetStream(serialNumber, out var stream)) return false;

            return stream.Unsubscribe(subKey);
        }

        /// <summary>
        /// 打开指定相机的图像采集功能
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult StartGrab(string serialNumber)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

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

        /// <summary>
        /// 停止指定相机的图像采集功能
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult StopGrab(string serialNumber)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            camera.StopGrab();
            return CameraResult.Success(1);
        }

        /// <summary>
        /// 断开连接指定相机
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult Close(string serialNumber)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera))
                return CameraResult.Fail(-1, ErrorMsg);

            var closeResult = camera.Close();
            if (closeResult.IsSuccess)
                cameraManager.Remove(serialNumber);

            return closeResult;
        }

        /// <summary>
        /// 设置相机的指定参数
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult SetParam(string serialNumber, string key, string value)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetParam(key, value);
        }

        /// <summary>
        /// 执行相机的指定命令
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="command">命令</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult ExecuteCommand(string serialNumber, string command)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.ExecuteCommand(command);
        }

        /// <summary>
        /// 设置相机的触发方式
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="triggerWay">触发方式</param>
        /// <param name="isAcquisition">是否打开采集</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult SetTrigger(string serialNumber, string triggerWay, bool isAcquisition)
        {
            if (!cameraManager.TryGet(serialNumber ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            camera.StopGrab();

            if (string.IsNullOrEmpty(triggerWay))
                return CameraResult.Fail(-1, "Check the trigger source or trigger way");

            string acq = isAcquisition ? "On" : "Off";
            camera.SetParam("TriggerMode", acq);    

            return camera.SetParam("TriggerSource", triggerWay);
        }
    }
}
