using cSharpScraper.Crawler.HackerOneCrawling;
using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Models;
using cSharpScraper.Infrastructure;
using cSharpScraper.Reconnaisance.SubdomainDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper;

public static class CrawlCoordinator
{
    public static async Task StartAsync(CrawlerSettings crawlerSettings)
    {
        var serviceProvider = DepedencyInjectionBuilder.SetupDependencyInjection(crawlerSettings);
        var url = crawlerSettings.Url;

        if (crawlerSettings.Scope)
        {
            var scopeCrawler = serviceProvider.GetService<HackerOneCrawler>();
            await scopeCrawler.CrawlHackerOneCsvScopeAsync(crawlerSettings.Url);
        }
        else
        {
            var webCrawler = serviceProvider.GetService<WebsiteCrawler>();

            if (url.StartsWith("*"))
            {
                Console.WriteLine(
                    $"{url} allows for crawling subdomains. Enumerating and crawling its subdomains now");
                var subdomainEnumerator = serviceProvider.GetService<SublisterSubdomainEnumerator>();
                var subdomains = await subdomainEnumerator?.FindSubdomainsAsync(url);

                foreach (var subdomain in subdomains)
                {
                    webCrawler?.InitializeCrawler(subdomain);
                    webCrawler?.CrawlAsync(subdomain);
                }
            }
        }
    }
}