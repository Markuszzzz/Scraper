using System.Collections.Concurrent;
using cSharpScraper.Features.ArchivedUrlDiscovery;
using cSharpScraper.Features.GoogleDorking;
using cSharpScraper.Features.Shared.Models;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.WebsiteCrawler.Models;
using cSharpScraper.Features.WebsiteCrawler.Persistence;
using cSharpScraper.Features.WebsiteCrawler.Services;
using Microsoft.Extensions.Options;

namespace cSharpScraper.Features.WebsiteCrawler;

public class WebCrawler(
    PageStorage pageStorage,
    DomainParser domainParser,
    ILogger<WebCrawler> logger,
    HttpClient httpClient,
    IOptions<CrawlerSettings> crawlerSettings,
    GoogleDorker googleDorker,
    IArchivedUrlDiscoveryService archivedUrlDiscoveryService,
    PageStorageFactory pageStorageFactory,
    WebDriverFactory webDriverFactory,
    PageDownloader pageDownloader,
    DomainService domainService,
    DocParser docParser,
    PageBatcher pageBatcher,
    IRedirectResolver redirectResolver) : ICrawler
{
    private readonly PageStorage _pageStorage = pageStorage;
    private readonly DomainParser _domainParser = domainParser;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOptions<CrawlerSettings> _crawlerSettings = crawlerSettings;
    private readonly GoogleDorker _googleDorker = googleDorker;
    private readonly IArchivedUrlDiscoveryService _archivedUrlDiscoveryService = archivedUrlDiscoveryService;
    private readonly PageStorageFactory _pageStorageFactory = pageStorageFactory;
    private readonly WebDriverFactory _webDriverFactory = webDriverFactory;
    private readonly PageDownloader _pageDownloader = pageDownloader;
    private readonly DomainService _domainService = domainService;
    private readonly DocParser _docParser = docParser;
    private readonly PageBatcher _pageBatcher = pageBatcher;
    private readonly IRedirectResolver _redirectResolver = redirectResolver;
    private readonly ILogger<WebCrawler> _logger = logger;
    private Stopwatch ConcurrencyStopwatch { get; } = new();
    private Stopwatch DownloadPageStopwatch { get; } = new();
    
    
    public async Task CrawlAsync(CrawlTarget crawlTarget)
    {
        var targetUrl = crawlTarget.Url!;
        
        _pageStorage.PersistDomain(crawlTarget.DomainInfo);
        
        if (!await CanBeCrawledAsync(targetUrl, crawlTarget.DomainInfo))
            return;
        
        if (IsFirstCrawl(crawlTarget))
        {
            _pageStorage.SavePageToBeCrawled(crawlTarget);

             var dorkedUrls = _googleDorker.DorkUrls(crawlTarget.DomainInfo);
             _pageStorage.SaveUrlsToBeScraped(dorkedUrls, crawlTarget.DomainInfo);

            var archivedUrls = await _archivedUrlDiscoveryService.DiscoverArchivedUrls(crawlTarget);
            _pageStorage.SaveUrlsToBeScraped(archivedUrls, crawlTarget.DomainInfo);
        }

        await CrawlConcurrentlyAsync(crawlTarget);

    }

    private async Task CrawlConcurrentlyAsync(CrawlTarget crawlTarget)
    {
        try
        {
            var driverPool = new BlockingCollection<IWebDriver>(8); 

            for (var i = 0; i < 1; i++)
            {
                driverPool.Add(_webDriverFactory.CreateScrapingWebdriver());
            }

            var temporaryPages = new BlockingCollection<Page>(1000);

            var databaseTask = Task.Run(() =>
            {
                _pageBatcher.PersistData(temporaryPages, crawlTarget.DomainInfo);
            });
            
            PageStorage pageStorage = _pageStorageFactory.CreatePageStorage();
            IEnumerable<Page> pages = pageStorage.GetNextPagesToScrape(crawlTarget.DomainInfo);

            while (pages.Any())
            {
                ConcurrencyStopwatch.Start();
                var maxDegreeOfParallelism = pages.Count() <= 8 ? pages.Count() : 8;
                
                await Parallel.ForEachAsync(pages,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                     async(page, _) => 
                    {
                        DownloadPageStopwatch.Start();
                        var driver = driverPool.Take();
                        try
                        {
                            var urlToCrawl = page.Url;

                            var pageContent = await _pageDownloader.DownloadPageAsync(driver, urlToCrawl);
                            
                            if (pageContent is null)
                                return;
                            
                            page.Content = pageContent;

                            page.UrlsOnPage = GetUrlsOnPage(page.Url, page.Content, crawlTarget.DomainInfo).ToList();
                            temporaryPages.Add(page);
                            
                            _logger.LogInformation("temporary pages count" + temporaryPages.Count);
                            DownloadPageStopwatch.Stop();
                            if (DownloadPageStopwatch.ElapsedMilliseconds < 1000)
                            {
                                Thread.Sleep(1000-(int)DownloadPageStopwatch.ElapsedMilliseconds);
                            }
                        }
                        catch (WebDriverException ex)
                        {
                            if (ex.Message.Contains("timed out after") && ex.Message.Contains("localhost"))
                            {
                                driver.Dispose();
                                driver = _webDriverFactory.CreateScrapingWebdriver();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Errors: " + ex.Message);
                        }
                        finally
                        {
                            driverPool.Add(driver);
                        }
                    });
                
                ConcurrencyStopwatch.Stop();
                
                _logger.LogDebug("Downloaded " + temporaryPages.Count + " in " + ConcurrencyStopwatch.Elapsed.Seconds + " seconds");

                pages = pageStorage.GetNextPagesToScrape(crawlTarget.DomainInfo);
            }
            
            temporaryPages.CompleteAdding();
            await databaseTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during crawling: {ex.Message}");
        }
        
        _logger.LogInformation("Completed crawling " +  "\n");

    }

    private IEnumerable<string> GetUrlsOnPage(string url, string pageContent, DomainInfo domainInfo)
    {
        var allLinksOnPage =  _docParser.GetLinksFromPageSource(pageContent);
        var linksToCrawl = UrlUtility.FilterUrlsOnPage(allLinksOnPage, url).ToList();
        linksToCrawl = linksToCrawl.Where(x => _domainService.IsInScope(x, domainInfo)).ToList();

        _logger.LogDebug($"Currently on page {url}\n");

        return linksToCrawl;
    }


   

    private async Task<bool> CanBeCrawledAsync(string url, DomainInfo domainInfo)
    {
        HttpResponseMessage res;
        try
        {
            res = await _httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Can not be crawled");
            _logger.LogInformation("Exception: " + ex.Message);
            return false;
        }

        if (res.IsSuccessStatusCode)
        {
            return true;
        }

        if (!res.StatusCode.IsRedirect())
        {
            _logger.LogInformation($"Status code: {(int)res.StatusCode} {res.StatusCode}. Can not crawl.");
            return false;
        }

        var newUrl = await _redirectResolver.ResolveFinalUrlAsync(url);
        
        if (string.IsNullOrEmpty(newUrl))
        {
            _logger.LogWarning("Unable to resolve final URL from {OriginalUrl}. Crawling aborted.", url);
            return false;
        }

        if (_domainService.ShouldHaveWwwSubdomain(newUrl, domainInfo))
        {
            DomainInfo? newParsedDomain = TryParseDomain(newUrl);
            if (newParsedDomain == null)
                return false;
            
            domainInfo = newParsedDomain;
        }

        return _domainService.IsInScope(url, domainInfo) && _pageStorage.UrlHasBeenCrawled(url);
    }
    
    private DomainInfo? TryParseDomain(string url)
    {
        try
        {
            var info = _domainParser.Parse(url);
            if (info == null)
            {
                _logger.LogWarning("Domain parsing returned null for: {Domain}", url);
            }
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Domain parsing threw exception for: {Domain}", url);
            return null;
        }
    }
    
    private bool IsFirstCrawl(CrawlTarget crawlTarget)
    {
        return !_pageStorage.HasMoreUrlsToScrape(crawlTarget.DomainInfo);
    }
}