namespace cSharpScraper.Features.Shared.Services;

public static class UrlUtility
{
    
    private static readonly string[] BlacklistedExtensions = [".png", ".jpg", ".jpeg", ".gif",];


    public static string RemoveAnchorTagFromUrl(string url)
    {
        string trimmedUrl;
        try
        {
            trimmedUrl = new Uri(url).GetLeftPart(UriPartial.Query);

        }
        catch (Exception)
        {
            Console.WriteLine($"Url: {url}");
            throw new Exception();
        }

        return trimmedUrl;
    }
    
    public static bool IsWantedFileType(string url)
    {
        string absoluteUrl;
        try
        {
            absoluteUrl = new Uri(url).AbsolutePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
            return false;
        }

        var extension = Path.GetExtension(absoluteUrl);
        
        return !BlacklistedExtensions.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetDomainWithSubdomains(string url)
    {
        var uri = new Uri(url);

        // Extract the scheme (e.g., https) and the host (e.g., vg.no)
        string domainWithSubdomains = $"{uri.Scheme}://{uri.Host}";

        return domainWithSubdomains;
    }

    public static bool IsSubdomainWildcard(string url)
    {
        return url.StartsWith("*");
    }

    public static bool HasWildcardThatIsNotInTheStart(string url)
    {
        return !IsSubdomainWildcard(url) && url.Contains("*");
    }
    
    public static string RemoveQueryParamsFromUrl(string url)
    {
        var baseUrl = new Uri(url).GetLeftPart(UriPartial.Path);
        return baseUrl;
        }

    public static string ConvertToAbsoluteUrl(string scriptUrl, string currentUrl)
    {
        if (scriptUrl.StartsWith("http"))
            return scriptUrl;
        if (scriptUrl.StartsWith("/"))
        {
            //protocol relative URl
            if (scriptUrl.Length is > 1 and '/')
            {
                var scheme = new Uri(currentUrl).GetLeftPart(UriPartial.Scheme);
                
                scheme = scheme.Substring(0, scheme.Length - 2);
                return scheme + scriptUrl;
            }
            var baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
            return baseUrl + scriptUrl;
        }

        //remove query part of currentUrl
        currentUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
        return currentUrl + "/" + scriptUrl;
    }
    
/// <summary>
/// Checks that ir is not a telephone, mailto or javascript url
/// </summary>
/// <param name="url"></param>
/// <returns></returns>
    public static bool IsHttpxUrl(string url)
    {
        return !url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<string> FilterUrlsOnPage(IEnumerable<string> urlsOnPage, string url)
    {
        urlsOnPage = urlsOnPage.Where(IsHttpxUrl);
        urlsOnPage = urlsOnPage.Select(x => ConvertToAbsoluteUrl(x, url));
        urlsOnPage = urlsOnPage.Where(x => Uri.IsWellFormedUriString(x, UriKind.Absolute));
        urlsOnPage = urlsOnPage.Select(RemoveAnchorTagFromUrl).Distinct();
        urlsOnPage = urlsOnPage.Where(IsWantedFileType).AsEnumerable().ToList();
        return urlsOnPage.AsEnumerable();
    }

public static string GetSecondLevelDomainFromWildcardUrl(string wildcardUrl)
    {
        var i = 0;
        while (wildcardUrl[i] is '*' or '.')
            i++;

        return wildcardUrl[i..];
    }

    public static string NormalizeDomain(string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        try
        {
            var uri = new Uri(url);
            return uri.Host; // returnerer "example.com" eller "sub.example.com"
        }
        catch
        {
            return url.Trim().TrimEnd('/'); // fallback
        }
    }
    
    
}