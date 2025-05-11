using cSharpScraper.Reconnaissance.Httpx;

namespace cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;

public interface IArchivedUrlDiscoveryService
{
    Task<List<HttpxResult>> DiscoverArchivedUrls(CrawlTarget crawlTarget);
}