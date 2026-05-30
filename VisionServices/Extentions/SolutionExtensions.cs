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
            serviceCollection.AddSingleton<IControl, VmControls>();
            //serviceCollection.AddSingleton(VmSolution.Instance);
            serviceCollection.AddSingleton<VmSolutionService>();
            serviceCollection.AddSingleton<ISolution>(sp => sp.GetRequiredService<VmSolutionService>());
            serviceCollection.AddSingleton<IGroupSolution>(sp => sp.GetRequiredService<VmSolutionService>());

            return serviceCollection;
        }
    }
}
