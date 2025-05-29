using StardewModdingAPI;

namespace MushroomLogFramework;

public class MushroomLogData
{
    public Dictionary<string, float> DefaultTreeWeights { get; set; }
    public Dictionary<string, Dictionary<string, float>> SpecificTreeWeights { get; set; }
}