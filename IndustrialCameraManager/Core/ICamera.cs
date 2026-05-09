using System;

namespace IndustrialCameraManager.Core
{
    public interface ICamera : IDisposable
    {
        bool IsOpen { get; }

        CameraResult Open();

        CameraResult Close();

        //Task GrabAsync(CancellationToken ct = default);

        CameraResult Grab();

        CameraResult Trigger(string triggerWay);

        void StopGrab();

        CameraResult SetParam(string key, string value);

        T GetParam<T>(string paramName);

    }
}
