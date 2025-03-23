using StardewModdingAPI;

namespace ModListPageGen;

public record NexusInfo(string Name, string Summary, string PictureUrl, int Downloads,
    int UniqueDownloads, int ModId, int Endorsements, bool AdultContent);

public class ModInfo
{
    private IManifest _manifest;
    private NexusInfo? _nexusInfo;
    public ModInfo(IManifest manifest, NexusInfo? nexus)
    {
        _manifest = manifest;
        _nexusInfo = nexus;
    }

    public class TemplatedModInfo
    {
        public bool HasNexus;
        public string Name;
        public string Author;
        public int? NexusId;
        public string Summary;
        public string Downloads;
        public string Endorsements;
        public bool? AdultContent;
        public string? PictureUrl;
    }

    public TemplatedModInfo ToTemplate()
    {
        return new TemplatedModInfo
        {
            HasNexus = _nexusInfo != null,
            
            Name = _manifest.Name,
            Author = _manifest.Author,
            NexusId = _nexusInfo?.ModId,
            Summary = _nexusInfo?.Summary ?? _manifest.Description,
            Downloads = _nexusInfo != null ? _nexusInfo.Downloads.ToString("N0") : "-",
            Endorsements = _nexusInfo != null ? _nexusInfo.Endorsements.ToString("N0") : "-",
            AdultContent = _nexusInfo?.AdultContent,
            PictureUrl = _nexusInfo?.PictureUrl,
        };
    }
};