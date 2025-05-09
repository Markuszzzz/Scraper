using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaisance.SiteArchive;

public static class ArchivedUrlCollectorRegistration
{
    public static IServiceCollection AddArchivedUrlCollector(this IServiceCollection services)
    {
        services.AddTransient<ArchivedUrlCollector>();
        return services;
    }
}