using Nager.PublicSuffix.RuleProviders;

namespace cSharpScraper.Crawler.WebCrawler;

public static class DomainParserProvider
{
    private static DomainParser? _cachedParser;

    public static async Task<DomainParser> GetAsync()
    {
        if (_cachedParser is not null)
            return _cachedParser;

        var path = Path.Combine(AppContext.BaseDirectory, "public_suffix_list.dat");
        var ruleProvider = new LocalFileRuleProvider(path);
        await ruleProvider.BuildAsync();
        _cachedParser = new DomainParser(ruleProvider);

        return _cachedParser;
    }
}