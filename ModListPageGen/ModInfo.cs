using ModListPageGen.ShareableLinkClient;
using StardewModdingAPI;

namespace ModListPageGen;

public record NexusInfo(string Name, string Summary, string PictureUrl, int Downloads,
    int ModId, int Endorsements, bool AdultContent, string categoryName);

public class ModInfo
{
    private IManifest _manifest;
    private NexusInfo? _nexusInfo;
    private string? _nexusId;

    public string Name => _manifest.Name;
    public string UniqueId => _manifest.UniqueID;

    public ModInfo(IManifest manifest, NexusInfo? nexus, string? nexusId = null)
    {
        _manifest = manifest;
        _nexusInfo = nexus;
        _nexusId = nexusId;
    }

    public ModListMod ToModListMod(IModHelper helper)
    {
        // find everything this mod depends on
        HashSet<string> dependsOn = new();
        if (_manifest.ContentPackFor != null)
        {
            dependsOn.Add(_manifest.ContentPackFor.UniqueID);
        }

        foreach (var dep in _manifest.Dependencies)
        {
            if (dep.IsRequired) dependsOn.Add(dep.UniqueID);
        }


        var summary = _nexusInfo?.Summary ?? _manifest.Description;
        var categoryName = _nexusInfo?.categoryName ?? "No Category";

        ContentPackFor? contentPackFor = null;
        if (_manifest.ContentPackFor != null)
        {
            var packForInfo = helper.ModRegistry.Get(_manifest.ContentPackFor.UniqueID);
            if (packForInfo != null)
                contentPackFor = new ContentPackFor(packForInfo.Manifest.Name, packForInfo.Manifest.UniqueID);
        }
        
        var hasNexusInfo = _nexusInfo != null;
        var downloads = _nexusInfo != null ? _nexusInfo.Downloads.ToString("N0") : "-";
        var endorsements = _nexusInfo != null ? _nexusInfo.Endorsements.ToString("N0") : "-";

        ShareableLinkClient.NexusInfo? shareableNexusInfo = null;
        if (hasNexusInfo)
        {
            shareableNexusInfo = new ShareableLinkClient.NexusInfo(downloads, endorsements);
        }

        UrlInfo? urlInfo = null;
        if (_nexusId != null)
        {
            urlInfo = new UrlInfo("Nexus", $"https://www.nexusmods.com/stardewvalley/mods/{_nexusId}");
        }
        

        return new ModListMod(_manifest.UniqueID, _manifest.Name, _manifest.Author, summary, categoryName, 
            contentPackFor, dependsOn.ToList(), urlInfo, shareableNexusInfo);
    }
};