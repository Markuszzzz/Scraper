using Nager.PublicSuffix;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class DomainService
{
    private readonly DomainParser _domainParser;

    public DomainService(DomainParser domainParser)
    {
        _domainParser = domainParser;
    }
    public bool IsInScope(string url, DomainInfo domainInfo)
    {
        try
        {
            var crawledDomainInfo = _domainParser.Parse(url);

            if (crawledDomainInfo == null)
                return false;
            if (crawledDomainInfo.FullyQualifiedDomainName != domainInfo.FullyQualifiedDomainName)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"url is {url}");
            throw new Exception("new", e);
        }
    }

    public bool ShouldHaveWwwSubdomain(string url, DomainInfo domainInfo)
    {
        if (domainInfo.Subdomain != null)
            return false;

        var redirectDomain = _domainParser.Parse(url);

        if (redirectDomain.Subdomain == "www")
            return true;

        return false;
    }
}