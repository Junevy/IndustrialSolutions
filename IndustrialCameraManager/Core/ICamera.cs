using MvCameraControl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Core
{
    public interface ICamera : IDisposable
    {
        bool IsOpen { get; }
        //string SerialNumber { get; }


        CameraResult Open();

        CameraResult Close();

        Task GrabAsync(CancellationToken ct = default);

        CameraResult Grab();

        void StopGrab();

        CameraResult SetParam(string key, string value);


    }
}
