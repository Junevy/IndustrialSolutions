using Microsoft.Extensions.DependencyInjection;
using VisionServices.Core;
using VisionServices.Services.VisionMaster;
using VM.Core;

namespace VisionServices.Extentions
{
    public static class SolutionExtensions
    {
        public static IServiceCollection AddVmSolution(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(VmSolution.Instance);
            serviceCollection.AddSingleton<VmSolutionService>();
            serviceCollection.AddSingleton<ISolution>(sp => sp.GetRequiredService<VmSolutionService>());
            serviceCollection.AddSingleton<IGroupSolution>(sp => sp.GetRequiredService<VmSolutionService>());

            return serviceCollection;
        }
    }
}
