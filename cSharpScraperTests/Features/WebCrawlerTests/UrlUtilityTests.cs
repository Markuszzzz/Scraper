using cSharpScraper.Features.Shared.Services;

namespace cSharpScraperTests.Features.WebCrawlerTests;

public class UrlUtilityTests
{
    [Fact]
    public void RemoveAnchorTagFromUrl_GivenUrlContainingAnAnchorTag_ReturnsUrlWIthoutAnchor()
    {
        var url = "https://www.ruffles.com/#recipes";
        
        Assert.Equal("https://www.ruffles.com/", UrlUtility.RemoveAnchorTagFromUrl(url));
    }

    [Fact]
    public void ConvertToAbsoluteUrl_GivenNormalUrl_ReturnsAbsoluteUrl()
    {
        var scriptUrl = "/products";
        var currentUrl = "https://www.ruffles.com";
        
        Assert.Equal("https://www.ruffles.com/products", UrlUtility.ConvertToAbsoluteUrl(scriptUrl, currentUrl));
    }
    
    [Fact]
    public void ConvertToAbsoluteUrl_GivenEmptyProtocolRelativeUrl_ReturnsAbsoluteUrl()
    {
        var scriptUrl = "//";
        var currentUrl = "https://www.ruffles.com";
        
        Assert.Equal("https://", UrlUtility.ConvertToAbsoluteUrl(scriptUrl, currentUrl));
    }

    [Fact]
    public void ConvertToAbsoluteUrl_ScriptUrlHasParameter_ReturnProperUrl()
    {
        var scriptUrl = "?page=2";
        var currentUrl = "https://www.vg.no/?page=1";
        
        Assert.Equal("https://www.vg.no/?page=2", UrlUtility.ConvertToAbsoluteUrl(scriptUrl, currentUrl));
    }
    
    [Fact]
    public void ConvertToAbsoluteUrl_GivenNormalUrl2_ReturnsAbsoluteUrl()
    {
        var scriptUrl = "https://x.com/intent/tweet?url=http://www.ruffles.com/recipes/ruffles-baked-potato-dip&text=RUFFLES%C2%AE%20Baked%20Potato%20Dip";
        var currentUrl = "https://www.ruffles.com/recipes/ruffles-baked-potato-dip";
        
        Assert.Equal("https://x.com/intent/tweet?url=http://www.ruffles.com/recipes/ruffles-baked-potato-dip&text=RUFFLES%C2%AE%20Baked%20Potato%20Dip", UrlUtility.ConvertToAbsoluteUrl(scriptUrl, currentUrl));
    }

    [Fact]
    public void ConvertToAbsoluteUrl_GivenProtocolRelativeScriptUrl_ReturnProperUrl()
    {
        var scriptUrl =
            "//consent.trustarc.com/notice?domain=pepsico-na-foods.com&amp;c=teconsent&amp;text=true&amp;js=nj&amp;noticeType=bb&amp;language=&amp;cookieLink=https%3A%2F%2Fcontact.pepsico.com%2Ffritolay%2Fprivacy-policy&amp;privacypolicylink=https%3A%2F%2Fcontact.pepsico.com%2Ffritolay%2Fprivacy-policy";
        var currentUrl = "https://www.ruffles.com/";
        
        Assert.Equal("https://consent.trustarc.com/notice?domain=pepsico-na-foods.com&amp;c=teconsent&amp;text=true&amp;js=nj&amp;noticeType=bb&amp;language=&amp;cookieLink=https%3A%2F%2Fcontact.pepsico.com%2Ffritolay%2Fprivacy-policy&amp;privacypolicylink=https%3A%2F%2Fcontact.pepsico.com%2Ffritolay%2Fprivacy-policy", 
            UrlUtility.ConvertToAbsoluteUrl(scriptUrl, currentUrl));
    }
}