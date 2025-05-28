using StardewModdingAPI;

namespace MushroomLogFramework;

public class MushroomLogData
{
    public Dictionary<string, int> DefaultTreeProbabilities { get; set; }
    public Dictionary<string, Dictionary<string, int>> SpecificTreeProbabilities { get; set; }
}