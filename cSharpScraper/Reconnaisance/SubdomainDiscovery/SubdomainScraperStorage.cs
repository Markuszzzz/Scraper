using System.Text.Json;

namespace cSharpScraper.Reconnaisance.SubdomainDiscovery;

public class SubdomainScraperStorage
{
    public int LoadSubdomainToScrape(string domainsToScrape)
    {
        var path = Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json");
        if (File.Exists(path))
        {
            var jsonString = File.ReadAllText(Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json"));
        
            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

            return data["domainNumber"];
        }

        return 0;
    }
    
    public void IncrementSubdomainToScrape(string domainsToScrape)
    {
        var path = Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json");
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.Combine("/Users/mhellestveit/domains", domainsToScrape));
            File.WriteAllText(Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json"), "");

            var dictionary = new Dictionary<string, int> { { "domainNumber", 0 } };
            var initialData = JsonSerializer.Serialize(dictionary);
            File.WriteAllText(Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json"), initialData);
        }

        var jsonString = File.ReadAllText(Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json"));
    
        var data = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);
        data["domainNumber"] += 1;

        var text = JsonSerializer.Serialize(data);

        File.WriteAllText(Path.Combine("/Users/mhellestveit/domains", domainsToScrape, "domains.json"), text);

    }
}