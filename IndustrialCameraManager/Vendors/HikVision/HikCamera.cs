using IndustrialCameraManager.Abstractions;
using MvCameraControl;
using System;
using System.Threading;

namespace IndustrialCameraManager.Vendors.HikVision
{
    /// <summary>
    /// Hik相机实现
    /// </summary>
    public class HikCamera : ICamera
    {
        private volatile int isCreate = 0;
        private volatile int isGrabbing = 0;

        private IDevice camera;
        private readonly IDeviceInfo deviceInfo;
        private readonly ICameraStream stream;

        public bool IsOpen => camera != null && (camera?.IsConnected ?? false) && isCreate == 1;
        public bool IsGrabbing => Interlocked.CompareExchange(ref isGrabbing, 1, 1) == 1;

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
                Interlocked.Increment(ref isCreate);
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
                if (Interlocked.CompareExchange(ref isCreate, 1, 0) == 0)
                    camera = DeviceFactory.CreateDevice(deviceInfo);

                int result = camera.Open();
                if (result != MvError.MV_OK)
                    return CameraResult.Fail(result, "Open camera failed");

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
            if (camera == null) return CameraResult.Fail(-1, "Camera not initialized");
            StopGrab();

            try
            {
                int result = camera.Close();
                return CameraResult.Result(result == MvError.MV_OK, result);
            }
            finally
            {
                camera.StreamGrabber.FrameGrabedEvent -= ProcessFrameCallBack;
            }
        }

        public CameraResult Grab()
        {
            if (Interlocked.CompareExchange(ref isGrabbing, 1, 1) == 1)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            try
            {
                var grabResult = camera.StreamGrabber.StartGrabbing();

                if (grabResult != MvError.MV_OK) Interlocked.Exchange(ref isGrabbing, 0);
                else Interlocked.Exchange(ref isGrabbing, 1);

                return CameraResult.Result(grabResult == MvError.MV_OK, grabResult);
            }
            catch (MvException ex)
            {
                StopGrab();
                return CameraResult.Fail(ex.ErrorCode, ex.Message);
            }
        }

        private void ProcessFrameCallBack(object sender, FrameGrabbedEventArgs e)
        {
            var clonedFrame = (IFrameOut)e.FrameOut.Clone();
            camera.StreamGrabber.FreeImageBuffer(e.FrameOut);

            IFrame frame = new HikFrameWrapper(clonedFrame);
            stream.Publish(frame);
        }

        public void StopGrab()
        {
            if (Interlocked.CompareExchange(ref isGrabbing, 0, 1) == 1)
                camera.StreamGrabber.StopGrabbing();
        }

        public CameraResult SetParam(string key, string value)
        {
            int result = camera.Parameters.SetEnumValueByString(key, value);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return CameraResult.Fail(-1, "Check the command");

            int result = camera.Parameters.SetCommandValue(command);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public void Dispose()
        {
            if (camera != null)
            {
                Close();
                camera.Dispose();
                camera = null;
            }
            Interlocked.CompareExchange(ref isCreate, 0, 1);
            Interlocked.CompareExchange(ref isGrabbing, 0, 1);
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
