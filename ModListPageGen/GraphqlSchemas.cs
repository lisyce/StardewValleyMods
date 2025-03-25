namespace ModListPageGen;

public class GraphqlSchemas
{
    public class LegacyModsType
    {
        public NodesType legacyMods { get; set; }
    }

    public class NodesType
    {
        public NodeType[] nodes { get; set; }
    }

    public class NodeType
    {
        public string name { get; set; }
        public string summary { get; set; }
        public int downloads { get; set; }
        public int endorsements { get; set; }
        public bool adultContent { get; set; }
        public int modId { get; set; }
        public string pictureUrl { get; set; }
        public ModCategoryType modCategory { get; set; }

        public NexusInfo ToNexusInfo()
        {
            return new NexusInfo(name, summary, pictureUrl, downloads, modId, endorsements, adultContent, modCategory.name);
        }
    }

    public class ModCategoryType
    {
        public string name { get; set; }
    }

    public class LegacyModId
    {
        public int gameId { get; set; }
        public int modId { get; set; }
    }
}