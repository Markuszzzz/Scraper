using System.Collections.Concurrent;
using cSharpScraper.Reconnaisance.GoogleDorking;
using cSharpScraper.Reconnaisance.SiteArchive;

namespace cSharpScraper.Crawler.WebCrawler;

public class WebsiteCrawler
{
    private readonly PageStorage _pageStorage;
    private readonly DomainParser _domainParser;
    private readonly ILogger<WebsiteCrawler> _logger;
    private readonly HttpClient _httpClient;
    private readonly CrawlerSettings _crawlerSettings;
    private readonly GoogleDorker _googleDorker;
    private readonly ArchivedUrlCollector _archivedUrlCollector;
    private readonly PageStorageFactory _pageStorageFactory;
    private readonly WebDriverFactory _webDriverFactory;
    private readonly PageDownloader _pageDownloader;
    private readonly DomainService _domainService;
    private readonly DocParser _docParser;
    private readonly PageBatcher _pageBatcher;
    private DomainInfo? DomainInfo { get; set; }

    private Stopwatch ConcurrencyStopwatch { get; } = new();
    private Stopwatch DownloadPageStopwatch { get; } = new();


    public WebsiteCrawler(PageStorage pageStorage, 
        DomainParser domainParser,
        ILogger<WebsiteCrawler> logger, HttpClient httpClient, CrawlerSettings crawlerSettings,
        GoogleDorker googleDorker, ArchivedUrlCollector archivedUrlCollector, PageStorageFactory pageStorageFactory,
        WebDriverFactory webDriverFactory, PageDownloader pageDownloader, DomainService domainService, DocParser docParser, PageBatcher pageBatcher)
    {
        _pageStorage = pageStorage;
        _domainParser = domainParser;
        _logger = logger;
        _httpClient = httpClient;
        _crawlerSettings = crawlerSettings;
        _googleDorker = googleDorker;
        _archivedUrlCollector = archivedUrlCollector;
        _pageStorageFactory = pageStorageFactory;
        _webDriverFactory = webDriverFactory;
        _pageDownloader = pageDownloader;
        _domainService = domainService;
        _docParser = docParser;
        _pageBatcher = pageBatcher;
    }

    public void InitializeCrawler(string url)
    {
        url = url.StartsWith("*") ? UrlUtility.GetSecondLevelDomainFromWildcardUrl(url) : url;
        DomainInfo = _domainParser.Parse(url);
        _pageStorage.PersistDomain(DomainInfo);
    }
    
    public async Task CrawlAsync(string pageToCrawl)
    {
        if (!_pageStorage.HasMoreUrlsToScrape(DomainInfo))
        {
            _pageStorage.SavePageToBeCrawled("https://" + DomainInfo.FullyQualifiedDomainName + "/", DomainInfo);

            var dorkedUrls = _googleDorker.DorkUrls(DomainInfo);
            _pageStorage.SaveUrlsToBeScraped(dorkedUrls, DomainInfo);

            var archivedUrls = _archivedUrlCollector.GetArchivedUrlsForDomain(pageToCrawl)
                .Where(x => !string.IsNullOrWhiteSpace(x));
            archivedUrls = FilterUrlsOnPage(archivedUrls, pageToCrawl).Where(x => _domainService.IsInScope(x, DomainInfo));

            archivedUrls = _archivedUrlCollector.FilterOutDeadUrls(archivedUrls, DomainInfo);
            _pageStorage.SaveUrlsToBeScraped(archivedUrls, DomainInfo);
        }

        await CrawlConcurrentlyAsync();

        _logger.LogInformation("Completed crawling " + DomainInfo.FullyQualifiedDomainName + "\n");
    }

