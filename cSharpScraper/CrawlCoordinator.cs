using cSharpScraper.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper;

public static class CrawlCoordinator
{
    public static async Task StartAsync(CrawlerSettings crawlerSettings)
    {
        var serviceProvider = await DependencyInjectionBuilder.BuildServiceProviderAsync(crawlerSettings);
        
        var crawler = serviceProvider.GetRequiredService<ICrawler>();
        var crawlTargetLogger = serviceProvider.GetRequiredService<ILogger<CrawlTarget>>();
        
        var crawlTarget = crawlerSettings.CrawlSubdomains 
            ? await CrawlTargetFactory.FromRegistrableDomainAsync(crawlerSettings.Target, crawlTargetLogger) 
            :  await CrawlTargetFactory.FromFullUrlAsync(crawlerSettings.Target, crawlTargetLogger);
        
        if(crawlTarget is null)
        {
            Console.WriteLine("Failed to create crawl target from URL.");
            return;
        }
        
        await crawler.CrawlAsync(crawlTarget);
    }
}

