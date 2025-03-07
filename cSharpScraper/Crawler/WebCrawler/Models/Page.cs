namespace cSharpScraper.Crawler.WebCrawler.Models;

public class Page
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Content { get; set; }
    public bool HasBeenCrawled { get; set; }
    public bool HasCompoundUrl { get; set; } 
    public bool HasBeenScanned { get; set; }
    
    public List<string> UrlsOnPage { get; set; }
    
    
    public int RootDomainId { get; set; }
    public RootDomain RootDomain { get; set; } = null!;
    
    public int? SubdomainId { get; set; }
    public Subdomain? Subdomain { get; set; }
}