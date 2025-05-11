using cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;
using cSharpScraper.Reconnaissance.GoogleDorking;
using cSharpScraper.Reconnaissance.Httpx;
using cSharpScraper.Reconnaissance.SubdomainDiscovery;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace cSharpScraper.Infrastructure;

public static class DependencyInjectionBuilder
{
    public static async Task<ServiceProvider> BuildServiceProviderAsync(CrawlerSettings crawlerSettings)
    {
        DomainParser domainParser = await DomainParserProvider.GetAsync();

        var serviceCollection = new ServiceCollection()
            .Configure<CrawlerSettings>(options =>
            {
                options.Target = crawlerSettings.Target;
                options.Eager = crawlerSettings.Eager;
                options.CrawlSubdomains = crawlerSettings.CrawlSubdomains;
                options.Headless = crawlerSettings.Headless;
                options.RequestDelay = crawlerSettings.RequestDelay;
                options.Headers = crawlerSettings.Headers;
                options.ProxyAddress = crawlerSettings.ProxyAddress;
            })
            .AddLogging(config =>
            {
                config.AddConsole(options => { options.FormatterName = nameof(CustomConsoleFormatter); });
                config.SetMinimumLevel(LogLevel.Debug);
                config.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
                config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command",
                    LogLevel.Warning);
            })
            .AddTransient<DomainDbContextFactory>()
            .AddTransient<HtmlDocument>()
            .AddTransient<DocParser>()
            .AddSingleton<DomainParser>(_ => domainParser)
            .AddGoogleDorker()
            .AddHttpx()
            .AddArchivedUrlCollector()
            .AddSubdomainDiscovery()
            .AddWebCrawler();

        return serviceCollection.BuildServiceProvider();
    }   
}