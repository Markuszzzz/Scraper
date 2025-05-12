using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaissance.GoogleDorking;

public static class GoogleDorkerRegistration
{
    //todo lazy initialization
    public static IServiceCollection AddGoogleDorker(this IServiceCollection services)
    {
        services.AddTransient<GoogleDorker>(provider => 
            new GoogleDorker(provider.GetRequiredService<WebDriverFactory>().CreateGoogleDorkingWebdriver()));
        
        return services;
    }
}