using IndustrialCameraManager.Abstractions;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        public IEnumerable<ICameraInfo> EnumerateCameras(CameraType type = CameraType.ALL) => provider.Enumerate(type);

        /// <summary>
        /// 打开相机
        /// </summary>
        /// <param name="info">相机信息</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult OpenCamera(ICameraInfo info, string cameraKey)
        {
            if (info == null)
                return CameraResult.Fail(-1, "The camera info is null");

            if (string.IsNullOrEmpty(cameraKey))
                return CameraResult.Fail(-1, "The camera serial number is empty");

            try
            {
                if (!cameraManager.TryGet(cameraKey, out var camera))
                {
                    camera = provider.Create(info);
                    cameraManager.Register(cameraKey, camera);
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
        /// <param name="cameraKey">需要订阅图像流的相机序列号</param>
        /// <param name="subKey">订阅者标识</param>
        /// <param name="processFrame">处理图像帧的回调函数</param>
        /// <param name="whenException">异常发生处理回调方法，当不提供异常处理回调时，将抛出该异常</param>
        /// <param name="capacity">订阅Channel的边界（容量）</param>
        /// <returns>
        /// 是否成功订阅
        /// </returns>
        public bool SubscribeFrameStream(string cameraKey, string subKey, Func<IFrame, Task> processFrame, Action<Exception> whenException = null, int capacity = 5)
        {
            if (processFrame == null) return false;
            if (string.IsNullOrEmpty(cameraKey) || string.IsNullOrEmpty(subKey)) return false;

            var serialNumber = GetOnlineCameraSerialNumber(cameraKey);
            if (string.IsNullOrEmpty(serialNumber)) return false;
            if (!streamManager.GetStream(serialNumber, out var stream)) return false;
            stream.Subscribe(subKey, capacity, processFrame, whenException);
            return true;
        }

        /// <summary>
        /// 取消订阅指定相机的图像帧数据
        /// </summary>
        /// <param name="cameraKey">相机序列号</param>
        /// <param name="subKey">订阅者标识</param>
        /// <returns>
        /// 是否成功取消订阅
        /// </returns>
        public bool UnsubscribeFrameStream(string cameraKey, string subKey)
        {
            if (string.IsNullOrEmpty(cameraKey) || string.IsNullOrEmpty(subKey)) return false;
            if (!streamManager.GetStream(cameraKey, out var stream)) return false;

            return stream.Unsubscribe(subKey);
        }

        /// <summary>
        /// 打开指定相机的图像采集功能
        /// </summary>
        /// <param name="cameraKey">注册的相机名称</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult StartGrab(string cameraKey)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
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
        /// <param name="cameraKey">注册的相机名称</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult StopGrab(string cameraKey)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            camera.StopGrab();
            return CameraResult.Success(1);
        }

        /// <summary>
        /// 断开连接指定相机
        /// </summary>
        /// <param name="cameraKey">相机序列号</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult Close(string cameraKey)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera))
                return CameraResult.Fail(-1, ErrorMsg);

            var closeResult = camera.Close();
            //if (closeResult.IsSuccess)
            cameraManager.Remove(cameraKey);

            return closeResult;
        }

        public string GetOnlineCameraSerialNumber(string cameraKey)
        {
            cameraManager.TryGet(cameraKey, out var camera);
            return camera.GetSerialNumber();
        }

        /// <summary>
        /// 设置相机的指定参数
        /// </summary>
        /// <param name="cameraKey">相机序列号</param>
        /// <param name="paramName">参数键</param>
        /// <param name="value">参数值</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult SetParam(string cameraKey, string paramName, string value)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetParam(paramName, value);
        }

        public CameraResult SetParam(string cameraKey, string paramName, int value)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetParam(paramName, value);
        }

        public CameraResult SetParam(string cameraKey, string paramName, bool value)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetParam(paramName, value);
        }

        public CameraResult SetParam(string cameraKey, string paramName, float value)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetParam(paramName, value);
        }

        public CameraResult SetEnumParam(string cameraKey, string paramName, string value)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.SetEnumParam(paramName, value);
        }


        /// <summary>
        /// 执行相机的指定命令
        /// </summary>
        /// <param name="cameraKey">相机序列号</param>
        /// <param name="command">命令</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult ExecuteCommand(string cameraKey, string command)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            return camera.ExecuteCommand(command);
        }

        /// <summary>
        /// 设置相机的触发方式
        /// </summary>
        /// <param name="cameraKey">相机序列号</param>
        /// <param name="triggerWay">触发方式</param>
        /// <param name="isAcquisition">是否打开触发</param>
        /// <returns>
        /// 相机操作结果
        /// </returns>
        public CameraResult SetTrigger(string cameraKey, string triggerWay, bool isAcquisition)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return CameraResult.Fail(-1, ErrorMsg);

            camera.StopGrab();

            if (string.IsNullOrEmpty(triggerWay))
                return CameraResult.Fail(-1, "Check the trigger source or trigger way");

            string acq = isAcquisition ? "On" : "Off";
            var acqResult = camera.SetEnumParam("TriggerMode", acq);
            if (!acqResult.IsSuccess) return acqResult;

            return camera.SetEnumParam("TriggerSource", triggerWay);
        }

        public T GetParam<T>(string cameraKey, string paramName)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return default;

            return camera.GetParam<T>(paramName);
        }

        public string GetEnumParam(string cameraKey, string paramName)
        {
            if (!cameraManager.TryGet(cameraKey ?? "", out var camera) || !camera.IsOpen)
                return string.Empty;

            return camera.GetEnumValue(paramName);
        }
    }
}
