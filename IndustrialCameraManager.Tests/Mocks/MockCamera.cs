using IndustrialCameraManager.Abstractions;
using System;
using System.Drawing;

namespace IndustrialCameraManager.Tests.Mocks
{
    public class MockCamera : ICamera
    {
        public bool IsOpen { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool IsGrabbing { get; private set; }

        public CameraResult Open()
        {
            IsOpen = true;
            return CameraResult.Success(0);
        }

        public CameraResult Close()
        {
            IsOpen = false;
            return CameraResult.Success(0);
        }

        public CameraResult Grab()
        {
            IsGrabbing = true;
            return CameraResult.Success(0);
        }

        public void StopGrab()
        {
            IsGrabbing = false;
        }

        public CameraResult SetParam(string key, string value)
        {
            return CameraResult.Success(0);
        }

        public CameraResult ExecuteCommand(string command)
        {
            return CameraResult.Success(0);
        }

        public CameraResult SetTrigger(string triggerWay, bool isTrigger = false)
        {
            return CameraResult.Success(0);
        }

        public T GetParam<T>(string paramName)
        {
            return default(T);
        }

        public void Dispose()
        {
            IsDisposed = true;
            IsOpen = false;
        }
    }
}
