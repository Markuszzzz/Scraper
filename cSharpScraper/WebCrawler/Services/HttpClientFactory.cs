using System.Net;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class HttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        var proxy = new WebProxy
        {
            Address = new Uri("http://127.0.0.1:8080"),
            BypassProxyOnLocal = false,                
            UseDefaultCredentials = false      
        };
        
        var handler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = false, 
            AllowAutoRedirect = false
        };
        

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        
        client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6778.86 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        return client;
    }
}