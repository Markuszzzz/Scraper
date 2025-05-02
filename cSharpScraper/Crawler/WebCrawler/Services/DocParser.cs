using HtmlAgilityPack;

namespace cSharpScraper.Crawler.WebCrawler.Services;

public class DocParser
{
    private readonly HtmlDocument _htmlDocument;

    public DocParser(HtmlDocument htmlDocument)
    {
        _htmlDocument = htmlDocument;
    }
    
    public IEnumerable<string> GetLinksFromPageSource(string pageSource)
    {
        _htmlDocument.LoadHtml(pageSource);
        var urlsOnPage = _htmlDocument.DocumentNode.SelectNodes("//a[@href] | //script[@src]")
            ?.Select(x => x.GetAttributeValue("href", null) ?? x.GetAttributeValue("src", null)).Where(x => x is not null);

        return urlsOnPage?.ToList() ?? new List<string>();
    }
}