using cSharpScraper.Features.Shared.Infrastructure;

namespace cSharpScraper.Features.WebsiteCrawler.Persistence;

public class PageStorageFactory(DomainDbContextFactory domainDbContextFactory, ILogger<PageStorage> pageStorageLogger)
{
    private readonly DomainDbContextFactory _domainDbContextFactory = domainDbContextFactory;
    private readonly ILogger<PageStorage> _pageStorageLogger = pageStorageLogger;

    public PageStorage CreatePageStorage()
    {
        var dbContext = _domainDbContextFactory.CreateDbContext();
        var pageStorage = new PageStorage(_pageStorageLogger, dbContext);
        return pageStorage;
    }

}