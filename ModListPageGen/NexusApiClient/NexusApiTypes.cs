namespace ModListPageGen;

public class LegacyMods
{
    public Nodes legacyMods { get; set; }
}

public class Nodes
{
    public Node[] nodes { get; set; }
}

public class Node
{
    public string name { get; set; }
    public string summary { get; set; }
    public int downloads { get; set; }
    public int endorsements { get; set; }
    public bool adultContent { get; set; }
    public int modId { get; set; }
    public string pictureUrl { get; set; }
    public ModCategory modCategory { get; set; }

    public NexusInfo ToNexusInfo()
    {
        return new NexusInfo(name, summary, pictureUrl, downloads, modId, endorsements, adultContent, modCategory.name);
    }
}

public class ModCategory
{
    public string name { get; set; }
}

public class LegacyModId
{
    public int gameId { get; set; }
    public int modId { get; set; }
}