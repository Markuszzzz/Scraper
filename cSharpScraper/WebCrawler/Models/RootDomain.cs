namespace cSharpScraper.WebCrawler.Models;

public class RootDomain
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public ICollection<Subdomain> Subdomains { get; set; } = new List<Subdomain>();
    public ICollection<Page> Pages { get; set; } = new List<Page>(); 
}