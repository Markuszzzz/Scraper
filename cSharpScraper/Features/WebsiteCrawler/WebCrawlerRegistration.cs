using cSharpScraper.Features.Shared.Models;
using cSharpScraper.Features.Shared.Services;
using cSharpScraper.Features.SubDomainCrawler;
using cSharpScraper.Features.WebsiteCrawler.Models;
using cSharpScraper.Features.WebsiteCrawler.Persistence;
using cSharpScraper.Features.WebsiteCrawler.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace cSharpScraper.Features.WebsiteCrawler;

public static class WebCrawlerRegistration
{
    public static IServiceCollection AddWebCrawler(this IServiceCollection services)
    {
        //todo: Use AZ key vault to store credentials
        services.AddDbContext<DomainDbContext>(options =>
                options.UseMySql("server=localhost;port=3307;database=scraper;user=root;password=SecurePassword",
                        new MySqlServerVersion(new Version(9, 1, 0)))
                    .LogTo(Console.WriteLine, LogLevel.Critical))
            .AddTransient<DocParser>()
            .AddTransient<PageStorage>()
            .AddTransient<PageStorageFactory>()
            .AddTransient<IRedirectResolver, RedirectResolver>()
            .AddTransient<WebCrawler>()
            .AddSingleton<CrawlerSettings>()
            .AddTransient<DomainService>()
            .AddSingleton<PageDownloader>()
            .AddTransient<WebDriverFactory>()
            .AddTransient<PageBatcher>()
            .AddTransient<ICrawler>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<CrawlerSettings>>().Value;

                if (settings.CrawlSubdomains)
                    return ActivatorUtilities.CreateInstance<SubdomainCrawlerDecorator>(provider);

                return ActivatorUtilities.CreateInstance<WebCrawler>(provider);

            })
            .AddHttpClient<WebCrawler, WebCrawler>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = new WebProxy("http://127.0.0.1:8080"),
                UseProxy = true,
                AllowAutoRedirect = false
            });
            
        return services;
    }
}