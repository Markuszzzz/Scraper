using cSharpScraper.Reconnaisance.GoogleDorking;
using cSharpScraper.Reconnaisance.SiteArchive;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Nager.PublicSuffix.RuleProviders;

namespace cSharpScraper.Infrastructure;

public static class DependencyInjectionBuilder
{
    //todo se om jeg trenger å legge til crawlerSettings
    public static ServiceProvider SetupDependencyInjection(CrawlerSettings crawlerSettings)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "public_suffix_list.dat");
        var ruleProvider = new LocalFileRuleProvider(path);
        _ = ruleProvider.BuildAsync().Result;

        var serviceCollection = new ServiceCollection()
            .Configure<CrawlerSettings>(options =>
            {
                options.Url = crawlerSettings.Url;
                options.Scope = crawlerSettings.Scope;
                options.Eager = crawlerSettings.Eager;
                options.Headless = crawlerSettings.Headless;
                options.RequestDelay = crawlerSettings.RequestDelay;
                options.Headers = crawlerSettings.Headers;
                options.ProxyAddress = crawlerSettings.ProxyAddress;
            })
            .AddTransient<DomainDbContextFactory>()
            .AddTransient<HtmlDocument>()
            .AddTransient<DocParser>()
            .AddTransient<DomainParser>(_ => new DomainParser(ruleProvider))
            .AddGoogleDorker()
            .AddArchivedUrlCollector()
            .AddWebCrawler()
            .AddSubdomainDiscovery()
            .BuildServiceProvider();

        return serviceCollection;
    }   
}