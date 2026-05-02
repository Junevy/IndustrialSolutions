using IndustrialCameraManager.Core;
using MvCameraControl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialCameraManager.HikVision
{
    public class HikCamera : ICamera
    {
        private IDevice camera;
        private readonly IDeviceInfo deviceInfo;
        private bool isCreate = false;
        private readonly SemaphoreSlim grabLocker = new(1, 1);

        public bool IsOpen => false;

        public HikCamera(IDeviceInfo info)
        {
            this.deviceInfo = info;
            camera.StreamGrabber.FrameGrabedEvent += GrabedProcess;

        }

        public HikCamera(string ipAddress, string netExport)
        {
            try
            {
                this.camera = DeviceFactory.CreateDeviceByIp(ipAddress, netExport);
                camera.StreamGrabber.FrameGrabedEvent += GrabedProcess;
                isCreate = true;
            }
            catch (MvException)
            {
                throw;
            }
        }

        private void GrabedProcess(object sender, FrameGrabbedEventArgs e)
        {
            throw new NotImplementedException();
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
                    return CameraResult.Fail(result);

                if (camera is IGigEDevice)
                {
                    IGigEDevice gigEDevice = camera as IGigEDevice;

                    // 配置网口相机的最佳包大小
                    gigEDevice.GetOptimalPacketSize(out int packetSize);
                    gigEDevice.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                }
                else if (camera is IUSBDevice)
                {
                    // 设置USB同步读写超时时间
                    IUSBDevice usbDevice = camera as IUSBDevice;
                    usbDevice.SetSyncTimeOut(1000);
                }

                return CameraResult.Success(result);
            }
            catch (MvException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CameraResult Close()
        {
            int result = camera.Close();
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult Grab()
        {
            int result = camera.StreamGrabber.StartGrabbing();
            return CameraResult.Result(result == MvError.MV_OK, result);

        }

        public async Task GrabAsync(CancellationToken ct = default)
        {
            await grabLocker.WaitAsync();

            try
            {
                ct.ThrowIfCancellationRequested();

                SetParam("TriggerMode", "Off");
                await Task.Run(() =>
                {
                    int result = camera.StreamGrabber.StartGrabbing();
                    if (result != MvError.MV_OK)
                        throw new InvalidOperationException("Start acquisition error");

                }, ct);
            }
            finally
            {
                grabLocker.Release();
            }
        }

        public void StopGrab()
        {

            throw new System.NotImplementedException();
        }

        public CameraResult SetParam(string key, string value)
        {
            int result = camera.Parameters.SetEnumValueByString(key, value);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public void Dispose()
        {
            camera.Dispose();
            camera = null;
        }
    }
}
