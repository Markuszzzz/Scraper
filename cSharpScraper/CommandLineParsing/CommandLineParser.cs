using System.CommandLine;
using cSharpScraper.Crawler.WebCrawler.Models;

namespace cSharpScraper.CommandLineParsing;

public static class CommandLineParser
{
    public static RootCommand SetupCommandLineParser()
    {
        var urlArgument = new Argument<string>(
            "url",
            description: "The URL to crawl or the HackerOne CSV scope file");

        var scopeOption = new Option<bool>(
            "--scope",
            description: "Crawl using scope logic");

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
                    if (parts.Length == 2)
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
            urlArgument,
            scopeOption,
            eagerOption,
            headlessOption,
            requestDelayOption,
            proxyAddressOption,
            headersOption
        };

        rootCommand.SetHandler(async (url, eager, scope, headless, requestDelay, proxyAddress, header) =>
            {
                var crawlerSettings = new CrawlerSettings
                {
                    Url = url,
                    Eager = eager,
                    Scope = scope,
                    Headless = headless,
                    RequestDelay = requestDelay,
                    ProxyAddress = proxyAddress,
                    Headers = header
                };
                await CrawlCoordinator.StartAsync(crawlerSettings);
            }, urlArgument, eagerOption, scopeOption, headlessOption, requestDelayOption, proxyAddressOption,
            headersOption);

        return rootCommand;
    }
}