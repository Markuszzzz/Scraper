using cSharpScraper.Reconnaissance.Httpx;
using cSharpScraper.WebCrawler.CrawlTargeting;

namespace cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;

public class ArchivedUrlDiscoveryService(IArchivedUrlCollector archivedUrlCollector, DomainService domainService, IHttpxExecutor httpxExecutor) : IArchivedUrlDiscoveryService 
{
    private readonly IArchivedUrlCollector _archivedUrlCollector = archivedUrlCollector;
    private readonly DomainService _domainService = domainService;
    private readonly IHttpxExecutor _httpxExecutor = httpxExecutor;


    public async Task<List<HttpxResult>> DiscoverArchivedUrls(CrawlTarget crawlTarget)
    {
        var urls = _archivedUrlCollector.GetArchivedUrlsForDomain(crawlTarget.DomainInfo.RegistrableDomain);
        urls = UrlUtility.FilterUrlsOnPage(urls, crawlTarget.Url!)
            .Where(x => _domainService.IsInScope(x, crawlTarget.DomainInfo)).ToList();

        List<HttpxResult> archivedHttpxResults = await _httpxExecutor.GetResponsiveUrlsAsync(urls);
        
        return archivedHttpxResults;
    }
}