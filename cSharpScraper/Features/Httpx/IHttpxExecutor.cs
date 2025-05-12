namespace cSharpScraper.Features.Httpx;

public interface IHttpxExecutor
{
    Task<List<HttpxResult>> GetResponsiveUrlsAsync(IEnumerable<string> urls);
}