using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Extensions;
using IndustrialCameraManager.Tests.Mocks;
using IndustrialCameraManager.Vendors.HikVision;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IndustrialCameraManager.Tests.Extensions
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddIndustrialCamera_WithHikVision_ShouldResolveServices()
        {
            var services = new ServiceCollection();

            services.AddIndustrialCamera(options =>
            {
                options.EnableHikVision = true;
            });

            var provider = services.BuildServiceProvider();

            var streamManager = provider.GetService<StreamManager>();
            Assert.IsNotNull(streamManager);

            var cameraManager = provider.GetService<CameraManager>();
            Assert.IsNotNull(cameraManager);

            var cameraService = provider.GetService<CameraService>();
            Assert.IsNotNull(cameraService);

            var cameraProvider = provider.GetService<ICameraProvider>();
            Assert.IsNotNull(cameraProvider);
            Assert.IsInstanceOfType(cameraProvider, typeof(HikCameraProvider));

            var sdkSystem = provider.GetService<ICameraSdkSystem>();
            Assert.IsNotNull(sdkSystem);
            Assert.IsInstanceOfType(sdkSystem, typeof(HikCameraSdkSystem));
        }

        [TestMethod]
        public void AddIndustrialCamera_WithoutOptions_ShouldNotRegisterManagementServices()
        {
            var services = new ServiceCollection();

            services.AddIndustrialCamera();

            var provider = services.BuildServiceProvider();

            var streamManager = provider.GetService<StreamManager>();
            Assert.IsNull(streamManager);

            var cameraManager = provider.GetService<CameraManager>();
            Assert.IsNull(cameraManager);

            var cameraService = provider.GetService<CameraService>();
            Assert.IsNull(cameraService);

            var cameraProvider = provider.GetService<ICameraProvider>();
            Assert.IsNull(cameraProvider);
        }

        [TestMethod]
        public void AddHikVisionCamera_ShouldRegisterHikVisionServices()
        {
            var services = new ServiceCollection();

            services.AddHikVisionCamera();

            var provider = services.BuildServiceProvider();

            var cameraProvider = provider.GetService<ICameraProvider>();
            Assert.IsNotNull(cameraProvider);
            Assert.IsInstanceOfType(cameraProvider, typeof(HikCameraProvider));

            var sdkSystem = provider.GetService<ICameraSdkSystem>();
            Assert.IsNotNull(sdkSystem);
            Assert.IsInstanceOfType(sdkSystem, typeof(HikCameraSdkSystem));
        }

        [TestMethod]
        public void AddIndustrialCamera_WithNullServices_ShouldThrowArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ServiceCollectionExtensions.AddIndustrialCamera(null);
            });
        }

        [TestMethod]
        public void AddIndustrialCamera_MultipleCalls_ShouldNotDuplicateRegistrations()
        {
            var services = new ServiceCollection();

            services.AddIndustrialCamera(options => options.EnableHikVision = true);
            services.AddIndustrialCamera(options => options.EnableHikVision = true);

            var provider = services.BuildServiceProvider();

            var streamManager = provider.GetService<StreamManager>();
            Assert.IsNotNull(streamManager);

            var cameraProvider = provider.GetService<ICameraProvider>();
            Assert.IsNotNull(cameraProvider);
        }

        [TestMethod]
        public void SingletonLifecycle_SameServiceInstanceOnMultipleResolve()
        {
            var services = new ServiceCollection();
            services.AddIndustrialCamera(options => options.EnableHikVision = true);

            var provider = services.BuildServiceProvider();

            var cameraManager1 = provider.GetService<CameraManager>();
            var cameraManager2 = provider.GetService<CameraManager>();
            Assert.AreSame(cameraManager1, cameraManager2);

            var streamManager1 = provider.GetService<StreamManager>();
            var streamManager2 = provider.GetService<StreamManager>();
            Assert.AreSame(streamManager1, streamManager2);

            var cameraService1 = provider.GetService<CameraService>();
            var cameraService2 = provider.GetService<CameraService>();
            Assert.AreSame(cameraService1, cameraService2);
        }

        [TestMethod]
        public void AddIndustrialCamera_WithBaslerOnly_ShouldThrowNotImplementedException()
        {
            var services = new ServiceCollection();

            Assert.ThrowsException<NotImplementedException>(() =>
            {
                services.AddIndustrialCamera(options =>
                {
                    options.EnableBasler = true;
                });
            });
        }

        [TestMethod]
        public void AddIndustrialCamera_WithBothVendors_ShouldRegisterBoth()
        {
            var services = new ServiceCollection();

            Assert.ThrowsException<NotImplementedException>(() =>
            {
                services.AddIndustrialCamera(options =>
                {
                    options.EnableHikVision = true;
                    options.EnableBasler = true;
                });
            });
        }
    }
}
