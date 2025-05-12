namespace cSharpScraper.Features.WebsiteCrawler.Services;

public class PageDownloader(ILogger<PageDownloader> logger)
{
    private readonly ILogger<PageDownloader> _logger = logger;

    public async Task<string?> DownloadPageAsync(IWebDriver webDriver, string url)
    {
        _logger.LogDebug("Crawling: " + url);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await webDriver.Navigate().GoToUrlAsync(url);
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogError($"Error navigating to the URL: {url}");
            _logger.LogError($"Exception message: {ex.Message}");
            _logger.LogError("Moves on to next url");
            return null;
        }
        catch (WebDriverException ex)
        {
            if (ex.Message.Contains("net::ERR_NAME_NOT_RESOLVED"))
            {
                _logger.LogError($"Error navigating to the URL: {url}");
                _logger.LogError($"Exception message: {ex.Message}");
                _logger.LogError("Moves on to next url");
                return null;
            }

            if (ex.Message.Contains("timed out after") && ex.Message.Contains("localhost"))
            {
                _logger.LogError($"Error navigating to the URL: {url}. timed out");
                _logger.LogError($"Exception message: {ex.Message}");
                _logger.LogError("Creates a new instance of WebCrawler");

                throw;
            }

            _logger.LogError($"Error navigating to the URL: {url}");
            _logger.LogError($"Exception message: {ex.Message}");
            throw;
        }

        stopwatch.Stop();
        _logger.LogInformation($"Time to crawl {url}: {stopwatch.ElapsedMilliseconds}");

        try
        {
            return webDriver.PageSource;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error RENDERING to the URL: {url}");
            _logger.LogError($"Exception message: {ex.Message}");
            _logger.LogError("Moves on to next url");
            return null;
        }
    }
}