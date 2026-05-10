using IndustrialCameraManager.Core;
using System;
using System.Collections.Concurrent;

namespace IndustrialCameraManager.Stream
{
    public class StreamManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, ICameraStream> streams = new();

        public void Dispose()
        {
            foreach(var stream in streams.Values) { stream.Dispose(); }
            streams.Clear();
        }

        public ICameraStream GetOrCreateStream(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (streams.TryGetValue(key, out var existing))
                return existing;

            var stream = new CameraStream();
            streams[key] = stream;
            return stream;
        }

        public bool GetStream(string serialNumber, out ICameraStream stream)
        {
            stream = null;

            if (string.IsNullOrEmpty(serialNumber))
                return false;

            return streams.TryGetValue(serialNumber, out stream);
        }
    }
}
