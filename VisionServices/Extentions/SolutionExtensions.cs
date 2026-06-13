using Microsoft.Extensions.DependencyInjection;
using VisionServices.Controls;
using VisionServices.Core;
using VisionServices.Services.VisionMaster;

namespace VisionServices.Extentions
{
    public static class SolutionExtensions
    {
        public static IServiceCollection AddVmSolution(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IVisionControls, VmControls>();
            serviceCollection.AddSingleton<VmService>();
            serviceCollection.AddSingleton<ISolution>(sp => sp.GetRequiredService<VmService>());
            serviceCollection.AddSingleton<IGroupSolution>(sp => sp.GetRequiredService<VmService>());

            return serviceCollection;
        }
    }
}
