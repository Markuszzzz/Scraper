using cSharpScraper.Features.WebsiteCrawler.Models;

namespace cSharpScraper.Features.Shared.Models;

public interface ICrawler
{
    Task CrawlAsync(CrawlTarget crawlTarget);
}