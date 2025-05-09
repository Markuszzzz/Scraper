namespace cSharpScraper.Crawler.WebCrawler.Models;

public class CrawlerSettings
{
    public string Url { get; set; }
    public bool Scope { get; set; }
    public bool Eager { get; set; }
    public bool Headless { get; set; }
    public int RequestDelay { get; set; }
    public Dictionary<string, object> Headers { get; set; } = new();
    public string ProxyAddress { get; set; }
}