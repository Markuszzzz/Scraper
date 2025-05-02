using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace cSharpScraper.Reconnaisance.SiteArchive;

public class ArchivedUrlCollector
{
    public List<string> GetArchivedUrlsForDomain(string domain)
    {

        var list = new List<string>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash", // Use `cmd.exe` on Windows
                Arguments = $"-c \"waybackurls {domain} --no-subs > /Users/mhellestveit/text.txt\"",
                UseShellExecute = false,
            }
        };
        
        
        process.Start();
        process.WaitForExit();
        
        list = File.ReadAllLines("/Users/mhellestveit/text.txt").ToList();

        return list;
    }

    public List<string> FilterOutDeadUrls(IEnumerable<string> list, DomainInfo domain)
    {
        list = GetUrlsThatContainRedirectUrl(list, domain);
        var results = new ConcurrentBag<(string Url, int? Status)>();
        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 8
        };

        var proxy = new WebProxy
        {
            Address = new Uri("http://localhost:8081"),
        };
        
        HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            Proxy = proxy,
            UseProxy = true,
        };
        
        var httpClient = new HttpClient(httpClientHandler);
        

        Parallel.ForEach(list, parallelOptions, url =>
        {
            try
            {
                Console.WriteLine($"{results.Count}/{list.Count()}");
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = httpClient.Send(request);
                results.Add((url, (int)response.StatusCode));
            }
            catch
            {
                results.Add((url, null));
            }
        });


        var ulrsThatAreUp = new List<string>();
        
        foreach (var result in results)
        {
            if (result.Status is < 400)
                ulrsThatAreUp.Add(result.Url);
            if (result.Status is >= 300 and < 400)
            {
                Console.WriteLine("Redirected: " + result.Url);
            }
        }

        return ulrsThatAreUp;
    }
    
    private static IEnumerable<string> GetUrlsThatContainRedirectUrl(IEnumerable<string> urls, DomainInfo domain)
    {
        var regexSafeDomain = CreateRegexSafeDomain(domain.FullyQualifiedDomainName);
        
        var pattern = $@"https?\:\/\/[^\s""'&<>]*?{regexSafeDomain}(?:(?!\\n)[^\s""',<>)\]])+?\?[^\s""',<>)\]]+?https?\:\/\/[^\s""'&]+";

        List<string> filteredUrls = new List<string>();
        foreach (var url in urls)
        {
            Console.WriteLine("url");
            var match = Regex.Match(url, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                filteredUrls.Add(match.Groups[0].Value);
                Console.WriteLine("found url!!!");
                Console.WriteLine(match.Groups[0].Value);
            }
        }
        

        return filteredUrls;
    }

    private static string CreateRegexSafeDomain(string domain)
    {
        var regexStafeDomain = domain.Replace(".", @"\.");
        if (regexStafeDomain.StartsWith("www"))
        {
            regexStafeDomain = regexStafeDomain.Replace(@"www\.", "");
        }

        return regexStafeDomain;
    }
}