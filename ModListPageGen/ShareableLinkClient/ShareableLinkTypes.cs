namespace ModListPageGen.ShareableLinkClient;

public record Category(string Name, string CssClass, int Count);

public class DependencyListEntry
{
    public string Name { get; init; }
    public int DepsCount { get; set; }
    public string CssClass { get; init; }
}

public record ModListMod(
    string Name,
    string Author,
    string Summary,

    string CategoryName,
    string CategoryClass,
    string DepsClasses,
    string? ContentPackFor,

    string NexusId,
    bool HasNexusInfo,
    string Downloads,
    string Endorsements);

public record ModList(
    string Title,
    string Author,
    int ModCount,
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