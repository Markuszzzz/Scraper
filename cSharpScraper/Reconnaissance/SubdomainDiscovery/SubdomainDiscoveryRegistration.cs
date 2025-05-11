using cSharpScraper.Crawler.SubDomainCrawler;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaissance.SubdomainDiscovery;

public static class SubdomainDiscoveryRegistration
{
    public static IServiceCollection AddSubdomainDiscovery(this IServiceCollection services)
    {
        
        services.AddTransient<SubdomainScraperStorage>()
            .AddTransient<ISubdomainDiscoveryService, SubdomainDiscoveryService>()
            .AddSingleton<SubdomainCrawlerDecorator>();
        
        return services;
    }
}