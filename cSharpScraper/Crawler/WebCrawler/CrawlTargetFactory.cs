namespace cSharpScraper.Crawler.WebCrawler;

public static class CrawlTargetFactory
{
    public static async Task<CrawlTarget?> FromFullUrlAsync(string url, ILogger logger)
    {
        var parser = await DomainParserProvider.GetAsync();
        DomainInfo? domainInfo;

        try
        {
            domainInfo = parser.Parse(url);
            if (domainInfo == null)
            {
                logger.LogWarning("Failed to parse domain from URL: {Url}", url);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception while parsing domain from URL: {Url}", url);
            return null;
        }

        return new CrawlTarget
        {
            DomainInfo = domainInfo,
            Url = url
        };
    }

    public static async Task<CrawlTarget?> FromRegistrableDomainAsync(string registrableDomain, ILogger logger)
    {
        var parser = await DomainParserProvider.GetAsync();
        DomainInfo? domainInfo;

        try
        {
            domainInfo = parser.Parse(registrableDomain);
            if (domainInfo == null)
            {
                logger.LogWarning("Failed to parse registrable domain: {Domain}", registrableDomain);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception while parsing registrable domain: {Domain}", registrableDomain);
            return null;
        }

        return new CrawlTarget
        {
            DomainInfo = domainInfo
        };
    }
}