using IndustrialCameraManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Stream
{
    public class CameraStream : ICameraStream
    {
        private readonly List<Channel<IFrame>> subscribers = new();
        private readonly object locker = new();

        public int SubscriberCount => subscribers.Count;

        public IDisposable Subscribe(Func<IFrame, Task> handler, int capacity = 5)
        {
            var channel = Channel.CreateBounded<IFrame>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

            lock (locker)
            {
                subscribers.Add(channel);
            }

            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    while (await channel.Reader.WaitToReadAsync(cts.Token))
                    {
                        while (channel.Reader.TryRead(out var frame))
                        {
                            try
                            {
                                await handler(frame);
                            }
                            finally
                            {
                                frame.Dispose();
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { }
            });

            return new Subscription(() =>
            {
                cts.Cancel();

                lock (locker)
                {
                    subscribers.Remove(channel);
                }

                channel.Writer.TryComplete();
            });
        }

        public void Publish(IFrame frame)
        {

            List<Channel<IFrame>> subs;

            lock (locker)
            {
                subs = subscribers.ToList(); // 拷贝一份
            }

            foreach (var sub in subs)
            {
                frame.AddRef();

                if (!sub.Writer.TryWrite(frame))
                {
                    frame.Dispose(); // 写失败要释放
                }
            }

            frame.Dispose(); // 初始引用释放
        }

        public void Dispose()
        {
            lock (locker)
            {
                foreach (var sub in subscribers)
                {
                    sub.Writer.TryComplete();
                }
            }
        }
    }
}
