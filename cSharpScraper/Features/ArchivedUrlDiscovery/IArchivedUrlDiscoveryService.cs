using cSharpScraper.Features.Httpx;
using cSharpScraper.Features.Shared.Models;
using cSharpScraper.Features.WebsiteCrawler.Models;

namespace cSharpScraper.Features.ArchivedUrlDiscovery;

public interface IArchivedUrlDiscoveryService
{
    Task<List<HttpxResult>> DiscoverArchivedUrls(CrawlTarget crawlTarget);
}