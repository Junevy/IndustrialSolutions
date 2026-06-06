using IndustrialCameraManager.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Common
{
    /// <summary>
    /// 相机图像帧数据流类，用于发布和订阅相机图像帧数据。
    /// </summary>
    public class CameraStream : ICameraStream
    {
        private readonly ConcurrentDictionary<string, CameraStreamSuber> subscribers = new();

        // 订阅数量
        public int SubscriberCount => subscribers.Count;


        public void Subscribe(string key, Func<IFrame, Task> handler, int capacity)
        {
            var channel = Channel.CreateBounded<IFrame>(
                new BoundedChannelOptions(capacity)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                });

            if (subscribers.TryRemove(key, out var oldSuber))
                oldSuber.Dispose();

            var cts = new CancellationTokenSource();
            var worker = Task.Run( async () =>
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
                                // 处理异常...
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
            if (frame == null || frame.Data == null) return;
            if (subscribers.Count <= 0) return;

            foreach (var sub in subscribers.Values)
            {
                frame.AddRef();

                if (!sub.Channel.Writer.TryWrite(frame))
                    frame.Dispose();
            }
            frame.Dispose();
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
