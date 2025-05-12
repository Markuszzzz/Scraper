namespace cSharpScraper.Features.WebsiteCrawler.Services;

public interface IRedirectResolver
{
    Task<string?> ResolveFinalUrlAsync(string redirectedUrl);
}