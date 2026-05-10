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

        void StopGrab();

        CameraResult SetParam(string key, string value);
        CameraResult ExecuteCommand(string value);

        CameraResult SetTrigger(string triggerWay, bool isTrigger = false);

        T GetParam<T>(string paramName);

    }
}
