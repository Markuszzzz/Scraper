using System.Text.Json.Serialization;

namespace cSharpScraper.Features.Httpx;

public class HttpxResult
{
    [JsonPropertyName("host")]
    public string Host { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }
    [JsonPropertyName("webserver")]
    public string? Webserver { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("input")]
    public string? RedirectedFrom { get; set; } 

}