    private async Task CrawlConcurrentlyAsync()
    {
        try
        {
            var driverPool = new BlockingCollection<IWebDriver>(8); 

            for (var i = 0; i < 8; i++)
            {
                driverPool.Add(_webDriverFactory.CreateScrapingWebdriver(_crawlerSettings));
            }

            var temporaryPages = new BlockingCollection<Page>(1000);

            var databaseTask = Task.Run(() =>
            {
                _pageBatcher.PersistData(temporaryPages);
            });
            
            PageStorage pageStorage = _pageStorageFactory.CreatePageStorage();
            IEnumerable<Page> pages = pageStorage.GetNextPagesToScrape(DomainInfo);

            while (pages.Any())
            {
                ConcurrencyStopwatch.Start();
                var maxDegreeOfParallelism = pages.Count() <= 8 ? pages.Count() : 8;
                
                Parallel.ForEach(pages,
                    new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                     (page, _) => 
                    {
                        DownloadPageStopwatch.Start();
                        var driver = driverPool.Take();
                        try
                        {
                            var urlToCrawl = page.Url;

                            var pageContent = _pageDownloader.DownloadPage(driver, urlToCrawl);
                            
                            if (pageContent is null)
                                return;
                            
                            page.Content = pageContent;

                            page.UrlsOnPage = GetUrlsOnPage(page.Url, page.Content).ToList();
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
                                driver = _webDriverFactory.CreateScrapingWebdriver(_crawlerSettings);
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

                pages = pageStorage.GetNextPagesToScrape(DomainInfo);
            }
            
            temporaryPages.CompleteAdding();
            await databaseTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during crawling: {ex.Message}");
        }
    }

    private IEnumerable<string> GetUrlsOnPage(string url, string pageContent)
    {
        var allLinksOnPage =  _docParser.GetLinksFromPageSource(pageContent);
        var linksToCrawl = FilterUrlsOnPage(allLinksOnPage, url).ToList();
        linksToCrawl = linksToCrawl.Where(x => _domainService.IsInScope(x, DomainInfo)).ToList();

        _logger.LogDebug($"Currently on page {url}\n");

        return linksToCrawl;
    }


    private static IEnumerable<string> FilterUrlsOnPage(IEnumerable<string> urlsOnPage, string url)
    {
        urlsOnPage = urlsOnPage.Where(UrlUtility.IsHttpUrl);
        urlsOnPage = urlsOnPage.Select(x => UrlUtility.ConvertToAbsoluteUrl(x, url));
        urlsOnPage = urlsOnPage.Where(x => Uri.IsWellFormedUriString(x, UriKind.Absolute));
        urlsOnPage = urlsOnPage.Select(UrlUtility.RemoveAnchorTagFromUrl).Distinct();
        urlsOnPage = urlsOnPage.Where(UrlUtility.IsWantedFileType).AsEnumerable().ToList();
        return urlsOnPage.AsEnumerable();
    }

    public async Task<bool> CanBeCrawledAsync(string url)
    {
        url = url.StartsWith("*") ? UrlUtility.GetSecondLevelDomainFromWildcardUrl(url) : url;

        url = "https://" + url;

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

        if ((int)res.StatusCode is < 300 or > 400)
        {
            _logger.LogInformation($"Status code: {(int)res.StatusCode} {res.StatusCode}.");
            _logger.LogInformation("Can not be crawled");
            return false;
        }

        var newUrl = FollowRedirects(url, res);

        if (_domainService.ShouldHaveWwwSubdomain(newUrl, DomainInfo))
        {
            DomainInfo = _domainParser.Parse(newUrl);
        }

        return _domainService.IsInScope(url, DomainInfo) && _pageStorage.UrlHasBeenCrawled(url);
    }


    private string FollowRedirects(string newUrl, HttpResponseMessage res)
    {
        var maxRedirect = 0;
        while ((int)res.StatusCode is >= 300 and < 400)
        {
            if (maxRedirect > 10)
                return newUrl;

            var location = res.Headers.Location.ToString();
            newUrl = UrlUtility.ConvertToAbsoluteUrl(location, UrlUtility.GetDomainWithSubdomains(newUrl));
            _logger.LogInformation("The location of the resource has been moved, it is now at: " + location);

            try
            {
                res = _httpClient.GetAsync(newUrl).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check if site can be crawled: {ex.Message}");
                return newUrl;
            }

            maxRedirect++;
        }

        return newUrl;
    }
}