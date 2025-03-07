using cSharpScraper.Crawler;
using cSharpScraper.Crawler.HackerOneCrawling;
using cSharpScraper.Crawler.Services;
using cSharpScraper.Crawler.SubDomainCrawler;
using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Models;
using cSharpScraper.Crawler.WebCrawler.Services;
using cSharpScraper.Reconnaisance.SiteArchive;
using cSharpScraper.Reconnaisance.SubdomainDiscovery;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace cSharpScraper.Infrastructure;

public static class DepedencyInjectionBuilder
{
    public static ServiceProvider SetupDependencyInjection(CrawlerSettings crawlerSettings)
    {
        var ruleProvider = new LocalFileRuleProvider("/Users/mhellestveit/Documents/BugBounty/1SeleniumScrapyScraper/cSharpSCraperSolution/cSharpScraperTests/public_suffix_list.dat");
        var _ =ruleProvider.BuildAsync().Result;

        var serviceCollection = new ServiceCollection()
            .AddDbContext<DomainDbContext>(options => 
                options.UseMySql("server=localhost;port=3307;database=scraper;user=root;password=Passord4321Pass", new MySqlServerVersion(new Version(9,1,0)))
                    .LogTo(Console.WriteLine, LogLevel.Critical))
            
            .AddLogging(config =>
            {
                config.AddConsole(options => { options.FormatterName = nameof(CustomConsoleFormatter); });
                config.SetMinimumLevel(LogLevel.Critical);
                config.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
                config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Suppress SQL logs
            })
            .AddSingleton(x => new ScopeStorage(crawlerSettings.Url))
            .AddTransient<DomainDbContext>()
            .AddTransient(provider => new PageStorage(provider.GetService<ILogger<PageStorage>>(),provider.GetService<DomainDbContext>()))
            .AddTransient(_ => new WebDriverFactory(_.GetService<ILogger<WebDriverFactory>>()))
            .AddTransient<GoogleDorker>()
            .AddSingleton(_ => new HttpClientFactory())
            .AddTransient(_ => new GoogleDorker(_.GetService<WebDriverFactory>()))
            .AddTransient<DomainDbContextFactory>()
            .AddTransient<PageStorageFactory>()
            .AddSingleton(provider => new PageDownloader(provider.GetService<ILogger<PageDownloader>>()))
            .AddTransient(provider => new DomainService(provider.GetService<DomainParser>()))
            .AddTransient(provider => new DocParser(new HtmlDocument()))
            .AddTransient(provider =>
            {
                var httpClient = provider.GetService<HttpClientFactory>().GetHttpClient();
                return new WebsiteCrawler(
                    provider.GetService<PageStorage>(), new DomainParser(ruleProvider), provider.GetService<ILogger<WebsiteCrawler>>(), 
                   httpClient, crawlerSettings, provider.GetService<GoogleDorker>(), provider.GetService<ArchivedUrlCollector>(), provider.GetService<PageStorageFactory>(), provider.GetService<WebDriverFactory>()
                        , provider.GetService<PageDownloader>(), provider.GetService<DomainService>(), provider.GetService<DocParser>(), provider.GetService<PageBatcher>());
            }).AddSingleton(x => new SublisterSubdomainEnumerator(x.GetService<ILogger<SublisterSubdomainEnumerator>>(), new DomainParser(ruleProvider)))
            .AddSingleton<ISubdomainEnumerator>(x => x.GetService<SublisterSubdomainEnumerator>())
            .AddSingleton(provider => new SubdomainCrawler(provider.GetService<WebsiteCrawler>(), 
                provider.GetService<ILogger<SubdomainCrawler>>(), 
                provider.GetService<ISubdomainEnumerator>(), 
                provider.GetService<SubdomainScraperStorage>()))
            .AddSingleton(provider =>
            {
                return new HackerOneCrawler(provider.GetService<WebsiteCrawler>(), provider.GetService<ScopeStorage>(),
                    provider.GetService<ILogger<HackerOneCrawler>>(), provider.GetService<SubdomainCrawler>());
            })
            
            
                .BuildServiceProvider();

        return serviceCollection;
    }   
}