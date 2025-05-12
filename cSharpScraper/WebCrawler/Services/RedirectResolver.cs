namespace cSharpScraper.Crawler.WebCrawler;

public class RedirectResolver(HttpClient httpClient, ILogger<RedirectResolver> logger) : IRedirectResolver
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<RedirectResolver> _logger = logger;

    const int MaxRedirects = 10;

    public async Task<string?> ResolveFinalUrlAsync(string initialUrl)
    {
        var currentUrl = initialUrl;

        HttpResponseMessage res;
        try
        {
            res = await _httpClient.GetAsync(initialUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial request to {Url} failed", initialUrl);
            return null;
        }
         
        
        if (InitialRequestFails(res))
        {
            return null;
        }
        
        var redirects = 0;
        
        while (res.StatusCode.IsRedirect())
        {
            if (redirects >= MaxRedirects)
            {
                _logger.LogWarning("Max redirect limit ({Limit}) reached for {Url}", MaxRedirects, currentUrl);
                return null;
            }

            if (res.Headers.Location == null)
            {
                _logger.LogWarning("Redirect status received but no Location header for {Url}", currentUrl);
                return currentUrl;
            }
            
            var nextUrl = res.Headers.Location.IsAbsoluteUri
                ? res.Headers.Location.AbsoluteUri
                : new Uri(new Uri(currentUrl), res.Headers.Location).ToString();            
            
            _logger.LogInformation("Redirecting from {FromUrl} to {ToUrl}", currentUrl, nextUrl);
            
            try
            {
                res = await _httpClient.GetAsync(nextUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redirect failed for URL: {NextUrl}", nextUrl);
                return currentUrl;
            }

            redirects++;
            currentUrl = nextUrl;
        }

        return currentUrl;
    }

    private static bool InitialRequestFails(HttpResponseMessage res)
    {
        return !(res.IsSuccessStatusCode || res.StatusCode.IsRedirect());
    }
}