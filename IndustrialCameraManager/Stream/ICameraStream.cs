using IndustrialCameraManager.Core;
using System;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Stream
{
    public interface ICameraStream : IDisposable
    {
        int SubscriberCount { get; }

        void Publish(IFrame frame);

        IDisposable Subscribe(Func<IFrame, Task> handler, int capacity = 5);
    }
}