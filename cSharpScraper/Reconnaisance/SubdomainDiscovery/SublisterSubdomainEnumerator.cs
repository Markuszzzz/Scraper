using System.Text.RegularExpressions;

namespace cSharpScraper.Reconnaisance.SubdomainDiscovery;


public class SublisterSubdomainEnumerator : ISubdomainEnumerator
{
    private readonly ILogger<SublisterSubdomainEnumerator> _logger;
    private readonly DomainParser _domainParser;

    public SublisterSubdomainEnumerator(ILogger<SublisterSubdomainEnumerator> logger, DomainParser domainParser)
    {
        _logger = logger;
        _domainParser = domainParser;
    }
    public async Task<List<string>> FindSubdomainsAsync(string domain)
    {
        domain = domain.Remove(0, 1);

        var domainInfo = _domainParser.Parse(domain);

        while (domain.StartsWith("."))
        {
            domain = domain.Remove(0, 1);
        }

        if (!File.Exists(domainInfo.RegistrableDomain))
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {   
                        FileName = "sublist3r", // Ensure 'amass' is in your PATH
                        Arguments = $"-d {domainInfo.RegistrableDomain} -o {domainInfo.RegistrableDomain} -v",
                    }
                };

                _logger.LogDebug($"Running: sublist3r -d {domainInfo.RegistrableDomain} -o {domainInfo.RegistrableDomain} -v");

                process.Start();
                await process.WaitForExitAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError("Error running sublist3r: Exception " + ex.Message);
                
            }
        }
        
        if (!File.Exists(domainInfo.RegistrableDomain))
            return Array.Empty<string>().ToList();

        var subdomains = File.ReadAllLines(domainInfo.RegistrableDomain).Distinct().ToList();

        var aliveSubdomains = await GetResponsiveUrlsAsync(subdomains);
        var filteredSubdomains = Array.Empty<string>().ToList();

        var regexDomain = domain.Replace("-", @"\-").Replace(@".", @"\.");
    
        foreach (var currentDomain in aliveSubdomains)
        {
            var match = Regex.Match(currentDomain, $@".*?{regexDomain}");
            if(match.Success)
                filteredSubdomains.Add(match.Value);

        }
        
        
        return filteredSubdomains;
        
    }


    private async Task<List<string>> GetResponsiveUrlsAsync(List<string> urls)
    {
        using HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15) // Set a timeout for requests
        };
        
        var tasks = urls.Select(async url =>
        {
            url = "https://" + url;
            try
            {
                var response = await client.GetAsync( url);

                var maxRedirect = 0;
                while ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                {
                    if (maxRedirect > 5)
                        return null;
            
                    var location = response.Headers.Location.ToString();
                    url = UrlUtility.ConvertToAbsoluteUrl(location,  UrlUtility.GetDomainWithSubdomains(url));

                    try
                    {
                        response = await client.GetAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to check if site can be crawled: {ex.Message}");
                        return null;
                    }

                    maxRedirect++;
                }
                return response.IsSuccessStatusCode ? url : null;
            }
            catch(Exception ex)
            {
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(url => url is not null).ToList();
    }
}

