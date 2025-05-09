using cSharpScraper.Crawler.SubDomainCrawler;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaisance.SubdomainDiscovery;

public static class SubdomainDiscoveryRegistration
{
    public static IServiceCollection AddSubdomainDiscovery(this IServiceCollection services)
    {
        services.AddTransient<ISubdomainDiscoveryService, SubdomainDiscoveryService>()
            .AddSingleton<SubdomainCrawlerDecorator>()
            .AddHttpClient<IUrlAvailabilityChecker, UrlAvailabilityChecker>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        
        return services;
    }
}