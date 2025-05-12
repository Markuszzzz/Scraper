using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Features.SubDomainCrawler;

public static class SubdomainCrawlerRegistration
{
    public static IServiceCollection AddSubdomainCrawler(this IServiceCollection services)
    {

        services.AddTransient<SubdomainScraperStorage>()
            .AddTransient<ISubdomainDiscoveryService, SubdomainDiscoveryService>();
        
        return services;
    }
}