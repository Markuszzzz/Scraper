namespace cSharpScraper.Crawler.WebCrawler.Models;

public interface ISubdomainDiscoveryService
{
    public Task<List<string>> DiscoverAsync(string domain);
}