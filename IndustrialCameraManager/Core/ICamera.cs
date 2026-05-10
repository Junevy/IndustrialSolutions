using IndustrialCameraManager.Common;
using System;

namespace IndustrialCameraManager.Core
{
    public interface ICamera : IDisposable
    {
        bool IsOpen { get; }

        CameraResult Open();

        CameraResult Close();

        CameraResult Grab();

        CameraResult Trigger(string triggerWay);

        void StopGrab();

        CameraResult SetParam(string key, string value);
        CameraResult SetParam(string value);

        T GetParam<T>(string paramName);

    }
}
