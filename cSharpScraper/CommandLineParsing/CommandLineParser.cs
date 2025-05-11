using System.CommandLine;
using Nager.PublicSuffix.RuleProviders;

namespace cSharpScraper.CommandLineParsing;

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
                await CrawlCoordinator.StartAsync(crawlerSettings);
            }, targetArgument, eagerOption, crawlSubdomainsOption, headlessOption, requestDelayOption, proxyAddressOption,
            headersOption);

        return rootCommand;
    }
}