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

    public ModListMod ToModListMod(IModHelper helper, IMonitor monitor)
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
        else if (TryGetGithubUpdateKey(out var githubKey))
        {
            urlInfo = new UrlInfo("GitHub", $"https://github.com/{githubKey}");
        }
        
        // validate manifest fields
        if (_manifest.Author == null)
        {
            monitor.Log($"{_manifest.Name} ({_manifest.UniqueID}) has no \"Author\" field specified in their manifest.json", LogLevel.Debug);
        }
        
        if (summary == null)
        {
            monitor.Log($"{_manifest.Name} ({_manifest.UniqueID}) has no \"Description\" field specified in their manifest.json", LogLevel.Debug);
        }

        return new ModListMod(_manifest.UniqueID, _manifest.Name, _manifest.Author ?? "(No Author Provided)", summary ?? "(No Summary Provided)", categoryName, 
            contentPackFor, dependsOn.ToList(), urlInfo, shareableNexusInfo);
    }

    private bool TryGetGithubUpdateKey(out string key)
    {
        key = "";  // default
        
        foreach (var k in _manifest.UpdateKeys)
        {
            if (k.ToLower().StartsWith("github:"))
            {
                key = k[7..];
                return true;
            }
        }

        return false;
    }
};