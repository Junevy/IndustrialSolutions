using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using IndustrialCameraManager.Stream;
using MvCameraControl;
using System;

namespace IndustrialCameraManager.HikVision
{
    public class HikCamera : ICamera
    {
        private bool isCreate = false;
        public bool IsGrabbing { get; private set; } = false;

        public bool IsOpen => (camera?.IsConnected) ?? false;   // The value is true When Camera.Open()
        private IDevice camera;
        private readonly IDeviceInfo deviceInfo;
        //private readonly SemaphoreSlim grabLocker = new(1, 1);

        public ICameraStream Stream { get; private set; }

        public HikCamera(IDeviceInfo info, ICameraStream cameraStream)
        {
            this.deviceInfo = info;
            this.Stream = cameraStream;
        }

        public HikCamera(string ipAddress, string netExport, ICameraStream cameraStream)
        {
            try
            {
                this.camera = DeviceFactory.CreateDeviceByIp(ipAddress, netExport);
                this.Stream = cameraStream;
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
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult Grab()
        {
            if (IsGrabbing)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            var result = SetParam("TriggerMode", "Off");
            if (!result.IsSuccess)
            {
                IsGrabbing = false;
                return result;
            }

            try
            {
                camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
                camera.StreamGrabber.FrameGrabedEvent += GrabedProcess;
                var grabResult = camera.StreamGrabber.StartGrabbing();
                IsGrabbing = grabResult == MvError.MV_OK;
                return CameraResult.Result(IsGrabbing, grabResult);
            }
            catch (MvException ex)
            {
                IsGrabbing = false;
                StopGrab();
                return CameraResult.Fail(ex.ErrorCode, ex.Message);
            }
        }

        private void GrabedProcess(object sender, FrameGrabbedEventArgs e)
        {
            var clonedFrame = (IFrameOut)e.FrameOut.Clone();
            camera.StreamGrabber.FreeImageBuffer(e.FrameOut);   // copy完毕后释放原始帧

            IFrame frame = new HikFrameWrapper(clonedFrame);
            Stream.Publish(frame);
        }

        public void StopGrab()
        {
            camera.StreamGrabber.StopGrabbing();
            camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
            IsGrabbing = false;
        }

        public CameraResult SetParam(string key, string value)
        {
            int result = camera.Parameters.SetEnumValueByString(key, value);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult SetParam(string value)
        {
            int result = camera.Parameters.SetCommandValue(value);
            return CameraResult.Result(result != MvError.MV_OK, result);
        }

        public void Dispose()
        {
            Close();
            isCreate = false;
            camera.Dispose();
            camera = null;
        }

        public CameraResult Trigger(string triggerWay)
        {
            if (IsGrabbing) StopGrab();

            if (string.IsNullOrEmpty(triggerWay)) throw new ArgumentException("Trigger way is invalid");

            camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
            camera.StreamGrabber.FrameGrabedEvent += GrabedProcess;

            return SetParam("TriggerMode", "On");
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
