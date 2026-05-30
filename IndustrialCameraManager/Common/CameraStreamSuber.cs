using IndustrialCameraManager.Abstractions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机图像帧数据流订阅者，用于取消订阅（Dispose）或手动停止取流（Cancel）。
    /// </summary>
    public class CameraStreamSuber(Channel<IFrame> channel, CancellationTokenSource cts, Task worker)
    {
        public Channel<IFrame> Channel { get; private set; } = channel;
        public CancellationTokenSource Cts { get; private set; } = cts;
        public Task Subber { get; private set; } = worker;

        public void Dispose()
        {
            Channel.Writer.TryComplete();
            Cts.Cancel();
            Cts.Dispose();
        }
    }
}
