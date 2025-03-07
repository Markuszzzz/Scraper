using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Models;
using cSharpScraper.Crawler.WebCrawler.Services;
using cSharpScraper.Reconnaisance.SubdomainDiscovery;
using cSharpScraper.Storage.Models;
using Microsoft.Extensions.Logging;

namespace cSharpScraper.Crawler.SubDomainCrawler;

public class SubdomainCrawler
{
    private readonly WebsiteCrawler _webCrawler;
    private readonly ILogger<SubdomainCrawler> _logger;
    private readonly ISubdomainEnumerator _subdomainEnumerator;
    private readonly SubdomainScraperStorage _subdomainScraperStorage;

    public SubdomainCrawler(WebsiteCrawler webCrawler, ILogger<SubdomainCrawler> logger,
        ISubdomainEnumerator subdomainEnumerator,
        SubdomainScraperStorage subdomainScraperStorage)

    {
        _webCrawler = webCrawler;
        _logger = logger;
        _subdomainEnumerator = subdomainEnumerator;
        _subdomainScraperStorage = subdomainScraperStorage;
    }

    public async Task Crawl(string domainToScrape)
    {
        var domainsToScrape = new List<string>();
        if (UrlUtility.IsSubdomainWildcard(domainToScrape))
        {
            domainsToScrape.AddRange(await _subdomainEnumerator.FindSubdomains(domainToScrape));
        }


        for (var i = _subdomainScraperStorage.LoadSubdomainToScrape(domainToScrape); i < domainsToScrape.Count; i++)
        {
            _logger.LogInformation(
                $"Currently attempting to scraped enumerated subdomain of {domainToScrape}: {domainsToScrape[i]}");
            _logger.LogInformation($"Enumerated subdomain {i}/{domainsToScrape.Count}" + Environment.NewLine);


            var currentDomain = domainsToScrape[i];

            if (currentDomain.StartsWith("https://"))
                currentDomain = currentDomain.Remove(0, 8);

            if (currentDomain.EndsWith("/"))
                currentDomain = currentDomain.Substring(0, currentDomain.Length - 1);


            _webCrawler.InitializeCrawler(currentDomain);

            if (!await _webCrawler.CanBeCrawled(currentDomain))
            {
                _logger.LogInformation("Moving on to the next site\n");

                _subdomainScraperStorage.IncrementSubdomainToScrape(domainToScrape);

                continue;
            }


            await _webCrawler.Crawl(currentDomain);

            _subdomainScraperStorage.IncrementSubdomainToScrape(domainToScrape);
        }
    }
}