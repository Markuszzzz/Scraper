namespace cSharpScraper.Features.WebsiteCrawler.Models;

public class CrawlerSettings
{
    public required string Target { get; set; }
    public bool CrawlSubdomains { get; set; }
    public bool Eager { get; set; }
    public bool Headless { get; set; }
    public int RequestDelay { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
    public string? ProxyAddress { get; set; }
}