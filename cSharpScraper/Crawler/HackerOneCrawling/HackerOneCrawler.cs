using System.Net;
using cSharpScraper.Crawler.SubDomainCrawler;
using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Services;
using cSharpScraper.Storage.Models;
using Microsoft.Extensions.Logging;

namespace cSharpScraper.Crawler.HackerOneCrawling;

public class HackerOneCrawler
{
    private readonly WebsiteCrawler _webCrawler;
    private readonly ScopeStorage _scopeStorage;
    private readonly ILogger<HackerOneCrawler> _logger;
    private readonly SubdomainCrawler _subdomainCrawler;

    public HackerOneCrawler(WebsiteCrawler webCrawler, ScopeStorage scopeStorage, ILogger<HackerOneCrawler> logger, SubdomainCrawler subdomainCrawler)

    {
        _webCrawler = webCrawler;
        _scopeStorage = scopeStorage;
        _logger = logger;
        _subdomainCrawler = subdomainCrawler;
    }


    public async Task CrawlHackerOneCsvScopeAsync(string scopeFile)
    {
        var scopes = _scopeStorage.loadScopesFromCsvFile(scopeFile);

        for (var i = _scopeStorage.LoadLastCompletedIndex(); i < scopes.Count; i++)
        {
            _logger.LogInformation("Current scope: " + scopes[i].Identifier);
            _logger.LogInformation("Scope " + i + "/" + scopes.Count);

            if (!scopes[i].AssetType.Equals("URL") && !scopes[i].AssetType.Equals("WILDCARD") &&
                !scopes[i].AssetType.Equals("DOMAIN") && !scopes[i].AssetType.Equals("IP_ADDRESS"))
            {
                _scopeStorage.SaveLastCompletedScopeIndex(i + 1);
                _logger.LogInformation($"Asset is of type {scopes[i].AssetType}. Moving on to the next site.\n");
                continue;
            }

            if (!scopes[i].EligibleForSubmission)
            {
                _scopeStorage.SaveLastCompletedScopeIndex(i + 1);
                _logger.LogInformation("Site is not eligible for submission. Moving on to the next site\n");
                continue;
            }

            if (scopes[i].AssetType.Equals("IP_ADDRESS"))
            {
                if (await GetDomainNameFromIpAddressAsync(scopes, i)) continue;
            }

            var domainToScrape = scopes[i].Identifier;

            if (UrlUtility.HasWildcardThatIsNotInTheStart(scopes[i].Identifier))
            {
                //TODO: add support for wildcard subdomain enumeration
                _logger.LogInformation(
                    $"Url {domainToScrape} is a wildcard domain, but enumation is not supported yet\n");
                _scopeStorage.SaveLastCompletedScopeIndex(i + 1);
                continue;
            }

            if (UrlUtility.IsSubdomainWildcard(scopes[i].Identifier))
            {
                await _subdomainCrawler.CrawlAsync(domainToScrape);
            }
            else
            {
                _webCrawler.InitializeCrawler(domainToScrape);
                
                if (!await _webCrawler.CanBeCrawledAsync(domainToScrape))
                {
                    continue;
                }
                
                await _webCrawler.CrawlAsync(domainToScrape);
            }


            _scopeStorage.SaveLastCompletedScopeIndex(i + 1);
        }
    }

    private async Task<bool> GetDomainNameFromIpAddressAsync(List<StructuredScope> scopes, int i)
    {
        IPHostEntry hostEntry;
        try
        {
            hostEntry = await Dns.GetHostEntryAsync(scopes[i].Identifier);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Failed to perform dns lookupe. Exception: {ex.Message}\n");
            _scopeStorage.SaveLastCompletedScopeIndex(i + 1);
            return true;
        }

        scopes[i].Identifier = hostEntry.HostName;
        _logger.LogInformation($"Dns lookup gives domain: {scopes[i].Identifier}. Scanning that domain.");
        return false;
    }
}