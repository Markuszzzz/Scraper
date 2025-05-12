using cSharpScraper.WebCrawler.Models;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class WebDriverFactory(ILogger<WebDriverFactory> logger, IOptions<CrawlerSettings> crawlerSettings)
{
    private readonly ILogger<WebDriverFactory> _logger = logger;
    private readonly IOptions<CrawlerSettings> _crawlerSettings = crawlerSettings;


    public IWebDriver CreateGoogleDorkingWebdriver()
    {
        _logger.LogInformation("Creating google driver for google dorking");

        new DriverManager().SetUpDriver(new ChromeConfig());

        ChromeOptions options = new ChromeOptions();
        var service = ChromeDriverService.CreateDefaultService();
        service.Port = new Random().Next(10000, 65535);
        var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(15));

        Task.Delay(2000);

        return driver;
    }

    public IWebDriver CreateScrapingWebdriver()
    {
        _logger.LogInformation("Creating WebDriver for scraping...");
        ChromeOptions options = new ChromeOptions();
        options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
        options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--window-size=800,600");

        if (!string.IsNullOrWhiteSpace(_crawlerSettings.Value.ProxyAddress))
        {
            options.AddArgument($"--proxy-server={_crawlerSettings.Value.ProxyAddress}");
        }
        
        options.AddArgument("--disable-gpu");

        var service = ChromeDriverService.CreateDefaultService();
        service.SuppressInitialDiagnosticInformation = true;
        service.HideCommandPromptWindow = true;
        service.Port = new Random().Next(10000, 65535);

        var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));

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