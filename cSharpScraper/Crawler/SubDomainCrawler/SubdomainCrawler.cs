namespace cSharpScraper.Crawler.SubDomainCrawler;

public class SubdomainCrawler(
    WebsiteCrawler webCrawler,
    ILogger<SubdomainCrawler> logger,
    ISubdomainEnumerator subdomainEnumerator,
    SubdomainScraperStorage subdomainScraperStorage)
{
    private readonly WebsiteCrawler _webCrawler = webCrawler;
    private readonly ILogger<SubdomainCrawler> _logger = logger;
    private readonly ISubdomainEnumerator _subdomainEnumerator = subdomainEnumerator;
    private readonly SubdomainScraperStorage _subdomainScraperStorage = subdomainScraperStorage;

    public async Task CrawlAsync(string domainToScrape)
    {
        var domainsToScrape = new List<string>();
        if (UrlUtility.IsSubdomainWildcard(domainToScrape))
        {
            domainsToScrape.AddRange(await _subdomainEnumerator.FindSubdomainsAsync(domainToScrape));
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

            if (!await _webCrawler.CanBeCrawledAsync(currentDomain))
            {
                _logger.LogInformation("Moving on to the next site\n");

                _subdomainScraperStorage.IncrementSubdomainToScrape(domainToScrape);

                continue;
            }


            await _webCrawler.CrawlAsync(currentDomain);

            _subdomainScraperStorage.IncrementSubdomainToScrape(domainToScrape);
        }
    }
}