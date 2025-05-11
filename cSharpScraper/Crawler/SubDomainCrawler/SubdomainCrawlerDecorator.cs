using cSharpScraper.Reconnaissance.SubdomainDiscovery;
using Microsoft.Extensions.Options;

namespace cSharpScraper.Crawler.SubDomainCrawler;

public class SubdomainCrawlerDecorator(
    WebsiteCrawler innerCrawler,
    ILogger<SubdomainCrawlerDecorator> logger,
    ISubdomainDiscoveryService subdomainDiscoveryService,
    IOptions<CrawlerSettings> crawlerSettings,
    SubdomainScraperStorage subdomainScraperStorage,
    ILogger<CrawlTarget> crawlTargetLogger) : ICrawler
{
    private readonly WebsiteCrawler _innerCrawler = innerCrawler;
    private readonly ILogger<SubdomainCrawlerDecorator> _logger = logger;
    private readonly ISubdomainDiscoveryService _subdomainDiscoveryService = subdomainDiscoveryService;
    private readonly SubdomainScraperStorage _subdomainScraperStorage = subdomainScraperStorage;
    private readonly ILogger<CrawlTarget> _crawlTargetLogger = crawlTargetLogger;

    public async Task CrawlAsync(CrawlTarget crawlTarget)
    {
        var registrableDomain = crawlTarget.DomainInfo.RegistrableDomain;
        List<string> subdomainUrls = await _subdomainDiscoveryService.DiscoverAsync(registrableDomain);
        
        foreach (var subdomainUrl in subdomainUrls)
        {
            _logger.LogInformation("Currently attempting to scraped subdomain: {SubdomainUrl} for domain: {DomainToScrape}", subdomainUrl, registrableDomain);
            
            
            var subdomainCrawlTarget = await CrawlTargetFactory.FromFullUrlAsync(subdomainUrl, _crawlTargetLogger);
            if (subdomainCrawlTarget is null)
            {
                _logger.LogWarning("Failed to create crawl target for subdomain: {SubdomainUrl}", subdomainUrl);
                continue;
            }   
            
            await _innerCrawler.CrawlAsync(subdomainCrawlTarget);

            _subdomainScraperStorage.IncrementSubdomainToScrape(registrableDomain);
        }

    
    }
    
}