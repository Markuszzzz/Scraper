using System.Diagnostics;
using cSharpScraper.Crawler.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class PageDownloader
{
    private readonly ILogger<PageDownloader> _logger;

    public PageDownloader(ILogger<PageDownloader> logger)
    {
        _logger = logger;
    }

    public string? DownloadPage(IWebDriver webDriver, string url)
    {
        _logger.LogDebug("Crawling: " + url);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            webDriver.Navigate().GoToUrl(url);
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