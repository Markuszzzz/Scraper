using Nager.PublicSuffix.RuleProviders;

namespace cSharpScraper.Features.Shared.Services;

public static class DomainParserProvider
{
    private static DomainParser? _cachedParser;

    public static async Task<DomainParser> GetAsync()
    {
        if (_cachedParser is not null)
            return _cachedParser;

        var path = Path.Combine(AppContext.BaseDirectory, "Features", "Shared", "Resources", "public_suffix_list.dat");
        var ruleProvider = new LocalFileRuleProvider(path);
        await ruleProvider.BuildAsync();
        _cachedParser = new DomainParser(ruleProvider);

        return _cachedParser;
    }
}