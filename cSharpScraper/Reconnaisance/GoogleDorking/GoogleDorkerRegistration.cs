using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaisance.GoogleDorking;

public static class GoogleDorkerRegistration
{
    public static IServiceCollection AddGoogleDorker(this IServiceCollection services)
    {
        services.AddTransient<GoogleDorker>();
        return services;
    }
}