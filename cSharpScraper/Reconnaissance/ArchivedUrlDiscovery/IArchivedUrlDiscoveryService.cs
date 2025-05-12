using cSharpScraper.Reconnaissance.Httpx;
using cSharpScraper.WebCrawler.CrawlTargeting;

namespace cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;

public interface IArchivedUrlDiscoveryService
{
    Task<List<HttpxResult>> DiscoverArchivedUrls(CrawlTarget crawlTarget);
}