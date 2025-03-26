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

    public class TemplatedModInfo
    {
        public bool HasNexus;
        public string Name;
        public string Author;
        public string? NexusId;
        public string Summary;
        public string Downloads;
        public string Endorsements;
        public bool? AdultContent;
        public string? PictureUrl;
        public string? ContentPackFor;
        public string CategoryName;
        public string CategoryClass;
        public string DepsClasses;
    }

    public TemplatedModInfo ToTemplate(IModHelper helper)
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
            dependsOn.Add(depManifest.Name);
        }
        
        return new TemplatedModInfo
        {
            HasNexus = _nexusInfo != null,
            
            Name = _manifest.Name,
            Author = _manifest.Author,
            NexusId = _nexusId,
            Summary = _nexusInfo?.Summary ?? _manifest.Description,
            Downloads = _nexusInfo != null ? _nexusInfo.Downloads.ToString("N0") : "-",
            Endorsements = _nexusInfo != null ? _nexusInfo.Endorsements.ToString("N0") : "-",
            AdultContent = _nexusInfo?.AdultContent,
            PictureUrl = _nexusInfo?.PictureUrl,
            ContentPackFor = helper.ModRegistry.Get(_manifest.ContentPackFor?.UniqueID ?? "")?.Manifest.Name,
            CategoryName = _nexusInfo?.categoryName ?? "No Category",
            CategoryClass = _nexusInfo?.categoryName.Replace(" ", "_") ?? "No_Category",
            DepsClasses = dependsOn.Count > 0 ? string.Join(" ", dependsOn.Select(d => d.Replace(" ", "_"))) : "No_Deps",
        };
    }
};