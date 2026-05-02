using System;

namespace IndustrialCameraManager.Stream
{
    public class Subscription : IDisposable
    {
        private readonly Action dispose;

        public Subscription(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            dispose();
        }
    }
}
