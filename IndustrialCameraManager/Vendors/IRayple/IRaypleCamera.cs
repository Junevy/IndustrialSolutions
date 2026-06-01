using IndustrialCameraManager.Abstractions;
using System;
using System.Net;

namespace IndustrialCameraManager.Vendors.IRayple
{
    public class IRaypleCamera : ICamera
    {
        private volatile int _isOpen = 0;
        private volatile int _isGrabbing = 0;
        private volatile int _disposed = 0;

        private readonly ICameraStream _stream;
        private object _cameraDevice;
        private object _deviceInfo;
        private Delegate _frameHandler;

        public bool IsOpen => _isOpen == 1;
        public bool IsGrabbing => _isGrabbing == 1;

        public IRaypleCamera(object deviceInfo, ICameraStream stream)
        {
            _deviceInfo = deviceInfo;
            _stream = stream;
        }

        public IRaypleCamera(string ipAddress, ICameraStream stream)
        {
            _stream = stream;
            CreateCameraByIP(this, ipAddress);
        }

        public CameraResult Open()
        {
            if (_isOpen == 1)
                return CameraResult.Fail(-1, "Camera is already open");

            try
            {
                if (_cameraDevice == null)
                {
                    var info = _deviceInfo as dynamic;
                    var cameraKey = info?.CameraKey ?? info?.IPAddress ?? string.Empty;

                    _cameraDevice = EvokeCameraCreate(cameraKey);
                }

                if (_cameraDevice == null)
                    return CameraResult.Fail(-1, "Failed to create camera device");

                EvokeCameraOpen(_cameraDevice);
                EvokeFrameCallbackRegister(_cameraDevice, OnFrameGrabbedIntPtr);

                _isOpen = 1;
                return CameraResult.Success(0);
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-1, e.Message);
            }
        }

        public CameraResult Close()
        {
            if (_cameraDevice == null)
                return CameraResult.Fail(-1, "Camera not initialized");

            StopGrab();

            try
            {
                EvokeFrameCallbackUnregister(_cameraDevice);
                EvokeCameraClose(_cameraDevice);
                return CameraResult.Success(0);
            }
            catch (Exception e)
            {
                return CameraResult.Fail(-1, e.Message);
            }
            finally
            {
                _isOpen = 0;
            }
        }

