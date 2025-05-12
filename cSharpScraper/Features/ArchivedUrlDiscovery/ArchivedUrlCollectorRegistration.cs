using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Features.ArchivedUrlDiscovery;

public static class ArchivedUrlCollectorRegistration
{
    public static IServiceCollection AddArchivedUrlDiscovery(this IServiceCollection services)
    {
        services.AddTransient<IArchivedUrlCollector, WayBackUrlCollector>();
        services.AddTransient<IArchivedUrlDiscoveryService, ArchivedUrlDiscoveryService>();
        return services;
    }
}