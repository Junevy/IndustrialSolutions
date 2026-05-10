using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using MvCameraControl;
using System;

namespace IndustrialCameraManager.HikVision
{
    public class HikCamera : ICamera
    {
        private bool isCreate = false;
        private bool isGrabbing = false;

        private IDevice camera;
        private readonly IDeviceInfo deviceInfo;
        private readonly ICameraStream stream;
        public bool IsOpen => (camera?.IsConnected) ?? false;   // The value is true When Camera.Open()

        public HikCamera(IDeviceInfo info, ICameraStream cameraStream)
        {
            this.deviceInfo = info;
            this.stream = cameraStream;
        }

        public HikCamera(string ipAddress, string netExport, ICameraStream cameraStream)
        {
            try
            {
                this.camera = DeviceFactory.CreateDeviceByIp(ipAddress, netExport);
                this.stream = cameraStream;
                isCreate = true;
            }
            catch (MvException)
            {
                throw;
            }
        }

        public CameraResult Open()
        {
            try
            {
                if (!isCreate)
                {
                    camera = DeviceFactory.CreateDevice(deviceInfo);
                    isCreate = true;
                }

                int result = camera.Open();

                if (result != MvError.MV_OK)
                    return CameraResult.Fail(result, "Open camera failed");

                //camera.Open(DeviceAccessMode.)
                //if (camera is IGigEDevice)
                //{
                //    IGigEDevice gigEDevice = camera as IGigEDevice;

                //    // 配置网口相机的最佳包大小
                //    gigEDevice.GetOptimalPacketSize(out int packetSize);
                //    gigEDevice.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                //}
                //else if (camera is IUSBDevice)
                //{
                //    // 设置USB同步读写超时时间
                //    IUSBDevice usbDevice = camera as IUSBDevice;
                //    usbDevice.SetSyncTimeOut(1000);
                //}

                camera.StreamGrabber.FrameGrabedEvent -= ProcessFrameCallBack;
                camera.StreamGrabber.FrameGrabedEvent += ProcessFrameCallBack;
                return CameraResult.Success(result);
            }
            catch (MvException me)
            {
                return CameraResult.Fail(me.ErrorCode, me.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CameraResult Close()
        {
            StopGrab();
            int result = camera.Close();
            camera.StreamGrabber.FrameGrabedEvent -= ProcessFrameCallBack;
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult Grab()
        {
            if (isGrabbing)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            //var result = SetParam("TriggerMode", "Off");
            //if (!result.IsSuccess)
            //{
            //    isGrabbing = false;
            //    return result;
            //}

            try
            {
                var grabResult = camera.StreamGrabber.StartGrabbing();
                isGrabbing = grabResult == MvError.MV_OK;
                return CameraResult.Result(isGrabbing, grabResult);
            }
            catch (MvException ex)
            {
                isGrabbing = false;
                StopGrab();
                return CameraResult.Fail(ex.ErrorCode, ex.Message);
            }
        }

        private void ProcessFrameCallBack(object sender, FrameGrabbedEventArgs e)
        {
            var clonedFrame = (IFrameOut)e.FrameOut.Clone();
            camera.StreamGrabber.FreeImageBuffer(e.FrameOut);   // copy完毕后释放原始帧

            IFrame frame = new HikFrameWrapper(clonedFrame);
            stream.Publish(frame);
        }

        public void StopGrab()
        {
            camera.StreamGrabber.StopGrabbing();
            isGrabbing = false;
        }

        public CameraResult SetParam(string key, string value)
        {
            int result = camera.Parameters.SetEnumValueByString(key, value);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult ExecuteCommand(string command)
        {
            int result = camera.Parameters.SetCommandValue(command);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public void Dispose()
        {
            Close();
            isCreate = false;
            camera.Dispose();
            camera = null;
        }

        public CameraResult SetTrigger(string triggerWay, bool isGrab)
        {
            if (isGrabbing) StopGrab();

            if (string.IsNullOrEmpty(triggerWay))
                return CameraResult.Fail(-1, "Check the trigger source or trigger way");

            string grabMode = isGrab ? "On" : "Off";
            var result = SetParam("TriggerMode", grabMode);
            if (!result.IsSuccess)
                return result;

            return SetParam("TriggerSource", triggerWay);
        }

        public T GetParam<T>(string paramName)
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(int):
                    camera.Parameters.GetIntValue(paramName, out IIntValue intValue);
                    return (T)(object)intValue.CurValue;
                case Type t when t == typeof(float):
                    camera.Parameters.GetFloatValue(paramName, out IFloatValue floatValue);
                    return (T)(object)floatValue.CurValue;
                default: throw new NotSupportedException($"Parameter type {typeof(T)} is not supported");
            }
        }
    }
}
