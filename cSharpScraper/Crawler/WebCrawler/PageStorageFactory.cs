using cSharpScraper.Infrastructure;
using Microsoft.Extensions.Logging;

namespace cSharpScraper.Crawler.WebCrawler;

public class PageStorageFactory
{
    private readonly DomainDbContextFactory _domainDbContextFactory;
    private readonly ILogger<PageStorage> _pageStorageLogger;

    public PageStorageFactory(DomainDbContextFactory domainDbContextFactory, ILogger<PageStorage> pageStorageLogger)
    {
        _domainDbContextFactory = domainDbContextFactory;
        _pageStorageLogger = pageStorageLogger;
    }

    public PageStorage CreatePageStorage()
    {
        var dbContext = _domainDbContextFactory.CreateDbContext();
        var pageStorage = new PageStorage(_pageStorageLogger, dbContext);
        return pageStorage;
    }

}