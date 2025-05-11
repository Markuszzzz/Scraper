using Microsoft.Extensions.DependencyInjection;

namespace cSharpScraper.Reconnaissance.Httpx;

public static class HttpxRegistration
{
    public static IServiceCollection AddHttpx(this IServiceCollection services)
    {
        services.AddTransient<IHttpxExecutor, HttpxExecutor>();
        return services;
    }
}