namespace cSharpScraper.Crawler.WebCrawler.Models;

public class CrawlerSettings
{
    public string Url { get; init; }
    public bool Scope { get; init; }
    public bool Eager { get; init; }
    public bool Headless { get; init; }
    public int RequestDelay { get; init; }
    public Dictionary<string, object> Headers { get; init; } = new();
    public string ProxyAddress { get; init; }
}