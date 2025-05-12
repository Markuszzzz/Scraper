using cSharpScraper.Features.Httpx;
using cSharpScraper.Features.Shared.Models;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.WebsiteCrawler.Models;
using EFCore.BulkExtensions;

namespace cSharpScraper.Features.WebsiteCrawler.Persistence;

public class PageStorage(ILogger<PageStorage> logger, DomainDbContext context)
{
    private readonly ILogger<PageStorage> _logger = logger;
    private readonly DomainDbContext _context = context;


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
        var rootDomain = FindOrCreateRootDomain(rootDomainName);

        if (domainInfo.Subdomain.Count() == 0) 
            return;
        
        var subdomainName = domainInfo.Subdomain;
        FindOrCreateSubdomain(subdomainName, rootDomain);
    }

    public void SavePageToBeCrawled(CrawlTarget crawlTarget)
    {
        var (rootDomain, subdomain) = FindOrCreateDomainEntities(crawlTarget.DomainInfo);

        var urlWithoutQueryParams = UrlUtility.RemoveQueryParamsFromUrl(crawlTarget.Url);

        if (_context.Pages.Where(x => x.HasBeenCrawled && x.RootDomainId.Equals(rootDomain.Id)).AsEnumerable().Any(x =>
                UrlUtility.RemoveQueryParamsFromUrl(x.Url).Equals(urlWithoutQueryParams)))
            return;

        if (_context.Pages.Where(x => !x.HasBeenCrawled && x.RootDomainId.Equals(rootDomain.Id)).AsEnumerable().Any(x =>
                UrlUtility.RemoveQueryParamsFromUrl(x.Url).Equals(urlWithoutQueryParams)))
            return;

        _context.Pages.Add(new Page
        {
            Url = crawlTarget.Url,
            HasBeenCrawled = false,
            Content = string.Empty,
            RootDomainId = rootDomain.Id,
            SubdomainId = subdomain.Id
        });

        _context.SaveChanges();
    }

    public bool HasMoreUrlsToScrape(DomainInfo domainInfo)
    {
        var pages = _context.Pages.Where(x => x.RootDomain.Name == domainInfo.RegistrableDomain && !x.HasBeenCrawled);
        if (domainInfo.Subdomain is not null)
            pages = _context.Pages.Where(x => x.Subdomain.Name == domainInfo.Subdomain && x.RootDomain.Name == domainInfo.RegistrableDomain);

        return pages.Count() > 0;
    }

    public void SaveUrlsToBeScraped(IEnumerable<string> urls, DomainInfo domainInfo)
    {
        if (!urls.Any())
            return;

        var (rootDomain, subdomain) = FindOrCreateDomainEntities(domainInfo);

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
    
    public void SaveUrlsToBeScraped(IEnumerable<HttpxResult> httpxResults, DomainInfo domainInfo)
    {
        if (!httpxResults.Any())
            return;

        var (rootDomain, subdomain) = FindOrCreateDomainEntities(domainInfo);

        foreach (var httpxResult in httpxResults)
        {
            if (_context.Pages.Any(p => p.Url == httpxResult.Url)) continue;

            var page = new Page
            {
                Url = httpxResult.Url,
                StatusCode = httpxResult.StatusCode,
                HasBeenCrawled = false,
                Content = string.Empty, 
                SubdomainId = subdomain.Id,
                RootDomainId = rootDomain.Id
            };

            if (httpxResult.RedirectedFrom is not null)
            {
                page.RedirectedFrom = httpxResult.RedirectedFrom;
            }

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

    public void SavePagesToBeCrawled(List<string> urls, DomainInfo domainInfo)
    {
        if (!urls.Any())
            return;
        
        var (rootDomain, subdomain) = FindOrCreateDomainEntities(domainInfo);

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

    private (RootDomain, Subdomain) FindOrCreateDomainEntities(DomainInfo domainInfo)
    {
        var rootDomainName = domainInfo.RegistrableDomain;
        var subdomainName = domainInfo.Subdomain;
        var rootDomain = FindOrCreateRootDomain(rootDomainName);
        var subdomain = FindOrCreateSubdomain(subdomainName, rootDomain);
        return (rootDomain, subdomain);
    }
    
    private RootDomain FindOrCreateRootDomain(string rootDomainName)
    {
        var rootDomain = _context.RootDomains.FirstOrDefault(d => d.Name == rootDomainName);
        if (rootDomain is null)
        {
            rootDomain = new RootDomain { Name = rootDomainName };
            _context.RootDomains.Add(rootDomain);
            _context.SaveChanges();
        }

        return rootDomain;
    }
    
    private Subdomain FindOrCreateSubdomain(string subdomainName, RootDomain rootDomain)
    {
        var subdomain = _context.Subdomains.FirstOrDefault(s => s.Name == subdomainName && s.DomainId == rootDomain.Id);
        if (subdomain is null)
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

}