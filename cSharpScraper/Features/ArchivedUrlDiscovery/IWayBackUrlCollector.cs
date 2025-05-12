namespace cSharpScraper.Features.ArchivedUrlDiscovery;

public interface IArchivedUrlCollector
{
    List<string> GetArchivedUrlsForDomain(string domain);
}