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
            var depManifest = helper.ModRegistry.Get(_manifest.ContentPackFor.UniqueID)?.Manifest;
            if (depManifest != null)
            {
                dependsOn.Add(depManifest.Name);
            }
        }

        foreach (var dep in _manifest.Dependencies)
        {
            var depManifest = helper.ModRegistry.Get(dep.UniqueID)?.Manifest;
            if (depManifest == null || !dep.IsRequired) continue;
            dependsOn.Add(depManifest.UniqueID);
        }


        var summary = _nexusInfo?.Summary ?? _manifest.Description;
        var categoryName = _nexusInfo?.categoryName ?? "No Category";
        var contentPackFor = helper.ModRegistry.Get(_manifest.ContentPackFor?.UniqueID ?? "")?.Manifest.Name;
        var hasNexusInfo = _nexusInfo != null;
        var downloads = _nexusInfo != null ? _nexusInfo.Downloads.ToString("N0") : "-";
        var endorsements = _nexusInfo != null ? _nexusInfo.Endorsements.ToString("N0") : "-";

        return new ModListMod(_manifest.UniqueID, _manifest.Name, _manifest.Author, summary, categoryName, 
            contentPackFor, dependsOn.ToList(), _nexusId, hasNexusInfo, downloads, endorsements);
    }
};