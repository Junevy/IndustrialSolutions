using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionServices.Controls;
using VisionServices.Core;
using VisionServices.Services.VisionMaster;
using VisionServices.Store;

namespace VisionServices.Extentions
{
    public static class SolutionExtensions
    {
        public static IServiceCollection AddVmSolution(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ProcedureStore>();
            serviceCollection.TryAddSingleton<IVisionControls, VmControls>();
            serviceCollection.TryAddSingleton<VmService>();
            serviceCollection.TryAddSingleton<ISolution>(sp => sp.GetRequiredService<VmService>());
            serviceCollection.TryAddSingleton<IGroupSolution>(sp => sp.GetRequiredService<VmService>());

            return serviceCollection;
        }
    }
}
