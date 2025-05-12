namespace cSharpScraper.WebCrawler.CrawlTargeting;

public class CrawlTarget
{
    public required DomainInfo DomainInfo { get; set; }
    public string? Url { get; set; }
}