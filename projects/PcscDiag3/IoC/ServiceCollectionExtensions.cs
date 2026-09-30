using Microsoft.Extensions.DependencyInjection;
using SpringCard.LibCs.Windows.UI.IoC;
using System.Linq;

namespace PcscDiag3.IoC;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        return services
            .AddSpringCardUI()
            .AddViewModels();
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        typeof(ServiceCollectionExtensions)
            .Assembly
            .GetTypes()
            .Where(x => x.Name.EndsWith("ViewModel"))
            .ToList()
            .ForEach(x => services.AddTransient(x));

        return services;
    }
}
