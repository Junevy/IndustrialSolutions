using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Vendors.HikVision;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IndustrialCameraManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndustrialCamera(this IServiceCollection services, Action<CameraOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var options = new CameraOptions();
            configure?.Invoke(options);

            var hasVendor = false;

            if (options.EnableHikVision)
            {
                RegisterHikVisionServices(services);
                hasVendor = true;
            }

            if (options.EnableBasler)
            {
                RegisterBaslerServices(services);
                hasVendor = true;
            }

            if (hasVendor)
                RegisterManagementServices(services);

            return services;
        }

        public static IServiceCollection AddHikVisionCamera(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            RegisterHikVisionServices(services);
            RegisterManagementServices(services);

            return services;
        }

        private static void RegisterManagementServices(IServiceCollection services)
        {
            services.TryAddSingleton<StreamManager>();
            services.TryAddSingleton<CameraManager>();
            services.TryAddSingleton<CameraService>();
        }

        private static void RegisterHikVisionServices(IServiceCollection services)
        {
            services.TryAddSingleton<ICameraProvider, HikCameraProvider>();
            services.TryAddSingleton<ICameraSdkSystem, HikCameraSdkSystem>();
        }

        private static void RegisterBaslerServices(IServiceCollection services)
        {
            throw new NotImplementedException("Basler camera support is not yet implemented. Register ICameraProvider and ICameraSdkSystem for Basler here.");
        }
    }
}
