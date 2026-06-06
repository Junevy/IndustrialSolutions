using IndustrialCameraManager.Abstractions;
using Microsoft.Win32;
using MvCameraControl;
using System;
using System.Net;
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
            if (string.IsNullOrEmpty(ipAddress) || IPAddress.TryParse(ipAddress, out _)) throw new ArgumentException(nameof(ipAddress));

            try
            {
                this.camera = DeviceFactory.CreateDeviceByIp(ipAddress, netExport);
                this.stream = cameraStream;
                Interlocked.Increment(ref isCreate);
            }
            catch (Exception)
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

        public CameraResult SetParam<T>(string paramName, T value)
        {
            int result = int.MinValue;

            if (typeof(T) == typeof(int))
            {
                var intValue = (int)(object)value!;
                return SetParam(paramName, intValue);
            }
            else if (typeof(T) == typeof(float))
            {
                var floatValue = (float)(object)value!;
                return SetParam(paramName, floatValue);
            }
            else if (typeof(T) == typeof(bool))
            {
                var boolValue = (bool)(object)value!;
                return SetParam(paramName, boolValue);
            }
            else if (typeof(T) == typeof(string))
            {
                var stringValue = (string)(object)value!;
                return SetParam(paramName, stringValue);
            }

            bool success = result == MvError.MV_OK;
            return CameraResult.Result(
                success,
                result,
                success ? "" : "Check the type of value or paramName!");
        }

        public CameraResult SetParam(string paramName, int value)
        {
            int result = camera.Parameters.SetIntValue(paramName, value);

            bool success = result == MvError.MV_OK;

            return CameraResult.Result(
                success,
                result,
                success ? string.Empty : "Check the paramName!");
        }

        public CameraResult SetParam(string paramName, float value)
        {
            int result = camera.Parameters.SetFloatValue(paramName, value);

            bool success = result == MvError.MV_OK;

            return CameraResult.Result(
                success,
                result,
                success ? string.Empty : "Check the paramName!");
        }

        public CameraResult SetParam(string paramName, bool value)
        {
            int result = camera.Parameters.SetBoolValue(paramName, value);

            bool success = result == MvError.MV_OK;

            return CameraResult.Result(
                success,
                result,
                success ? string.Empty : "Check the paramName!");
        }

        public CameraResult SetParam(string paramName, string value)
        {
            var result = camera.Parameters.SetStringValue(paramName, value);

            var success = result == MvError.MV_OK;
            return CameraResult.Result(
                success,
                result,
                success ? string.Empty : "Check the paramName!");
        }

        public CameraResult SetEnumParam(string paramName, string value)
        {
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(value))
                return CameraResult.Result(
                    false,
                    int.MinValue,
                    "The paramName or value is null");

            var result = camera.Parameters.SetEnumValueByString(paramName, value);
            var success = result == MvError.MV_OK;
            return CameraResult.Result(
                success,
                result,
                success ? string.Empty : "Check the paramName!");
        }

        public T GetParam<T>(string paramName)
        {
            if (typeof(T) == typeof(int))
            {
                camera.Parameters.GetIntValue(paramName, out IIntValue value);
                return (T)(object)(int)value.CurValue;
            }
            else if (typeof(T) == typeof(float))
            {
                camera.Parameters.GetFloatValue(paramName, out IFloatValue value);
                return (T)(object)value.CurValue;
            }
            else if (typeof(T) == typeof(string))
            {
                camera.Parameters.GetStringValue(paramName, out IStringValue value);
                return (T)(object)value.CurValue;
            }
            else if (typeof(T) == typeof(bool))
            {
                camera.Parameters.GetBoolValue(paramName, out bool value);
                return (T)(object)value;
            }
            else if (typeof(T) == typeof(byte))
            {
                camera.Parameters.GetEnumValue(paramName, out IEnumValue value);
                return (T)(object)value;
            }

            return default;
        }

        public string GetEnumValue(string paramName)
        {
            var result = camera.Parameters.GetEnumValue(paramName, out IEnumValue enumValue);
            return result == MvError.MV_OK ? enumValue.CurEnumEntry.Symbolic.ToString() : "ERROR";
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

    }
}
