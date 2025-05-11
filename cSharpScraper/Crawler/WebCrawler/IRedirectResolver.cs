namespace cSharpScraper.Crawler.WebCrawler;

public interface IRedirectResolver
{
    Task<string?> ResolveFinalUrlAsync(string redirectedUrl);
}