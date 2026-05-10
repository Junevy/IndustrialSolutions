using System;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Core
{
    public interface ICameraStream : IDisposable
    {
        int SubscriberCount { get; }

        void Publish(IFrame frame);

        void Subscribe(string key, Func<IFrame, Task> handler, int capacity = 5);

        bool Unsubscribe(string key);

    }
}