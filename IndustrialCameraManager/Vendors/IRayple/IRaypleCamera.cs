using IndustrialCameraManager.Abstractions;
using MVSDK_Net;
using System;
using System.Net;
using System.Threading;
using static MVSDK_Net.IMVDefine;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCamera : ICamera
    {
        protected volatile int disposed = 0;
        protected ICameraStream stream;
        protected MyCamera camera;
        protected IMV_DeviceInfo cameraInfo;
        protected readonly IMV_FrameCallBack frameHandler;

        public bool IsOpen => camera?.IMV_IsOpen() ?? false;
        public bool IsGrabbing => camera?.IMV_IsGrabbing() ?? false;
        protected IRaypleCamera() { }

        public IRaypleCamera(IMV_DeviceInfo deviceInfo, ICameraStream stream)
        {
            this.camera = new();
            this.cameraInfo = deviceInfo;
            this.stream = stream;
            this.frameHandler = ProcessFrameCallBack;

            // 是否可以直接创建 handle？
        }
        public IRaypleCamera(string ipAddress, ICameraStream stream) : base()
        {
            if (string.IsNullOrEmpty(ipAddress) || IPAddress.TryParse(ipAddress, out _)) throw new ArgumentException(nameof(ipAddress));

            try
            {
                this.stream = stream;
                this.camera = new();
                var result = camera.IMV_CreateHandle(IMV_ECreateHandleMode.modeByIPAddress, cameraStr: ipAddress);
                if (result != IMV_OK) throw new ArgumentException($"Failed to create camera: {result}");
            }
            catch { throw; }
        }


        public CameraResult Open()
        {
            if (this.IsOpen)
                return CameraResult.Fail(-1, "Camera is already open");

            try
            {
                var result = camera.IMV_CreateHandle(IMV_ECreateHandleMode.modeByCameraKey, cameraStr: cameraInfo.serialNumber);
                //camera.info

                if (result != IMV_OK)
                    return CameraResult.Fail(result, "Open camera failed");

                var openCameraResult = camera.IMV_Open();

                if (openCameraResult != IMV_OK)
                    return CameraResult.Fail(result, "Open camera failed");

                return CameraResult.Success(result);
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-1, e.Message);
            }
        }

        public CameraResult Close()
        {
            if (!this.IsOpen)
                return CameraResult.Fail(-1, "Camera is not open");

            StopGrab();

            try
            {
                if (camera.IMV_IsOpen())
                {
                    var result = camera.IMV_Close();
                    if (result != IMV_OK)
                        return CameraResult.Fail(-1, "Camera close error");
                }
                return CameraResult.Success(IMV_OK, "Camera closed");
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-1, e.Message);
            }
        }

        public CameraResult Grab()
        {
            if (!this.IsOpen)
                return CameraResult.Fail(-1, "Camera is not open");

            if (this.IsGrabbing)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            var result = camera.IMV_AttachGrabbing(frameHandler, IntPtr.Zero);
            if (result != IMV_OK)
                return CameraResult.Fail(result, "Attach grabbing failed");

            var startResult = camera.IMV_StartGrabbing();
            if (startResult != IMV_OK)
                return CameraResult.Fail(result, "Start grabbing failed");

            return CameraResult.Success(startResult, "Start grabbing is successful");
        }

        public void StopGrab()
        {
            if (!this.IsOpen) return;

            if (camera.IMV_IsGrabbing())
                camera.IMV_StopGrabbing();
        }

        public CameraResult SetParam<T>(string paramName, T value)
        {
            try
            {
                var type = typeof(T);
                if (type == typeof(int)) return SetParam(paramName, (int)(object)value);
                if (type == typeof(float)) return SetParam(paramName, (float)(object)value);
                if (type == typeof(bool)) return SetParam(paramName, (bool)(object)value);
                if (type == typeof(string)) return SetParam(paramName, (string)(object)value);
                return CameraResult.Fail(-1, "Unsupported parameter type");
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        /// <summary>
        /// 操作相机参数
        ///     option-0：Write（默认）,
        ///     option-1：Read
        /// </summary>
        /// <param name="param">参数名</param>
        /// <param name="option">模式</param>
        /// <returns></returns>
        private CameraResult CheckParam(string param, int option = 0)
        {
            if (!this.IsOpen)
                return CameraResult.Fail(-1, "Camera is not open");

            if (string.IsNullOrEmpty(param))
                return CameraResult.Fail(-1, "The paramName or value is null");

            if (!this.IsOpen) return CameraResult.Fail(-1, "The camera is not open");
            if (!camera.IMV_FeatureIsAvailable(param)) return CameraResult.Fail(-1, "The param is not avalidable");

            if (option == 0)
            {
                if (!camera.IMV_FeatureIsWriteable(param))
                    return CameraResult.Fail(-1, "The param is not writeable");
            }
            else if (option == 1)
            {
                if (!camera.IMV_FeatureIsReadable(param))
                    return CameraResult.Fail(-1, "The param is not readable");
            }
            else
            {
                throw new InvalidOperationException(param.ToString());
            }
            return CameraResult.Result(true, 0);
        }

        public CameraResult SetParam(string paramName, int value)
        {
            var result = CheckParam(paramName);
            if (!result.IsSuccess) return result;
            var r = camera.IMV_SetIntFeatureValue(paramName, value);
            if (r != IMV_OK) return CameraResult.Fail(-1, "Set param is failed");
            return CameraResult.Success(r, $"Set param:{paramName} is successful");
        }

        public CameraResult SetParam(string paramName, float value)
        {
            var result = CheckParam(paramName);
            if (!result.IsSuccess) return result;
            var r = camera.IMV_SetDoubleFeatureValue(paramName, value);
            if (r != IMV_OK) return CameraResult.Fail(-1, "Set param is failed");
            return CameraResult.Success(r, $"Set param:{paramName} is successful");
        }

        public CameraResult SetParam(string paramName, bool value)
        {
            var result = CheckParam(paramName);
            if (!result.IsSuccess) return result;
            var r = camera.IMV_SetBoolFeatureValue(paramName, value);
            if (r != IMV_OK) return CameraResult.Fail(-1, "Set param is failed");
            return CameraResult.Success(r, $"Set param:{paramName} is successful");
        }

        public CameraResult SetParam(string paramName, string value)
        {
            var result = CheckParam(paramName);
            if (!result.IsSuccess) return result;
            var r = camera.IMV_SetStringFeatureValue(paramName, value);
            if (r != IMV_OK) return CameraResult.Fail(-1, "Set param is failed");
            return CameraResult.Success(r, $"Set param:{paramName} is successful");
        }

        public CameraResult SetEnumParam(string paramName, string value)
        {
            try
            {
                var result = CheckParam(paramName);
                if (!result.IsSuccess) return result;
                var r = camera.IMV_SetEnumFeatureSymbol(paramName, value);
                if (r != IMV_OK) return CameraResult.Fail(-1, "Set param is failed");
                return CameraResult.Success(r, $"Set param:{paramName} is successful");
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public T GetParam<T>(string paramName)
        {
            if (!this.IsOpen) return default;
            if (string.IsNullOrWhiteSpace(paramName)) return default;

            try
            {
                var type = typeof(T);
                if (type == typeof(int)) return (T)(object)GetIntParam(paramName);
                if (type == typeof(float)) return (T)(object)GetFloatParam(paramName);
                if (type == typeof(bool)) return (T)(object)GetBoolParam(paramName);
                if (type == typeof(string)) return (T)(object)GetStringParam(paramName);
                return default;
            }
            catch { return default; }
        }

        private object GetStringParam(string paramName)
        {
            if (paramName.Length > 256) return string.Empty;

            var pStr = string.Empty;
            IMV_String str = new() { str = pStr };
            var result = camera.IMV_GetStringFeatureValue(paramName, ref str);
            if (result != IMV_OK) return string.Empty;
            return str.str;
        }

        private bool GetBoolParam(string paramName)
        {
            var pValue = false;
            var result = camera.IMV_GetBoolFeatureValue(paramName, ref pValue);

            if (result != IMV_OK) return false;
            return pValue;
        }

        private float GetFloatParam(string paramName)
        {
            var pValue = double.MinValue;
            var result = camera.IMV_GetDoubleFeatureValue(paramName, ref pValue);

            if (result != IMV_OK) return int.MinValue;
            return (float)pValue;
        }

        private int GetIntParam(string paramName)
        {
            var pValue = long.MinValue;
            var result = camera.IMV_GetIntFeatureValue(paramName, ref pValue);

            if (result != IMV_OK) return int.MinValue;
            return (int)pValue;
        }

        public string GetEnumValue(string paramName)
        {
            if (paramName.Length > 256) return string.Empty;

            var pStr = string.Empty;
            IMV_String str = new() { str = pStr };
            var result = camera.IMV_GetEnumFeatureSymbol(paramName, ref str);
            if (result != IMV_OK) return string.Empty;
            return str.str;
        }

        public CameraResult ExecuteCommand(string command)
        {
            if (!this.IsOpen)
                return CameraResult.Fail(-1, "Camera is not open");

            if (string.IsNullOrEmpty(command)) return CameraResult.Fail(-1, "Check the command");
            try
            {
                var result = camera.IMV_ExecuteCommandFeature(command);
                if (result == IMV_OK)
                    return CameraResult.Success(result);
                return CameraResult.Fail(result, "Execute command error");
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            if (camera != null && Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
            {
                try
                {
                    StopGrab();
                    Close();
                    camera.IMV_DestroyHandle();
                    camera = null;
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void ProcessFrameCallBack(ref IMV_Frame pFrame, IntPtr pUser)
        {
            if (pFrame.pData == IntPtr.Zero) return;
            var wrapped = new IRaypleFrameWrapper(pFrame);
            camera.IMV_ReleaseFrame(ref pFrame);
            stream.Publish(wrapped);
        }
    }
}
