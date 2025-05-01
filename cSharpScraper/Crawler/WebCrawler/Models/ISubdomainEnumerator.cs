namespace cSharpScraper.Crawler.WebCrawler.Models;

public interface ISubdomainEnumerator
{
    public Task<List<string>> FindSubdomainsAsync(string domain);
}