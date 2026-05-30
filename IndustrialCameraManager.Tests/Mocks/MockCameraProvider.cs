using IndustrialCameraManager.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IndustrialCameraManager.Tests.Mocks
{
    public class MockCameraProvider : ICameraProvider
    {
        private readonly List<MockCameraInfo> cameraInfos;
        private readonly List<MockCamera> createdCameras = new List<MockCamera>();

        public bool IsDisposed { get; private set; }

        public IReadOnlyList<MockCamera> CreatedCameras => createdCameras;

        public MockCameraProvider(List<MockCameraInfo> cameraInfos)
        {
            this.cameraInfos = cameraInfos ?? new List<MockCameraInfo>();
        }

        public ICamera Create(ICameraInfo info)
        {
            var mockCamera = new MockCamera();
            createdCameras.Add(mockCamera);
            return mockCamera;
        }

        public IEnumerable<ICamera> CreateAll()
        {
            return cameraInfos.Select(ci => Create(ci));
        }

        public IEnumerable<ICameraInfo> Enumerate()
        {
            return cameraInfos;
        }

        public IEnumerable<ICameraInfo> Enumerate(CameraType type)
        {
            return cameraInfos;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
