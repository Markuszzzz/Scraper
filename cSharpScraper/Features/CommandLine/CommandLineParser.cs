using System.CommandLine;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.WebsiteCrawler.Models;

namespace cSharpScraper.Features.CommandLine;

public static class CommandLineParser
{
    public static RootCommand SetupCommandLineParser()
    {
        var targetArgument = new Argument<string>(
            "url",
            description:
            "The full URL to crawl (e.g. 'https://www.example.com'). If --crawl-subdomains is enabled, provide a registrable domain (e.g. 'example.com').");
        
        var crawlSubdomainsOption = new Option<bool>(
            "--crawl-subdomains",
            description: "Allow crawling of subdomains");

        var eagerOption = new Option<bool>(
            "--eager",
            description: "Enable eager initialization");

        var headlessOption = new Option<bool>(
            "--headless",
            description: "Enable headless mode");

        var requestDelayOption = new Option<int>(
            "--request-delay",
            description: "Delay between requests");

        var proxyAddressOption = new Option<string>(
            "--proxy-address",
            description: "Address of proxy server");

        var headersOption = new Option<Dictionary<string, object>>(
            "--header",
            description: "Custom HTTP headers in the format Key=Value, separated by commas",
            parseArgument: result =>
            {
                var headers = new Dictionary<string, object>();
                foreach (var item in result.Tokens)
                {
                    var parts = item.Value.Split('=');
                    if (parts.Length is 2)
                    {
                        headers[parts[0].Trim()] = parts[1].Trim();
                    }
                    else
                    {
                        result.ErrorMessage = $"Invalid header format: '{item.Value}'. Expected format is Key=Value.";
                    }
                }

                return headers;
            })
        {
            Arity = ArgumentArity.ZeroOrMore 
        };

        var rootCommand = new RootCommand("Crawler CLI Application")
        {
            targetArgument,
            crawlSubdomainsOption,
            eagerOption,
            headlessOption,
            requestDelayOption,
            proxyAddressOption,
            headersOption
        };

        rootCommand.SetHandler(async (target, eager, crawlSubdomains, headless, requestDelay, proxyAddress, header) =>
            {
                var domainParser = await DomainParserProvider.GetAsync();
                var domainInfo = domainParser.Parse(target);
                if (domainInfo is null)
                {
                    Console.WriteLine("Could not parse domain information for target {Target}", target);
                    return;
                }
                if(!ValidateTarget(domainInfo, crawlSubdomains))
                {
                    Console.WriteLine("Invalid target for the specified crawl mode.");
                    return;
                }
                var crawlerSettings = new CrawlerSettings
                {
                    Target = target,
                    Eager = eager,
                    CrawlSubdomains = crawlSubdomains,
                    Headless = headless,
                    RequestDelay = requestDelay,
                    ProxyAddress = proxyAddress,
                    Headers = header
                };
                await CrawlOrchestrator.StartAsync(crawlerSettings);
                
            }, targetArgument, eagerOption, crawlSubdomainsOption, headlessOption, requestDelayOption, proxyAddressOption,
            headersOption);

        return rootCommand;
    }
    
    
    private static bool ValidateTarget(DomainInfo domain, bool crawlSubdomains)
    {
        if (!crawlSubdomains && domain.Subdomain is null)
        {
            Console.WriteLine("For single-domain crawling, use a FQDN like 'www.example.com'");
            return false;
        }

        if (crawlSubdomains && domain.Subdomain is not null)
        {
            Console.WriteLine("For subdomain discovery, use a registrable domain like 'example.com', not 'www.example.com'");
            return false;
        }

        return true;
    }
}