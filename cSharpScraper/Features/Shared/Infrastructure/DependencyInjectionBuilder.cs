using cSharpScraper.Features.ArchivedUrlDiscovery;
using cSharpScraper.Features.GoogleDorking;
using cSharpScraper.Features.Httpx;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.SubDomainCrawler;
using cSharpScraper.Features.WebsiteCrawler;
using cSharpScraper.Features.WebsiteCrawler.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace cSharpScraper.Features.Shared.Infrastructure;

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
                config.SetMinimumLevel(LogLevel.Information);
                config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                config.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
                config.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Warning);
                config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Critical);
                config.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();

            })
            .AddTransient<DomainDbContextFactory>()
            .AddTransient<HtmlDocument>()
            .AddSingleton<DomainParser>(_ => domainParser)
            .AddGoogleDorker()
            .AddHttpx()
            .AddArchivedUrlDiscovery()
            .AddSubdomainCrawler()
            .AddWebCrawler();

        return serviceCollection.BuildServiceProvider();
    }   
}