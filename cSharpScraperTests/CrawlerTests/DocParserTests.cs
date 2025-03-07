using cSharpScraper.Crawler.WebCrawler;
using cSharpScraper.Crawler.WebCrawler.Services;
using HtmlAgilityPack;
using Nager.PublicSuffix;

namespace cSharpScraperTests.CrawlerTests;

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