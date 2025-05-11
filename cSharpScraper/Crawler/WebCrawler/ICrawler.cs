namespace cSharpScraper.Crawler.WebCrawler;

public interface ICrawler
{
    Task CrawlAsync(CrawlTarget crawlTarget);
}