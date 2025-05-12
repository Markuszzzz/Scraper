using cSharpScraper.Features.Shared.Infrastructure;
using cSharpScraper.Features.Shared.Models;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.WebsiteCrawler.Models;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper;

public static class CrawlOrchestrator
{
    public static async Task StartAsync(CrawlerSettings crawlerSettings)
    {
        var serviceProvider = await DependencyInjectionBuilder.BuildServiceProviderAsync(crawlerSettings);

        var crawlTarget = await CrawlTargetFactory.CreateAsync(crawlerSettings.Target, crawlerSettings.CrawlSubdomains, serviceProvider.GetRequiredService<ILogger<CrawlTarget>>());
        if (crawlTarget == null)
        {
            Console.WriteLine("Failed to create crawl target from target {Target}", crawlerSettings.Target);
            return;
        }

        var crawler = serviceProvider.GetRequiredService<ICrawler>();

        await crawler.CrawlAsync(crawlTarget);
    }
}

