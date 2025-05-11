namespace cSharpScraper.Crawler.WebCrawler;

public class CrawlTarget
{
    public required DomainInfo DomainInfo { get; set; }
    public string? Url { get; set; }


    private static bool ValidateTarget(DomainInfo domain, bool crawlSubdomains, ILogger logger)
    {
        if (!crawlSubdomains && domain.Subdomain is null)
        {
            logger.LogWarning("For single-domain crawling, use a FQDN like 'www.example.com'");
            return false;
        }

        if (crawlSubdomains && domain.Subdomain is not null)
        {
            logger.LogWarning("For subdomain discovery, use a registrable domain like 'example.com', not 'www.example.com'");
            return false;
        }

        return true;
    }
}