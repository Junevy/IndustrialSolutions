using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using System;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Stream
{
    public interface ICameraStream : IDisposable
    {
        int SubscriberCount { get; }

        void Publish(IFrame frame);

        IDisposable Subscribe(Func<RefCountFrame, Task> handler, int capacity = 5);
    }
}