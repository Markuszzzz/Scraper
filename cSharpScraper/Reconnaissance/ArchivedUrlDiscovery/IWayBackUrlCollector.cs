namespace cSharpScraper.Reconnaissance.ArchivedUrlDiscovery;

public interface IArchivedUrlCollector
{
    List<string> GetArchivedUrlsForDomain(string domain);
}