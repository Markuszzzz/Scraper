using EFCore.BulkExtensions;

namespace cSharpScraper.Crawler.WebCrawler;

public class PageStorage
{
    private readonly ILogger<PageStorage> _logger;
    private readonly DomainDbContext _context;


    public PageStorage(ILogger<PageStorage> logger, DomainDbContext context)        
    {
        _logger = logger;       
        _context = context;
    }

    public IEnumerable<Page> GetNextPagesToScrape(DomainInfo domainInfo)
    {
        var pages = _context.Pages
            .Where(x => x.RootDomain.Name == domainInfo.RegistrableDomain && !x.HasBeenCrawled)
            .OrderBy(p => p.Id).Take(100).AsEnumerable();

        return pages; 
    }
    

    public void PersistDomain(DomainInfo domainInfo)
    {
        var rootDomainName = domainInfo.RegistrableDomain;
        var subdomainName = domainInfo.Subdomain;
        var rootDomain = FindOrCreateRootDomain(rootDomainName);
        if (domainInfo.Subdomain.Count() > 0)
        {
            FindOrCreateSubdomain(subdomainName, rootDomain);
        }
    }

    public void SavePageToBeCrawled(string url, DomainInfo domainInfo)
    {
        var rootDomainName = domainInfo.RegistrableDomain;
        var subdomainName = domainInfo.Subdomain;
        var rootDomain = FindOrCreateRootDomain(rootDomainName);
        var subdomain = FindOrCreateSubdomain(subdomainName, rootDomain);

        var urlWithoutQueryParams = UrlUtility.RemoveQueryParamsFromUrl(url);

        if (_context.Pages.Where(x => x.HasBeenCrawled && x.RootDomainId.Equals(rootDomain.Id)).AsEnumerable().Any(x =>
                UrlUtility.RemoveQueryParamsFromUrl(x.Url).Equals(urlWithoutQueryParams)))
            return;

        if (_context.Pages.Where(x => !x.HasBeenCrawled && x.RootDomainId.Equals(rootDomain.Id)).AsEnumerable().Any(x =>
                UrlUtility.RemoveQueryParamsFromUrl(x.Url).Equals(urlWithoutQueryParams)))
            return;

        _context.Pages.Add(new Page
        {
            Url = url,
            HasBeenCrawled = false,
            Content = string.Empty,
            RootDomainId = rootDomain.Id,
            SubdomainId = subdomain?.Id
        });

        _context.SaveChanges();
    }

    public bool HasMoreUrlsToScrape(DomainInfo domainInfo)
    {
        var pages = _context.Pages.Where(x => x.RootDomain.Name == domainInfo.RegistrableDomain && !x.HasBeenCrawled);
        if (domainInfo.Subdomain != null)
            pages = _context.Pages.Where(x => x.Subdomain.Name == domainInfo.Subdomain && x.RootDomain.Name == domainInfo.RegistrableDomain);

        return pages.Count() > 0;
    }

    public void SaveUrlsToBeScraped(IEnumerable<string> urls, DomainInfo domainInfo)
    {
        if (!urls.Any())
            return;

        var rootDomainName = domainInfo.RegistrableDomain;
        var subdomainName = domainInfo.Subdomain;
        var rootDomain = FindOrCreateRootDomain(rootDomainName);
        var subdomain = FindOrCreateSubdomain(subdomainName, rootDomain);

        foreach (var url in urls)
        {
            if (_context.Pages.Any(p => p.Url == url)) continue;

            var page = new Page
            {
                Url = url,
                HasBeenCrawled = false,
                Content = string.Empty,
                SubdomainId = subdomain.Id,
                RootDomainId = rootDomain.Id
            };

            _context.Pages.Add(page);
        }

        _context.SaveChanges();
    }

    public bool UrlHasBeenCrawled(string url)
    {
        return _context.Pages.Where(x => x.HasBeenCrawled).Any(x => x.Url.Equals(url));
    }

    public void  MarkPagesAsCrawled(IEnumerable<Page> pages)
    {
        foreach (var page in pages)
        {
            page.HasBeenCrawled = true;
        }

        _context.BulkUpdate(pages);
    }

    private Subdomain FindOrCreateSubdomain(string subdomainName, RootDomain rootDomain)
    {
        var subdomain = _context.Subdomains.FirstOrDefault(s => s.Name == subdomainName && s.DomainId == rootDomain.Id);
        if (subdomain == null)
        {
            subdomain = new Subdomain
            {
                Name = subdomainName,
                DomainId = rootDomain.Id
            };
            _context.Subdomains.Add(subdomain);
            _context.SaveChanges();
        }

        return subdomain;
    }

    private RootDomain FindOrCreateRootDomain(string rootDomainName)
    {
        var rootDomain = _context.RootDomains.FirstOrDefault(d => d.Name == rootDomainName);
        if (rootDomain == null)
        {
            rootDomain = new RootDomain { Name = rootDomainName };
            _context.RootDomains.Add(rootDomain);
            _context.SaveChanges();
        }

        return rootDomain;
    }
    


    public void SavePagesToBeCrawled(List<string> urls, DomainInfo domainInfo)
    {
        var rootDomainName = domainInfo.RegistrableDomain;
        var subdomainName = domainInfo.Subdomain;
        var rootDomain = FindOrCreateRootDomain(rootDomainName);
        var subdomain = FindOrCreateSubdomain(subdomainName, rootDomain);

        var newPages = urls
            .Where(url => !_context.Pages.Any(p => p.Url == url)) 
            .Select(url => new Page
            {
                Url = url,
                HasBeenCrawled = false,
                Content = string.Empty,
                RootDomainId = rootDomain.Id,
                SubdomainId = subdomain?.Id
            })
            .ToList();

        _context.Pages.AddRange(newPages);
        _context.SaveChanges(); 
    }
}