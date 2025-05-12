using cSharpScraper.Features.WebsiteCrawler.Services;
using HtmlAgilityPack;

namespace cSharpScraperTests.Features.WebCrawlerTests;

public class DocParserTests
{
    [Fact]
    public void GetLinksFromPageSource_GivenPageSourceWithScriptTag_ShouldGetScriptSrc()
    {
        var docParser = new DocParser(new HtmlDocument()); 

        var pageSource = @"<script src=""/sites/crackerjill.com/themes/crackerjill/js/app.min.js?v=1721726273338""></script>";

        var links = docParser.GetLinksFromPageSource(pageSource);

        Assert.Contains("/sites/crackerjill.com/themes/crackerjill/js/app.min.js?v=1721726273338", links);
    }
}