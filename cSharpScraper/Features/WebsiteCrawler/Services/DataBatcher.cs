using System.Collections.Concurrent;
using cSharpScraper.Features.WebsiteCrawler.Models;
using cSharpScraper.Features.WebsiteCrawler.Persistence;

namespace cSharpScraper.Features.WebsiteCrawler.Services;

public class PageBatcher(PageStorageFactory pageStorageFactory, ILogger<PageBatcher> logger)
{
    private readonly Stopwatch _databaseStopwatch = new();
    private readonly PageStorageFactory _pageStorageFactory = pageStorageFactory;
    private readonly ILogger<PageBatcher> _logger = logger;

    public void PersistData(BlockingCollection<Page> temporaryPages, DomainInfo domainInfo)
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
                SaveBatch(batch, pageStorage, domainInfo);
                batch.Clear();
                _databaseStopwatch.Restart();
            }
        }

        if (batch.Count > 0)
        {
            SaveBatch(batch, pageStorage, domainInfo);
        }
    }
    
    private void SaveBatch(List<Page> batch, PageStorage pageStorage, DomainInfo domainInfo)
    {
        try
        {
            pageStorage.MarkPagesAsCrawled(batch);  

            var allLinks = batch
                .SelectMany(page => page.UrlsOnPage)
                .Distinct()
                .ToList();
            
            pageStorage.SavePagesToBeCrawled(allLinks, domainInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Batch database error: {ex.Message}");
        }
    }
}