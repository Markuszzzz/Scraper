using cSharpScraper.Features.WebsiteCrawler.Models;

namespace cSharpScraper.Features.WebsiteCrawler.Persistence;

public class DomainDbContext(DbContextOptions<DomainDbContext> options) : DbContext(options)
{
    public DbSet<RootDomain> RootDomains { get; set; } = null!;
    public DbSet<Subdomain> Subdomains { get; set; } = null!;
    public DbSet<Page> Pages { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subdomain>()
            .HasOne(s => s.RootDomain)
            .WithMany(d => d.Subdomains)
            .HasForeignKey(s => s.DomainId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(true);
        
        modelBuilder.Entity<Subdomain>()
            .HasIndex(s => new {s.Name, s.DomainId})
            .IsUnique(); 

        modelBuilder.Entity<Page>()
            .HasOne(p => p.Subdomain)
            .WithMany(s => s.Pages)
            .HasForeignKey(p => p.SubdomainId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false); 

        modelBuilder.Entity<Page>()
            .HasOne(p => p.RootDomain)
            .WithMany(d => d.Pages)
            .HasForeignKey(p => p.RootDomainId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(true);
        
    
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.Content);
            entity.Property(e => e.HasBeenCrawled).IsRequired();
        });
        
        modelBuilder.Entity<RootDomain>().HasIndex(d => d.Name).IsUnique();
    }

}