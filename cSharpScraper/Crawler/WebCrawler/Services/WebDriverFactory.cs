using cSharpScraper.Crawler.WebCrawler.Models;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class WebDriverFactory(ILogger<WebDriverFactory> logger, IOptions<CrawlerSettings> crawlerSettings)
{
    private readonly ILogger<WebDriverFactory> _logger = logger;
    private readonly IOptions<CrawlerSettings> _crawlerSettings = crawlerSettings;

    public WebDriverFactory(ILogger<WebDriverFactory> logger)
    {
        _logger = logger;
    }
    public IWebDriver CreateGoogleDorkingWebdriver(CrawlerSettings crawlerSettings)
    {
        _logger.LogInformation("Creating google driver for google dorking");
        ChromeOptions options = new ChromeOptions();
        var service = ChromeDriverService.CreateDefaultService();
        service.Port = new Random().Next(10000, 65535);
        var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(15));
        return driver;
    }
    
    public IWebDriver CreateScrapingWebdriver()
    {
        var maxRetries = 3;
        var retries = 0;
        ChromeDriver driver = null;

        while (retries < maxRetries)
        {
            try
            {
                _logger.LogInformation("Creating WebDriver for scraping...");
                ChromeOptions options = new ChromeOptions();
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
                options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--window-size=800,600");
                options.AddArgument($"--proxy-server=http://localhost:8081");

                options.AddArgument("--disable-gpu"); 

                
                var service = ChromeDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;
                service.Port = new Random().Next(10000, 65535);
                
                driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                break; 
            }
            catch (Exception ex)
            {
                Thread.Sleep(5000);
                throw;
            }
        }

        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        return driver;
    }

    // private void AddCustomHttpHeader(ChromeDriver driver, Dictionary<string, object> headers)
    // {
    //     driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
    //     driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
    //     {
    //         { "headers", headers }
    //     });
    //     
    //     driver.Navigate().GoToUrl("https://example.com");
    // }
}