using System;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Core
{
    public interface ICameraStream : IDisposable
    {
        int SubscriberCount { get; }

        void Publish(IFrame frame);

        IDisposable Subscribe(Func<IFrame, Task> handler, int capacity = 5);
    }
}