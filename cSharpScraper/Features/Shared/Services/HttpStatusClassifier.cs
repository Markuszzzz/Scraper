namespace cSharpScraper.Features.Shared.Services;

public static class HttpStatusClassifier
{
    public static bool IsRedirect(this HttpStatusCode statusCode)
    {
        return (int)statusCode is >= 300 and <= 399;
    }
}