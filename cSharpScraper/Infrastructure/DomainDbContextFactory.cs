using cSharpScraper.Crawler.WebCrawler;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace cSharpScraper.Infrastructure;

public class DomainDbContextFactory : IDesignTimeDbContextFactory<DomainDbContext>
{
    public DomainDbContext CreateDbContext(string[] args = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DomainDbContext>();
        
        optionsBuilder.UseMySql("server=localhost;port=3307;database=scraper;user=root;password=Passord4321Pass;AllowLoadLocalInfile=true;",
                new MySqlServerVersion(new Version(9, 1, 0)))
            .LogTo(Console.WriteLine, LogLevel.Critical);

        return new DomainDbContext(optionsBuilder.Options);
    }
}