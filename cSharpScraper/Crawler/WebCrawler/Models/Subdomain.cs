namespace cSharpScraper.Crawler.WebCrawler.Models;

public class Subdomain
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; 

    public int DomainId { get; set; }
    public RootDomain RootDomain { get; set; } = null!;

    public ICollection<Page> Pages { get; set; } = new List<Page>();
}