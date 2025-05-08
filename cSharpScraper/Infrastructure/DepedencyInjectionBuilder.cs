using cSharpScraper.Crawler.HackerOneCrawling;
using cSharpScraper.Crawler.SubDomainCrawler;
using cSharpScraper.Reconnaisance.GoogleDorking;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Nager.PublicSuffix.RuleProviders;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace cSharpScraper.Infrastructure;

public static class DepedencyInjectionBuilder
{
    public static ServiceProvider SetupDependencyInjection(CrawlerSettings crawlerSettings)
    {
        var ruleProvider = new LocalFileRuleProvider("/Users/mhellestveit/Documents/BugBounty/1SeleniumScrapyScraper/cSharpSCraperSolution/cSharpScraperTests/public_suffix_list.dat");
        _ = ruleProvider.BuildAsync().Result;

        var serviceCollection = new ServiceCollection()
            //todo: Use AZ key vault to store credentials
            .AddDbContext<DomainDbContext>(options => 
                options.UseMySql("server=localhost;port=3307;database=scraper;user=root;password=xxxx", new MySqlServerVersion(new Version(9,1,0)))
                    .LogTo(Console.WriteLine, LogLevel.Critical))
            .AddLogging(config =>
            {
                config.AddConsole(options => { options.FormatterName = nameof(CustomConsoleFormatter); });
                config.SetMinimumLevel(LogLevel.Critical);
                config.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
                config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Suppress SQL logs
            })
            .AddHttpClient<IUrlAvailabilityChecker, UrlAvailabilityChecker>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            }).Services
            .AddHttpClient<WebsiteCrawler, WebsiteCrawler>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:8080"),
                UseProxy = true,
                AllowAutoRedirect = false
            }).Services
            .AddSingleton<CrawlerSettings>(x => crawlerSettings)
            .AddSingleton<ScopeStorage>()
            .AddTransient<DomainDbContext>()
            .AddTransient<PageStorage>()
            .AddTransient<WebDriverFactory>()
            .AddTransient(provider => new GoogleDorker(provider.GetService<WebDriverFactory>()?.CreateGoogleDorkingWebdriver(null)))
            .AddTransient<DomainDbContextFactory>()
            .AddTransient<PageStorageFactory>()
            .AddSingleton<PageDownloader>()
            .AddTransient<DomainParser>()
            .AddTransient<DomainService>()
            .AddTransient<HtmlDocument>()
            .AddTransient<DocParser>()
            .AddTransient<ISubdomainEnumerator, SublisterSubdomainEnumerator>()
            .AddTransient<DomainParser>(_ => new DomainParser(ruleProvider))
            .AddTransient<WebsiteCrawler>()
            .AddSingleton<SublisterSubdomainEnumerator>()
            .AddSingleton<SubdomainCrawlerDecorator>()
            .AddSingleton<HackerOneCrawler>()
            .BuildServiceProvider();

        return serviceCollection;
    }   
}