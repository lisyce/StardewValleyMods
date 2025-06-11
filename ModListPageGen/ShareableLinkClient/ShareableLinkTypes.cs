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

public record ModList(
    string Title,
    string Author,
    List<ModListMod> Mods,
    List<Category> Categories,
    List<DependencyListEntry> DependencyList,
    string DateCreated,
    string GameVersion,
    string SmapiVersion,
    string GeneratorVersion = "2.0.0");
    
public record ModPageData(
    ModList ModList,
    string ColorTheme);