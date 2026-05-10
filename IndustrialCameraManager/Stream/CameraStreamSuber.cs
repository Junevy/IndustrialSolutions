using IndustrialCameraManager.Core;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Stream
{
    public class CameraStreamSuber : IDisposable
    {
        public Channel<IFrame> Channel { get; private set; }
        public CancellationTokenSource Cts { get; private set; }
        public Task Woker { get; private set; }

        public CameraStreamSuber(Channel<IFrame> channel, CancellationTokenSource cts, Task worker)
        {
            this.Channel = channel;
            this.Cts = cts;
            this.Woker = worker;
        }

        public void Dispose()
        {
            Channel.Writer.TryComplete();
            Cts.Cancel();
            Cts.Dispose();
        }
    }
}