        public CameraResult Grab()
        {
            if (_isGrabbing == 1)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            try
            {
                EvokeCameraStartGrabbing(_cameraDevice);
                _isGrabbing = 1;
                return CameraResult.Success(0);
            }
            catch (Exception ex)
            {
                StopGrab();
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public void StopGrab()
        {
            if (_isGrabbing == 1)
            {
                try { EvokeCameraStopGrabbing(_cameraDevice); } catch { }
                _isGrabbing = 0;
            }
        }

        public CameraResult SetParam<T>(string paramName, T value)
        {
            try
            {
                var type = typeof(T);
                if (type == typeof(int)) return SetIntParam(paramName, (int)(object)value);
                if (type == typeof(float)) return SetFloatParam(paramName, (float)(object)value);
                if (type == typeof(bool)) return SetBoolParam(paramName, (bool)(object)value);
                if (type == typeof(string)) return SetStringParam(paramName, (string)(object)value);
                return CameraResult.Fail(-1, "Unsupported parameter type");
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public CameraResult SetParam(string paramName, int value)
            => SetIntParam(paramName, value);

        public CameraResult SetParam(string paramName, float value)
            => SetFloatParam(paramName, value);

        public CameraResult SetParam(string paramName, bool value)
            => SetBoolParam(paramName, value);

        public CameraResult SetParam(string paramName, string value)
            => SetStringParam(paramName, value);

        public CameraResult SetEnumParam(string paramName, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(value))
                    return CameraResult.Fail(-1, "The paramName or value is null");
                EvokeSetEnumValue(_cameraDevice, paramName, value);
                return CameraResult.Success(0);
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public T GetParam<T>(string paramName)
        {
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

        public string GetEnumValue(string paramName)
        {
            try { return EvokeGetEnumValue(_cameraDevice, paramName); }
            catch { return "ERROR"; }
        }

        public CameraResult ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return CameraResult.Fail(-1, "Check the command");
            try
            {
                EvokeExecuteCommand(_cameraDevice, command);
                return CameraResult.Success(0);
            }
            catch (Exception ex)
            {
                return CameraResult.Fail(-1, ex.Message);
            }
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            if (_cameraDevice != null)
            {
                Close();
                EvokeCameraDestroy(_cameraDevice);
                _cameraDevice = null;
            }
            _isOpen = 0;
            _isGrabbing = 0;
        }

        private void OnFrameGrabbedIntPtr(IntPtr framePtr)
        {
            if (framePtr == IntPtr.Zero) return;
            var wrapped = new IRaypleFrameWrapper(framePtr);
            _stream.Publish(wrapped);
        }

        private CameraResult SetIntParam(string key, int value)
        {
            try { EvokeSetIntValue(_cameraDevice, key, value); return CameraResult.Success(0); }
            catch (Exception ex) { return CameraResult.Fail(-1, ex.Message); }
        }

        private CameraResult SetFloatParam(string key, float value)
        {
            try { EvokeSetFloatValue(_cameraDevice, key, value); return CameraResult.Success(0); }
            catch (Exception ex) { return CameraResult.Fail(-1, ex.Message); }
        }

        private CameraResult SetBoolParam(string key, bool value)
        {
            try { EvokeSetBoolValue(_cameraDevice, key, value); return CameraResult.Success(0); }
            catch (Exception ex) { return CameraResult.Fail(-1, ex.Message); }
        }

        private CameraResult SetStringParam(string key, string value)
        {
            try { EvokeSetStringValue(_cameraDevice, key, value); return CameraResult.Success(0); }
            catch (Exception ex) { return CameraResult.Fail(-1, ex.Message); }
        }

        private int GetIntParam(string key)
        {
            try { return EvokeGetIntValue(_cameraDevice, key); } catch { return 0; }
        }

        private float GetFloatParam(string key)
        {
            try { return EvokeGetFloatValue(_cameraDevice, key); } catch { return 0f; }
        }

        private bool GetBoolParam(string key)
        {
            try { return EvokeGetBoolValue(_cameraDevice, key); } catch { return false; }
        }

        private string GetStringParam(string key)
        {
            try { return EvokeGetStringValue(_cameraDevice, key); } catch { return string.Empty; }
        }

        private static void CreateCameraByIP(IRaypleCamera parent, string ipAddress)
        {
        }

        private static object EvokeCameraCreate(string cameraKey) => null;
        private static void EvokeCameraOpen(object device) { }
        private static void EvokeCameraClose(object device) { }
        private static void EvokeCameraStartGrabbing(object device) { }
        private static void EvokeCameraStopGrabbing(object device) { }
        private static void EvokeCameraDestroy(object device) { (device as IDisposable)?.Dispose(); }
        private static void EvokeFrameCallbackRegister(object device, Action<IntPtr> handler) { }
        private static void EvokeFrameCallbackUnregister(object device) { }
        private static void EvokeSetIntValue(object device, string key, int value) { }
        private static void EvokeSetFloatValue(object device, string key, float value) { }
        private static void EvokeSetBoolValue(object device, string key, bool value) { }
        private static void EvokeSetStringValue(object device, string key, string value) { }
        private static void EvokeSetEnumValue(object device, string key, string value) { }
        private static int EvokeGetIntValue(object device, string key) => 0;
        private static float EvokeGetFloatValue(object device, string key) => 0f;
        private static bool EvokeGetBoolValue(object device, string key) => false;
        private static string EvokeGetStringValue(object device, string key) => string.Empty;
        private static string EvokeGetEnumValue(object device, string key) => string.Empty;
        private static void EvokeExecuteCommand(object device, string command) { }
    }
}
