using HtmlAgilityPack;
using OpenQA.Selenium.Support.UI;

namespace cSharpScraper.Reconnaisance.GoogleDorking;

public class GoogleDorker(IWebDriver webDriver)
{
    private readonly IWebDriver _webDriver = webDriver;

    public List<string> DorkUrls(DomainInfo domainInfo)
    {
        var openRedirectKeywords = new List<string>
        {
            "redirect_url",
            "next",
            "continue",
            "goto",
            "return_Url",
            "destination",
            "fromURI",
            "redirect",
            "go",
            "from",
            "return",
            "rurl",
            "target",
            "callback",
            "back",
            "forward",
            "fwd",
            "login_redirect",
            "logout_redirect",
            "auth",
            "dest",
            "returnto",
            "backto",
            "originalurl",
            "original_url",
            "jump",
            "nav"
        };
        
        AcceptCookieBanner(_webDriver);

        var redirectUrls = new List<string>();

        foreach (var keyword in openRedirectKeywords)
        {
            
            var searchBox = _webDriver.FindElement(By.TagName("textarea"));
            searchBox.Clear();
            searchBox.SendKeys($"site:{domainInfo.FullyQualifiedDomainName} inurl:{keyword}");
            searchBox.SendKeys(Keys.Enter);
            if(_webDriver.PageSource.Contains("Our systems have detected unusual traffic from your computer network."))
                Debugger.Break();

            redirectUrls.AddRange(Dork(_webDriver, domainInfo));
        }
        
        
        _webDriver.Quit();
        _webDriver.Dispose();
        return redirectUrls;

    }
    
    static List<string> Dork(IWebDriver driver, DomainInfo domainInfo)
    {
        var pageSource = driver.PageSource;
        var allUrls = new List<string>();
        
        if(driver.PageSource.Contains("Our systems have detected unusual traffic from your computer network."))
            Debugger.Break();

        if (pageSource.Contains("did not match any documents."))
            return Array.Empty<string>().ToList();
        
        string? href;
        var maxTabs = 8;
        var nextTab = 2;

        do
        {
            var urlsOnPage = GetUrlsOnPage(pageSource, domainInfo);
            allUrls.AddRange(urlsOnPage);
            href = FindNextTab(nextTab++, pageSource);
            if (href is null) continue;
            
            var toGo = "https://" + "www.google.com" + href;
            try
            {
                driver.Navigate().GoToUrl(WebUtility.HtmlDecode(toGo));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error navigating using google dorking: " + ex);
                nextTab++;
            }
            Thread.Sleep((int)(1 + new Random().NextDouble() * 0.5) * 1000);
            
        } while (href is not null && nextTab < maxTabs);

        return allUrls;
    }
    
    static string? FindNextTab(int nextTab, string pageSource)
    {
        var document = new HtmlDocument();
        document.LoadHtml(pageSource);
        var tbodyNode = document.DocumentNode.SelectSingleNode("//tbody");

        if (tbodyNode is not null)
        {
            var tdNodes = tbodyNode.SelectNodes(".//td[position() > 1]");
            if (tdNodes is not null)
            {
                foreach (var td in tdNodes)
                {
                    var anchorNode = td.SelectSingleNode(".//a[@href]");
                    if (anchorNode is not null)
                    {
                        var href = anchorNode.GetAttributeValue("href", "No href found");
                        if (td.InnerText == nextTab.ToString())
                            return href;
                    }
                }
            }
        }
        return null;
    }

    static List<string> GetUrlsOnPage(string pageSource, DomainInfo domain)
    {
        var document = new HtmlDocument();
        document.LoadHtml(pageSource);
        var urlsOnPage = document.DocumentNode.SelectNodes("//a[@href] | //script[@src]")
            ?.Select(x => x.GetAttributeValue("href", null) ?? x.GetAttributeValue("src", null))?.Where(x => x is not null);

        var urls = urlsOnPage?.ToList() ?? new List<string>();
        return urls.Where(x => x.StartsWith("https://" + domain.FullyQualifiedDomainName)).Select(WebUtility.UrlDecode).Select(WebUtility.HtmlDecode).ToList();
    }
    
    private static void AcceptCookieBanner(IWebDriver driver)
    {
        driver.Navigate().GoToUrl("https://www.google.com");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        By acceptAllButton = By.XPath("//div[text()='Accept all']");


        IWebElement consentBtn = wait.Until(drv => drv.FindElement(acceptAllButton));
        consentBtn.Click();

        Thread.Sleep((int)(1 + new Random().NextDouble() * 0.5) * 1000);
    }
}