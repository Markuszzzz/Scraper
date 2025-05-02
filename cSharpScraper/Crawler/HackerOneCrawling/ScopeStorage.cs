using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace cSharpScraper.Crawler.HackerOneCrawling;

public class ScopeStorage
{
    private readonly string _scopesCsvPath;
    public string ScopeName { get; }

    public ScopeStorage(string scopesCsvPath)
    {
        _scopesCsvPath = scopesCsvPath;
        ScopeName = _scopesCsvPath.Substring(_scopesCsvPath.LastIndexOf("/")+1);
        CreateSuperScopeFolder();
    }

    private void CreateSuperScopeFolder()
    {

        if (File.Exists(Path.Combine("/Users/mhellestveit/ScrapedWebsites", ScopeName, "scope.json")))
            return;
        
        Directory.CreateDirectory(Path.Combine("/Users/mhellestveit/ScrapedWebsites",ScopeName));
        var data = new Dictionary<string, int>();
        data["ScopeNumber"] = 0;
        var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(Path.Combine(GetScopeFolder(), "scope.json"), jsonString);
    }
    
    
    public void SaveLastCompletedScopeIndex(int scopeIndex)
    {
        string jsonString = File.ReadAllText(Path.Combine(GetScopeFolder(), "scope.json"));

        var data = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

        data["ScopeNumber"] = scopeIndex;

        jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        
        File.WriteAllText(Path.Combine(GetScopeFolder(), "scope.json"), jsonString);
    }
    
    public int LoadLastCompletedIndex()
    {
        if (File.Exists(Path.Combine(GetScopeFolder(), "scope.json")))
        {
            return ReadScopeNumber();
        }

        return 0;
    }
    
    public List<StructuredScope> loadScopesFromCsvFile(string scopeFile)
    {
        var scopes = new List<StructuredScope>();
        using (var reader = new StreamReader(scopeFile))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            scopes = csv.GetRecords<StructuredScope>().ToList();
        }

        return scopes;
    }

    private string GetScopeFolder()
    {
        var c = Path.Combine("/Users/mhellestveit/ScrapedWebsites/", ScopeName); 
        return Path.Combine("/Users/mhellestveit/ScrapedWebsites/", ScopeName);
    }

    private int ReadScopeNumber()
    {
        var jsonString = File.ReadAllText(Path.Combine(GetScopeFolder(), "scope.json"));
        
        var data = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);

        return data["ScopeNumber"];
    }
}