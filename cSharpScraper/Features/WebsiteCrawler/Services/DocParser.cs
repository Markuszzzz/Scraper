using HtmlAgilityPack;

namespace cSharpScraper.Features.WebsiteCrawler.Services;

public class DocParser(HtmlDocument htmlDocument)
{
    private readonly HtmlDocument _htmlDocument = htmlDocument;

    public IEnumerable<string> GetLinksFromPageSource(string pageSource)
    {
        _htmlDocument.LoadHtml(pageSource);
        var urlsOnPage = _htmlDocument.DocumentNode.SelectNodes("//a[@href] | //script[@src]")
            ?.Select(x => x.GetAttributeValue("href", null) ?? x.GetAttributeValue("src", null)).Where(x => x is not null);

        return urlsOnPage?.ToList() ?? new List<string>();
    }
}