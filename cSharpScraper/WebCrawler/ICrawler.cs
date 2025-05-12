using cSharpScraper.WebCrawler.CrawlTargeting;

namespace cSharpScraper.WebCrawler;

public interface ICrawler
{
    Task CrawlAsync(CrawlTarget crawlTarget);
}