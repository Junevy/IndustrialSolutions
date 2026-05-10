using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using IndustrialCameraManager.HikVision;
using IndustrialCameraManager.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IndustrialCameraManager.Extensions
{
    public static class CameraExtensions
    {
        public static IServiceCollection AddHikCameraService(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ICameraStream, CameraStream>();
            services.TryAddSingleton<ICameraProvider, HikCameraProvider>();
            services.TryAddSingleton<CameraManager>();
            services.TryAddSingleton<CameraService>();
            services.TryAddSingleton<ICameraSdkSystem, HikCameraSdkSystem>();
            
            return services;
        }
    }
}
