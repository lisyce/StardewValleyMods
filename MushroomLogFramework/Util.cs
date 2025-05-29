using StardewValley;

namespace MushroomLogFramework;

public class Util
{
    public static void NormalizeDistribution(Dictionary<string, float> distribution)
    {
        var total = distribution.Values.Sum();
        foreach (var key in distribution.Keys)
        {
            distribution[key] /= total;
        }
    }
    
    public static string DrawFromDistribution(Dictionary<string, float> distribution, string fallback)
    {
        if (!distribution.Any()) return fallback;
        
        NormalizeDistribution(distribution);
        
        var rand = Game1.random.NextDouble();
        var ptr = 0d;
        foreach (var pair in distribution)
        {
            ptr += pair.Value;
            if (ptr >= rand) return pair.Key;
        }

        return fallback;  // should never get here
    }
}