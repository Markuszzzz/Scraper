using cSharpScraper.Features.WebsiteCrawler.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace cSharpScraper.Features.Shared.Infrastructure;

public class DomainDbContextFactory : IDesignTimeDbContextFactory<DomainDbContext>
{
    public DomainDbContext CreateDbContext(string[]? args = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DomainDbContext>();
        
        optionsBuilder.UseMySql("server=localhost;port=3307;database=scraper;user=root;password=SecurePassword;AllowLoadLocalInfile=true;",
                new MySqlServerVersion(new Version(9, 1, 0)))
            .LogTo(Console.WriteLine, LogLevel.Critical);

        return new DomainDbContext(optionsBuilder.Options);
    }
}