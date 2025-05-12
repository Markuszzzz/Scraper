namespace cSharpScraper.Features.SubDomainCrawler;

public interface ISubdomainDiscoveryService
{
    public Task<List<string>> DiscoverAsync(string registrableDomain);
}