using System.Text;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MushroomLogFramework;

public class ModEntry : Mod
{
    private static readonly string ProduceRulesAssetName = "BarleyZP.MushroomLogFramework/ProduceRules";
    private static Dictionary<string, MushroomLogData>? _produceRules;
    
    internal static Dictionary<string, MushroomLogData> ProduceRules
    {
        get
        {
            _produceRules ??= Game1.content.Load<Dictionary<string, MushroomLogData>>(ProduceRulesAssetName);
            return _produceRules;
        }
    }
    
    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;

        helper.ConsoleCommands.Add("mushroom_log_summary", "Print a summary of the mushroom log data",
            PrintDistributions);

        var harmony = new Harmony(ModManifest.UniqueID);
        HarmonyPatches.Patch(harmony);
    }

    private void PrintDistributions(string command, string[] args)
    {
        var builder = new StringBuilder();
        foreach (var pair in ProduceRules)
        {
            builder.Append("\n\n").Append(pair.Key).Append("\n--------------------");
            builder.Append($"\nDefaults: [ {DistributionToString(pair.Value.DefaultTreeWeights)} ]");
            foreach (var specific in pair.Value.SpecificTreeWeights)
            {
                builder.Append($"\n{specific.Key}: [ {DistributionToString(specific.Value)} ]");
            }
        }
        
        Monitor.Log(builder.ToString(), LogLevel.Info);
    }

    private string DistributionToString(Dictionary<string, float> distribution)
    {
        Util.NormalizeDistribution(distribution);
        
        var sb = new StringBuilder();
        foreach (var pair in distribution)
        {
            sb.Append(pair.Key).Append(": ").Append(Math.Round(pair.Value, 2)).Append(", ");
        }
        
        return sb.ToString().TrimEnd(',', ' ');
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(ProduceRulesAssetName))
        {
            e.LoadFromModFile<Dictionary<string, MushroomLogData>>("assets/VanillaProduceRules.json", AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(ProduceRulesAssetName)))
        {
            _produceRules = null;
        }
    }
}