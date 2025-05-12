using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.SubDomainCrawler;

public static class SubdomainDiscoveryRegistration
{
    public static IServiceCollection AddSubdomainDiscovery(this IServiceCollection services)
    {

        services.AddTransient<SubdomainScraperStorage>()
            .AddTransient<ISubdomainDiscoveryService, SubdomainDiscoveryService>();
        
        return services;
    }
}