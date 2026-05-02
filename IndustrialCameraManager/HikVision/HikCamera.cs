using IndustrialCameraManager.Core;
using IndustrialCameraManager.Stream;
using MvCameraControl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialCameraManager.HikVision
{
    public class HikCamera : ICamera
    {
        private bool isCreate = false;
        private bool isGrabbing = false;

        public bool IsOpen => (camera?.IsConnected) ?? false;   // The value is true When Camera.Open()
        private IDevice camera;
        private readonly IDeviceInfo deviceInfo;
        private readonly SemaphoreSlim grabLocker = new(1, 1);

        public ICameraStream Stream { get; } = new CameraStream();

        public HikCamera(IDeviceInfo info)
        {
            this.deviceInfo = info;
        }

        public HikCamera(string ipAddress, string netExport)
        {
            try
            {
                this.camera = DeviceFactory.CreateDeviceByIp(ipAddress, netExport);
                isCreate = true;
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
                if (!isCreate)
                {
                    camera = DeviceFactory.CreateDevice(deviceInfo);
                    isCreate = true;
                }

                int result = camera.Open();

                if (result != MvError.MV_OK)
                    return CameraResult.Fail(result, "Open camera failed");

                //if (camera is IGigEDevice)
                //{
                //    IGigEDevice gigEDevice = camera as IGigEDevice;

                //    // 配置网口相机的最佳包大小
                //    gigEDevice.GetOptimalPacketSize(out int packetSize);
                //    gigEDevice.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                //}
                //else if (camera is IUSBDevice)
                //{
                //    // 设置USB同步读写超时时间
                //    IUSBDevice usbDevice = camera as IUSBDevice;
                //    usbDevice.SetSyncTimeOut(1000);
                //}

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
            StopGrab();
            int result = camera.Close();
            isCreate = false;
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public CameraResult Grab()
        {
            if (isGrabbing)
                return CameraResult.Fail(-1, "Camera is already grabbing");

            var result = SetParam("TriggerMode", "Off");
            if (!result.IsSuccess)
            {
                isGrabbing = false;
                return result;
            }

            try
            {
                camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
                camera.StreamGrabber.FrameGrabedEvent += GrabedProcess;
                var grabResult = camera.StreamGrabber.StartGrabbing();
                isGrabbing = grabResult == MvError.MV_OK;
                return CameraResult.Result(isGrabbing, grabResult);
            }
            catch (MvException ex)
            {
                isGrabbing = false;
                camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
                return CameraResult.Fail(ex.ErrorCode, ex.Message);
            }
        }

        private void GrabedProcess(object sender, FrameGrabbedEventArgs e)
        {
            var clonedFrame = (IFrameOut)e.FrameOut.Clone();
            camera.StreamGrabber.FreeImageBuffer(e.FrameOut);   // copy完毕后释放原始帧

            IFrame frame = new HikFrameWrapper(clonedFrame);
            Stream.Publish(frame);
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
            camera.StreamGrabber.StopGrabbing();
            camera.StreamGrabber.FrameGrabedEvent -= GrabedProcess;
            isGrabbing = false;
        }

        public CameraResult SetParam(string key, string value)
        {
            int result = camera.Parameters.SetEnumValueByString(key, value);
            return CameraResult.Result(result == MvError.MV_OK, result);
        }

        public void Dispose()
        {
            Close();
            camera.Dispose();
            camera = null;
        }
    }
}
