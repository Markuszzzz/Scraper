namespace cSharpScraper.Crawler.WebCrawler.Services;

public class DomainService(DomainParser domainParser)
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

            if (crawledDomainInfo is null)
                return false;
            if (crawledDomainInfo.FullyQualifiedDomainName != domainInfo.FullyQualifiedDomainName)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Error while testing if {nameof(url)}:{url} is in scope", e);
        }
    }

    public bool ShouldHaveWwwSubdomain(string url, DomainInfo domainInfo)
    {
        if (domainInfo.Subdomain is not null)
            return false;

        var redirectDomain = _domainParser.Parse(url);

        if (redirectDomain.Subdomain is "www")
            return true;

        return false;
    }
}