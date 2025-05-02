namespace cSharpScraper.Crawler.WebCrawler.Models;

public class TempPage
{
    public string Url { get; set; }
    public string Content { get; set; }
    public IEnumerable<string> UrlsOnPage { get; set; }
    public DomainInfo ScopeDomain { get; set; }
}