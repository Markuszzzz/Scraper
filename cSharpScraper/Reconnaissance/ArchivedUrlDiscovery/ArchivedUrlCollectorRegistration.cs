using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;

public static class ArchivedUrlCollectorRegistration
{
    public static IServiceCollection AddArchivedUrlCollector(this IServiceCollection services)
    {
        services.AddTransient<IArchivedUrlCollector, WayBackUrlCollector>();
        services.AddTransient<IArchivedUrlDiscoveryService, ArchivedUrlDiscoveryService>();
        return services;
    }
}