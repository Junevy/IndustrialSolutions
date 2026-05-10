using IndustrialCameraManager.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Stream
{
    public class CameraStream : ICameraStream
    {
        private readonly ConcurrentDictionary<string, CameraStreamSuber> subscribers = new();
        public int SubscriberCount => subscribers.Count;

        public void Subscribe(string key, Func<IFrame, Task> handler, int capacity = 5)
        {
            var channel = Channel.CreateBounded<IFrame>(
                new BoundedChannelOptions(capacity)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                });

            if (subscribers.TryRemove(key, out var old))
                old.Dispose();

            var cts = new CancellationTokenSource();
            var worker = Task.Run(async () =>
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
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
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
            subscribers[key] = new CameraStreamSuber(channel, cts, worker);
        }

        public void Publish(IFrame frame)
        {
            foreach (var sub in subscribers.Values)
            {
                frame.AddRef();

                if (!sub.Channel.Writer.TryWrite(frame))
                    frame.Dispose(); // 写失败要释放
            }
            frame.Dispose(); // 初始引用释放
        }

        public bool Unsubscribe(string key)
        {
            if (subscribers.TryRemove(key, out var suber))
            {
                suber.Dispose();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var sub in subscribers.Values)
            {
                sub.Dispose();
            }
            subscribers.Clear();

        }
    }
}

