namespace cSharpScraper.Features.Shared.Models;

public class CrawlTarget
{
    public required DomainInfo DomainInfo { get; set; }
    public string? Url { get; set; }
}