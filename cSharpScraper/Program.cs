using System.CommandLine;
using cSharpScraper.Crawler.HackerOneCrawling;
using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Models;
using cSharpScraper.Infrastructure;
using cSharpScraper.Reconnaisance.SubdomainDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        await CommandLineParsing.CommandLineParser.SetupCommandLineParser().InvokeAsync(args);
    }
    
    public static async Task Start(CrawlerSettings crawlerSettings)
    {
        var serviceProvider = DepedencyInjectionBuilder.SetupDependencyInjection(crawlerSettings);
        var url = crawlerSettings.Url;

        if (crawlerSettings.Scope)
        {
            var scopeCrawler = serviceProvider.GetService<HackerOneCrawler>();
            await scopeCrawler.CrawlHackerOneCsvScope(crawlerSettings.Url);

        }else
        {
            var webCrawler = serviceProvider.GetService<WebsiteCrawler>();

            if (url.StartsWith("*"))
            {
                Console.WriteLine(
                    $"{url} allows for crawling subdomains. Enumerating and crawling its subdomains now");
                var subdomainEnumerator = serviceProvider.GetService<SublisterSubdomainEnumerator>();
                var subdomains = await subdomainEnumerator?.FindSubdomains(url);

                foreach (var subdomain in subdomains)
                {
                    webCrawler?.InitializeCrawler(subdomain);
                    webCrawler?.Crawl(subdomain);
                }
            }
        }
    }
    
    
}