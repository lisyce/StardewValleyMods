using Newtonsoft.Json;

namespace ModListPageGen.ShareableLinkClient;

public record ContentPackFor(string Name, string UniqueId);

public record UrlInfo(string SiteName, string Url);

public record Category(string Name, int Count);

public record NexusInfo(string Downloads, string Endorsements);

public class DependencyListEntry
{
    public string Name { get; set; }
    public int DepsCount { get; set; }
    public string UniqueId { get; set; }
}

public record ModListMod(
    string UniqueId,
    string Name,
    string Author,
    string Summary,

    string CategoryName,
    ContentPackFor? ContentPackFor,
    List<string> DependsOn,

    UrlInfo? UrlInfo,
    NexusInfo? NexusInfo);

public class ModList
{
    [JsonProperty("$schema")]
    public string Schema => "https://modlists.barleyzp.com/schemas/modlist-v2.json";

    public string Title { get; set; }
    public string Author { get; set; }
    public List<ModListMod> Mods { get; set; }
    public List<Category> Categories { get; set; }
    public List<DependencyListEntry> DependencyList { get; set; }
    public string DateCreated { get; set; }
    public string GameVersion { get; set; }
    public string SmapiVersion { get; set; }
    public string GeneratorVersion { get; set; }
    
    public ModList(
        string title,
        string author,
        List<ModListMod> mods,
        List<Category> categories,
        List<DependencyListEntry> dependencyList,
        string dateCreated,
        string gameVersion,
        string smapiVersion,
        string generatorVersion = "2.0.0")
    {
        Title = title;
        Author = author;
        Mods = mods;
        Categories = categories;
        DependencyList = dependencyList;
        DateCreated = dateCreated;
        GameVersion = gameVersion;
        SmapiVersion = smapiVersion;
        GeneratorVersion = generatorVersion;
    }
}
    
public record ModPageData(
    ModList ModList,
    string ColorTheme);