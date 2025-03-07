using System.Collections.Concurrent;
using System.Diagnostics;
using cSharpScraper.Crawler.WebCrawler.Models;
using Microsoft.Extensions.Logging;
using Nager.PublicSuffix;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class PageBatcher
{
    private readonly PageStorageFactory _pageStorageFactory;
    private readonly ILogger<PageBatcher> _logger;
    private readonly DomainInfo _domainInfo;
    private readonly Stopwatch _databaseStopwatch;


    public PageBatcher(PageStorageFactory pageStorageFactory, ILogger<PageBatcher> logger, DomainInfo domainInfo)
    {
        _pageStorageFactory = pageStorageFactory;
        _logger = logger;
        _domainInfo = domainInfo;
        _databaseStopwatch = new Stopwatch();
    }

    public void PersistData(BlockingCollection<Page> temporaryPages)
    {
        var pageStorage = _pageStorageFactory.CreatePageStorage();
        var batchSize = 50;
        var batch = new List<Page>();

        _databaseStopwatch.Start();
        foreach (var page in temporaryPages.GetConsumingEnumerable())
        {
            batch.Add(page);

            if (batch.Count >= batchSize)
            {
                _databaseStopwatch.Stop();
                _logger.LogInformation(
                    $"Crawled {batchSize} pages with average speed of {(double)_databaseStopwatch.ElapsedMilliseconds / batchSize} ms per page");
                SaveBatch(batch, pageStorage);
                batch.Clear();
                _databaseStopwatch.Restart();
            }
        }

        if (batch.Count > 0)
        {
            SaveBatch(batch, pageStorage);
        }
    }
    
    private void SaveBatch(List<Page> batch, PageStorage pageStorage)
    {
        try
        {
            pageStorage.MarkPagesAsCrawled(batch);  

            var allLinks = batch
                .SelectMany(page => page.UrlsOnPage)
                .Distinct()
                .ToList();
            
            pageStorage.SavePagesToBeCrawled(allLinks, _domainInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Batch database error: {ex.Message}");
        }
    }
}