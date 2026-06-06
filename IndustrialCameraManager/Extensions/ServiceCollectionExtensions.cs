using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Vendors.HikVision;
using IndustrialCameraManager.Vendors.IRayple;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IndustrialCameraManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndustrialCamera(this IServiceCollection services, Action<CameraOptions> configure = null, Action<StreamOptions> streamConfigure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var options = new CameraOptions();
            configure?.Invoke(options);

            var streamOptions = new StreamOptions();
            streamConfigure?.Invoke(streamOptions);

            var hasVendor = false;

            if (options.EnableHikVision)
            {
                RegisterHikVisionServices(services);
                hasVendor = true;
            }

            if (options.EnableIRayple)
            {
                RegisterIRaypleServices(services);
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

        public static IServiceCollection AddIRaypleCamera(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            RegisterIRaypleServices(services);
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

        private static void RegisterIRaypleServices(IServiceCollection services)
        {
            services.TryAddSingleton<ICameraProvider, IRaypleCameraProvider>();
            services.TryAddSingleton<ICameraSdkSystem, IRaypleCameraSdkSystem>();
        }

        private static void RegisterBaslerServices(IServiceCollection services)
        {
            throw new NotImplementedException("Basler camera support is not yet implemented.");
        }
    }
}
